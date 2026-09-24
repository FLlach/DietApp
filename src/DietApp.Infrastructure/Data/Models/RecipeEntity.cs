using DietApp.Domain.Entities;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para el almacenamiento de recetas culinarias.
/// Almacena titulo, descripcion, porciones y la imagen final de la receta terminada.
/// Por que se tomo esta decision: Permite consultar el catalogo de recetas rapidamente
/// y enlazar los ingredientes y pasos numerados asociados mediante relaciones foraneas.
/// </summary>
[Table("Recipes")]
public class RecipeEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Servings { get; set; } = 1;

    public string FinalImagePath { get; set; } = string.Empty;

    public Recipe ToDomain(IEnumerable<RecipeIngredient> ingredients, IEnumerable<RecipeStep> steps)
    {
        return new Recipe(
            Id,
            Title,
            Description,
            Servings,
            FinalImagePath,
            ingredients,
            steps);
    }

    public static RecipeEntity FromDomain(Recipe recipe)
    {
        return new RecipeEntity
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            Servings = recipe.Servings,
            FinalImagePath = recipe.FinalImagePath
        };
    }
}
