using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

public partial class FoodCatalogPage : ContentPage
{
    public FoodCatalogPage(FoodCatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
