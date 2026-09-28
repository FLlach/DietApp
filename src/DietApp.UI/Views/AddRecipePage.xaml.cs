using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

public partial class AddRecipePage : ContentPage
{
    public AddRecipePage(AddRecipeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
