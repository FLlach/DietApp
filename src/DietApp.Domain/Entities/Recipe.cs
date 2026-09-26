using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Raiz de Agregado (Aggregate Root) que representa una receta culinaria completa.
/// Contiene el titulo, descripcion, numero de porciones, imagen final de la receta terminada,
/// la coleccion de ingredientes dosificados y los pasos secuenciales numerados con imagenes por etapa.
/// Provee metodos para calcular el aporte total y por porcion de minerales y calorias.
/// Por que se tomo esta decision: En DDD, la entidad Recipe asegura que el computo de minerales
/// y calorias por porcion sea matematicamente riguroso y centralizado, asegurando que cualquier
/// cambio en las porciones o ingredientes actualice de inmediato los subtitulos nutricionales.
/// </summary>
public class Recipe
{
    private readonly List<RecipeIngredient> _ingredients;
    private readonly List<RecipeStep> _steps;

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Servings { get; private set; }
    public string FinalImagePath { get; private set; }
    public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();
    public IReadOnlyList<RecipeStep> Steps => _steps.AsReadOnly();

    public Recipe(
        Guid id,
        string title,
        string description,
        int servings,
        string finalImagePath = "",
        IEnumerable<RecipeIngredient>? ingredients = null,
        IEnumerable<RecipeStep>? steps = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("El titulo de la receta no puede estar vacio.", nameof(title));
        }

        if (servings <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(servings), "El numero de porciones debe ser al menos 1.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Servings = servings;
        FinalImagePath = finalImagePath?.Trim() ?? string.Empty;
        _ingredients = ingredients?.ToList() ?? new List<RecipeIngredient>();
        _steps = steps?.OrderBy(s => s.StepNumber).ToList() ?? new List<RecipeStep>();
    }

    public void AddIngredient(RecipeIngredient ingredient)
    {
        if (ingredient == null)
        {
            throw new ArgumentNullException(nameof(ingredient));
        }

        _ingredients.Add(ingredient);
    }

    public void AddStep(RecipeStep step)
    {
        if (step == null)
        {
            throw new ArgumentNullException(nameof(step));
        }

        _steps.Add(step);
    }

    /// <summary>
    /// Calcula las calorias totales de la receta completa.
    /// </summary>
    public double CalculateTotalCalories()
    {
        double total = 0.0;
        foreach (var ingredient in _ingredients)
        {
            total += ingredient.CalculatedCalories;
        }

        return total;
    }

    /// <summary>
    /// Calcula las calorias por porcion individual dividiendo el total por el numero de porciones.
    /// </summary>
    public double CalculateCaloriesPerServing()
    {
        return CalculateTotalCalories() / Servings;
    }

    /// <summary>
    /// Calcula los gramos de proteina totales de la receta completa.
    /// </summary>
    public double CalculateTotalProtein()
    {
        double total = 0.0;
        foreach (var ingredient in _ingredients)
        {
            total += ingredient.CalculatedProtein;
        }

        return total;
    }

    /// <summary>
    /// Calcula los gramos de proteina por porcion individual dividiendo el total por el numero de porciones.
    /// </summary>
    public double CalculateProteinPerServing()
    {
        return Servings > 0 ? CalculateTotalProtein() / Servings : 0.0;
    }

    /// <summary>
    /// Calcula los minerales totales acumulados por todos los ingredientes de la receta.
    /// </summary>
    public IReadOnlyList<MineralAmount> CalculateTotalMinerals()
    {
        var accumulated = new Dictionary<MineralType, double>();

        foreach (var ingredient in _ingredients)
        {
            foreach (var mineral in ingredient.CalculatedMinerals)
            {
                if (accumulated.ContainsKey(mineral.Type))
                {
                    accumulated[mineral.Type] += mineral.Milligrams;
                }
                else
                {
                    accumulated[mineral.Type] = mineral.Milligrams;
                }
            }
        }

        var results = new List<MineralAmount>(accumulated.Count);
        foreach (var entry in accumulated)
        {
            results.Add(new MineralAmount(entry.Key, entry.Value));
        }

        return results;
    }

    /// <summary>
    /// Calcula la cantidad de cada mineral por porcion individual.
    /// </summary>
    public IReadOnlyList<MineralAmount> CalculateMineralsPerServing()
    {
        var totalMinerals = CalculateTotalMinerals();
        double ratio = 1.0 / Servings;

        var perServing = new List<MineralAmount>(totalMinerals.Count);
        foreach (var mineral in totalMinerals)
        {
            perServing.Add(mineral.Scale(ratio));
        }

        return perServing;
    }

    /// <summary>
    /// Como funciona: Consulta la cantidad en miligramos de un mineral especifico por porcion individual.
    /// Por que se tomo esta decision: Permite ordenar y comparar recetas por contenido mineral en el dominio.
    /// </summary>
    public double GetMineralAmountPerServing(MineralType mineralType)
    {
        var minerals = CalculateMineralsPerServing();
        foreach (var mineral in minerals)
        {
            if (mineral.Type == mineralType)
            {
                return mineral.Milligrams;
            }
        }
        return 0.0;
    }
}
