using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que representa una comida completa con su fecha, tipo (Desayuno, Almuerzo, etc.),
/// la lista de alimentos consumidos y la sumatoria total consolidada de minerales.
/// Por que se tomo esta decision: Presenta a la UI una estructura consolidada lista para renderizar
/// en pantallas de lista, detalles o graficos, evitando que la capa visual realice calculos o dependa
/// de metodos internos del agregado de dominio.
/// </summary>
public class MealDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public MealType Type { get; set; }
    public string MealTypeName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public List<MealItemDto> Items { get; set; } = new();
    public List<MineralAmountDto> TotalMinerals { get; set; } = new();

    /// <summary>
    /// Como funciona: Formatea el titulo descriptivo de la comida para el registro diario con su tipo,
    /// nota (si existe) y fecha (dd/MM/yyyy), omitiendo intencionalmente la hora (HH:mm) para no mostrar 00:00.
    /// Por que se tomo esta decision: El registro diario trabaja a nivel de fecha sin especificacion horaria,
    /// por lo que omitir la hora evita ruido visual y valores en cero innecesarios.
    /// </summary>
    public string TitleDisplay => string.IsNullOrWhiteSpace(Note)
        ? $"{MealTypeName} - {Date:dd/MM/yyyy}"
        : $"{MealTypeName} ({Note}) - {Date:dd/MM/yyyy}";
}
