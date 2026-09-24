namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO para transportar datos de un paso numerado de elaboracion de receta,
/// incluyendo la instruccion textual y la ruta o URI de la imagen ilustrativa del paso.
/// Por que se tomo esta decision: Permite a las vistas de .NET MAUI enlazar directamente
/// el numero, texto e imagen del paso sin exponer detalles de entidad del dominio.
/// </summary>
public class RecipeStepDto
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

    public string StepTitle => $"Paso {StepNumber}";
}
