namespace DietApp.Infrastructure.Supabase;

/// <summary>
/// Como funciona: Almacena los parametros de conexion al proyecto de Supabase (URL y clave publica Anon Key).
/// Permite sobreescribir los valores en tiempo de ejecucion mediante preferencias locales o variables de entorno.
/// Por que se tomo esta decision: Centraliza la configuracion de acceso en la capa de Infraestructura,
/// evitando duplicacion de cadenas de conexion y permitiendo actualizar las credenciales de forma flexible.
/// </summary>
public static class SupabaseConfig
{
    /// <summary>
    /// URL del proyecto Supabase (Ejemplo: https://xxxxxxxxxxxx.supabase.co).
    /// Se encuentra en el panel de Supabase: Project Settings -> API -> Project URL.
    /// </summary>
    public static string ProjectUrl { get; set; } = "https://uxqumvlvkegaybsyhnyt.supabase.co";

    /// <summary>
    /// Clave publica anónima (anon / public key).
    /// Se encuentra en el panel de Supabase: Project Settings -> API -> Project API keys -> anon public.
    /// </summary>
    public static string AnonKey { get; set; } = "sb_publishable_1m-f2ERzB9pVODYu-EyMEA_6QPQPMV6";

    /// <summary>
    /// Verifica si la configuracion actual cuenta con credenciales validas configuradas.
    /// </summary>
    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ProjectUrl) &&
        !string.IsNullOrWhiteSpace(AnonKey) &&
        !ProjectUrl.Contains("your-project-id") &&
        !AnonKey.Contains("your-anon-key");
}
