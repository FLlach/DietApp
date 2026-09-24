using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un mineral que ha alcanzado o superado el limite
/// maximo diario o el umbral preventivo configurado por el usuario, indicando la cantidad actual, el umbral,
/// el exceso o margen restante, el nivel de severidad y el formato visual para las vistas.
/// Por que se tomo esta decision: Permite a las vistas de seguimiento diario y registro de comidas
/// renderizar alertas visuales y mensajes claros diferenciando entre advertencias preventivas y limites superados
/// sin acoplarse a la logica de almacenamiento o calculo.
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
    public MineralAlertSeverity Severity { get; set; } = MineralAlertSeverity.ExceededLimit;
    public bool IsNearLimit => Severity == MineralAlertSeverity.NearLimit;
    public bool IsExceeded => Severity == MineralAlertSeverity.ExceededLimit;
    public string SeverityBadgeText { get; set; } = string.Empty;
    public string CardBackgroundHex => IsExceeded ? "#FADBD8" : "#FCF3CF";
    public string CardBorderHex => IsExceeded ? "#E74C3C" : "#F39C12";
    public string CardTextHex => IsExceeded ? "#922B21" : "#7D6608";
    public string IconText => IsExceeded ? "!" : "i";
}
