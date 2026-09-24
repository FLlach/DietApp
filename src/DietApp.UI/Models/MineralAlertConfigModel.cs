using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modela el estado visual y editable de la configuracion de alerta para un mineral individual
/// en la pantalla de Ajustes.
/// Por que se tomo esta decision: Permite enlace bidireccional reactivo (TwoWay Binding) en XAML para
/// el campo de texto de miligramos y el conmutador de activacion, facilitando la validacion y guardado agrupado.
/// </summary>
public partial class MineralAlertConfigModel : ObservableObject
{
    public MineralType Mineral { get; set; }

    [ObservableProperty]
    public partial string MineralName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ThresholdText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public string Unit { get; set; } = "mg";

    public double? GetValidThreshold()
    {
        if (!IsEnabled) return null;
        if (double.TryParse(ThresholdText, out double val) && val > 0)
        {
            return val;
        }
        return null;
    }
}
