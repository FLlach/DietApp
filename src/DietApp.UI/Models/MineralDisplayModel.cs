using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion para las tarjetas de la cuadricula de los 7 minerales criticos de Stitch.
/// Contiene valores formateados, simbolo quimico, colores semanticos y estado de alerta clinica.
/// Por que se tomo esta decision: Permite a la vista enlazar directamente propiedades visuales
/// sin logica de negocio en el XAML, respetando MVVM estricto.
/// </summary>
public class MineralDisplayModel
{
    public MineralType MineralType { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double ConsumedAmount { get; set; }
    public string FormattedAmount { get; set; } = "0";
    public string Unit { get; set; } = "mg";
    public double? LimitAmount { get; set; }
    public string FormattedLimit { get; set; } = string.Empty;
    public bool IsExceeded { get; set; }
    public bool IsWarning { get; set; }
    public bool IsColSpan2 { get; set; }
    public string StatusText { get; set; } = string.Empty;

    // Tokens visuales Stitch
    public Color CardBackground { get; set; } = Color.FromArgb("#F2F3FF");
    public Color CardBorderColor { get; set; } = Color.FromArgb("#DAE2FD");
    public Color SymbolBackground { get; set; } = Color.FromArgb("#DAE2FD");
    public Color SymbolTextColor { get; set; } = Color.FromArgb("#131B2E");
    public Color ValueTextColor { get; set; } = Color.FromArgb("#131B2E");
    public Color TitleTextColor { get; set; } = Color.FromArgb("#131B2E");
}
