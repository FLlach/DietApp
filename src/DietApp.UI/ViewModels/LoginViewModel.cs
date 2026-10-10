using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.Services;
using DietApp.Domain.Services;
using DietApp.UI.Helpers;
using DietApp.UI.Localization;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de Inicio de Sesion (LoginPage) segun prototipo Stitch.
/// Gestiona la captura del correo o ID de paciente, contrasena o PIN clinico, alternancia de visibilidad de contrasena,
/// modo de registro vs login, simulacion de autenticacion biometrica (Android BiometricPrompt),
/// continuacion en modo offline y retroalimentacion de errores clinicos.
/// Por que se tomo esta decision: Orquesta el flujo de autenticacion mediante IAuthService desacoplando la UI
/// de los detalles de red y permitiendo navegacion fluida con AppShell.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private string _emailOrPatientId = "renaldiet.kdoqi@clinic.org";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    [ObservableProperty]
    private bool _rememberSession = true;

    [ObservableProperty]
    private bool _isSignUpMode = false;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private bool _isBiometricModalVisible = false;

    public string PasswordToggleIcon => IsPasswordHidden
        ? MaterialIconFont.Visibility
        : MaterialIconFont.VisibilityOff;

    public string SubmitButtonText => IsSignUpMode
        ? "Crear Cuenta Clínica"
        : "Iniciar Sesión";

    public string SwitchModeText => IsSignUpMode
        ? "¿Ya tienes cuenta? Inicia sesión aquí"
        : "¿No tienes cuenta? Regístrate aquí";

    public LoginViewModel(
        IAuthService authService,
        IUserProfileService userProfileService,
        ILocalizationService localizationService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    partial void OnIsPasswordHiddenChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordToggleIcon));
    }

    partial void OnIsSignUpModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SubmitButtonText));
        OnPropertyChanged(nameof(SwitchModeText));
        HasError = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    [RelayCommand]
    private void ToggleAuthMode()
    {
        IsSignUpMode = !IsSignUpMode;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy) return;

        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(EmailOrPatientId))
        {
            HasError = true;
            ErrorMessage = "Ingresa tu correo electrónico o ID de paciente.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            HasError = true;
            ErrorMessage = "Ingresa tu contraseña o PIN clínico.";
            return;
        }

        IsBusy = true;
        try
        {
            if (IsSignUpMode)
            {
                var signUpResult = await _authService.SignUpWithEmailAsync(EmailOrPatientId, Password, DisplayName);
                if (signUpResult.Success && signUpResult.Session != null)
                {
                    await _userProfileService.GetCurrentProfileAsync();
                    await NavigateToMainAppAsync();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = signUpResult.ErrorMessage ?? "Error al registrar la cuenta.";
                }
            }
            else
            {
                var loginResult = await _authService.SignInWithEmailAsync(EmailOrPatientId, Password, RememberSession);
                if (loginResult.Success && loginResult.Session != null)
                {
                    await _userProfileService.GetCurrentProfileAsync();
                    await NavigateToMainAppAsync();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = loginResult.ErrorMessage ?? "Credenciales incorrectas.";
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenBiometricModal()
    {
        IsBiometricModalVisible = true;
    }

    [RelayCommand]
    private void CloseBiometricModal()
    {
        IsBiometricModalVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmBiometricAsync()
    {
        IsBiometricModalVisible = false;
        IsBusy = true;
        try
        {
            var result = await _authService.SignInBiometricAsync();
            if (result.Success)
            {
                await _userProfileService.GetCurrentProfileAsync();
                await NavigateToMainAppAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Autenticación biométrica no completada.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ContinueOfflineAsync()
    {
        IsBusy = true;
        try
        {
            await _authService.SignInOfflineAsync();
            await _userProfileService.GetCurrentProfileAsync();
            await NavigateToMainAppAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Recuperación de Acceso",
                "Para restablecer tu contraseña o PIN clínico, contacta a tu especialista en nutrición o revisa el correo de verificación enviado por Supabase.",
                "Entendido");
        }
    }

    private async Task NavigateToMainAppAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("//MealTrackingPage");
        }
    }
}
