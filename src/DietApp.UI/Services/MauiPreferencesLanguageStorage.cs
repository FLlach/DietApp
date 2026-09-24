using DietApp.Application.Services;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Implementa ILanguagePreferenceStorage utilizando la API multiplataforma Preferences de .NET MAUI.
/// Por que se tomo esta decision: Permite persistir la preferencia de idioma seleccionada por el usuario
/// en el almacenamiento local del dispositivo (Android SharedPreferences, Windows LocalSettings / Registry, iOS NSUserDefaults)
/// sin acoplar la logica de negocio a la plataforma.
/// </summary>
public class MauiPreferencesLanguageStorage : ILanguagePreferenceStorage
{
    private const string LanguageKey = "app_preferred_language";

    public string? GetSavedLanguage()
    {
        return Preferences.Default.Get<string?>(LanguageKey, null);
    }

    public void SaveLanguage(string languageCode)
    {
        Preferences.Default.Set(LanguageKey, languageCode);
    }
}
