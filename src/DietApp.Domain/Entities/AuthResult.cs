namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Objeto de valor del dominio que encapsula el resultado de cualquier intento de autenticacion.
/// Proporciona bandera booleana de exito, la sesion resultante (UserSession) o el mensaje descriptivo del fallo.
/// Por que se tomo esta decision: Permite comunicar el estado de autenticacion entre la infraestructura
/// y las capas superiores respetando Domain-Driven Design sin dependencias externas.
/// </summary>
public class AuthResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public UserSession? Session { get; }

    public static AuthResult Ok(UserSession session) => new(true, session, null);
    public static AuthResult Fail(string errorMessage) => new(false, null, errorMessage);

    private AuthResult(bool success, UserSession? session, string? errorMessage)
    {
        Success = success;
        Session = session;
        ErrorMessage = errorMessage;
    }
}
