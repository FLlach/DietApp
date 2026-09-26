using DietApp.Application.DTOs;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Application.Mapping;

/// <summary>
/// Como funciona: Clase estatica con funciones puras para transformar entidades de dominio a DTOs y viceversa.
/// Traduce tipos de minerales a nombres legibles en espanol y genera proyecciones completas para alimentos,
/// comidas y recetas culinarias.
/// Por que se tomo esta decision: Centraliza toda la logica de conversion y traduccion en un unico punto
/// de alta cohesion, evitando duplicacion de mapeos a lo largo de los casos de uso y manteniendo
/// el dominio libre de cadenas de localizacion de interfaz.
/// </summary>
public static class DomainDtoMapper
{
    public static string ToFriendlyName(this MineralType type)
    {
        bool isEnglish = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

        if (isEnglish)
        {
            return type switch
            {
                MineralType.Phosphorus => "Phosphorus",
                MineralType.Potassium => "Potassium",
                MineralType.Sodium => "Sodium",
                MineralType.Calcium => "Calcium",
                MineralType.Magnesium => "Magnesium",
                MineralType.Iron => "Iron",
                MineralType.Zinc => "Zinc",
                _ => type.ToString()
            };
        }

        return type switch
        {
            MineralType.Phosphorus => "Fosforo",
            MineralType.Potassium => "Potasio",
            MineralType.Sodium => "Sodio",
            MineralType.Calcium => "Calcio",
            MineralType.Magnesium => "Magnesio",
            MineralType.Iron => "Hierro",
            MineralType.Zinc => "Zinc",
            _ => type.ToString()
        };
    }

    public static string ToFriendlyName(this MealType type)
    {
        bool isEnglish = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

        if (isEnglish)
        {
            return type switch
            {
                MealType.Breakfast => "Breakfast",
                MealType.Lunch => "Lunch",
                MealType.Dinner => "Dinner",
                MealType.Snack => "Snack / Collation",
                MealType.Other => "Other",
                _ => type.ToString()
            };
        }

        return type switch
        {
            MealType.Breakfast => "Desayuno",
            MealType.Lunch => "Almuerzo",
            MealType.Dinner => "Cena",
            MealType.Snack => "Merienda / Colacion",
            MealType.Other => "Otro",
            _ => type.ToString()
        };
    }

    public static MineralAmountDto ToDto(this MineralAmount value)
    {
        return new MineralAmountDto
        {
            Type = value.Type,
            MineralName = value.Type.ToFriendlyName(),
            Milligrams = value.Milligrams,
            Unit = "mg"
        };
    }

    public static FoodItemDto ToDto(this FoodItem food)
    {
        return new FoodItemDto
        {
            Id = food.Id,
            Name = food.Name,
            Category = food.Category,
            ReferenceGrams = food.ReferenceGrams,
            Calories = food.Calories,
            ProteinGrams = food.ProteinGrams,
            Minerals = food.Minerals.Select(m => m.ToDto()).ToList()
        };
    }

    public static MealItemDto ToDto(this MealItem item)
    {
        return new MealItemDto
        {
            Id = item.Id,
            FoodItemId = item.FoodItemId,
            FoodName = item.FoodName,
            PortionInGrams = item.PortionInGrams,
            CalculatedCalories = item.CalculatedCalories,
            CalculatedProtein = item.CalculatedProtein,
            CalculatedMinerals = item.CalculatedMinerals.Select(m => m.ToDto()).ToList()
        };
    }

    public static MealDto ToDto(this Meal meal)
    {
        var totalMinerals = meal.CalculateTotalMinerals();

        return new MealDto
        {
            Id = meal.Id,
            Date = meal.Date,
            Type = meal.Type,
            MealTypeName = meal.Type.ToFriendlyName(),
            Note = meal.Note,
            Items = meal.Items.Select(i => i.ToDto()).ToList(),
            TotalMinerals = totalMinerals.Select(m => m.ToDto()).ToList()
        };
    }

    public static RecipeIngredientDto ToDto(this RecipeIngredient ingredient)
    {
        return new RecipeIngredientDto
        {
            Id = ingredient.Id,
            FoodItemId = ingredient.FoodItemId,
            FoodName = ingredient.FoodName,
            Grams = ingredient.Grams,
            CalculatedCalories = ingredient.CalculatedCalories,
            CalculatedProtein = ingredient.CalculatedProtein,
            CalculatedMinerals = ingredient.CalculatedMinerals.Select(m => m.ToDto()).ToList()
        };
    }

    public static RecipeStepDto ToDto(this RecipeStep step)
    {
        return new RecipeStepDto
        {
            StepNumber = step.StepNumber,
            Instruction = step.Instruction,
            ImagePath = step.ImagePath
        };
    }

    public static RecipeDto ToDto(this Recipe recipe)
    {
        var totalMinerals = recipe.CalculateTotalMinerals();
        var mineralsPerServing = recipe.CalculateMineralsPerServing();

        return new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            Servings = recipe.Servings,
            FinalImagePath = recipe.FinalImagePath,
            TotalCalories = recipe.CalculateTotalCalories(),
            CaloriesPerServing = recipe.CalculateCaloriesPerServing(),
            TotalProtein = recipe.CalculateTotalProtein(),
            ProteinPerServing = recipe.CalculateProteinPerServing(),
            TotalMinerals = totalMinerals.Select(m => m.ToDto()).ToList(),
            MineralsPerServing = mineralsPerServing.Select(m => m.ToDto()).ToList(),
            Ingredients = recipe.Ingredients.Select(i => i.ToDto()).ToList(),
            Steps = recipe.Steps.Select(s => s.ToDto()).ToList()
        };
    }

    public static SeasoningItemDto ToDto(this SeasoningItem item)
    {
        return new SeasoningItemDto
        {
            Id = item.Id,
            FoodItemId = item.FoodItemId,
            FoodName = item.FoodName,
            Grams = item.Grams,
            Calories = item.CalculatedCalories,
            Protein = item.CalculatedProtein,
            Minerals = item.CalculatedMinerals.Select(m => m.ToDto()).ToList()
        };
    }

    public static SeasoningDto ToDto(this Seasoning seasoning)
    {
        var totalMinerals = seasoning.CalculateTotalMinerals();

        return new SeasoningDto
        {
            Id = seasoning.Id,
            Name = seasoning.Name,
            Description = seasoning.Description,
            TotalGrams = seasoning.CalculateTotalGrams(),
            TotalCalories = seasoning.CalculateTotalCalories(),
            TotalProtein = seasoning.CalculateTotalProtein(),
            TotalMinerals = totalMinerals.Select(m => m.ToDto()).ToList(),
            Items = seasoning.Items.Select(i => i.ToDto()).ToList()
        };
    }
}
