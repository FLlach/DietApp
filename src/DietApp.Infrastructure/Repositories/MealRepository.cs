using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;

namespace DietApp.Infrastructure.Repositories;

/// <summary>
/// Como funciona: Repositorio en memoria concurrente para almacenar las comidas e ingestas del usuario.
/// Permite recuperar registros filtrados por rango de fechas para generar totales de minerales.
/// Por que se tomo esta decision: Aísla el almacenamiento de comidas tras la interfaz IMealRepository,
/// permitiendo realizar pruebas unitarias y prototipado sin friccion ni dependencias externas.
/// </summary>
public class MealRepository : IMealRepository
{
    private readonly List<Meal> _meals = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Meal?> GetByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            return _meals.FirstOrDefault(m => m.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Meal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await _lock.WaitAsync();
        try
        {
            return _meals
                .Where(m => m.Date >= startDate && m.Date <= endDate)
                .OrderBy(m => m.Date)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Meal>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _meals.OrderByDescending(m => m.Date).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(Meal meal)
    {
        if (meal == null)
        {
            throw new ArgumentNullException(nameof(meal));
        }

        await _lock.WaitAsync();
        try
        {
            int index = _meals.FindIndex(m => m.Id == meal.Id);
            if (index >= 0)
            {
                _meals[index] = meal;
            }
            else
            {
                _meals.Add(meal);
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
            _meals.RemoveAll(m => m.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }
}
