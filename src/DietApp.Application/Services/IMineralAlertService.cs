using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para la gestion integral de limites y alertas de minerales. Permite consultar
/// y actualizar umbrales maximos diarios, fijar el umbral porcentual para avisos preventivos, evaluar
/// si una coleccion de minerales consumidos ha alcanzado la advertencia o ha superado el limite maximo,
/// y auditar si consumir porciones de una receta culinaria superara el limite diario preestablecido.
/// Por que se tomo esta decision: Centraliza en la capa de aplicacion las reglas de deteccion de excesos
/// y cercania a limites nutricionales, emitiendo alertas coherentes y localizadas para cualquier pantalla de la aplicacion.
/// </summary>
public interface IMineralAlertService
{
    IReadOnlyDictionary<MineralType, double> GetThresholds();
    double? GetThreshold(MineralType mineral);
    void SetThreshold(MineralType mineral, double? maxMilligrams);
    void SaveThresholds(IDictionary<MineralType, double?> thresholds);
    double WarningPercentage { get; }
    void SetWarningPercentage(double percentage);
    IReadOnlyList<MineralAlertExceededDto> CheckExceededThresholds(IEnumerable<MineralAmountDto> dailyMinerals);
    IReadOnlyList<RecipePortionWarningDto> CheckRecipePortionWarnings(
        IEnumerable<MineralAmountDto> portionMinerals,
        IEnumerable<MineralAmountDto>? currentDailyMinerals = null,
        double servings = 1.0);
    event EventHandler? ThresholdsChanged;
}
