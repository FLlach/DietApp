using DietApp.Domain.Entities;

namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para el almacenamiento y consulta de alinos y condimentos.
/// Permite obtener el listado completo, consultar por identificador unico, guardar y eliminar registros.
/// Por que se tomo esta decision: En DDD, separa el modelo de dominio de las operaciones concretas en bases de datos relacionales,
/// posibilitando pruebas unitarias sencillas y desacoplamiento con SQLite.
/// </summary>
public interface ISeasoningRepository
{
    Task<IReadOnlyList<Seasoning>> GetAllAsync();
    Task<Seasoning?> GetByIdAsync(Guid id);
    Task SaveAsync(Seasoning seasoning);
    Task DeleteAsync(Guid id);
}
