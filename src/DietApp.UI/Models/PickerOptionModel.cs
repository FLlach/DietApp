namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Elemento para la lista de busqueda y seleccion rapida en el compositor de comidas.
/// Por que se tomo esta decision: Permite mostrar alimentos y recetas en un selector unificado con datos nutricionales clave.
/// </summary>
public class PickerOptionModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string TagText { get; set; } = string.Empty;
    public bool IsRecipe { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
}
