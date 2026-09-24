using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;
using DietApp.Domain.Services;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Coordina el registro de ingesta de alimentos, asociacion de porciones
/// y agregacion de los minerales resultantes a nivel de comida y a nivel de dia completo.
/// Por que se tomo esta decision: El conteo de minerales es el objetivo central de la aplicacion.
/// Al aislar esta orquestacion en un servicio de aplicacion, se permite que tanto la interfaz grafica
/// como futuros servicios de exportacion o sincronizacion compartan la misma logica sin duplicacion.
/// </summary>
public class MealTrackingService : IMealTrackingService
{
    private readonly IMealRepository _mealRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly DailyMineralAggregatorService _aggregatorService;

    public MealTrackingService(
        IMealRepository mealRepository,
        IFoodRepository foodRepository,
        DailyMineralAggregatorService aggregatorService)
    {
        _mealRepository = mealRepository ?? throw new ArgumentNullException(nameof(mealRepository));
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
        _aggregatorService = aggregatorService ?? throw new ArgumentNullException(nameof(aggregatorService));
    }

    public async Task<IReadOnlyList<MealDto>> GetMealsForDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var meals = await _mealRepository.GetByDateRangeAsync(startOfDay, endOfDay);
        return meals.Select(meal => meal.ToDto()).ToList();
    }

    public async Task<MealDto> RecordMealAsync(
        DateTime date,
        MealType mealType,
        string note,
        IEnumerable<(Guid foodId, double grams)> items)
    {
        var meal = new Meal(Guid.NewGuid(), date, mealType, note);

        foreach (var (foodId, grams) in items)
        {
            var food = await _foodRepository.GetByIdAsync(foodId);
            if (food != null)
            {
                var mealItem = MealItem.FromFoodItem(food, grams);
                meal.AddItem(mealItem);
            }
        }

        await _mealRepository.SaveAsync(meal);
        return meal.ToDto();
    }

    public async Task<IReadOnlyList<MineralAmountDto>> GetDailyMineralTotalsAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var meals = await _mealRepository.GetByDateRangeAsync(startOfDay, endOfDay);
        var aggregateTotals = _aggregatorService.AggregateMinerals(meals);

        return aggregateTotals.Select(m => m.ToDto()).ToList();
    }

    public async Task DeleteMealAsync(Guid mealId)
    {
        await _mealRepository.DeleteAsync(mealId);
    }
}
