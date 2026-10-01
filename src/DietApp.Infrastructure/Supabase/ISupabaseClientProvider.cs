using Supabase;

namespace DietApp.Infrastructure.Supabase;

/// <summary>
/// Como funciona: Contrato para el proveedor del cliente oficial de Supabase (`Supabase.Client`).
/// Provee acceso asincrono seguro a la instancia inicializada y verifica si las credenciales estan configuradas.
/// Por que se tomo esta decision: Permite inicializar el cliente de forma perezosa (Lazy) y segura entre hilos,
/// desacoplando la creacion de la conexion de los repositorios que consumen PostgREST.
/// </summary>
public interface ISupabaseClientProvider
{
    Task<Client> GetClientAsync();
    bool IsConfigured { get; }
}
