using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para la Ficha Detallada de Receta.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de RecipeDetailPage.
/// </summary>
public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IMineralAlertService _mineralAlertService;

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        IMineralAlertService mineralAlertService)
    {
        _recipeService = recipeService;
        _mineralAlertService = mineralAlertService;
    }
}
