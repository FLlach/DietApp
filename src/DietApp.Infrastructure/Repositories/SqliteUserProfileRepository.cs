using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para la gestion del perfil y las preferencias globales del usuario.
/// Realiza operaciones idempotentes de insercion y actualizacion (Upsert) garantizando que siempre
/// exista un unico perfil activo por usuario local (`local_user`).
/// Por que se tomo esta decision: Cumple con el contrato `IUserProfileRepository` desacoplando
/// el motor fisico de base de datos de la logica de negocio, permitiendo reemplazar esta implementacion
/// por un repositorio de Supabase cuando se inicie la migracion a la nube.
/// </summary>
public class SqliteUserProfileRepository : IUserProfileRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteUserProfileRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserProfile?> GetProfileAsync(string? userId = null)
    {
        await _dbContext.InitializeAsync();
        string targetUserId = string.IsNullOrWhiteSpace(userId) ? "local_user" : userId.Trim();

        var entity = await _dbContext.Connection.Table<UserProfileEntity>()
            .FirstOrDefaultAsync(u => u.UserId == targetUserId);

        // Si no se encuentra por UserId exacto pero existe algun perfil registrado, devolver el primero
        if (entity == null)
        {
            entity = await _dbContext.Connection.Table<UserProfileEntity>().FirstOrDefaultAsync();
        }

        return entity?.ToDomain();
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        await _dbContext.InitializeAsync();

        var existingEntity = await _dbContext.Connection.Table<UserProfileEntity>()
            .FirstOrDefaultAsync(u => u.Id == profile.Id || u.UserId == profile.UserId);

        var entityToSave = UserProfileEntity.FromDomain(profile);

        if (existingEntity != null)
        {
            entityToSave.Id = existingEntity.Id;
            await _dbContext.Connection.UpdateAsync(entityToSave);
        }
        else
        {
            await _dbContext.Connection.InsertAsync(entityToSave);
        }
    }

    public async Task<bool> HasProfileAsync(string? userId = null)
    {
        await _dbContext.InitializeAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            int count = await _dbContext.Connection.Table<UserProfileEntity>().CountAsync();
            return count > 0;
        }

        string targetUserId = userId.Trim();
        int matchingCount = await _dbContext.Connection.Table<UserProfileEntity>()
            .Where(u => u.UserId == targetUserId)
            .CountAsync();

        return matchingCount > 0;
    }
}
