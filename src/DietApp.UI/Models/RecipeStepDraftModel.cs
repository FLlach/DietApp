namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de borrador para los pasos de una receta en la pantalla de creacion.
/// Contiene el numero secuencial del paso, la instruccion y la ruta de la imagen seleccionada.
/// Por que se tomo esta decision: Permite manipular la lista de pasos en el AddRecipeViewModel
/// y enlazar los datos fuertemente tipados a la vista XAML sin acoplarse a las entidades de dominio.
/// </summary>
public class RecipeStepDraftModel
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

    public string StepTitle => $"Paso {StepNumber}";
}
