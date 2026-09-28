using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para el Catalogo de Alimentos.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de FoodCatalogPage.
/// </summary>
public partial class FoodCatalogViewModel : ObservableObject
{
    private readonly IFoodCatalogService _catalogService;

    public FoodCatalogViewModel(IFoodCatalogService catalogService)
    {
        _catalogService = catalogService;
    }
}
