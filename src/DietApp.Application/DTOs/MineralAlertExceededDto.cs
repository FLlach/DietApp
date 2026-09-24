using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un mineral que ha alcanzado o superado el limite
/// maximo diario configurado por el usuario, indicando la cantidad actual, el umbral y el exceso.
/// Por que se tomo esta decision: Permite a las vistas de seguimiento diario y registro de comidas
/// renderizar alertas visuales y mensajes claros sin acoplarse a la logica de almacenamiento o calculo.
/// </summary>
public class MineralAlertExceededDto
{
    public MineralType Mineral { get; set; }
    public string MineralName { get; set; } = string.Empty;
    public double CurrentMilligrams { get; set; }
    public double MaxMilligrams { get; set; }
    public double ExcessMilligrams => Math.Max(0, CurrentMilligrams - MaxMilligrams);
    public double PercentageOfLimit => MaxMilligrams > 0 ? (CurrentMilligrams / MaxMilligrams) * 100 : 0;
    public string WarningMessage { get; set; } = string.Empty;
}
