using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion para un ingrediente en proceso de composicion dentro de AddRecipePage.
/// Almacena el alimento seleccionado, su gramaje dosificado, el calculo proporcional de calorias, proteinas y minerales,
/// y si proviene de un alino guardado para exhibir el distintivo visual correspondiente.
/// Por que se tomo esta decision: Permite actualizar reactivamente el panel de proyeccion clinica por porcion
/// a medida que el usuario agrega, ajusta o elimina ingredientes del borrador de la receta.
/// </summary>
public class RecipeIngredientDraftModel
{
    public Guid FoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Grams { get; set; }
    public string GramsDisplay => $"{Grams:N0} g";
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double SodiumMg { get; set; }
    public double PotassiumMg { get; set; }
    public double PhosphorusMg { get; set; }
    public string IconGlyph { get; set; } = Helpers.MaterialIconFont.Restaurant;
    public bool IsFromSeasoning { get; set; }
    public string SeasoningBadgeText { get; set; } = "ALIÑO";

    public string SubtitleLine =>
        $"{Grams:N0} g • {Calories:N0} kcal • P: {PhosphorusMg:N0}mg • K: {PotassiumMg:N0}mg";
}

/// <summary>
/// Como funciona: Modelo de presentacion para un paso secuencial de elaboracion culinaria dentro de AddRecipePage.
/// Por que se tomo esta decision: Permite enumerar y visualizar de forma ordenada cada directriz de coccion,
/// con soporte opcional de fotografia adjunta y etiquetas de tecnica culinaria.
/// </summary>
public class RecipeStepDraftModel
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string TimeDisplayText { get; set; } = "10 min";
    public string TechniqueTag { get; set; } = "Cocción dosificada";
    public bool HasPhoto => !string.IsNullOrWhiteSpace(ImagePath);
}

/// <summary>
/// Como funciona: Modelo para una tarjeta de proyeccion mineral por racion en el panel clinico de AddRecipePage.
/// Por que se tomo esta decision: Reproduce la cuadricula de balance de los 7 minerales criticos del prototipo Stitch,
/// asociando colores de alerta clinica cuando se sobrepasan los umbrales recomendados.
/// </summary>
public class MineralProjectionModel
{
    public string Symbol { get; set; } = string.Empty;
    public string AmountDisplay { get; set; } = "0 mg";
    public string StatusText { get; set; } = "Óptimo";
    public Color BackgroundColor { get; set; } = Color.FromArgb("#EAEDFF");
    public Color BorderColor { get; set; } = Color.FromArgb("#C1CAB0");
    public Color TextColor { get; set; } = Color.FromArgb("#131B2E");
    public Color StatusColor { get; set; } = Color.FromArgb("#416900");
}
