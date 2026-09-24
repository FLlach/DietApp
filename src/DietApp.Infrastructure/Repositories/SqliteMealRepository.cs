using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para el registro y consulta de comidas consumidas.
/// Recupera las entidades de comida y asocia sus alimentos ingeridos recuperando los registros de MealItems.
/// Por que se tomo esta decision: Cumple con el contrato IMealRepository y garantiza la persistencia
/// del historial diario de comidas a traves de SQLite.
/// </summary>
public class SqliteMealRepository : IMealRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteMealRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Meal?> GetByIdAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        var mealEntity = await _dbContext.Connection.Table<MealEntity>().FirstOrDefaultAsync(m => m.Id == id);
        if (mealEntity == null) return null;

        var itemEntities = await _dbContext.Connection.Table<MealItemEntity>()
            .Where(i => i.MealId == id)
            .ToListAsync();

        var domainItems = itemEntities.Select(i => i.ToDomain()).ToList();
        return mealEntity.ToDomain(domainItems);
    }

    public async Task<IReadOnlyList<Meal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await _dbContext.InitializeAsync();

        var mealEntities = await _dbContext.Connection.Table<MealEntity>()
            .Where(m => m.Date >= startDate && m.Date <= endDate)
            .OrderBy(m => m.Date)
            .ToListAsync();

        var meals = new List<Meal>(mealEntities.Count);
        foreach (var entity in mealEntities)
        {
            var itemEntities = await _dbContext.Connection.Table<MealItemEntity>()
                .Where(i => i.MealId == entity.Id)
                .ToListAsync();

            var domainItems = itemEntities.Select(i => i.ToDomain()).ToList();
            meals.Add(entity.ToDomain(domainItems));
        }

        return meals;
    }

    public async Task<IReadOnlyList<Meal>> GetAllAsync()
    {
        await _dbContext.InitializeAsync();

        var mealEntities = await _dbContext.Connection.Table<MealEntity>()
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        var meals = new List<Meal>(mealEntities.Count);
        foreach (var entity in mealEntities)
        {
            var itemEntities = await _dbContext.Connection.Table<MealItemEntity>()
                .Where(i => i.MealId == entity.Id)
                .ToListAsync();

            meals.Add(entity.ToDomain(itemEntities.Select(i => i.ToDomain())));
        }

        return meals;
    }

    public async Task SaveAsync(Meal meal)
    {
        if (meal == null)
        {
            throw new ArgumentNullException(nameof(meal));
        }

        await _dbContext.InitializeAsync();

        var mealEntity = MealEntity.FromDomain(meal);

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            var existing = conn.Find<MealEntity>(meal.Id);
            if (existing != null)
            {
                conn.Update(mealEntity);
                conn.Execute("DELETE FROM MealItems WHERE MealId = ?", meal.Id);
            }
            else
            {
                conn.Insert(mealEntity);
            }

            foreach (var item in meal.Items)
            {
                var itemEntity = MealItemEntity.FromDomain(meal.Id, item);
                conn.Insert(itemEntity);
            }
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _dbContext.InitializeAsync();

        await _dbContext.Connection.RunInTransactionAsync(conn =>
        {
            conn.Execute("DELETE FROM MealItems WHERE MealId = ?", id);
            conn.Delete<MealEntity>(id);
        });
    }
}
