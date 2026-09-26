using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Modela un ingrediente o condimento elemental dentro de una mezcla o alino (aceites, hierbas, sales, vinagres, especias).
/// Contiene la referencia al alimento base del catalogo, los gramos utilizados y la instantanea calculada de minerales y calorias.
/// Por que se tomo esta decision: En DDD, cada item de alino congela sus valores nutricionales segun el gramaje especificado,
/// permitiendo calcular el aporte exacto del condimento y trasladarlo fielmente al agregarse a recetas culinarias.
/// </summary>
public class SeasoningItem
{
    public Guid Id { get; private set; }
    public Guid FoodItemId { get; private set; }
    public string FoodName { get; private set; }
    public double Grams { get; private set; }
    public double CalculatedCalories { get; private set; }
    public double CalculatedProtein { get; private set; }
    public IReadOnlyList<MineralAmount> CalculatedMinerals { get; private set; }

    public SeasoningItem(
        Guid id,
        Guid foodItemId,
        string foodName,
        double grams,
        double calculatedCalories,
        IReadOnlyList<MineralAmount> calculatedMinerals,
        double calculatedProtein = 0.0)
    {
        if (grams <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grams),
                "La cantidad en gramos del ingrediente del alino debe ser mayor a cero.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FoodItemId = foodItemId;
        FoodName = string.IsNullOrWhiteSpace(foodName) ? "Condimento sin nombre" : foodName.Trim();
        Grams = grams;
        CalculatedCalories = calculatedCalories >= 0 ? calculatedCalories : 0;
        CalculatedProtein = calculatedProtein >= 0 ? calculatedProtein : 0;
        CalculatedMinerals = calculatedMinerals ?? Array.Empty<MineralAmount>();
    }

    /// <summary>
    /// Fabrica un SeasoningItem a partir de un alimento del catalogo y el gramaje requerido.
    /// </summary>
    public static SeasoningItem FromFoodItem(FoodItem foodItem, double grams)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        var minerals = foodItem.CalculateMineralsForPortion(grams);
        var calories = foodItem.CalculateCaloriesForPortion(grams);
        var protein = foodItem.CalculateProteinForPortion(grams);

        return new SeasoningItem(
            Guid.NewGuid(),
            foodItem.Id,
            foodItem.Name,
            grams,
            calories,
            minerals,
            protein);
    }

    /// <summary>
    /// Consulta los miligramos aportados de un mineral especifico por este componente del alino.
    /// </summary>
    public double GetMineralAmount(MineralType mineralType)
    {
        foreach (var mineral in CalculatedMinerals)
        {
            if (mineral.Type == mineralType)
            {
                return mineral.Milligrams;
            }
        }
        return 0.0;
    }
}
