using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un ingrediente utilizado en una receta culinaria,
/// conteniendo los gramos requeridos y el desglose de calorias y minerales aportados.
/// Por que se tomo esta decision: Permite a las vistas de detalle de receta mostrar claramente
/// la cantidad en gramos y el aporte energetico individual de cada ingrediente del plato.
/// </summary>
public class RecipeIngredientDto
{
    public Guid Id { get; set; }
    public Guid FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }
    public double CalculatedCalories { get; set; }
    public double CalculatedProtein { get; set; }
    public List<MineralAmountDto> CalculatedMinerals { get; set; } = new();

    public string DisplayText => $"{FoodName}: {Grams:F0}g ({CalculatedCalories:F0} kcal, {CalculatedProtein:F1}g prot.)";

    public string MineralsSummary
    {
        get
        {
            var parts = CalculatedMinerals
                .Where(m => m.Milligrams > 0)
                .Select(m => $"{m.MineralName}: {m.Milligrams:F0}mg");
            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Como funciona: Retorna la cantidad en miligramos de un mineral especifico aportado por este ingrediente.
    /// Por que se tomo esta decision: Facilita el ordenamiento de los ingredientes en la pantalla de detalle.
    /// </summary>
    public double GetMineralAmount(MineralType mineralType)
    {
        var item = CalculatedMinerals.FirstOrDefault(m => m.Type == mineralType);
        return item?.Milligrams ?? 0.0;
    }
}
