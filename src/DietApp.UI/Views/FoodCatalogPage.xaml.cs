using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de FoodCatalogPage. Inyecta el ViewModel mediante el constructor
/// y establece el BindingContext. Dispara la carga inicial al aparecer la pagina.
/// Por que se tomo esta decision: Mantiene el code-behind libre de logica de negocio,
/// limitandose estrictamente al ciclo de vida del componente visual y delegando todo el estado
/// al FoodCatalogViewModel.
/// </summary>
public partial class FoodCatalogPage : ContentPage
{
    private readonly FoodCatalogViewModel _viewModel;

    public FoodCatalogPage(FoodCatalogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadFoodsAsync();
    }
}
