using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de configuracion y seleccion de idioma.
/// Permite conmutar interactivamente entre Espanol e Ingles, actualizando el servicio de localizacion
/// e informando el estado resultante a la interfaz de usuario.
/// Por que se tomo esta decision: Separa la logica de seleccion de idioma de la vista de configuracion,
/// garantizando que la preferencia se guarde permanentemente y se propague reactivamente a toda la aplicacion.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial bool IsSpanishSelected { get; set; }

    [ObservableProperty]
    public partial bool IsEnglishSelected { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public SettingsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        UpdateSelectionState();

        _localizationService.LanguageChanged += (s, e) =>
        {
            UpdateSelectionState();
        };
    }

    private void UpdateSelectionState()
    {
        var lang = _localizationService.CurrentLanguage;
        IsSpanishSelected = lang == "es";
        IsEnglishSelected = lang == "en";
    }

    [RelayCommand]
    public void SelectSpanish()
    {
        _localizationService.SetLanguage("es");
        UpdateSelectionState();
        StatusMessage = _localizationService.GetString("Settings_CurrentLanguageNotice");
    }

    [RelayCommand]
    public void SelectEnglish()
    {
        _localizationService.SetLanguage("en");
        UpdateSelectionState();
        StatusMessage = _localizationService.GetString("Settings_CurrentLanguageNotice");
    }
}
