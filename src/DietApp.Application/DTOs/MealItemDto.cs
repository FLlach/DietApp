namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un alimento consumido dentro de una comida.
/// Incluye los gramos consumidos y el desglose de minerales resultantes para esa porcion.
/// Por que se tomo esta decision: Facilita que las vistas de resumen de comidas muestren
/// de manera directa el aporte especifico de cada elemento consumido.
/// </summary>
public class MealItemDto
{
    public Guid Id { get; set; }
    public Guid FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double PortionInGrams { get; set; }
    public double CalculatedCalories { get; set; }
    public double CalculatedProtein { get; set; }
    public List<MineralAmountDto> CalculatedMinerals { get; set; } = new();

    public string PortionSummary => $"{PortionInGrams:F0}g ({CalculatedCalories:F0} kcal, {CalculatedProtein:F1}g prot.)";
    public string ProteinSummary => $"{CalculatedProtein:F1} g prot.";
}
