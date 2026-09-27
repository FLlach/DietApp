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

        // Buscar alimentos para la Receta ABCDE (mockupBase.jpeg)
        var foodPotatoes = availableFoods.FirstOrDefault(f => f.Name.Contains("Potato", StringComparison.OrdinalIgnoreCase) ||
                                                              f.Name.Contains("Papa", StringComparison.OrdinalIgnoreCase))
                           ?? availableFoods[0];

        var foodRice = availableFoods.FirstOrDefault(f => f.Name.Contains("Rice", StringComparison.OrdinalIgnoreCase) ||
                                                          f.Name.Contains("Arroz", StringComparison.OrdinalIgnoreCase))
                       ?? (availableFoods.Count > 1 ? availableFoods[1] : availableFoods[0]);

        var foodBroccoli = availableFoods.FirstOrDefault(f => f.Name.Contains("Broccoli", StringComparison.OrdinalIgnoreCase) ||
                                                              f.Name.Contains("Brocoli", StringComparison.OrdinalIgnoreCase))
                           ?? (availableFoods.Count > 2 ? availableFoods[2] : availableFoods[0]);

        var foodChicken = availableFoods.FirstOrDefault(f => f.Name.Contains("Chicken", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.Contains("Pollo", StringComparison.OrdinalIgnoreCase))
                          ?? (availableFoods.Count > 3 ? availableFoods[3] : availableFoods[0]);

        // Receta 1: Receta ABCDE (Vista A en mockupBase.jpeg y DESIGN.md)
        var recipe1 = new Recipe(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Receta ABCDE",
            "Plato equilibrado de pechuga de pollo, arroz al vapor, brocoli y papas doradas. 500 kcal por porcion.",
            2,
            "recipe_hero_dish.jpg");

        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(foodPotatoes, 200.0));
        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(foodRice, 200.0));
        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(foodBroccoli, 200.0));
        recipe1.AddIngredient(RecipeIngredient.FromFoodItem(foodChicken, 400.0));

        recipe1.AddStep(new RecipeStep(
            1,
            "Lavar y pelar las papas y cortar el brocoli en ramilletes regulares para coccion uniforme.",
            "food_potatoes.jpg"));

        recipe1.AddStep(new RecipeStep(
            2,
            "Cocinar el arroz blanco y dorar la pechuga de pollo a fuego medio hasta lograr punto jugoso.",
            "food_chicken.jpg"));

        recipe1.AddStep(new RecipeStep(
            3,
            "Emplatar la pechuga fileteada, la porcion de arroz en domo, brocoli al vapor y papas asadas.",
            "recipe_hero_dish.jpg"));

        recipes.Add(recipe1);

        // Receta 2: Receta ABCD (Modulo de resumen en Vista B)
        var recipe2 = new Recipe(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Receta ABCD",
            "Preparacion rica en potasio y proteinas para ingesta diaria balanceada con minerales controlados.",
            1,
            "recipe_hero_dish.jpg");

        recipe2.AddIngredient(RecipeIngredient.FromFoodItem(foodChicken, 200.0));
        recipe2.AddIngredient(RecipeIngredient.FromFoodItem(foodPotatoes, 150.0));

        recipe2.AddStep(new RecipeStep(
            1,
            "Hervir o cocinar los vegetales al vapor hasta que queden tiernos pero firmes.",
            "food_broccoli.jpg"));

        recipe2.AddStep(new RecipeStep(
            2,
            "Trocear la proteina principal y saltear ligeramente junto con el resto de ingredientes.",
            "food_chicken.jpg"));

        recipe2.AddStep(new RecipeStep(
            3,
            "Servir en cuencos individuales colocando los vegetales en la base y el pollo dorado encima.",
            "recipe_hero_dish.jpg"));

        recipes.Add(recipe2);

        return recipes;
    }
}
