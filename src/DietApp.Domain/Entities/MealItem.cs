using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Representa un item o porcion individual de alimento o receta ingerido dentro de una comida.
/// Contiene una instantanea de los minerales calculados especificamente para los gramos o porciones consumidas.
/// Por que se tomo esta decision: Almacenar la instantanea calculada previene que modificaciones
/// posteriores en la definicion del alimento o receta en catalogo alteren retroactivamente el historial
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

    /// <summary>
    /// Fabrica un MealItem a partir de una receta culinaria y la cantidad de porciones consumidas.
    /// Calcula los minerales resultantes escalando el aporte por porcion de la receta.
    /// </summary>
    public static MealItem FromRecipe(Recipe recipe, double servingsConsumed)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        if (servingsConsumed <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(servingsConsumed),
                "La cantidad de porciones consumidas debe ser mayor a cero.");
        }

        double totalRecipeGrams = recipe.Ingredients.Sum(i => i.Grams);
        double consumedGrams = totalRecipeGrams > 0 
            ? totalRecipeGrams * (servingsConsumed / recipe.Servings)
            : 100.0 * servingsConsumed;

        var mineralsPerServing = recipe.CalculateMineralsPerServing();
        var scaledMinerals = new List<MineralAmount>(mineralsPerServing.Count);
        for (int i = 0; i < mineralsPerServing.Count; i++)
        {
            scaledMinerals.Add(mineralsPerServing[i].Scale(servingsConsumed));
        }

        string servingText = servingsConsumed == 1 ? "1 porcion" : $"{servingsConsumed:0.##} porciones";
        string foodName = $"{recipe.Title} ({servingText})";

        return new MealItem(
            Guid.NewGuid(),
            recipe.Id,
            foodName,
            consumedGrams,
            scaledMinerals);
    }
}
