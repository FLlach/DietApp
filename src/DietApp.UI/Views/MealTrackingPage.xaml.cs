using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de MealTrackingPage. Gestiona la conexion con MealTrackingViewModel
/// y activa la actualizacion de datos cada vez que la pagina pasa a primer plano.
/// Por que se tomo esta decision: Asegura que si el usuario registra una nueva comida en otra pestana,
/// al volver a la pantalla de seguimiento los totales y la lista se sincronicen de inmediato.
/// </summary>
public partial class MealTrackingPage : ContentPage
{
    private readonly MealTrackingViewModel _viewModel;

    public MealTrackingPage(MealTrackingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDayDataAsync();
    }
}
