namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de borrador para los ingredientes seleccionados al crear una receta culinaria.
/// Contiene el identificador del alimento, su nombre y los gramos dosificados.
/// Por que se tomo esta decision: Evita el uso de tuplas no tipadas en XAML, facilitando
/// Compiled Bindings y eliminando advertencias y errores del compilador XAML.
/// </summary>
public class RecipeIngredientDraftModel
{
    public Guid FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }

    public string DisplayText => $"{FoodName} - {Grams:F0} g";
}
