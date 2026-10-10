using DietApp.Domain.Entities;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `recipe_steps` en Supabase PostgreSQL.
/// Representa cada paso numerado e instruccion de preparacion de una receta.
/// Por que se tomo esta decision: Permite recuperar los pasos de recetas sincronizados en la nube.
/// </summary>
[Table("recipe_steps")]
public class SupabaseRecipeStepModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("recipe_id")]
    public string RecipeId { get; set; } = string.Empty;

    [Column("step_number")]
    public int StepNumber { get; set; }

    [Column("instruction")]
    public string Instruction { get; set; } = string.Empty;

    [Column("image_path")]
    public string ImagePath { get; set; } = string.Empty;

    public RecipeStep ToDomain()
    {
        return new RecipeStep(
            StepNumber <= 0 ? 1 : StepNumber,
            string.IsNullOrWhiteSpace(Instruction) ? "Paso de preparación" : Instruction,
            ImagePath ?? string.Empty);
    }

    public static SupabaseRecipeStepModel FromDomain(Guid recipeId, RecipeStep step)
    {
        return new SupabaseRecipeStepModel
        {
            Id = Guid.NewGuid().ToString(),
            RecipeId = recipeId.ToString(),
            StepNumber = step.StepNumber,
            Instruction = step.Instruction,
            ImagePath = step.ImagePath
        };
    }
}
