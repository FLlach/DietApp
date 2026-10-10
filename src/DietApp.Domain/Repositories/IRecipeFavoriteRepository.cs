namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para la administracion de recetas favoritas por perfil de usuario.
/// Permite consultar si una receta es favorita, agregar o eliminar de favoritos y listar los identificadores guardados.
/// Por que se tomo esta decision: Desacopla la persistencia de favoritos permitiendo implementaciones
/// hibridas en SQLite y Supabase asociadas al identificador del usuario.
/// </summary>
public interface IRecipeFavoriteRepository
{
    Task<bool> IsFavoriteAsync(string userId, Guid recipeId);
    Task AddFavoriteAsync(string userId, Guid recipeId);
    Task RemoveFavoriteAsync(string userId, Guid recipeId);
    Task<IReadOnlyList<Guid>> GetFavoriteRecipeIdsAsync(string userId);
}
