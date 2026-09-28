namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo para los banners de advertencia y limite superado en el Conteo Diario Stitch.
/// Por que se tomo esta decision: Permite mostrar avisos preventivos y excesos de minerales criticos
/// con codificacion visual clara (coral para limites superados, ambar para proximidad al umbral).
/// </summary>
public class ClinicalAlertBannerModel
{
    public string MineralName { get; set; } = string.Empty;
    public string BadgeText { get; set; } = string.Empty;
    public string DifferenceText { get; set; } = string.Empty;
    public string SummaryText { get; set; } = string.Empty;
    public string AdviceText { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
    public Color AccentColor { get; set; } = Color.FromArgb("#FC7B48");
    public Color BadgeBackground { get; set; } = Color.FromArgb("#FFDBCF");
    public Color BadgeTextColor { get; set; } = Color.FromArgb("#671F00");
}
