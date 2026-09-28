using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para el Alta de Alimentos.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de AddFoodPage.
/// </summary>
public partial class AddFoodViewModel : ObservableObject
{
    private readonly IFoodCatalogService _catalogService;

    public AddFoodViewModel(IFoodCatalogService catalogService)
    {
        _catalogService = catalogService;
    }
}
