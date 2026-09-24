using DietApp.Application.Services;
using DietApp.Domain.Repositories;
using DietApp.Domain.Services;
using DietApp.Infrastructure.Data;
using DietApp.Infrastructure.Repositories;
using DietApp.UI.Localization;
using DietApp.UI.Services;
using DietApp.UI.ViewModels;
using DietApp.UI.Views;
using Microsoft.Extensions.Logging;

namespace DietApp.UI;

/// <summary>
/// Como funciona: Punto de entrada y configuracion de la aplicacion .NET MAUI.
/// Inicializa la base de datos relacional SQLite con importacion automatica de los alimentos
/// de USDA FoodData Central y registra los repositorios SQLite en el contenedor de dependencias.
/// Por que se tomo esta decision: Permite la inversion de control garantizando bajo acoplamiento
/// entre capas y sustituyendo los repositorios en memoria por persistencia SQLite nativa.
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

        // Configuracion de Base de Datos SQLite
        string databasePath = Path.Combine(FileSystem.AppDataDirectory, "dietapp.db3");
        var dbContext = new DietAppDbContext(databasePath);

        // Inicializacion asincrona de SQLite con el asset empaquetado de FoodData Central
        Task.Run(async () =>
        {
            await dbContext.InitializeAsync(
                assetStreamProvider: async () =>
                {
                    try
                    {
                        return await FileSystem.OpenAppPackageFileAsync("fooddata_central_foundation.json");
                    }
                    catch
                    {
                        return null;
                    }
                });
        });

        builder.Services.AddSingleton(dbContext);

        // Capa de Dominio - Servicios de Dominio
        builder.Services.AddSingleton<DailyMineralAggregatorService>();

        // Capa de Infraestructura - Repositorios SQLite
        builder.Services.AddSingleton<IFoodRepository, SqliteFoodRepository>();
        builder.Services.AddSingleton<IMealRepository, SqliteMealRepository>();
        builder.Services.AddSingleton<IRecipeRepository, SqliteRecipeRepository>();
        builder.Services.AddSingleton<ISeasoningRepository, SqliteSeasoningRepository>();

        // Localizacion e Internacionalizacion
        builder.Services.AddSingleton<ILanguagePreferenceStorage, MauiPreferencesLanguageStorage>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

        // Alertas y Limites de Minerales
        builder.Services.AddSingleton<IMineralAlertStorage, MauiPreferencesMineralAlertStorage>();
        builder.Services.AddSingleton<IMineralAlertService, MineralAlertService>();

        // Capa de Aplicacion - Casos de Uso y Servicios
        builder.Services.AddTransient<IFoodCatalogService, FoodCatalogService>();
        builder.Services.AddTransient<IMealTrackingService, MealTrackingService>();
        builder.Services.AddTransient<IRecipeService, RecipeService>();
        builder.Services.AddTransient<ISeasoningService, SeasoningService>();

        // Capa de Presentacion - ViewModels
        builder.Services.AddTransient<FoodCatalogViewModel>();
        builder.Services.AddTransient<MealTrackingViewModel>();
        builder.Services.AddTransient<AddFoodViewModel>();
        builder.Services.AddTransient<AddMealViewModel>();
        builder.Services.AddTransient<RecipesViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<AddRecipeViewModel>();
        builder.Services.AddTransient<SeasoningsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Capa de Presentacion - Vistas (Pages)
        builder.Services.AddTransient<FoodCatalogPage>();
        builder.Services.AddTransient<MealTrackingPage>();
        builder.Services.AddTransient<AddFoodPage>();
        builder.Services.AddTransient<AddMealPage>();
        builder.Services.AddTransient<RecipesPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<AddRecipePage>();
        builder.Services.AddTransient<SeasoningsPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        var localizationService = app.Services.GetRequiredService<ILocalizationService>();
        LocalizationResourceManager.Instance.Initialize(localizationService);

        return app;
    }
}
