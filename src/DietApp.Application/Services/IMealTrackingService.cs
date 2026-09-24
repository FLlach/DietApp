using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato del servicio de aplicacion para el registro de comidas, inclusion de
/// porciones consumidas y obtencion de resumenes nutricionales diarios o por comida.
/// Por que se tomo esta decision: Orquesta la recuperacion de comidas, la adicion de alimentos
/// con calculo de porcion y la agregacion de minerales consumidos a traves del servicio de dominio.
/// </summary>
public interface IMealTrackingService
{
    Task<IReadOnlyList<MealDto>> GetMealsForDateAsync(DateTime date);
    Task<MealDto> RecordMealAsync(DateTime date, MealType mealType, string note, IEnumerable<(Guid foodId, double grams)> items);
    Task<IReadOnlyList<MineralAmountDto>> GetDailyMineralTotalsAsync(DateTime date);
    Task DeleteMealAsync(Guid mealId);
}
