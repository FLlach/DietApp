using DietApp.UI.ViewModels;

namespace DietApp.UI.Views;

/// <summary>
/// Como funciona: Code-behind de la vista de autenticacion (LoginPage).
/// Vincula el ciclo de vida de la pagina con LoginViewModel inyectado mediante IoC.
/// Por que se tomo esta decision: Sigue el patron MVVM estricto delegando toda la logica
/// interactiva y de navegacion al ViewModel.
/// </summary>
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
