using DietApp.Domain.Entities;

namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para el registro de comidas consumidas.
/// Permite recuperar comidas por rango de fechas (para conteos diarios o semanales) y guardar agregados.
/// Por que se tomo esta decision: Sigue el principio de inversion de dependencias de DDD y SOLID,
/// asegurando que la logica de conteo y acumulacion nutricional no dependa de implementaciones
/// especificas de almacenamiento.
/// </summary>
public interface IMealRepository
{
    Task<Meal?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Meal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<Meal>> GetAllAsync();
    Task SaveAsync(Meal meal);
    Task DeleteAsync(Guid id);
}
