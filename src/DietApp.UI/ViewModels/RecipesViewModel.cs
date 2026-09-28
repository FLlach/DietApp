using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para el Catalogo de Recetas Culinarias.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de RecipesPage.
/// </summary>
public partial class RecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    public RecipesViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }
}
