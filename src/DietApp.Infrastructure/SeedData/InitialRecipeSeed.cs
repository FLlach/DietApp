using DietApp.Domain.Entities;

namespace DietApp.Infrastructure.SeedData;

/// <summary>
/// Como funciona: Provee un catalogo semilla de recetas culinarias con ingredientes dosificados,
/// pasos secuenciales numerados con soporte de imagenes y calculo automatico de minerales y calorias.
/// Por que se tomo esta decision: Permite validar inmediatamente la visualizacion del listado,
/// la navegacion por pasos y la precision del subtitulo nutricional por porcion.
/// </summary>
public static class InitialRecipeSeed
{
    public static List<Recipe> GetPreloadedRecipes(IReadOnlyList<FoodItem> availableFoods)
    {
        var foodMap = availableFoods.ToDictionary(f => f.Name);

        var recipes = new List<Recipe>();

        // Receta 1: Ensalada de Salmon, Espinacas y Parmesano
        if (foodMap.TryGetValue("Salmon fresco a la plancha", out var salmon) &&
            foodMap.TryGetValue("Espinaca fresca cruda", out var espinaca) &&
            foodMap.TryGetValue("Queso parmesano", out var queso))
        {
            var recipe1 = new Recipe(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Ensalada Vital de Salmon y Espinacas",
                "Plato equilibrado rico en potasio, fosforo y proteinas de alto valor biologico, ideal para almuerzos nutritivos.",
                2,
                "dotnet_bot.png");

            recipe1.AddIngredient(RecipeIngredient.FromFoodItem(salmon, 200.0));
            recipe1.AddIngredient(RecipeIngredient.FromFoodItem(espinaca, 120.0));
            recipe1.AddIngredient(RecipeIngredient.FromFoodItem(queso, 30.0));

            recipe1.AddStep(new RecipeStep(
                1,
                "Lavar meticulosamente las hojas de espinaca fresca y secarlas bien con centrifugadora o papel absorbente.",
                "dotnet_bot.png"));

            recipe1.AddStep(new RecipeStep(
                2,
                "Cocinar los filetes de salmon a la plancha a fuego medio durante 3 minutos por lado hasta lograr un dorado uniforme.",
                "dotnet_bot.png"));

            recipe1.AddStep(new RecipeStep(
                3,
                "Disponer la espinaca como base, incorporar el salmon tibio troceado y coronar con las lascas de queso parmesano.",
                "dotnet_bot.png"));

            recipes.Add(recipe1);
        }

        // Receta 2: Bowl Rústico de Pollo, Lentejas y Papa
        if (foodMap.TryGetValue("Pechuga de pollo cocida", out var pollo) &&
            foodMap.TryGetValue("Lentejas cocidas", out var lentejas) &&
            foodMap.TryGetValue("Papa hervida sin cascara", out var papa))
        {
            var recipe2 = new Recipe(
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                "Bowl Calido de Pollo, Lentejas y Papa",
                "Preparacion reconstituyente con aporte sostenido de energia, potasio y hierro para recuperacion muscular.",
                2,
                "dotnet_bot.png");

            recipe2.AddIngredient(RecipeIngredient.FromFoodItem(pollo, 250.0));
            recipe2.AddIngredient(RecipeIngredient.FromFoodItem(lentejas, 200.0));
            recipe2.AddIngredient(RecipeIngredient.FromFoodItem(papa, 150.0));

            recipe2.AddStep(new RecipeStep(
                1,
                "Hervir la papa sin cascara hasta que quede tierna pero firme y cortarla en dados regulares.",
                "dotnet_bot.png"));

            recipe2.AddStep(new RecipeStep(
                2,
                "Trocear la pechuga de pollo previamente cocida y saltear ligeramente junto con las lentejas para integrar sabores.",
                "dotnet_bot.png"));

            recipe2.AddStep(new RecipeStep(
                3,
                "Servir en cuencos individuales colocando las lentejas y papas en la base y el pollo dorado en la superficie.",
                "dotnet_bot.png"));

            recipes.Add(recipe2);
        }

        return recipes;
    }
}
