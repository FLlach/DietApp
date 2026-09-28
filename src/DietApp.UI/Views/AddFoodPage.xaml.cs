using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

public partial class AddFoodPage : ContentPage
{
    public AddFoodPage(AddFoodViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
