namespace DietApp.Domain.Services;

/// <summary>
/// Como funciona: Contrato de dominio para persistir y recuperar la sesion recordada del usuario en almacenamiento local seguro.
/// Por que se tomo esta decision: Permite a la infraestructura y aplicacion interactuar con el almacenamiento de sesion
/// sin acoplarse a APIs especificas de plataforma (Preferences / SharedPreferences).
/// </summary>
public interface IUserSessionStorage
{
    string? GetSavedUserId();
    string? GetSavedUserEmail();
    string? GetSavedDisplayName();
    bool GetRememberSession();
    void SaveSession(string userId, string email, string displayName, bool remember);
    void ClearSession();
}
