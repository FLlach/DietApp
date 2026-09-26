using System.Text.Json;
using DietApp.Domain.Entities;
using DietApp.Domain.ValueObjects;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para cada componente de un alino o condimento.
/// Almacena el alimento de origen, los gramos especificados y los minerales calculados serializados como JSON.
/// Por que se tomo esta decision: Preserva la instantanea nutricional de cada ingrediente del alino,
/// facilitando su recuperacion rapida e incorporacion integra en cualquier receta.
/// </summary>
[Table("SeasoningItems")]
public class SeasoningItemEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid SeasoningId { get; set; }

    public Guid FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public double Grams { get; set; }

    public double CalculatedCalories { get; set; }

    public double CalculatedProtein { get; set; }

    public string CalculatedMineralsJson { get; set; } = "[]";

    public SeasoningItem ToDomain()
    {
        var minerals = JsonSerializer.Deserialize<List<MineralAmount>>(CalculatedMineralsJson)
                       ?? new List<MineralAmount>();

        return new SeasoningItem(
            Id,
            FoodItemId,
            FoodName,
            Grams,
            CalculatedCalories,
            minerals,
            CalculatedProtein);
    }

    public static SeasoningItemEntity FromDomain(Guid seasoningId, SeasoningItem item)
    {
        return new SeasoningItemEntity
        {
            Id = item.Id,
            SeasoningId = seasoningId,
            FoodItemId = item.FoodItemId,
            FoodName = item.FoodName,
            Grams = item.Grams,
            CalculatedCalories = item.CalculatedCalories,
            CalculatedProtein = item.CalculatedProtein,
            CalculatedMineralsJson = JsonSerializer.Serialize(item.CalculatedMinerals)
        };
    }
}
