using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Data.Models;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en SQLite para el catalogo de alimentos y minerales.
/// Aprovecha los indices B-Tree de SQLite en las columnas de minerales (PhosphorusMg, PotassiumMg, SodiumMg)
/// para responder a consultas de filtrado por rango de forma casi instantanea.
/// Por que se tomo esta decision: Reemplaza la coleccion en memoria con persistencia real y transaccional,
/// cumpliendo estrictamente con el contrato IFoodRepository definido en la capa de Dominio.
/// </summary>
public class SqliteFoodRepository : IFoodRepository
{
    private readonly DietAppDbContext _dbContext;

    public SqliteFoodRepository(DietAppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<FoodItem?> GetByIdAsync(Guid id)
    {
        await _dbContext.InitializeAsync();
        var entity = await _dbContext.Connection.Table<FoodEntity>().FirstOrDefaultAsync(f => f.Id == id);
        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        await _dbContext.InitializeAsync();
        var entities = await _dbContext.Connection.Table<FoodEntity>().OrderBy(f => f.Name).ToListAsync();
        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<FoodItem>> FilterByMineralRangeAsync(
        MineralType mineralType,
        double minimumMilligrams,
        double maximumMilligrams)
    {
        await _dbContext.InitializeAsync();

        string columnName = mineralType switch
        {
            MineralType.Phosphorus => nameof(FoodEntity.PhosphorusMg),
            MineralType.Potassium => nameof(FoodEntity.PotassiumMg),
            MineralType.Sodium => nameof(FoodEntity.SodiumMg),
            MineralType.Calcium => nameof(FoodEntity.CalciumMg),
            MineralType.Magnesium => nameof(FoodEntity.MagnesiumMg),
            MineralType.Iron => nameof(FoodEntity.IronMg),
            MineralType.Zinc => nameof(FoodEntity.ZincMg),
            _ => nameof(FoodEntity.PotassiumMg)
        };

        // Consulta optimizada aprovechando el indice de la columna
        string sql = $"SELECT * FROM Foods WHERE {columnName} >= ? AND {columnName} <= ? ORDER BY {columnName} DESC";
        var entities = await _dbContext.Connection.QueryAsync<FoodEntity>(sql, minimumMilligrams, maximumMilligrams);

        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<FoodItem>> SearchByNameOrCategoryAsync(string query)
    {
        await _dbContext.InitializeAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        string pattern = $"%{query.Trim()}%";
        string sql = "SELECT * FROM Foods WHERE Name LIKE ? OR Category LIKE ? ORDER BY Name ASC";
        var entities = await _dbContext.Connection.QueryAsync<FoodEntity>(sql, pattern, pattern);

        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task AddAsync(FoodItem foodItem)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        await _dbContext.InitializeAsync();
        var entity = FoodEntity.FromDomain(foodItem);
        await _dbContext.Connection.InsertAsync(entity);
    }

    public async Task UpdateAsync(FoodItem foodItem)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        await _dbContext.InitializeAsync();
        var entity = FoodEntity.FromDomain(foodItem);
        await _dbContext.Connection.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _dbContext.InitializeAsync();
        await _dbContext.Connection.DeleteAsync<FoodEntity>(id);
    }
}
