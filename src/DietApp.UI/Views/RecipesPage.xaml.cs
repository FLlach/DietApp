using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de RecipesPage. Asocia el ViewModel mediante inyeccion de dependencias
/// y dispara la recarga del catalogo de recetas cada vez que la pagina pasa a primer plano.
/// Por que se tomo esta decision: Permite que nuevas recetas creadas en AddRecipePage se reflejen
/// inmediatamente en el listado sin requerir eventos manuales o acoplamiento entre paginas.
/// </summary>
public partial class RecipesPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;

    public RecipesPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecipesAsync();
    }
}
