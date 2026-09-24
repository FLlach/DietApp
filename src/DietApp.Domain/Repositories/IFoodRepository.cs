using DietApp.Domain.Entities;
using DietApp.Domain.Enums;

namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para el catalogo de alimentos y sus perfiles de minerales.
/// Define las operaciones de consulta, filtrado especializado por rangos de minerales y almacenamiento.
/// Por que se tomo esta decision: En DDD, los repositorios se definen mediante interfaces en la capa
/// de Dominio para desacoplar las reglas de negocio de la tecnologia concreta de base de datos
/// (SQLite, base de datos remota o memoria).
/// </summary>
public interface IFoodRepository
{
    Task<FoodItem?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<FoodItem>> GetAllAsync();
    Task<IReadOnlyList<FoodItem>> FilterByMineralRangeAsync(MineralType mineralType, double minimumMilligrams, double maximumMilligrams);
    Task<IReadOnlyList<FoodItem>> SearchByNameOrCategoryAsync(string query);
    Task AddAsync(FoodItem foodItem);
    Task UpdateAsync(FoodItem foodItem);
    Task DeleteAsync(Guid id);
}
