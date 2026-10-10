using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Domain.Services;
using Supabase.Gotrue;

namespace DietApp.Infrastructure.Supabase;

/// <summary>
/// Como funciona: Implementacion del servicio de autenticacion utilizando el cliente de Supabase (Gotrue).
/// Gestiona la comunicacion con el backend en la nube para login y registro de pacientes,
/// mantiene persistencia de credenciales recordadas mediante IUserSessionStorage y soporta
/// fallback transparente a modo offline sin conexion a internet.
/// Por que se tomo esta decision: Centraliza la logica de identidad y manejo de tokens JWT en la capa
/// de infraestructura, garantizando desacoplamiento del dominio y de los ViewModels.
/// </summary>
public class SupabaseAuthService : IAuthService
{
    private readonly ISupabaseClientProvider _clientProvider;
    private readonly IUserSessionStorage _sessionStorage;
    private readonly IUserProfileRepository _userProfileRepository;

    private UserSession _currentSession = UserSession.CreateOffline("Usuario Principal");

    public event EventHandler<UserSession?>? AuthStateChanged;

    public SupabaseAuthService(
        ISupabaseClientProvider clientProvider,
        IUserSessionStorage sessionStorage,
        IUserProfileRepository userProfileRepository)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));

        RestoreSavedSession();
    }

    private void RestoreSavedSession()
    {
        if (_sessionStorage.GetRememberSession())
        {
            var savedId = _sessionStorage.GetSavedUserId();
            var savedEmail = _sessionStorage.GetSavedUserEmail();
            var savedName = _sessionStorage.GetSavedDisplayName();

            if (!string.IsNullOrWhiteSpace(savedId) && savedId != "local_user")
            {
                _currentSession = new UserSession(
                    userId: savedId,
                    email: savedEmail ?? string.Empty,
                    displayName: string.IsNullOrWhiteSpace(savedName) ? "Usuario" : savedName,
                    accessToken: null,
                    isAuthenticated: true,
                    isOfflineMode: false);
            }
        }
    }

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password, bool rememberSession = true)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Fail("Por favor, ingresa tu correo electronico o ID de paciente.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Fail("Por favor, ingresa tu contrasena o PIN clinico.");
        }

        if (!_clientProvider.IsConfigured)
        {
            return AuthResult.Fail("El cliente de Supabase no esta configurado.");
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var session = await client.Auth.SignInWithPassword(email.Trim(), password);

            if (session?.User != null)
            {
                string userId = session.User.Id ?? string.Empty;
                string userEmail = session.User.Email ?? email.Trim();

                string displayName = "Usuario";
                if (session.User.UserMetadata != null && session.User.UserMetadata.TryGetValue("display_name", out var metaName) && metaName != null)
                {
                    displayName = metaName.ToString() ?? "Usuario";
                }
                else
                {
                    displayName = userEmail.Contains('@') ? userEmail.Split('@')[0] : userEmail;
                }

                _currentSession = new UserSession(
                    userId: userId,
                    email: userEmail,
                    displayName: displayName,
                    accessToken: session.AccessToken,
                    isAuthenticated: true,
                    isOfflineMode: false);

                if (rememberSession)
                {
                    _sessionStorage.SaveSession(userId, userEmail, displayName, true);
                }
                else
                {
                    _sessionStorage.ClearSession();
                }

                AuthStateChanged?.Invoke(this, _currentSession);
                return AuthResult.Ok(_currentSession);
            }

            return AuthResult.Fail("No se pudo iniciar sesion. Credenciales invalidas.");
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            if (message.Contains("Invalid login credentials", StringComparison.OrdinalIgnoreCase))
            {
                message = "Correo electronico o contrasena incorrectos.";
            }
            else if (message.Contains("Email not confirmed", StringComparison.OrdinalIgnoreCase))
            {
                message = "Debes confirmar tu correo electronico antes de iniciar sesion.";
            }
            return AuthResult.Fail(message);
        }
    }

    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Fail("Por favor, ingresa tu correo electronico.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return AuthResult.Fail("La contrasena debe tener al menos 6 caracteres.");
        }

        if (!_clientProvider.IsConfigured)
        {
            return AuthResult.Fail("El cliente de Supabase no esta configurado.");
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            string finalName = string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName.Trim();

            var options = new SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "display_name", finalName }
                }
            };

            var session = await client.Auth.SignUp(email.Trim(), password, options);

            if (session?.User != null)
            {
                string userId = session.User.Id ?? string.Empty;
                string userEmail = session.User.Email ?? email.Trim();

                _currentSession = new UserSession(
                    userId: userId,
                    email: userEmail,
                    displayName: finalName,
                    accessToken: session.AccessToken,
                    isAuthenticated: true,
                    isOfflineMode: false);

                _sessionStorage.SaveSession(userId, userEmail, finalName, true);

                AuthStateChanged?.Invoke(this, _currentSession);
                return AuthResult.Ok(_currentSession);
            }

            return AuthResult.Fail("No se pudo completar el registro. Verifica los datos ingresados.");
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            if (message.Contains("User already registered", StringComparison.OrdinalIgnoreCase))
            {
                message = "Ya existe un usuario registrado con este correo electronico.";
            }
            return AuthResult.Fail(message);
        }
    }

    public Task<AuthResult> SignInOfflineAsync()
    {
        _currentSession = UserSession.CreateOffline("Usuario Local");
        AuthStateChanged?.Invoke(this, _currentSession);
        return Task.FromResult(AuthResult.Ok(_currentSession));
    }

    public async Task<AuthResult> SignInBiometricAsync()
    {
        var savedId = _sessionStorage.GetSavedUserId();
        var savedEmail = _sessionStorage.GetSavedUserEmail();
        var savedName = _sessionStorage.GetSavedDisplayName();

        if (!string.IsNullOrWhiteSpace(savedId))
        {
            bool isOffline = savedId == "local_user";
            _currentSession = new UserSession(
                userId: savedId,
                email: savedEmail ?? string.Empty,
                displayName: string.IsNullOrWhiteSpace(savedName) ? "Usuario" : savedName,
                accessToken: null,
                isAuthenticated: !isOffline,
                isOfflineMode: isOffline);

            AuthStateChanged?.Invoke(this, _currentSession);
            return AuthResult.Ok(_currentSession);
        }

        return await SignInOfflineAsync();
    }

    public async Task SignOutAsync()
    {
        if (_clientProvider.IsConfigured)
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                await client.Auth.SignOut();
            }
            catch
            {
                // Limpieza local prioritaria
            }
        }

        _sessionStorage.ClearSession();
        _currentSession = UserSession.CreateOffline("Usuario Principal");
        AuthStateChanged?.Invoke(this, null);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_currentSession.IsAuthenticated);
    }

    public Task<string> GetCurrentUserIdAsync()
    {
        return Task.FromResult(_currentSession.UserId);
    }

    public Task<string?> GetCurrentUserEmailAsync()
    {
        return Task.FromResult<string?>(_currentSession.Email);
    }
}
