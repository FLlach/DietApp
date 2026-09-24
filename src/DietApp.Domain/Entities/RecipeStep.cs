namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Modela un paso secuencial numerado dentro del procedimiento de elaboracion de una receta.
/// Permite asociar una instruccion explicativa y una imagen ilustrativa opcional de la etapa.
/// Por que se tomo esta decision: Mantener los pasos como entidad interna de la receta con numero
/// e imagen desacoplada garantiza que la vista pueda renderizar listas numeradas ordenadas
/// con soporte multimedia en cualquier dispositivo.
/// </summary>
public class RecipeStep
{
    public int StepNumber { get; private set; }
    public string Instruction { get; private set; }
    public string ImagePath { get; private set; }

    public RecipeStep(int stepNumber, string instruction, string imagePath = "")
    {
        if (stepNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stepNumber),
                "El numero del paso debe ser un entero positivo.");
        }

        if (string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentException("La instruccion del paso no puede estar vacia.", nameof(instruction));
        }

        StepNumber = stepNumber;
        Instruction = instruction.Trim();
        ImagePath = imagePath?.Trim() ?? string.Empty;
    }
}
