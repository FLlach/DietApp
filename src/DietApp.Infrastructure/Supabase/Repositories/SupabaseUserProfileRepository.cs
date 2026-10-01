using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Repositories;
using DietApp.Infrastructure.Supabase.Models;

namespace DietApp.Infrastructure.Supabase.Repositories;

/// <summary>
/// Como funciona: Repositorio hibrido (Offline-First) que sincroniza el perfil del usuario con Supabase (PostgreSQL).
/// Prioriza la persistencia local instantanea en SQLite para garantizar operatividad sin conexion a internet,
/// e intenta sincronizar de forma asincrona con la tabla `user_profiles` en Supabase cuando hay credenciales validas y conectividad.
/// Por que se tomo esta decision: Resuelve la fragilidad de las redes moviles evitando fallos en la aplicacion
/// si el dispositivo pierde la conexion o si las claves de Supabase no han sido configuradas todavia.
/// </summary>
public class SupabaseUserProfileRepository : IUserProfileRepository
{
    private readonly ISupabaseClientProvider _clientProvider;
    private readonly SqliteUserProfileRepository _localRepository;

    public SupabaseUserProfileRepository(
        ISupabaseClientProvider clientProvider,
        SqliteUserProfileRepository localRepository)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _localRepository = localRepository ?? throw new ArgumentNullException(nameof(localRepository));
    }

    public async Task<UserProfile?> GetProfileAsync(string? userId = null)
    {
        string targetUserId = string.IsNullOrWhiteSpace(userId) ? "local_user" : userId.Trim();

        // 1. Si Supabase esta configurado, intentar obtener desde la nube
        if (_clientProvider.IsConfigured)
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var response = await client
                    .From<SupabaseUserProfileModel>()
                    .Where(x => x.UserId == targetUserId)
                    .Get();

                var remoteModel = response.Models.FirstOrDefault();
                if (remoteModel != null)
                {
                    var remoteDomain = remoteModel.ToDomain();
                    // Actualizar cache local en SQLite
                    await _localRepository.SaveProfileAsync(remoteDomain);
                    return remoteDomain;
                }
            }
            catch
            {
                // Fallback silencioso hacia el repositorio local ante problemas de red o RLS
            }
        }

        // 2. Si no esta configurado o fallo la red, recuperar desde el almacenamiento local SQLite
        return await _localRepository.GetProfileAsync(targetUserId);
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        // 1. Siempre asegurar persistencia local inmediata
        await _localRepository.SaveProfileAsync(profile);

        // 2. Si Supabase esta configurado, sincronizar hacia PostgreSQL
        if (_clientProvider.IsConfigured)
        {
            try
            {
                var client = await _clientProvider.GetClientAsync();
                var model = SupabaseUserProfileModel.FromDomain(profile);

                await client
                    .From<SupabaseUserProfileModel>()
                    .Upsert(model);
            }
            catch
            {
                // La persistencia local asegura que ningun cambio del paciente se pierda
            }
        }
    }

    public async Task<bool> HasProfileAsync(string? userId = null)
    {
        return await _localRepository.HasProfileAsync(userId);
    }
}
