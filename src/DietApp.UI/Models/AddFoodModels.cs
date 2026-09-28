using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.UI.Helpers;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo para las pastillas y etiquetas de advertencia o recomendacion clinica en AddFoodPage.
/// Por que se tomo esta decision: Permite alternar dinamicamente el estado activo/inactivo con seleccion reactiva
/// y actualizacion de glifos vectoriales (Check / Add) segun el prototipo Stitch.
/// </summary>
public partial class ClinicalTagPillModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IconGlyph))]
    private bool _isSelected;

    public string IconGlyph => IsSelected ? MaterialIconFont.Check : MaterialIconFont.Add;
}
