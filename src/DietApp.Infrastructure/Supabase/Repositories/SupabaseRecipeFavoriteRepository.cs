using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Repositories;
using DietApp.Infrastructure.Supabase.Models;

namespace DietApp.Infrastructure.Supabase.Repositories;

/// <summary>
/// Como funciona: Repositorio hibrido (Offline-First) para favoritos de recetas culinarias.
/// Almacena instantaneamente en SQLite local para garantizar acceso y modificacion sin conexion a red,
/// y sincroniza con Supabase (tabla `user_recipe_favorites`) cuando hay conexion y credenciales activas.
/// Por que se tomo esta decision: Permite que los favoritos de cada perfil de paciente se preserven
/// tanto localmente como en la nube con politicas RLS activas.
/// </summary>
public class SupabaseRecipeFavoriteRepository : IRecipeFavoriteRepository
{
    private readonly ISupabaseClientProvider _clientProvider;
    private readonly SqliteRecipeFavoriteRepository _localRepository;

    public SupabaseRecipeFavoriteRepository(
        ISupabaseClientProvider clientProvider,
        SqliteRecipeFavoriteRepository localRepository)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _localRepository = localRepository ?? throw new ArgumentNullException(nameof(localRepository));
    }

    public async Task<bool> IsFavoriteAsync(string userId, Guid recipeId)
    {
        return await _localRepository.IsFavoriteAsync(userId, recipeId);
    }

    public async Task AddFavoriteAsync(string userId, Guid recipeId)
    {
        // 1. Guardar localmente de inmediato
        await _localRepository.AddFavoriteAsync(userId, recipeId);

        // 2. Sincronizar con Supabase si el usuario esta autenticado
        if (_clientProvider.IsConfigured && !string.IsNullOrWhiteSpace(userId) && userId != "local_user")
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var model = new SupabaseRecipeFavoriteModel
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId.Trim(),
                    RecipeId = recipeId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await client
                    .From<SupabaseRecipeFavoriteModel>()
                    .Upsert(model);
            }
            catch
            {
                // Fallback silencioso; los datos locales permanecen intactos
            }
        }
    }

    public async Task RemoveFavoriteAsync(string userId, Guid recipeId)
    {
        // 1. Eliminar localmente de inmediato
        await _localRepository.RemoveFavoriteAsync(userId, recipeId);

        // 2. Sincronizar eliminacion en Supabase
        if (_clientProvider.IsConfigured && !string.IsNullOrWhiteSpace(userId) && userId != "local_user")
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                await client
                    .From<SupabaseRecipeFavoriteModel>()
                    .Where(x => x.UserId == userId.Trim() && x.RecipeId == recipeId.ToString())
                    .Delete();
            }
            catch
            {
                // Fallback silencioso
            }
        }
    }

    public async Task<IReadOnlyList<Guid>> GetFavoriteRecipeIdsAsync(string userId)
    {
        // 1. Si hay sesion en la nube, sincronizar favoritos remotos hacia local
        if (_clientProvider.IsConfigured && !string.IsNullOrWhiteSpace(userId) && userId != "local_user")
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var response = await client
                    .From<SupabaseRecipeFavoriteModel>()
                    .Where(x => x.UserId == userId.Trim())
                    .Get();

                if (response?.Models != null)
                {
                    foreach (var model in response.Models)
                    {
                        if (Guid.TryParse(model.RecipeId, out var recipeGuid))
                        {
                            await _localRepository.AddFavoriteAsync(userId, recipeGuid);
                        }
                    }
                }
            }
            catch
            {
                // Fallback a almacenamiento local ante cortes de conexion
            }
        }

        return await _localRepository.GetFavoriteRecipeIdsAsync(userId);
    }
}
