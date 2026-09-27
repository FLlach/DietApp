using DietApp.Application.Services;
using DietApp.Domain.Enums;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Servicio que gobierna la configuracion y aplicacion del tema visual (Modo Claro, Modo Oscuro, Sistema)
/// en .NET MAUI. Persiste la eleccion mediante Preferences y conmuta Application.Current.UserAppTheme en el hilo principal.
/// Por que se tomo esta decision: Desacopla la logica de conmutacion visual de los ViewModels y garantiza que la aplicacion
/// inicie siempre con el tema deseado por el usuario, preservando el modo oscuro como predeterminado si no hay configuracion previa.
/// </summary>
public class AppThemeService : IThemeService
{
    private const string ThemePreferenceKey = "dietapp_visual_theme_mode";

    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Dark;

    public event EventHandler? ThemeChanged;

    public void Initialize()
    {
        var saved = Preferences.Default.Get(ThemePreferenceKey, nameof(ThemeMode.Dark));
        if (Enum.TryParse<ThemeMode>(saved, out var mode))
        {
            ApplyTheme(mode, persist: false);
        }
        else
        {
            ApplyTheme(ThemeMode.Dark, persist: false);
        }
    }

    public void SetTheme(ThemeMode theme)
    {
        ApplyTheme(theme, persist: true);
    }

    private void ApplyTheme(ThemeMode theme, bool persist)
    {
        CurrentTheme = theme;

        if (persist)
        {
            Preferences.Default.Set(ThemePreferenceKey, theme.ToString());
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Microsoft.Maui.Controls.Application.Current != null)
            {
                Microsoft.Maui.Controls.Application.Current.UserAppTheme = theme switch
                {
                    ThemeMode.Light => AppTheme.Light,
                    ThemeMode.Dark => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
            }
        });

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
