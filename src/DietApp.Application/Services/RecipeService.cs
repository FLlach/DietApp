using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Implementa los casos de uso para la creacion, busqueda y consulta de recetas.
/// Resuelve los alimentos de la receta contra el catalogo, genera los RecipeIngredient con su aporte
/// correspondiente de minerales y calorias, estructura los pasos numerados con imagenes y persiste la receta.
/// Por que se tomo esta decision: Asegura que el calculo de compuestos y balance por porcion
/// se realice rigurosamente a traves del agregado de dominio `Recipe`, devolviendo DTOs consolidados
/// listos para renderizar en la vista.
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IFoodRepository _foodRepository;

    public RecipeService(
        IRecipeRepository recipeRepository,
        IFoodRepository foodRepository)
    {
        _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
    }

    public async Task<IReadOnlyList<RecipeDto>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository.GetAllAsync();
        return recipes.Select(recipe => recipe.ToDto()).ToList();
    }

    public async Task<RecipeDto?> GetRecipeByIdAsync(Guid id)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        return recipe?.ToDto();
    }

    public async Task<IReadOnlyList<RecipeDto>> SearchRecipesAsync(string query)
    {
        var recipes = await _recipeRepository.SearchByTitleAsync(query);
        return recipes.Select(recipe => recipe.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<RecipeDto>> GetRecipesSortedByMineralAsync(MineralType mineral, bool descending = true)
    {
        var recipes = await GetAllRecipesAsync();
        return descending
            ? recipes.OrderByDescending(r => r.GetMineralAmountPerServing(mineral)).ThenBy(r => r.Title).ToList()
            : recipes.OrderBy(r => r.GetMineralAmountPerServing(mineral)).ThenBy(r => r.Title).ToList();
    }

    public async Task<RecipeDto> CreateRecipeAsync(
        string title,
        string description,
        int servings,
        string finalImagePath,
        IEnumerable<(Guid foodId, double grams)> ingredients,
        IEnumerable<(string instruction, string imagePath)> steps)
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            servings,
            finalImagePath);

        foreach (var (foodId, grams) in ingredients)
        {
            var food = await _foodRepository.GetByIdAsync(foodId);
            if (food != null)
            {
                var ingredient = RecipeIngredient.FromFoodItem(food, grams);
                recipe.AddIngredient(ingredient);
            }
        }

        int stepNumber = 1;
        foreach (var (instruction, imagePath) in steps)
        {
            var recipeStep = new RecipeStep(stepNumber, instruction, imagePath);
            recipe.AddStep(recipeStep);
            stepNumber++;
        }

        await _recipeRepository.SaveAsync(recipe);
        return recipe.ToDto();
    }

    public async Task DeleteRecipeAsync(Guid id)
    {
        await _recipeRepository.DeleteAsync(id);
    }
}
