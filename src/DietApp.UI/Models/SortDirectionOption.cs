namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modela la direccion de ordenamiento (mayor a menor / menor a mayor) para controles de seleccion.
/// Por que se tomo esta decision: Permite una seleccion comoda en Pickers con soporte
/// completo de traduccion y sin valores magicos embebidos en el codigo.
/// </summary>
public class SortDirectionOption
{
    public bool IsDescending { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}
