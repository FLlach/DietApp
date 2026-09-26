using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato del servicio de aplicacion para el registro de comidas, inclusion de
/// porciones consumidas de alimentos o recetas, y obtencion de resumenes nutricionales diarios o por comida.
/// Por que se tomo esta decision: Orquesta la recuperacion de comidas, la adicion de alimentos y recetas
/// con calculo de porcion y la agregacion de minerales consumidos a traves del servicio de dominio.
/// </summary>
public interface IMealTrackingService
{
    Task<IReadOnlyList<MealDto>> GetMealsForDateAsync(DateTime date);
    Task<MealDto> RecordMealAsync(DateTime date, MealType mealType, string note, IEnumerable<(Guid foodId, double grams)> items);
    Task<MealDto> RecordMealWithMixedItemsAsync(DateTime date, MealType mealType, string note, IEnumerable<(Guid id, double quantity, bool isRecipe)> items);
    Task<MealDto> RecordRecipeInMealAsync(DateTime date, MealType mealType, Guid recipeId, double servingsConsumed, string note = "");
    Task<IReadOnlyList<MineralAmountDto>> GetDailyMineralTotalsAsync(DateTime date);
    Task<double> GetDailyTotalProteinAsync(DateTime date);
    Task DeleteMealAsync(Guid mealId);
}
