using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para la gestion del tema visual de la aplicacion (Modo Claro, Modo Oscuro, Sistema).
/// Expone la modalidad activa, notifica cambios y permite modificar el tema de forma desacoplada de la UI.
/// Por que se tomo esta decision: En arquitectura limpia y DDD, la capa de aplicacion define los casos de uso
/// y reglas de cambio de configuracion sin acoplarse directamente a controles o APIs de plataformas nativas.
/// </summary>
public interface IThemeService
{
    ThemeMode CurrentTheme { get; }
    event EventHandler ThemeChanged;
    void SetTheme(ThemeMode theme);
    void Initialize();
}
