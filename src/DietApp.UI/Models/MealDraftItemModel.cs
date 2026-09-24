namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion temporal para los alimentos que el usuario va agregando
/// a la comida antes de confirmarla.
/// Por que se tomo esta decision: Reemplaza el uso de tuplas no tipadas en XAML, facilitando el enlace
/// de datos compilado (Compiled Bindings) con x:DataType y eliminando advertencias y errores del compilador XAML.
/// </summary>
public class MealDraftItemModel
{
    public Guid FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }

    public string GramsDisplay => $"{Grams:F0} g";
}
