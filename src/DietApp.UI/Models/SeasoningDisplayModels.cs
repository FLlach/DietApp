using CommunityToolkit.Mvvm.ComponentModel;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo reactivo para una tarjeta de alino preconfigurado dentro del Modulo de Alinos (SeasoningsPage).
/// Contiene datos nutricionales, etiquetas de control renal, estado de seleccion y balance de sodio/potasio.
/// Por que se tomo esta decision: Permite una seleccion rapida con retroalimentacion inmediata del perfil electrolitico.
/// </summary>
public partial class SeasoningOptionModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BadgeText { get; set; } = "Bajo Sodio";
    public Color BadgeBackgroundColor { get; set; } = Color.FromArgb("#2084CC16");
    public Color BadgeTextColor { get; set; } = Color.FromArgb("#315200");
    public string Category { get; set; } = "Vinagretas";

    [ObservableProperty]
    private bool _isSelected;

    public bool IsAlert { get; set; }
    public double SodiumMg { get; set; }
    public double PotassiumMg { get; set; }
    public double Calories { get; set; }

    public string SodiumDisplay => $"Na: {SodiumMg:0.#}mg";
    public string PotassiumDisplay => $"K: {PotassiumMg:0.#}mg";
    public string CaloriesDisplay => $"{Calories:N0} kcal";
}

/// <summary>
/// Como funciona: Modelo para un ingrediente del constructor casero de alinos con casilla de verificacion reactiva.
/// Por que se tomo esta decision: Al alternar la seleccion, recalcula inmediatamente el medidor bioquimico en tiempo real.
/// </summary>
public partial class CustomSeasoningIngredientModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string MineralBadge { get; set; } = "0mg Na";
    public Color MineralBadgeColor { get; set; } = Color.FromArgb("#416900");

    [ObservableProperty]
    private bool _isChecked = true;

    public double SodiumMg { get; set; }
    public double PotassiumMg { get; set; }
    public double Calories { get; set; }
    public double Grams { get; set; }
}

/// <summary>
/// Como funciona: Modelo de presentacion para las categorias de destino del alino (Plato Principal, Guarnicion, etc.).
/// Por que se tomo esta decision: Permite la navegacion horizontal por riel tactil conservando la seleccion del usuario.
/// </summary>
public partial class TargetDishOptionModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = Helpers.MaterialIconFont.DinnerDining;

    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>
/// Como funciona: Modelo para las pastillas de filtrado por subcategoria (Vinagretas, Marinadas, etc.).
/// Por que se tomo esta decision: Permite segmentar el catalogo de alinos preconfigurados de forma fluida.
/// </summary>
public partial class SeasoningCategoryFilterModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
