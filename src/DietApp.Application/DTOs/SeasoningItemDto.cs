using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que expone los datos de un ingrediente o condimento de un alino.
/// Incluye gramos, calorias y el listado de minerales aportados.
/// Por que se tomo esta decision: Permite a las vistas renderizar el desglose de componentes
/// y transferir directamente estos datos al borrador de ingredientes de una receta.
/// </summary>
public class SeasoningItemDto
{
    public Guid Id { get; set; }
    public Guid FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }
    public double Calories { get; set; }
    public List<MineralAmountDto> Minerals { get; set; } = new();

    public string DisplayText => $"{FoodName} ({Grams:F0}g)";
}
