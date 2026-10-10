using System.Text.Json;
using DietApp.Domain.Entities;
using DietApp.Domain.ValueObjects;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `meal_items` en Supabase PostgreSQL.
/// Preserva la instantanea nutricional calculada congelada en formato JSON.
/// Por que se tomo esta decision: Asegura inmutabilidad del historial de ingestas ante cambios futuros en el catalogo.
/// </summary>
[Table("meal_items")]
public class SupabaseMealItemModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("meal_id")]
    public string MealId { get; set; } = string.Empty;

    [Column("food_item_id")]
    public string? FoodItemId { get; set; }

    [Column("food_name")]
    public string FoodName { get; set; } = string.Empty;

    [Column("portion_in_grams")]
    public double PortionInGrams { get; set; }

    [Column("calculated_calories")]
    public double CalculatedCalories { get; set; }

    [Column("calculated_protein")]
    public double CalculatedProtein { get; set; }

    [Column("calculated_minerals_json")]
    public string CalculatedMineralsJson { get; set; } = "[]";

    public MealItem ToDomain()
    {
        Guid.TryParse(Id, out var parsedId);
        Guid.TryParse(FoodItemId, out var parsedFoodId);

        var minerals = JsonSerializer.Deserialize<List<MineralAmount>>(CalculatedMineralsJson) 
                       ?? new List<MineralAmount>();

        return new MealItem(
            parsedId == Guid.Empty ? Guid.NewGuid() : parsedId,
            parsedFoodId,
            FoodName,
            PortionInGrams,
            minerals,
            CalculatedCalories,
            CalculatedProtein);
    }

    public static SupabaseMealItemModel FromDomain(Guid mealId, MealItem item)
    {
        return new SupabaseMealItemModel
        {
            Id = item.Id.ToString(),
            MealId = mealId.ToString(),
            FoodItemId = item.FoodItemId.ToString(),
            FoodName = item.FoodName,
            PortionInGrams = item.PortionInGrams,
            CalculatedCalories = item.CalculatedCalories,
            CalculatedProtein = item.CalculatedProtein,
            CalculatedMineralsJson = JsonSerializer.Serialize(item.CalculatedMinerals)
        };
    }
}
