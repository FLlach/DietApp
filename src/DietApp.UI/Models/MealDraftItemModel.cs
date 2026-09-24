namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion temporal para alimentos individuales o recetas completas
/// que el usuario agrega a la comida en composicion antes de confirmarla.
/// Por que se tomo esta decision: Permite una representacion unificada en la interfaz grafica,
/// soportando porciones de recetas (en unidades de porcion) y alimentos del catalogo (en gramos),
/// manteniendo compatibilidad con enlaces compilados (x:DataType) en XAML.
/// </summary>
public class MealDraftItemModel
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public bool IsRecipe { get; set; }

    public Guid FoodId
    {
        get => ItemId;
        set => ItemId = value;
    }

    public string FoodName
    {
        get => ItemName;
        set => ItemName = value;
    }

    public double Grams
    {
        get => Quantity;
        set => Quantity = value;
    }

    public string QuantityDisplay => IsRecipe
        ? (Quantity == 1 ? "1 porcion" : $"{Quantity:0.##} porciones")
        : $"{Quantity:F0} g";

    public string GramsDisplay => QuantityDisplay;
}
