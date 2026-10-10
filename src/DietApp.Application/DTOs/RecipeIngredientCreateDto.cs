namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO para suministrar informacion de ingredientes al momento de crear o editar una receta.
/// Admite tanto alimentos vinculados por Id al catalogo general como ingredientes personalizados no registrados
/// en la base de datos (con sus aportes nutricionales directos).
/// Por que se tomo esta decision: Permite que las recetas admitan ingredientes ad-hoc y alinos
/// sin requerir que existan previamente en la tabla relacional de alimentos.
/// </summary>
public class RecipeIngredientCreateDto
{
    public Guid FoodId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public double Grams { get; set; } = 100.0;
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double SodiumMg { get; set; }
    public double PotassiumMg { get; set; }
    public double PhosphorusMg { get; set; }
}
