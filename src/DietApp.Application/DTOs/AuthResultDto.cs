namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que encapsula el resultado de cualquier operacion de autenticacion (login, registro, offline).
/// Proporciona bandera booleana de exito, mensaje de error en caso de fallo y datos esenciales de la sesion.
/// Por que se tomo esta decision: Evita acoplar la capa de aplicacion y de presentacion a excepciones o tipos de red de Supabase.
/// </summary>
public class AuthResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public bool IsOfflineMode { get; init; }

    public static AuthResultDto Ok(string userId, string email, string displayName, bool isOffline = false)
    {
        return new AuthResultDto
        {
            Success = true,
            UserId = userId,
            Email = email,
            DisplayName = displayName,
            IsOfflineMode = isOffline
        };
    }

    public static AuthResultDto Fail(string errorMessage)
    {
        return new AuthResultDto
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
