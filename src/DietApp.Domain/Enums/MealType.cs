namespace DietApp.Domain.Enums;

/// <summary>
/// Como funciona: Define los momentos o categorias de ingesta a lo largo del dia.
/// Por que se tomo esta decision: Permite organizar y agregar el consumo de minerales
/// segmentado por momento alimentario (desayuno, almuerzo, etc.), facilitando
/// el analisis comparativo y la identificacion de patrones de ingesta.
/// </summary>
public enum MealType
{
    Breakfast = 1,
    Lunch = 2,
    Dinner = 3,
    Snack = 4,
    Other = 5
}
