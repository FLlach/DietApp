using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de SettingsPage. Conecta el SettingsViewModel y establece el BindingContext.
/// Por que se tomo esta decision: Respeta el patron MVVM delegando toda la logica de seleccion
/// y persistencia de idioma al ViewModel correspondiente.
/// </summary>
public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
