using DietApp.UI.Localization;
using DietApp.UI.Views;

namespace DietApp.UI;

/// <summary>
/// Como funciona: Code-behind de AppShell. Registra las rutas de navegacion dinamicas
/// para pantallas secundarias como el detalle de receta (RecipeDetailPage) y el formulario
/// de creacion de recetas (AddRecipePage), y actualiza dinamicamente los titulos de las pestanas
/// al alternar entre ingles y espanol.
/// Por que se tomo esta decision: Asegura que el cambio de idioma impacte inmediatamente en la barra
/// de navegacion nativa inferior/superior en todas las plataformas soportadas por .NET MAUI.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(AddRecipePage), typeof(AddRecipePage));
        Routing.RegisterRoute(nameof(SeasoningsPage), typeof(SeasoningsPage));
        Routing.RegisterRoute(nameof(AddMealPage), typeof(AddMealPage));
        Routing.RegisterRoute(nameof(AddFoodPage), typeof(AddFoodPage));

        LocalizationResourceManager.Instance.PropertyChanged += (s, e) =>
        {
            ApplyLocalizedTitles();
        };

        ApplyLocalizedTitles();
    }

    private void ApplyLocalizedTitles()
    {
        TabDailyTracking.Title = LocalizationResourceManager.Instance["Tab_DailyTracking"];
        TabRecipes.Title = LocalizationResourceManager.Instance["Tab_Recipes"];
        TabSeasonings.Title = LocalizationResourceManager.Instance["Tab_Seasonings"];
        TabCatalog.Title = LocalizationResourceManager.Instance["Tab_Catalog"];
        TabSettings.Title = LocalizationResourceManager.Instance["Tab_Settings"];
    }
}
