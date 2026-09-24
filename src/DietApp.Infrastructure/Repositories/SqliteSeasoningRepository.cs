using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para el almacenamiento y recuperacion de alinos y condimentos.
/// Administra la persistencia de las cabeceras de alinos y sus items dosificados con sus minerales asociados.
/// Por que se tomo esta decision: Implementa ISeasoningRepository garantizando bajo acoplamiento
/// con la capa de aplicacion y asegurando consistencia transaccional al guardar o eliminar un alino y sus componentes.
/// </summary>
public class SqliteSeasoningRepository : ISeasoningRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteSeasoningRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<Seasoning>> GetAllAsync()
    {
        await _dbContext.InitializeAsync();

        var entities = await _dbContext.Connection.Table<SeasoningEntity>()
            .OrderBy(s => s.Name)
            .ToListAsync();

        var list = new List<Seasoning>(entities.Count);
        foreach (var entity in entities)
        {
            var itemEntities = await _dbContext.Connection.Table<SeasoningItemEntity>()
                .Where(i => i.SeasoningId == entity.Id)
                .ToListAsync();

            list.Add(entity.ToDomain(itemEntities.Select(i => i.ToDomain())));
        }

        return list;
    }

    public async Task<Seasoning?> GetByIdAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        var entity = await _dbContext.Connection.Table<SeasoningEntity>()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (entity == null) return null;

        var itemEntities = await _dbContext.Connection.Table<SeasoningItemEntity>()
            .Where(i => i.SeasoningId == id)
            .ToListAsync();

        return entity.ToDomain(itemEntities.Select(i => i.ToDomain()));
    }

    public async Task SaveAsync(Seasoning seasoning)
    {
        if (seasoning == null)
        {
            throw new ArgumentNullException(nameof(seasoning));
        }

        await _dbContext.InitializeAsync();

        var entity = SeasoningEntity.FromDomain(seasoning);

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            var existing = conn.Find<SeasoningEntity>(seasoning.Id);
            if (existing != null)
            {
                conn.Update(entity);
                conn.Execute("DELETE FROM SeasoningItems WHERE SeasoningId = ?", seasoning.Id);
            }
            else
            {
                conn.Insert(entity);
            }

            foreach (var item in seasoning.Items)
            {
                var itemEntity = SeasoningItemEntity.FromDomain(seasoning.Id, item);
                conn.Insert(itemEntity);
            }
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            conn.Execute("DELETE FROM SeasoningItems WHERE SeasoningId = ?", id);
            conn.Delete<SeasoningEntity>(id);
        });
    }
}
