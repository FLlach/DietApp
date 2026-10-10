using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Domain.Enums;
using DietApp.UI.Helpers;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion reactivo para las tarjetas del Catalogo de Recetas (RecipesPage).
/// Contiene metadatos de fotografia culinaria, badges de energia y proteina, origen global o personal,
/// estado de favorito por perfil y la coleccion de los 7 minerales criticos por porcion.
/// Por que se tomo esta decision: Desacopla RecipeDto de los requerimientos visuales complejos
/// del sistema de diseno Stitch en XAML, facilitando enlaces compilados de alto rendimiento y reaccion inmediata a favoritos.
/// </summary>
public partial class RecipeCardDisplayModel : ObservableObject
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

    public bool IsGlobal { get; set; }
    public string OriginBadge => IsGlobal ? "Global • Nube" : "Personal";
    public Color OriginBadgeBackground => IsGlobal ? Color.FromArgb("#E0F2FE") : Color.FromArgb("#F1F5F9");
    public Color OriginBadgeTextColor => IsGlobal ? Color.FromArgb("#0369A1") : Color.FromArgb("#475569");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BookmarkGlyph))]
    [NotifyPropertyChangedFor(nameof(BookmarkColor))]
    private bool _isFavorite;

    public string BookmarkGlyph => IsFavorite ? MaterialIconFont.Bookmark : MaterialIconFont.BookmarkBorder;
    public Color BookmarkColor => IsFavorite ? Color.FromArgb("#84CC16") : Color.FromArgb("#94A3B8");
}
