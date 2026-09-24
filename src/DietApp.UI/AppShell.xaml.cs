using DietApp.UI.Views;

namespace DietApp.UI;

/// <summary>
/// Como funciona: Code-behind de AppShell. Registra las rutas de navegacion dinamicas
/// para pantallas secundarias como el detalle de receta (RecipeDetailPage) y el formulario
/// de creacion de recetas (AddRecipePage).
/// Por que se tomo esta decision: Permite navegar mediante Shell.Current.GoToAsync utilizando
/// parametros de consulta fuertemente desacoplados de las instancias de vistas.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(AddRecipePage), typeof(AddRecipePage));
    }
}
