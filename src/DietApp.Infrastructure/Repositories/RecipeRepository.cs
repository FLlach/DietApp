using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.SeedData;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en memoria concurrente para la gestion y persistencia de recetas culinarias.
/// Se inicializa con recetas de ejemplo precargadas vinculadas a los alimentos del catalogo base.
/// Por que se tomo esta decision: Permite una ejecucion ligera y desacoplada de la base de datos subyacente,
/// asegurando disponibilidad inmediata en Android, iOS y Windows respetando el contrato IRecipeRepository.
/// </summary>
public class RecipeRepository : IRecipeRepository
{
    private readonly List<Recipe> _recipes;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RecipeRepository(IFoodRepository foodRepository)
    {
        var preloadedFoods = InitialFoodCatalogSeed.GetPreloadedFoods();
        _recipes = InitialRecipeSeed.GetPreloadedRecipes(preloadedFoods);
    }

    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _recipes.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            return _recipes.FirstOrDefault(r => r.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Recipe>> SearchByTitleAsync(string query)
    {
        await _lock.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _recipes.ToList();
            }

            string normalized = query.Trim().ToLowerInvariant();
            return _recipes
                .Where(r => r.Title.ToLowerInvariant().Contains(normalized) ||
                            r.Description.ToLowerInvariant().Contains(normalized))
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(Recipe recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        await _lock.WaitAsync();
        try
        {
            int index = _recipes.FindIndex(r => r.Id == recipe.Id);
            if (index >= 0)
            {
                _recipes[index] = recipe;
            }
            else
            {
                _recipes.Add(recipe);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            _recipes.RemoveAll(r => r.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }
}
