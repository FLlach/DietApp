using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta la informacion de advertencia preventiva o critica cuando el consumo
/// de una o mas porciones de una receta culinaria alcanza o supera el limite maximo de minerales fijado por el usuario.
/// Por que se tomo esta decision: Permite a RecipeDetailPage renderizar avisos claros e inmediatos
/// antes de que el usuario decida ingerir o registrar la receta en su diario nutricional.
/// </summary>
public class RecipePortionWarningDto
{
    public MineralType Mineral { get; set; }
    public string MineralName { get; set; } = string.Empty;
    public double PortionMilligrams { get; set; }
    public double MaxDailyMilligrams { get; set; }
    public double CurrentDailyMilligrams { get; set; }
    public double ProjectedTotalMilligrams { get; set; }
    public double ExcessMilligrams => Math.Max(0, ProjectedTotalMilligrams - MaxDailyMilligrams);
    public double PercentageOfLimit => MaxDailyMilligrams > 0 ? (ProjectedTotalMilligrams / MaxDailyMilligrams) * 100.0 : 0;
    public MineralAlertSeverity Severity { get; set; } = MineralAlertSeverity.ExceededLimit;
    public bool IsExceeded => Severity == MineralAlertSeverity.ExceededLimit;
    public bool IsNearLimit => Severity == MineralAlertSeverity.NearLimit;
    public string WarningMessage { get; set; } = string.Empty;
    public string SeverityBadgeText { get; set; } = string.Empty;
    public string CardBackgroundHex => IsExceeded ? "#FADBD8" : "#FCF3CF";
    public string CardBorderHex => IsExceeded ? "#E74C3C" : "#F39C12";
    public string CardTextHex => IsExceeded ? "#922B21" : "#7D6608";
    public string IconText => IsExceeded ? "!" : "i";
}
