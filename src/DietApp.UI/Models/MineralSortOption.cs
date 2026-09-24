using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modela una opcion seleccionable para ordenar colecciones (recetas o ingredientes) por mineral.
/// Permite tener un valor nulo para indicar el orden por defecto y una etiqueta visual traducida.
/// Por que se tomo esta decision: Desacopla la logica de ordenamiento del texto presentado al usuario,
/// facilitando el soporte multilingue y garantizando tipado fuerte en ViewModels y vistas sin strings magicos.
/// </summary>
public class MineralSortOption
{
    public MineralType? Mineral { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}
