using DietApp.Application.DTOs;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato del servicio de aplicacion para la gestion del catalogo de alimentos
/// y consultas especializadas por concentracion de minerales.
/// Por que se tomo esta decision: Encapsula los casos de uso relacionados con la consulta,
/// filtrado y registro de alimentos para que los ViewModels interactuen con operaciones
/// de alto nivel en lugar de llamar directamente a los repositorios de dominio.
/// </summary>
public interface IFoodCatalogService
{
    Task<IReadOnlyList<FoodItemDto>> GetAllFoodsAsync();
    Task<IReadOnlyList<FoodItemDto>> FilterFoodsAsync(MineralFilterCriteriaDto criteria);
    Task<FoodItemDto?> GetFoodByIdAsync(Guid id);
    Task SaveFoodAsync(FoodItemDto foodDto);
    Task DeleteFoodAsync(Guid id);
}
