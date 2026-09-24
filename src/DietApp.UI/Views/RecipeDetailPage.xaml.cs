using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de RecipeDetailPage. Asocia RecipeDetailViewModel y ejecuta la carga
/// de los datos de la receta segun el identificador recibido por navegacion.
/// Por que se tomo esta decision: Permite separar la vista del manejo de los datos y asegurar
/// que los pasos e imagenes se inicialicen correctamente al acceder a la pantalla.
/// </summary>
public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecipeAsync();
    }
}
