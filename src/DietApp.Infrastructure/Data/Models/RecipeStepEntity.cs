using DietApp.Domain.Entities;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para cada paso numerado de elaboracion de una receta.
/// Almacena el numero secuencial del paso, la instruccion y la ruta local de la imagen de la etapa.
/// Por que se tomo esta decision: Permite ordenar los pasos de forma confiable y asociar imagenes
/// ilustrativas persistidas localmente en el dispositivo.
/// </summary>
[Table("RecipeSteps")]
public class RecipeStepEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid RecipeId { get; set; }

    public int StepNumber { get; set; }

    public string Instruction { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public RecipeStep ToDomain()
    {
        return new RecipeStep(StepNumber, Instruction, ImagePath);
    }

    public static RecipeStepEntity FromDomain(Guid recipeId, RecipeStep step)
    {
        return new RecipeStepEntity
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            StepNumber = step.StepNumber,
            Instruction = step.Instruction,
            ImagePath = step.ImagePath
        };
    }
}
