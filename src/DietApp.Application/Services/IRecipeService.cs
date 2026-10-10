using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato del servicio de aplicacion para la gestion integral de recetas culinarias.
/// Permite listar recetas, buscar por texto, consultar detalles con calculo de minerales por porcion,
/// dar de alta nuevas recetas con pasos e imagenes, ordenar por contenido mineral y eliminar recetas.
/// Por que se tomo esta decision: En DDD, aísla los casos de uso relacionados a recetas para que
/// los ViewModels de la interfaz interactuen con un contrato limpio y desacoplado de la persistencia.
/// </summary>
public interface IRecipeService
{
    Task<IReadOnlyList<RecipeDto>> GetAllRecipesAsync();
    Task<IReadOnlyList<RecipeDto>> GetGlobalRecipesAsync();
    Task<RecipeDto?> GetRecipeByIdAsync(Guid id);
    Task<IReadOnlyList<RecipeDto>> SearchRecipesAsync(string query);
    Task<IReadOnlyList<RecipeDto>> SearchGlobalRecipesAsync(string query);
    Task<IReadOnlyList<RecipeDto>> GetFavoriteRecipesAsync(string userId);
    Task<bool> IsFavoriteAsync(string userId, Guid recipeId);
    Task<bool> ToggleFavoriteAsync(string userId, Guid recipeId);
    Task<IReadOnlyList<RecipeDto>> GetRecipesSortedByMineralAsync(MineralType mineral, bool descending = true);
    Task<RecipeDto> CreateRecipeAsync(
        string title,
        string description,
        int servings,
        string finalImagePath,
        IEnumerable<(Guid foodId, double grams)> ingredients,
        IEnumerable<(string instruction, string imagePath)> steps);
    Task DeleteRecipeAsync(Guid id);
}
