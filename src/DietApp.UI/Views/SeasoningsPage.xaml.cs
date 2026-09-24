using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de SeasoningsPage. Enlaza el ViewModel inyectado en el contenedor IoC
/// y dispara la recarga del catalogo de alinos y alimentos cada vez que la vista se vuelve visible.
/// Por que se tomo esta decision: Asegura que cualquier nuevo alino guardado o alimento incorporado
/// este inmediatamente actualizado y disponible sin necesidad de recargar manualmente la aplicacion.
/// </summary>
public partial class SeasoningsPage : ContentPage
{
    private readonly SeasoningsViewModel _viewModel;

    public SeasoningsPage(SeasoningsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSeasoningsAsync();
    }
}
