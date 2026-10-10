using DietApp.Domain.Entities;

namespace DietApp.Infrastructure.SeedData;

/// <summary>
/// Como funciona: Define el proveedor de recetas iniciales para el catalogo.
/// Por que se tomo esta decision: Al descartar las recetas de prueba del catalogo,
/// devuelve una coleccion vacia para que el sistema inicie limpio sin preparaciones de muestra.
/// </summary>
public static class InitialRecipeSeed
{
    public static List<Recipe> GetPreloadedRecipes(IReadOnlyList<FoodItem> availableFoods)
    {
        return new List<Recipe>();
    }
}
