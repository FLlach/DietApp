using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para la administracion de recetas favoritas asociadas a cada perfil de usuario.
/// Persiste y consulta registros en la tabla UserRecipeFavorites indexada por UserId y RecipeId.
/// Por que se tomo esta decision: Permite almacenar favoritos offline y mantenerlos aislados por perfil de paciente.
/// </summary>
public class SqliteRecipeFavoriteRepository : IRecipeFavoriteRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteRecipeFavoriteRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> IsFavoriteAsync(string userId, Guid recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId) || recipeId == Guid.Empty) return false;

        await _dbContext.InitializeAsync();
        var existing = await _dbContext.Connection.Table<UserRecipeFavoriteEntity>()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

        return existing != null;
    }

    public async Task AddFavoriteAsync(string userId, Guid recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId) || recipeId == Guid.Empty) return;

        await _dbContext.InitializeAsync();
        var existing = await _dbContext.Connection.Table<UserRecipeFavoriteEntity>()
            .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

        if (existing == null)
        {
            var entity = new UserRecipeFavoriteEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId.Trim(),
                RecipeId = recipeId,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.Connection.InsertAsync(entity);
        }
    }

    public async Task RemoveFavoriteAsync(string userId, Guid recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId) || recipeId == Guid.Empty) return;

        await _dbContext.InitializeAsync();
        await _dbContext.Connection.ExecuteAsync(
            "DELETE FROM UserRecipeFavorites WHERE UserId = ? AND RecipeId = ?",
            userId.Trim(),
            recipeId);
    }

    public async Task<IReadOnlyList<Guid>> GetFavoriteRecipeIdsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Array.Empty<Guid>();

        await _dbContext.InitializeAsync();
        var favorites = await _dbContext.Connection.Table<UserRecipeFavoriteEntity>()
            .Where(f => f.UserId == userId.Trim())
            .ToListAsync();

        return favorites.Select(f => f.RecipeId).ToList();
    }
}
