using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para el Compositor de Recetas.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de AddRecipePage.
/// </summary>
public partial class AddRecipeViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IFoodCatalogService _catalogService;

    public AddRecipeViewModel(
        IRecipeService recipeService,
        IFoodCatalogService catalogService)
    {
        _recipeService = recipeService;
        _catalogService = catalogService;
    }
}
