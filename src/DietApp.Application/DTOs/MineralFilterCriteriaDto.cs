using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: Modela los criterios de busqueda y filtrado avanzado para alimentos
/// en base a umbrales de minerales (como fosforo, potasio o sodio) y texto libre.
/// Por que se tomo esta decision: Agrupa todos los parametros de filtrado en un unico
/// contrato de entrada, evitando firmas de metodos con demasiados argumentos y facilitando
/// la persistencia o enlace bidireccional desde la interfaz de usuario.
/// </summary>
public class MineralFilterCriteriaDto
{
    public MineralType? SelectedMineral { get; set; }
    public bool FilterByProtein { get; set; }
    public double? MinimumMilligrams { get; set; }
    public double? MaximumMilligrams { get; set; }
    public double? MinimumProteinGrams { get; set; }
    public double? MaximumProteinGrams { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string? SortBy { get; set; }
}
