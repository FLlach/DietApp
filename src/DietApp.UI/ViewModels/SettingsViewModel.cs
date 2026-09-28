using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel base para los Ajustes y Configuracion.
/// Por que se tomo esta decision: Permite inicializar la vista con inyeccion de dependencias
/// lista para sincronizar con el prototipo Stitch de SettingsPage.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IMineralAlertService _mineralAlertService;
    private readonly IThemeService _themeService;

    public SettingsViewModel(
        ILocalizationService localizationService,
        IMineralAlertService mineralAlertService,
        IThemeService themeService)
    {
        _localizationService = localizationService;
        _mineralAlertService = mineralAlertService;
        _themeService = themeService;
    }
}
