using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para el Modulo de Alinos.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de SeasoningsPage.
/// </summary>
public partial class SeasoningsViewModel : ObservableObject
{
    private readonly ISeasoningService _seasoningService;
    private readonly IFoodCatalogService _catalogService;

    public SeasoningsViewModel(
        ISeasoningService seasoningService,
        IFoodCatalogService catalogService)
    {
        _seasoningService = seasoningService;
        _catalogService = catalogService;
    }
}
