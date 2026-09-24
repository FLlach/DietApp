using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de AddMealPage. Conecta el AddMealViewModel e inicia la carga
/// de alimentos disponibles cada vez que el usuario ingresa a la pantalla.
/// Por que se tomo esta decision: Permite que el selector de alimentos contemple inmediatamente
/// cualquier nuevo alimento que el usuario haya guardado previamente en el catalogo.
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
        await _viewModel.LoadAvailableFoodsAndRecipesAsync();
    }
}
