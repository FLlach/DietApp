using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.UI.Helpers;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion optimizado para las tarjetas del Catalogo de Alimentos (FoodCatalogPage).
/// Consolida las 7 cantidades de minerales criticos (P, K, Na, Ca, Mg, Fe, Zn), la proteina de alto valor biologico,
/// el calculo de alerta clinica y el veredicto contextual segun umbrales renales y metabolicos.
/// Por que se tomo esta decision: Permite renderizado rapido en Compiled Bindings sin calculos pesados en runtime.
/// </summary>
public class FoodCardModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double ReferenceGrams { get; set; } = 100.0;
    public double Calories { get; set; } = 0.0;
    public double ProteinGrams { get; set; } = 0.0;

    public string Subtitle => $"Porción base: {ReferenceGrams:N0} g • {Calories:N0} kcal";
    public string ProteinBadgeText => $"{ProteinGrams:0.#} g proteína";
    public bool HasHighProtein => ProteinGrams >= 10.0;

    public bool IsAlert { get; set; }
    public string AlertBadgeText { get; set; } = "Límite superado";

    public Color CardBackgroundColor { get; set; } = Color.FromArgb("#FFFFFF");
    public Color CardBorderColor { get; set; } = Color.FromArgb("#C1CAB0");

    public double PhosphorusMg { get; set; }
    public double PotassiumMg { get; set; }
    public double SodiumMg { get; set; }
    public double CalciumMg { get; set; }
    public double MagnesiumMg { get; set; }
    public double IronMg { get; set; }
    public double ZincMg { get; set; }

    public string PhosphorusDisplay => $"{PhosphorusMg:N0}";
    public string PotassiumDisplay => $"{PotassiumMg:N0}";
    public string SodiumDisplay => $"{SodiumMg:N0}";
    public string CalciumDisplay => $"{CalciumMg:N0}";
    public string MagnesiumDisplay => $"{MagnesiumMg:N0}";
    public string IronDisplay => $"{IronMg:0.#}";
    public string ZincDisplay => $"{ZincMg:0.#}";

    // Estilos especificos para la pastilla de Potasio
    public Color PotassiumChipBackground { get; set; } = Color.FromArgb("#F2F3FF");
    public Color PotassiumChipTextColor { get; set; } = Color.FromArgb("#416900");
    public Color PotassiumChipNumberColor { get; set; } = Color.FromArgb("#131B2E");
    public string PotassiumChipLabel { get; set; } = "K";

    // Veredicto clinico inferior
    public string VerdictIcon { get; set; } = MaterialIconFont.Verified;
    public string VerdictText { get; set; } = "Apto bajo potasio";
    public Color VerdictColor { get; set; } = Color.FromArgb("#416900");
}

/// <summary>
/// Como funciona: Modelo para las presintonias de filtrado clinico frecuente en FoodCatalogPage.
/// Por que se tomo esta decision: Permite al usuario aplicar con un solo toque filtros clinicos como
/// bajo en potasio (&lt;200mg), bajo en sodio (&lt;140mg) o alto en proteina.
/// </summary>
public partial class ClinicalPresetFilterModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
