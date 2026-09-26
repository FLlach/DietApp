using System.Text.Json;
using DietApp.Domain.Entities;
using DietApp.Domain.ValueObjects;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para cada ingrediente de una receta.
/// Almacena los gramos y los minerales calculados serializados como JSON.
/// Por que se tomo esta decision: Permite reconstruir de forma autonoma el calculo nutricional
/// de cada receta con base en las cantidades registradas al momento de su creacion.
/// </summary>
[Table("RecipeIngredients")]
public class RecipeIngredientEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid RecipeId { get; set; }

    public Guid FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public double Grams { get; set; }

    public double CalculatedCalories { get; set; }

    public double CalculatedProtein { get; set; }

    public string CalculatedMineralsJson { get; set; } = "[]";

    public RecipeIngredient ToDomain()
    {
        var minerals = JsonSerializer.Deserialize<List<MineralAmount>>(CalculatedMineralsJson) 
                       ?? new List<MineralAmount>();

        return new RecipeIngredient(
            Id,
            FoodItemId,
            FoodName,
            Grams,
            CalculatedCalories,
            minerals,
            CalculatedProtein);
    }

    public static RecipeIngredientEntity FromDomain(Guid recipeId, RecipeIngredient ingredient)
    {
        return new RecipeIngredientEntity
        {
            Id = ingredient.Id,
            RecipeId = recipeId,
            FoodItemId = ingredient.FoodItemId,
            FoodName = ingredient.FoodName,
            Grams = ingredient.Grams,
            CalculatedCalories = ingredient.CalculatedCalories,
            CalculatedProtein = ingredient.CalculatedProtein,
            CalculatedMineralsJson = JsonSerializer.Serialize(ingredient.CalculatedMinerals)
        };
    }
}
