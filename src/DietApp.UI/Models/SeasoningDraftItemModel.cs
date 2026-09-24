namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de borrador para los ingredientes o condimentos agregados durante la creacion de un alino.
/// Almacena el identificador del alimento, su nombre y la cantidad en gramos requerida.
/// Por que se tomo esta decision: Permite usar Compiled Bindings tipados en las vistas XAML sin depender de tuplas,
/// simplificando la interaccion y garantizando enlace reactivo de datos.
/// </summary>
public class SeasoningDraftItemModel
{
    public Guid FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }

    public string DisplayText => $"{FoodName} - {Grams:F0} g";
}
