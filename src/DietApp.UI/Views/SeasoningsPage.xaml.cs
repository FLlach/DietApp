using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

public partial class SeasoningsPage : ContentPage
{
    public SeasoningsPage(SeasoningsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
