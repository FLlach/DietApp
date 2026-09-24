using System.Text.Json;
using DietApp.Domain.Entities;
using DietApp.Domain.ValueObjects;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para cada alimento individual consumido en una comida.
/// Almacena los minerales calculados serializados como json para congelar la instantanea historica.
/// Por que se tomo esta decision: Asegura que el historial nutricional se mantenga inalterable
/// aunque el alimento base sea modificado en el catalogo en el futuro.
/// </summary>
[Table("MealItems")]
public class MealItemEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid MealId { get; set; }

    public Guid FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public double PortionInGrams { get; set; }

    public string CalculatedMineralsJson { get; set; } = "[]";

    public MealItem ToDomain()
    {
        var minerals = JsonSerializer.Deserialize<List<MineralAmount>>(CalculatedMineralsJson) 
                       ?? new List<MineralAmount>();

        return new MealItem(
            Id,
            FoodItemId,
            FoodName,
            PortionInGrams,
            minerals);
    }

    public static MealItemEntity FromDomain(Guid mealId, MealItem item)
    {
        return new MealItemEntity
        {
            Id = item.Id,
            MealId = mealId,
            FoodItemId = item.FoodItemId,
            FoodName = item.FoodName,
            PortionInGrams = item.PortionInGrams,
            CalculatedMineralsJson = JsonSerializer.Serialize(item.CalculatedMinerals)
        };
    }
}
