using System.Text.Json;
using DietApp.Domain.Entities;
using DietApp.Domain.ValueObjects;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `recipe_ingredients` en Supabase PostgreSQL.
/// Preserva los ingredientes dosificados de cada receta y su perfil de minerales.
/// Por que se tomo esta decision: Permite calcular de forma exacta el perfil nutricional por porcion.
/// </summary>
[Table("recipe_ingredients")]
public class SupabaseRecipeIngredientModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("recipe_id")]
    public string RecipeId { get; set; } = string.Empty;

    [Column("food_item_id")]
    public string? FoodItemId { get; set; }

    [Column("food_name")]
    public string FoodName { get; set; } = string.Empty;

    [Column("grams")]
    public double Grams { get; set; }

    [Column("calculated_calories")]
    public double CalculatedCalories { get; set; }

    [Column("calculated_protein")]
    public double CalculatedProtein { get; set; }

    [Column("calculated_minerals_json")]
    public string CalculatedMineralsJson { get; set; } = "[]";

    public RecipeIngredient ToDomain()
    {
        Guid.TryParse(Id, out var parsedId);
        Guid.TryParse(FoodItemId, out var parsedFoodId);

        var minerals = JsonSerializer.Deserialize<List<MineralAmount>>(CalculatedMineralsJson) 
                       ?? new List<MineralAmount>();

        return new RecipeIngredient(
            parsedId == Guid.Empty ? Guid.NewGuid() : parsedId,
            parsedFoodId,
            FoodName,
            Grams,
            CalculatedCalories,
            minerals,
            CalculatedProtein);
    }

    public static SupabaseRecipeIngredientModel FromDomain(Guid recipeId, RecipeIngredient ingredient)
    {
        return new SupabaseRecipeIngredientModel
        {
            Id = ingredient.Id.ToString(),
            RecipeId = recipeId.ToString(),
            FoodItemId = ingredient.FoodItemId.ToString(),
            FoodName = ingredient.FoodName,
            Grams = ingredient.Grams,
            CalculatedCalories = ingredient.CalculatedCalories,
            CalculatedProtein = ingredient.CalculatedProtein,
            CalculatedMineralsJson = JsonSerializer.Serialize(ingredient.CalculatedMinerals)
        };
    }
}
