namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo para un ingrediente dosificado dentro de la Ficha de Detalle de Receta (RecipeDetailPage).
/// Por que se tomo esta decision: Permite exhibir la cantidad en gramos, la categoria del ingrediente y el aporte mineral por porcion.
/// </summary>
public class RecipeIngredientDisplayModel
{
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string GramsDisplay { get; set; } = "100 g";
    public double PotassiumAmount { get; set; }
    public string IconGlyph { get; set; } = "\ue56c";
    public string MineralPreview1 { get; set; } = string.Empty;
    public string MineralPreview2 { get; set; } = string.Empty;
    public Color MineralColor1 { get; set; } = Color.FromArgb("#416900");
    public Color MineralColor2 { get; set; } = Color.FromArgb("#131B2E");
}

/// <summary>
/// Como funciona: Modelo para un paso de elaboracion numerado en RecipeDetailPage.
/// Por que se tomo esta decision: Permite renderizar el orden del procedimiento culinario con su titulo e instruccion.
/// </summary>
public class RecipeStepDisplayModel
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
}
