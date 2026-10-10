using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Repositories;
using DietApp.Infrastructure.Supabase.Models;

namespace DietApp.Infrastructure.Supabase.Repositories;

/// <summary>
/// Como funciona: Repositorio hibrido para recetas culinarias que combina la base local SQLite
/// con el catalogo global de recetas almacenado en Supabase PostgreSQL.
/// Permite descubrir y buscar recetas globales publicadas en la nube y persistirlas en la cache local.
/// Por que se tomo esta decision: Resuelve la busqueda de recetas globales sin perder la operatividad
/// y velocidad del almacenamiento offline-first.
/// </summary>
public class SupabaseRecipeRepository : IRecipeRepository
{
    private readonly ISupabaseClientProvider _clientProvider;
    private readonly SqliteRecipeRepository _localRepository;

    public SupabaseRecipeRepository(
        ISupabaseClientProvider clientProvider,
        SqliteRecipeRepository localRepository)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _localRepository = localRepository ?? throw new ArgumentNullException(nameof(localRepository));
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        return await _localRepository.GetAllAsync();
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        var local = await _localRepository.GetByIdAsync(id);
        if (local != null) return local;

        if (_clientProvider.IsConfigured)
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var response = await client
                    .From<SupabaseRecipeModel>()
                    .Where(x => x.Id == id.ToString())
                    .Get();

                var remoteModel = response?.Models?.FirstOrDefault();
                if (remoteModel != null)
                {
                    // Consultar ingredientes remotos
                    var ingResponse = await client
                        .From<SupabaseRecipeIngredientModel>()
                        .Where(x => x.RecipeId == id.ToString())
                        .Get();

                    var remoteIngredients = ingResponse?.Models?.Select(i => i.ToDomain()) ?? Enumerable.Empty<RecipeIngredient>();

                    // Consultar pasos remotos
                    var stepResponse = await client
                        .From<SupabaseRecipeStepModel>()
                        .Where(x => x.RecipeId == id.ToString())
                        .Get();

                    var remoteSteps = stepResponse?.Models?.Select(s => s.ToDomain()) ?? Enumerable.Empty<RecipeStep>();

                    var remoteRecipe = remoteModel.ToDomain(remoteIngredients, remoteSteps);
                    await _localRepository.SaveAsync(remoteRecipe);
                    return remoteRecipe;
                }
            }
            catch
            {
                // Fallback silencioso
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<Recipe>> SearchByTitleAsync(string query)
    {
        return await _localRepository.SearchByTitleAsync(query);
    }

    public async Task<IReadOnlyList<Recipe>> GetGlobalRecipesAsync()
    {
        // 1. Obtener recetas globales locales
        var localGlobalRecipes = await _localRepository.GetGlobalRecipesAsync();
        var recipeDict = localGlobalRecipes.ToDictionary(r => r.Id, r => r);

        // 2. Si hay conexion a Supabase, consultar catalogo global en la nube
        if (_clientProvider.IsConfigured)
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var response = await client
                    .From<SupabaseRecipeModel>()
                    .Where(x => x.IsGlobal == true)
                    .Get();

                if (response?.Models != null)
                {
                    foreach (var model in response.Models)
                    {
                        if (Guid.TryParse(model.Id, out var recipeId) && !recipeDict.ContainsKey(recipeId))
                        {
                            var domainRecipe = model.ToDomain(Enumerable.Empty<RecipeIngredient>(), Enumerable.Empty<RecipeStep>());
                            recipeDict[recipeId] = domainRecipe;
                            // Cachear en SQLite local
                            await _localRepository.SaveAsync(domainRecipe);
                        }
                    }
                }
            }
            catch
            {
                // Fallback a coleccion local
            }
        }

        return recipeDict.Values.OrderBy(r => r.Title).ToList();
    }

    public async Task<IReadOnlyList<Recipe>> SearchGlobalRecipesAsync(string query)
    {
        string trimmed = query?.Trim() ?? string.Empty;
        var globalRecipes = await GetGlobalRecipesAsync();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return globalRecipes;
        }

        return globalRecipes
            .Where(r => r.Title.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
                        r.Description.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Title)
            .ToList();
    }

    public async Task SaveAsync(Recipe recipe)
    {
        await _localRepository.SaveAsync(recipe);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _localRepository.DeleteAsync(id);
    }
}
