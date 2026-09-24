using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;
using DietApp.Domain.Services;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Coordina el registro de ingesta de alimentos y recetas, asociacion de porciones
/// y agregacion de los minerales resultantes a nivel de comida y a nivel de dia completo.
/// Por que se tomo esta decision: El conteo de minerales es el objetivo central de la aplicacion.
/// Al aislar esta orquestacion en un servicio de aplicacion, se permite que tanto la interfaz grafica
/// como futuros servicios de exportacion o sincronizacion compartan la misma logica sin duplicacion.
/// </summary>
public class MealTrackingService : IMealTrackingService
{
    private readonly IMealRepository _mealRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly DailyMineralAggregatorService _aggregatorService;

    public MealTrackingService(
        IMealRepository mealRepository,
        IFoodRepository foodRepository,
        IRecipeRepository recipeRepository,
        DailyMineralAggregatorService aggregatorService)
    {
        _mealRepository = mealRepository ?? throw new ArgumentNullException(nameof(mealRepository));
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
        _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
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
        return await RecordMealWithMixedItemsAsync(
            date,
            mealType,
            note,
            items.Select(i => (i.foodId, i.grams, false)));
    }

    public async Task<MealDto> RecordMealWithMixedItemsAsync(
        DateTime date,
        MealType mealType,
        string note,
        IEnumerable<(Guid id, double quantity, bool isRecipe)> items)
    {
        var meal = new Meal(Guid.NewGuid(), date, mealType, note);

        foreach (var (id, quantity, isRecipe) in items)
        {
            if (isRecipe)
            {
                var recipe = await _recipeRepository.GetByIdAsync(id);
                if (recipe != null)
                {
                    var recipeItem = MealItem.FromRecipe(recipe, quantity);
                    meal.AddItem(recipeItem);
                }
            }
            else
            {
                var food = await _foodRepository.GetByIdAsync(id);
                if (food != null)
                {
                    var foodItem = MealItem.FromFoodItem(food, quantity);
                    meal.AddItem(foodItem);
                }
            }
        }

        await _mealRepository.SaveAsync(meal);
        return meal.ToDto();
    }

    public async Task<MealDto> RecordRecipeInMealAsync(
        DateTime date,
        MealType mealType,
        Guid recipeId,
        double servingsConsumed,
        string note = "")
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            throw new InvalidOperationException($"No se encontro la receta con identificador {recipeId}.");
        }

        var mealItem = MealItem.FromRecipe(recipe, servingsConsumed);

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
        var existingMeals = await _mealRepository.GetByDateRangeAsync(startOfDay, endOfDay);
        var targetMeal = existingMeals.FirstOrDefault(m => m.Type == mealType);

        if (targetMeal == null)
        {
            targetMeal = new Meal(Guid.NewGuid(), date, mealType, note);
        }

        targetMeal.AddItem(mealItem);
        await _mealRepository.SaveAsync(targetMeal);
        return targetMeal.ToDto();
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
