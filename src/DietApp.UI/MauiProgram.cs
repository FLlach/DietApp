using DietApp.Application.Services;
using DietApp.Domain.Repositories;
using DietApp.Domain.Services;
using DietApp.Infrastructure.Repositories;
using DietApp.UI.ViewModels;
using DietApp.UI.Views;
using Microsoft.Extensions.Logging;

namespace DietApp.UI;

/// <summary>
/// Como funciona: Punto de entrada y configuracion de la aplicacion .NET MAUI.
/// Registra los servicios de dominio, infraestructura, aplicacion, viewmodels y vistas
/// en el contenedor de inyeccion de dependencias (IoC / DI container).
/// Por que se tomo esta decision: Permite la inversion de control garantizando bajo acoplamiento
/// entre capas y facilitando la sustitucion de repositorios o servicios sin modificar la interfaz.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Capa de Dominio - Servicios de Dominio
        builder.Services.AddSingleton<DailyMineralAggregatorService>();

        // Capa de Infraestructura - Repositorios (Singleton para conservar el estado durante la sesion)
        builder.Services.AddSingleton<IFoodRepository, FoodRepository>();
        builder.Services.AddSingleton<IMealRepository, MealRepository>();
        builder.Services.AddSingleton<IRecipeRepository, RecipeRepository>();

        // Capa de Aplicacion - Casos de Uso y Servicios
        builder.Services.AddTransient<IFoodCatalogService, FoodCatalogService>();
        builder.Services.AddTransient<IMealTrackingService, MealTrackingService>();
        builder.Services.AddTransient<IRecipeService, RecipeService>();

        // Capa de Presentacion - ViewModels
        builder.Services.AddTransient<FoodCatalogViewModel>();
        builder.Services.AddTransient<MealTrackingViewModel>();
        builder.Services.AddTransient<AddFoodViewModel>();
        builder.Services.AddTransient<AddMealViewModel>();
        builder.Services.AddTransient<RecipesViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<AddRecipeViewModel>();

        // Capa de Presentacion - Vistas (Pages)
        builder.Services.AddTransient<FoodCatalogPage>();
        builder.Services.AddTransient<MealTrackingPage>();
        builder.Services.AddTransient<AddFoodPage>();
        builder.Services.AddTransient<AddMealPage>();
        builder.Services.AddTransient<RecipesPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<AddRecipePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
