using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta un alino completo con su nombre, descripcion, lista de componentes,
/// peso total en gramos, calorias acumuladas y balance total de minerales.
/// Por que se tomo esta decision: Permite alimentar la pantalla de gestion de alinos y condimentos,
/// asi como los selectores para incorporacion directa en el asistente de creacion de recetas.
/// </summary>
public class SeasoningDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double TotalGrams { get; set; }
    public double TotalCalories { get; set; }
    public List<SeasoningItemDto> Items { get; set; } = new();
    public List<MineralAmountDto> TotalMinerals { get; set; } = new();

    public int ItemsCount => Items?.Count ?? 0;

    /// <summary>
    /// Indica si el alino cuenta con una descripcion no vacia para visibilidad reactiva en la interfaz.
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    /// <summary>
    /// Resumen sintetico de ingredientes y gramos totales para badges y pickers.
    /// </summary>
    public string SummaryDisplayText
    {
        get
        {
            return $"{Name} ({TotalGrams:F0}g | {TotalCalories:F0} kcal)";
        }
    }

    /// <summary>
    /// Resumen legible de los minerales mas significativos del alino.
    /// </summary>
    public string MineralsSummaryText
    {
        get
        {
            var parts = TotalMinerals
                .Where(m => m.Milligrams > 0.05)
                .Take(4)
                .Select(m => $"{m.MineralName}: {m.Milligrams:F0}mg");

            string joined = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(joined) ? "Sin minerales registrados" : joined;
        }
    }
}
