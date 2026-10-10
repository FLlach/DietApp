using DietApp.Domain.Entities;

namespace DietApp.Domain.Services;

/// <summary>
/// Como funciona: Contrato del servicio de autenticacion en la capa de Dominio.
/// Define las operaciones esenciales de identidad: inicio de sesion con correo y clave, registro,
/// acceso rapido biometrico, continuacion en modo offline y cierre de sesion.
/// Por que se tomo esta decision: Permite la inversion de control garantizando que tanto la implementacion
/// en Supabase como los servicios de aplicacion y ViewModels interactuen mediante abstracciones puras.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> SignInWithEmailAsync(string email, string password, bool rememberSession = true);
    Task<AuthResult> SignUpWithEmailAsync(string email, string password, string displayName);
    Task<AuthResult> SignInOfflineAsync();
    Task<AuthResult> SignInBiometricAsync();
    Task SignOutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserEmailAsync();

    event EventHandler<UserSession?>? AuthStateChanged;
}
