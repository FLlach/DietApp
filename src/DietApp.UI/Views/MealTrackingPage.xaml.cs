using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de la vista de Conteo Diario (MealTrackingPage).
/// Conecta la pagina con su ViewModel e inicializa la carga de datos al aparecer en pantalla.
/// Por que se tomo esta decision: Respeta el patron MVVM y garantiza que los datos diarios se actualicen
/// cuando el usuario regresa desde la pantalla de agregar comida o recetas.
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
        await _viewModel.InitializeAsync();
    }
}
