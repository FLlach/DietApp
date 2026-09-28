using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Chip de mineral compacto para la tarjeta de comida registrada.
/// Por que se tomo esta decision: Permite renderizar micro-etiquetas con fondo y bordes acordes a Stitch.
/// </summary>
public class MealMineralChipModel
{
    public string Symbol { get; set; } = string.Empty;
    public string AmountWithUnit { get; set; } = string.Empty;
    public bool IsAlert { get; set; }
    public Color BackgroundColor { get; set; } = Color.FromArgb("#EAEDFF");
    public Color BorderColor { get; set; } = Color.FromArgb("#C1CAB0");
    public Color TextColor { get; set; } = Color.FromArgb("#131B2E");
}

/// <summary>
/// Como funciona: Modelo de presentacion para las tarjetas de comidas registradas en MealTrackingPage.
/// Por que se tomo esta decision: Suministra a la vista las propiedades pre-calculadas y formateadas
/// evitando logica de presentacion en el code-behind.
/// </summary>
public class MealCardDisplayModel
{
    public Guid Id { get; set; }
    public MealType MealType { get; set; }
    public string HeaderText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;
    public Color IconContainerBackground { get; set; } = Color.FromArgb("#EAEDFF");
    public Color IconColor { get; set; } = Color.FromArgb("#416900");
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public string ImpactText { get; set; } = string.Empty;
    public Color ImpactTextColor { get; set; } = Color.FromArgb("#131B2E");
    public List<MealMineralChipModel> MineralChips { get; set; } = new();
}
