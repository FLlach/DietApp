using DietApp.Domain.Services;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Implementa IUserSessionStorage utilizando la API Preferences de .NET MAUI.
/// Almacena el identificador de usuario, correo, nombre y preferencia de recordar sesion
/// en el almacenamiento seguro local del dispositivo (Android SharedPreferences, Windows Settings).
/// Por que se tomo esta decision: Permite recordar la sesion del paciente entre aperturas de la app
/// preservando la separacion de capas segun Domain-Driven Design.
/// </summary>
public class MauiPreferencesUserSessionStorage : IUserSessionStorage
{
    private const string UserIdKey = "auth_saved_user_id";
    private const string UserEmailKey = "auth_saved_user_email";
    private const string DisplayNameKey = "auth_saved_display_name";
    private const string RememberSessionKey = "auth_remember_session";

    public string? GetSavedUserId()
    {
        return Preferences.Default.Get<string?>(UserIdKey, null);
    }

    public string? GetSavedUserEmail()
    {
        return Preferences.Default.Get<string?>(UserEmailKey, null);
    }

    public string? GetSavedDisplayName()
    {
        return Preferences.Default.Get<string?>(DisplayNameKey, null);
    }

    public bool GetRememberSession()
    {
        return Preferences.Default.Get(RememberSessionKey, true);
    }

    public void SaveSession(string userId, string email, string displayName, bool remember)
    {
        Preferences.Default.Set(UserIdKey, userId);
        Preferences.Default.Set(UserEmailKey, email);
        Preferences.Default.Set(DisplayNameKey, displayName);
        Preferences.Default.Set(RememberSessionKey, remember);
    }

    public void ClearSession()
    {
        Preferences.Default.Remove(UserIdKey);
        Preferences.Default.Remove(UserEmailKey);
        Preferences.Default.Remove(DisplayNameKey);
        Preferences.Default.Set(RememberSessionKey, false);
    }
}
