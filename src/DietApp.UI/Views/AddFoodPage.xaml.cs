using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de AddFoodPage. Vincula la pagina al AddFoodViewModel.
/// Por que se tomo esta decision: Respeta el patron MVVM y las directrices de bajo acoplamiento,
/// permitiendo probar la logica de validacion y almacenamiento de forma aislada.
/// </summary>
public partial class AddFoodPage : ContentPage
{
    public AddFoodPage(AddFoodViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
