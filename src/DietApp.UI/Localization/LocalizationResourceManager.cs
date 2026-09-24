using System.ComponentModel;
using DietApp.Application.Services;

namespace DietApp.UI.Localization;

/// <summary>
/// Como funciona: Puente singleton entre ILocalizationService y el motor de enlaces de datos de XAML en MAUI.
/// Implementa INotifyPropertyChanged y expone un indizador this[string key] que notifica cambios
/// globales mediante PropertyChangedEventArgs(null) cada vez que el usuario cambia de idioma.
/// Por que se tomo esta decision: Permite que las vistas XAML enlazadas con TranslateExtension
/// actualicen de inmediato todos sus textos en tiempo real sin requerir navegaciones ni reinicios de la aplicacion.
/// </summary>
public class LocalizationResourceManager : INotifyPropertyChanged
{
    public static LocalizationResourceManager Instance { get; } = new();

    private ILocalizationService? _localizationService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Initialize(ILocalizationService localizationService)
    {
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged -= OnLanguageChanged;
        }

        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    public string this[string key] => _localizationService?.GetString(key) ?? key;

    public string CurrentLanguage => _localizationService?.CurrentLanguage ?? "es";

    public void SetLanguage(string languageCode)
    {
        _localizationService?.SetLanguage(languageCode);
    }
}
