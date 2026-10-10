namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Entidad que representa la sesion de autenticacion activa del usuario en el sistema.
/// Almacena el identificador de usuario (UserId en Supabase Auth o 'local_user' en modo offline),
/// correo electronico, nombre visible, token de sesion y estado de conexion.
/// Por que se tomo esta decision: Permite al dominio desacoplarse del SDK especifico de Supabase Gotrue,
/// manteniendo inmutabilidad y control sobre los estados de sesion remota o local.
/// </summary>
public class UserSession
{
    public string UserId { get; }
    public string Email { get; }
    public string DisplayName { get; }
    public string? AccessToken { get; }
    public bool IsAuthenticated { get; }
    public bool IsOfflineMode { get; }
    public DateTime LoggedInAt { get; }

    public UserSession(
        string userId,
        string email,
        string displayName,
        string? accessToken = null,
        bool isAuthenticated = true,
        bool isOfflineMode = false,
        DateTime? loggedInAt = null)
    {
        UserId = string.IsNullOrWhiteSpace(userId) ? "local_user" : userId.Trim();
        Email = email ?? string.Empty;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Usuario" : displayName.Trim();
        AccessToken = accessToken;
        IsAuthenticated = isAuthenticated;
        IsOfflineMode = isOfflineMode;
        LoggedInAt = loggedInAt ?? DateTime.UtcNow;
    }

    public static UserSession CreateOffline(string displayName = "Usuario Local")
    {
        return new UserSession(
            userId: "local_user",
            email: string.Empty,
            displayName: displayName,
            accessToken: null,
            isAuthenticated: false,
            isOfflineMode: true);
    }
}
