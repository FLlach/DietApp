using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para la gestion integral de limites y alertas de minerales. Permite consultar
/// y actualizar umbrales maximos diarios y evaluar si una coleccion de minerales consumidos los ha superado.
/// Por que se tomo esta decision: Centraliza en la capa de aplicacion las reglas de deteccion de excesos
/// nutricionales, emitiendo alertas coherentes y localizadas para cualquier pantalla de la aplicacion.
/// </summary>
public interface IMineralAlertService
{
    IReadOnlyDictionary<MineralType, double> GetThresholds();
    double? GetThreshold(MineralType mineral);
    void SetThreshold(MineralType mineral, double? maxMilligrams);
    void SaveThresholds(IDictionary<MineralType, double?> thresholds);
    IReadOnlyList<MineralAlertExceededDto> CheckExceededThresholds(IEnumerable<MineralAmountDto> dailyMinerals);
    event EventHandler? ThresholdsChanged;
}
