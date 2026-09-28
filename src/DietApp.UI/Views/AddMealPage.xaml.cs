using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de AddMealPage. Conecta la pagina con su ViewModel e inicializa
/// el cache de alimentos y recetas para busqueda rapida en memoria.
/// Por que se tomo esta decision: Asegura separacion estricta MVVM sin acoplamientos ni dependencias directas en la vista.
/// </summary>
public partial class AddMealPage : ContentPage
{
    private readonly AddMealViewModel _viewModel;

    public AddMealPage(AddMealViewModel viewModel)
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
