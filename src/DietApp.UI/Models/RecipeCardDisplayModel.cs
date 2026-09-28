using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion para las tarjetas del Catalogo de Recetas (RecipesPage).
/// Contiene metadatos de fotografia culinaria, badges de energia y proteina, tiempo de preparacion,
/// etiqueta de adecuacion clinica y la coleccion de los 7 minerales criticos por porcion.
/// Por que se tomo esta decision: Desacopla RecipeDto de los requerimientos visuales complejos
/// del sistema de diseno Stitch en XAML, facilitando enlaces compilados de alto rendimiento.
/// </summary>
public class RecipeCardDisplayModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool HasNoImage => string.IsNullOrWhiteSpace(ImageUrl);
    public string EnergyBadge { get; set; } = string.Empty;
    public string ProteinBadge { get; set; } = string.Empty;
    public string PrepTimeDisplay { get; set; } = "20 min";
    public string ServingsDisplay { get; set; } = "1 ración clínica";
    public string ClinicalStatusBadge { get; set; } = "Apto Renal Controlado";
    public Color ClinicalStatusBackground { get; set; } = Color.FromArgb("#F0FDF4");
    public Color ClinicalStatusBorderColor { get; set; } = Color.FromArgb("#86EFAC");
    public Color ClinicalStatusTextColor { get; set; } = Color.FromArgb("#166534");
    public List<MealMineralChipModel> MineralChips { get; set; } = new();
}
