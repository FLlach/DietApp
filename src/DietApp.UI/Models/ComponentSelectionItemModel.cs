using CommunityToolkit.Mvvm.ComponentModel;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo observable para las tarjetas de seleccion de componentes y alinos (Vista B en mockupBase.jpeg).
/// Almacena el nombre, la imagen del producto y si se encuentra seleccionado por el usuario.
/// Por que se tomo esta decision: Permite una reactividad tipada e inmediata al conmutar el estado del check visual.
/// </summary>
public partial class ComponentSelectionItemModel : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
