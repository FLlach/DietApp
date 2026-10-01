using DietApp.Domain.Entities;

namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para el perfil y las preferencias integrales del usuario.
/// Define operaciones asincronas para recuperar el perfil activo por su `UserId`, persistir modificaciones
/// y verificar la existencia de un perfil registrado.
/// Por que se tomo esta decision: Aplica el principio de Inversion de Dependencias (DIP) de DDD,
/// permitiendo sustituir la implementacion local en SQLite por la integracion remota en Supabase
/// sin afectar a la capa de aplicacion ni a la presentacion.
/// </summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetProfileAsync(string? userId = null);
    Task SaveProfileAsync(UserProfile profile);
    Task<bool> HasProfileAsync(string? userId = null);
}
