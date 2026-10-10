using DietApp.Application.DTOs;
using DietApp.Application.Mapping;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.Repositories;
using DietApp.Domain.ValueObjects;

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
    private readonly IRecipeFavoriteRepository _favoriteRepository;

    public RecipeService(
        IRecipeRepository recipeRepository,
        IFoodRepository foodRepository,
        IRecipeFavoriteRepository favoriteRepository)
    {
        _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
        _favoriteRepository = favoriteRepository ?? throw new ArgumentNullException(nameof(favoriteRepository));
    }

    public async Task<IReadOnlyList<RecipeDto>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository.GetAllAsync();
        return recipes.Select(recipe => recipe.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<RecipeDto>> GetGlobalRecipesAsync()
    {
        var recipes = await _recipeRepository.GetGlobalRecipesAsync();
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

    public async Task<IReadOnlyList<RecipeDto>> SearchGlobalRecipesAsync(string query)
    {
        var recipes = await _recipeRepository.SearchGlobalRecipesAsync(query);
        return recipes.Select(recipe => recipe.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<RecipeDto>> GetFavoriteRecipesAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new List<RecipeDto>();

        var favIds = await _favoriteRepository.GetFavoriteRecipeIdsAsync(userId);
        if (favIds.Count == 0) return new List<RecipeDto>();

        var allRecipes = await _recipeRepository.GetAllAsync();
        var favSet = new HashSet<Guid>(favIds);

        return allRecipes
            .Where(r => favSet.Contains(r.Id))
            .Select(r =>
            {
                var dto = r.ToDto();
                dto.IsFavorite = true;
                return dto;
            })
            .ToList();
    }

    public async Task<bool> IsFavoriteAsync(string userId, Guid recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId) || recipeId == Guid.Empty) return false;
        return await _favoriteRepository.IsFavoriteAsync(userId, recipeId);
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, Guid recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId) || recipeId == Guid.Empty) return false;

        bool isFav = await _favoriteRepository.IsFavoriteAsync(userId, recipeId);
        if (isFav)
        {
            await _favoriteRepository.RemoveFavoriteAsync(userId, recipeId);
            return false;
        }
        else
        {
            await _favoriteRepository.AddFavoriteAsync(userId, recipeId);
            return true;
        }
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
        var dtos = ingredients.Select(i => new RecipeIngredientCreateDto
        {
            FoodId = i.foodId,
            Grams = i.grams
        });

        return await CreateRecipeAsync(title, description, servings, finalImagePath, dtos, steps);
    }

    public async Task<RecipeDto> CreateRecipeAsync(
        string title,
        string description,
        int servings,
        string finalImagePath,
        IEnumerable<RecipeIngredientCreateDto> ingredients,
        IEnumerable<(string instruction, string imagePath)> steps)
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            servings,
            finalImagePath);

        foreach (var item in ingredients)
        {
            if (item.FoodId != Guid.Empty)
            {
                var food = await _foodRepository.GetByIdAsync(item.FoodId);
                if (food != null)
                {
                    var ingredient = RecipeIngredient.FromFoodItem(food, item.Grams);
                    recipe.AddIngredient(ingredient);
                    continue;
                }
            }

            // Ingrediente ad-hoc o no presente en la base de datos de alimentos
            var minerals = new List<MineralAmount>
            {
                new(MineralType.Sodium, item.SodiumMg),
                new(MineralType.Potassium, item.PotassiumMg),
                new(MineralType.Phosphorus, item.PhosphorusMg)
            };

            var customIngredient = new RecipeIngredient(
                Guid.NewGuid(),
                item.FoodId == Guid.Empty ? Guid.NewGuid() : item.FoodId,
                string.IsNullOrWhiteSpace(item.Name) ? "Ingrediente" : item.Name.Trim(),
                item.Grams <= 0 ? 1.0 : item.Grams,
                item.Calories,
                minerals,
                item.ProteinGrams);

            recipe.AddIngredient(customIngredient);
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
