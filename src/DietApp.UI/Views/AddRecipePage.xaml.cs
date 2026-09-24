using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de AddRecipePage. Conecta AddRecipeViewModel e inicia la carga
/// de alimentos disponibles para su seleccion como ingredientes al ingresar a la pantalla.
/// Por que se tomo esta decision: Permite que el selector de ingredientes disponga siempre del listado
/// mas reciente del catalogo, aislando la logica visual del manejo de estado.
/// </summary>
public partial class AddRecipePage : ContentPage
{
    private readonly AddRecipeViewModel _viewModel;

    public AddRecipePage(AddRecipeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAvailableFoodsAsync();
    }
}
