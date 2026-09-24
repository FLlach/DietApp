using DietApp.Domain.Entities;

namespace DietApp.Domain.Repositories;

/// <summary>
/// Como funciona: Contrato de persistencia para el almacenamiento y consulta de recetas culinarias.
/// Permite listar todas las recetas, buscar por termino de texto y guardar nuevas creaciones.
/// Por que se tomo esta decision: En DDD, abstrae la persistencia de recetas en el dominio,
/// permitiendo desacoplar la logica de negocio de la tecnologia de almacenamiento.
/// </summary>
public interface IRecipeRepository
{
    Task<IReadOnlyList<Recipe>> GetAllAsync();
    Task<Recipe?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Recipe>> SearchByTitleAsync(string query);
    Task SaveAsync(Recipe recipe);
    Task DeleteAsync(Guid id);
}
