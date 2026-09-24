namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Abstraccion para persistir y recuperar la preferencia de idioma del usuario.
/// Por que se tomo esta decision: Permite desacoplar la capa de aplicacion de APIs dependientes
/// de la plataforma como Preferences o SharedPreferences, facilitando pruebas unitarias y respetando
/// los principios de Domain-Driven Design e inversion de dependencias.
/// </summary>
public interface ILanguagePreferenceStorage
{
    string? GetSavedLanguage();
    void SaveLanguage(string languageCode);
}
