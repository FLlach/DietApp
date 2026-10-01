using Supabase;

namespace DietApp.Infrastructure.Supabase;

/// <summary>
/// Como funciona: Proveedor singleton que gestiona el ciclo de vida e inicializacion asincrona
/// de `Supabase.Client`. Emplea un `SemaphoreSlim` para garantizar inicializacion thread-safe unica.
/// Por que se tomo esta decision: Evita reconexiones redundantes al motor de Supabase
/// y permite a los repositorios obtener la instancia de cliente lista para ejecutar consultas PostgREST.
/// </summary>
public class SupabaseClientProvider : ISupabaseClientProvider
{
    private Client? _client;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    public bool IsConfigured => SupabaseConfig.IsConfigured;

    public async Task<Client> GetClientAsync()
    {
        if (_client != null)
        {
            return _client;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_client != null)
            {
                return _client;
            }

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            var client = new Client(SupabaseConfig.ProjectUrl, SupabaseConfig.AnonKey, options);
            await client.InitializeAsync();

            _client = client;
            return _client;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}
