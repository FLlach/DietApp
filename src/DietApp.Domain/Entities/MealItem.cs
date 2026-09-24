using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Representa un item o porcion individual de alimento ingerido dentro de una comida.
/// Contiene una instantanea de los minerales calculados especificamente para los gramos consumidos.
/// Por que se tomo esta decision: Almacenar la instantanea calculada previene que modificaciones
/// posteriores en la definicion del alimento en catalogo alteren retroactivamente el historial
/// nutricional de comidas registradas en fechas anteriores.
/// </summary>
public class MealItem
{
    public Guid Id { get; private set; }
    public Guid FoodItemId { get; private set; }
    public string FoodName { get; private set; }
    public double PortionInGrams { get; private set; }
    public IReadOnlyList<MineralAmount> CalculatedMinerals { get; private set; }

    public MealItem(
        Guid id,
        Guid foodItemId,
        string foodName,
        double portionInGrams,
        IReadOnlyList<MineralAmount> calculatedMinerals)
    {
        if (portionInGrams <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(portionInGrams),
                "La porcion consumida en gramos debe ser mayor a cero.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FoodItemId = foodItemId;
        FoodName = string.IsNullOrWhiteSpace(foodName) ? "Alimento sin nombre" : foodName.Trim();
        PortionInGrams = portionInGrams;
        CalculatedMinerals = calculatedMinerals ?? Array.Empty<MineralAmount>();
    }

    /// <summary>
    /// Fabrica un MealItem a partir de un alimento del catalogo y el gramaje consumido.
    /// </summary>
    public static MealItem FromFoodItem(FoodItem foodItem, double portionInGrams)
    {
        if (foodItem == null)
        {
            throw new ArgumentNullException(nameof(foodItem));
        }

        var minerals = foodItem.CalculateMineralsForPortion(portionInGrams);

        return new MealItem(
            Guid.NewGuid(),
            foodItem.Id,
            foodItem.Name,
            portionInGrams,
            minerals);
    }
}
