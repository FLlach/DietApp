namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un alimento entre la logica de aplicacion y la interfaz.
/// Incluye la lista de minerales cuantificados por la porcion de referencia (por ejemplo 100g).
/// Por que se tomo esta decision: Permite a las vistas de MAUI consumir estructuras ligeras y planas,
/// evitando enlazar directamente las entidades ricas del dominio a controles de la vista.
/// </summary>
public class FoodItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double ReferenceGrams { get; set; } = 100.0;
    public List<MineralAmountDto> Minerals { get; set; } = new();

    public string SubtitleSummary => $"{Category} - Base {ReferenceGrams:F0}g";
}
