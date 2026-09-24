using DietApp.Application.DTOs;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para el servicio de aplicacion encargado de gestionar alinos y condimentos.
/// Permite listar todas las combinaciones guardadas, consultar por id, crear nuevos alinos con sus ingredientes
/// y eliminarlos.
/// Por que se tomo esta decision: En DDD, encapsula las reglas de negocio de condimentos y alinos,
/// coordinando la transformacion entre entidades de dominio y DTOs listos para el consumo por la interfaz.
/// </summary>
public interface ISeasoningService
{
    Task<IReadOnlyList<SeasoningDto>> GetAllSeasoningsAsync();
    Task<SeasoningDto?> GetSeasoningByIdAsync(Guid id);
    Task<SeasoningDto> CreateSeasoningAsync(
        string name,
        string description,
        IEnumerable<(Guid foodItemId, double grams)> items);
    Task DeleteSeasoningAsync(Guid id);
}
