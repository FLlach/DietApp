using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Modela un ingrediente especifico de una receta con referencia al alimento base,
/// la cantidad en gramos utilizada y la instantanea calculada de minerales y calorias.
/// Por que se tomo esta decision: Al igual que en las comidas, vincular el ingrediente con el gramaje
/// exacto y congelar la instantanea nutricional permite calcular el aporte exacto de compuestos
/// por porcion sin verse afectado si el alimento base es modificado posteriormente.
/// </summary>
public class RecipeIngredient
{
    public Guid Id { get; private set; }
    public Guid FoodItemId { get; private set; }
    public string FoodName { get; private set; }
    public double Grams { get; private set; }
    public double CalculatedCalories { get; private set; }
    public IReadOnlyList<MineralAmount> CalculatedMinerals { get; private set; }

    public RecipeIngredient(
        Guid id,
        Guid foodItemId,
        string foodName,
        double grams,
        double calculatedCalories,
        IReadOnlyList<MineralAmount> calculatedMinerals)
    {
        if (grams <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grams),
                "La cantidad de gramos del ingrediente debe ser mayor a cero.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FoodItemId = foodItemId;
        FoodName = string.IsNullOrWhiteSpace(foodName) ? "Ingrediente sin nombre" : foodName.Trim();
        Grams = grams;
        CalculatedCalories = calculatedCalories >= 0 ? calculatedCalories : 0;
        CalculatedMinerals = calculatedMinerals ?? Array.Empty<MineralAmount>();
    }

    /// <summary>
    /// Fabrica un RecipeIngredient a partir de un alimento del catalogo y el gramaje requerido.
    /// </summary>
    public static RecipeIngredient FromFoodItem(FoodItem foodItem, double grams)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        var minerals = foodItem.CalculateMineralsForPortion(grams);
        var calories = foodItem.CalculateCaloriesForPortion(grams);

        return new RecipeIngredient(
            Guid.NewGuid(),
            foodItem.Id,
            foodItem.Name,
            grams,
            calories,
            minerals);
    }
}
