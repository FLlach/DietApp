using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de RecipesPage. Conecta la pagina con su ViewModel e inicializa
/// la consulta asincrona de recetas desde la base de datos al aparecer en pantalla.
/// Por que se tomo esta decision: Asegura separacion estricta MVVM y refresco automatico
/// de las recetas creadas o modificadas al regresar desde AddRecipePage.
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
        await _viewModel.InitializeAsync();
    }
}
