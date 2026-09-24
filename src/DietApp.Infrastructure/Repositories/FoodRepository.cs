using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.SeedData;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en memoria concurrente para la gestion del catalogo de alimentos.
/// Se inicializa con los datos semilla de alimentos y minerales, y permite consultas filtradas por rangos.
/// Por que se tomo esta decision: Facilita una ejecucion veloz y confiable en todas las plataformas soportadas
/// por .NET MAUI (Android, Windows, iOS) sin riesgo de incompatibilidad de librerias nativas de base de datos
/// en etapas de desarrollo inicial, desacoplado completamente tras la interfaz IFoodRepository.
/// </summary>
public class FoodRepository : IFoodRepository
{
    private readonly List<FoodItem> _foods;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FoodRepository()
    {
        _foods = InitialFoodCatalogSeed.GetPreloadedFoods();
    }

    public async Task<FoodItem?> GetByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            return _foods.FirstOrDefault(f => f.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _foods.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<FoodItem>> FilterByMineralRangeAsync(
        MineralType mineralType,
        double minimumMilligrams,
        double maximumMilligrams)
    {
        await _lock.WaitAsync();
        try
        {
            var filtered = _foods
                .Where(food =>
                {
                    double mineralValue = food.GetMineralMilligrams(mineralType);
                    return mineralValue >= minimumMilligrams && mineralValue <= maximumMilligrams;
                })
                .OrderByDescending(food => food.GetMineralMilligrams(mineralType))
                .ToList();

            return filtered;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<FoodItem>> SearchByNameOrCategoryAsync(string query)
    {
        await _lock.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _foods.ToList();
            }

            string normalized = query.Trim().ToLowerInvariant();
            return _foods
                .Where(food => food.Name.ToLowerInvariant().Contains(normalized) ||
                               food.Category.ToLowerInvariant().Contains(normalized))
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(FoodItem foodItem)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        await _lock.WaitAsync();
        try
        {
            _foods.Add(foodItem);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(FoodItem foodItem)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        await _lock.WaitAsync();
        try
        {
            int index = _foods.FindIndex(f => f.Id == foodItem.Id);
            if (index >= 0)
            {
                _foods[index] = foodItem;
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
            _foods.RemoveAll(f => f.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }
}
