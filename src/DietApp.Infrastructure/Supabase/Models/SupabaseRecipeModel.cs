using DietApp.Domain.Entities;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `recipes` en Supabase PostgreSQL.
/// Permite almacenar recetas culinarias publicas o creadas por usuarios autenticados.
/// Por que se tomo esta decision: Permite sincronizar las recetas del usuario manteniendo compatibilidad RLS.
/// </summary>
[Table("recipes")]
public class SupabaseRecipeModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("servings")]
    public int Servings { get; set; } = 1;

    [Column("final_image_path")]
    public string FinalImagePath { get; set; } = string.Empty;

    public Recipe ToDomain(IEnumerable<RecipeIngredient> ingredients, IEnumerable<RecipeStep> steps)
    {
        Guid.TryParse(Id, out var parsedId);
        return new Recipe(
            parsedId == Guid.Empty ? Guid.NewGuid() : parsedId,
            Title,
            Description,
            Servings,
            FinalImagePath,
            ingredients,
            steps);
    }

    public static SupabaseRecipeModel FromDomain(Recipe recipe, string? userId = null)
    {
        return new SupabaseRecipeModel
        {
            Id = recipe.Id.ToString(),
            UserId = userId,
            Title = recipe.Title,
            Description = recipe.Description,
            Servings = recipe.Servings,
            FinalImagePath = recipe.FinalImagePath
        };
    }
}
