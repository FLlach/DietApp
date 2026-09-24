using DietApp.Application.DTOs;
using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Application.Mapping;

/// <summary>
/// Como funciona: Clase estatica con funciones puras para transformar entidades de dominio a DTOs y viceversa.
/// Traduce tipos de minerales a nombres legibles en espanol para la presentacion al usuario.
/// Por que se tomo esta decision: Centraliza toda la logica de conversion y traduccion en un unico punto
/// de alta cohesion, evitando duplicacion de mapeos a lo largo de los casos de uso y manteniendo
/// el dominio libre de cadenas de localizacion de interfaz.
/// </summary>
public static class DomainDtoMapper
{
    public static string ToFriendlyName(this MineralType type)
    {
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
}
