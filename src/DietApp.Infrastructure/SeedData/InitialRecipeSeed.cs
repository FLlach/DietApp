using DietApp.Domain.Entities;

namespace DietApp.Infrastructure.SeedData;

/// <summary>
/// Como funciona: Provee un catalogo semilla de recetas culinarias con ingredientes dosificados,
/// pasos secuenciales numerados con soporte de imagenes y calculo automatico de minerales y calorias.
/// Por que se tomo esta decision: Permite validar inmediatamente la visualizacion del listado,
/// la navegacion por pasos y la precision del subtitulo nutricional por porcion, resolviendo
/// los ingredientes disponibles de forma flexible tanto en catalogo importado como precargado.
/// </summary>
public static class InitialRecipeSeed
{
    public static List<Recipe> GetPreloadedRecipes(IReadOnlyList<FoodItem> availableFoods)
    {
        var recipes = new List<Recipe>();
        if (availableFoods == null || availableFoods.Count == 0)
        {
            return recipes;
        }

        // Buscar alimentos por coincidencia o fallback a los primeros disponibles
        var food1 = availableFoods.FirstOrDefault(f => f.Name.Contains("Salmon", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("fish", StringComparison.OrdinalIgnoreCase))
                    ?? availableFoods[0];

        var food2 = availableFoods.FirstOrDefault(f => f.Name.Contains("Spinach", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Espinaca", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Beans", StringComparison.OrdinalIgnoreCase))
                    ?? (availableFoods.Count > 1 ? availableFoods[1] : availableFoods[0]);

        var food3 = availableFoods.FirstOrDefault(f => f.Name.Contains("Cheese", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Queso", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Flour", StringComparison.OrdinalIgnoreCase))
                    ?? (availableFoods.Count > 2 ? availableFoods[2] : availableFoods[0]);

        // Receta 1: Preparacion Nutritiva Rica en Potasio y Fosforo
        var recipe1 = new Recipe(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Ensalada Vital Mediterranea",
            "Plato equilibrado rico en potasio, fosforo y proteinas de alto valor biologico, ideal para almuerzos nutritivos.",
            2,
            "dotnet_bot.png");

        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(food1, 150.0));
        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(food2, 100.0));
        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(food3, 30.0));

        recipe1.AddStep(new RecipeStep(
            1,
            "Lavar meticulosamente los ingredientes frescos y secar con centrifugadora o papel absorbente.",
            "dotnet_bot.png"));

        recipe1.AddStep(new RecipeStep(
            2,
            "Cocinar a fuego medio en plancha antiadherente durante 3 minutos por lado hasta lograr un dorado uniforme.",
            "dotnet_bot.png"));

        recipe1.AddStep(new RecipeStep(
            3,
            "Disponer los vegetales como base, incorporar la proteina troceada y coronar con las lascas de queso.",
            "dotnet_bot.png"));

        recipes.Add(recipe1);

        // Receta 2: Salteado Energetico con Legumbres
        var food4 = availableFoods.FirstOrDefault(f => f.Name.Contains("Chicken", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Pollo", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Beef", StringComparison.OrdinalIgnoreCase))
                    ?? (availableFoods.Count > 3 ? availableFoods[3] : availableFoods[0]);

        var food5 = availableFoods.FirstOrDefault(f => f.Name.Contains("Potato", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Papa", StringComparison.OrdinalIgnoreCase) ||
                                                       f.Name.Contains("Tomato", StringComparison.OrdinalIgnoreCase))
                    ?? (availableFoods.Count > 4 ? availableFoods[4] : availableFoods[0]);

        var recipe2 = new Recipe(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Bowl Calido Energetico",
            "Preparacion reconstituyente con aporte sostenido de energia, potasio y hierro para recuperacion muscular.",
            2,
            "dotnet_bot.png");

        recipe2.AddIngredient(RecipeIngredient.FromFoodItem(food4, 200.0));
        recipe2.AddIngredient(RecipeIngredient.FromFoodItem(food5, 150.0));

        recipe2.AddStep(new RecipeStep(
            1,
            "Hervir o cocinar los vegetales al vapor hasta que queden tiernos pero firmes y cortar en dados regulares.",
            "dotnet_bot.png"));

        recipe2.AddStep(new RecipeStep(
            2,
            "Trocear la proteina principal y saltear ligeramente junto con el resto de ingredientes para integrar sabores.",
            "dotnet_bot.png"));

        recipe2.AddStep(new RecipeStep(
            3,
            "Servir en cuencos individuales colocando los vegetales en la base y el salteado en la superficie.",
            "dotnet_bot.png"));

        recipes.Add(recipe2);

        return recipes;
    }
}
