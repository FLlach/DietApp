using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Raiz de Agregado (Aggregate Root) que representa una preparacion de alino, aderezo o mezcla de condimentos.
/// Agrupa un conjunto de ingredientes con sus respectivas proporciones en gramos y consolida el calculo total
/// de calorias y balance de minerales.
/// Por que se tomo esta decision: En DDD, encapsular la preparacion del alino en una entidad de dominio permite reutilizar
/// combinaciones frecuentes (vinagretas, adobos, mezclas de especias) al disenar recetas sin tener que ingresar
/// repetitivamente cada condimento de forma individual.
/// </summary>
public class Seasoning
{
    private readonly List<SeasoningItem> _items;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyList<SeasoningItem> Items => _items.AsReadOnly();

    public Seasoning(
        Guid id,
        string name,
        string description = "",
        IEnumerable<SeasoningItem>? items = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del alino no puede estar vacio.", nameof(name));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        _items = items?.ToList() ?? new List<SeasoningItem>();
    }

    public void AddItem(SeasoningItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        _items.RemoveAll(i => i.Id == itemId);
    }

    public double CalculateTotalGrams()
    {
        double totalGrams = 0.0;
        foreach (var item in _items)
        {
            totalGrams += item.Grams;
        }
        return totalGrams;
    }

    public double CalculateTotalCalories()
    {
        double totalCalories = 0.0;
        foreach (var item in _items)
        {
            totalCalories += item.CalculatedCalories;
        }
        return totalCalories;
    }

    public double CalculateTotalProtein()
    {
        double totalProtein = 0.0;
        foreach (var item in _items)
        {
            totalProtein += item.CalculatedProtein;
        }
        return totalProtein;
    }

    public IReadOnlyList<MineralAmount> CalculateTotalMinerals()
    {
        var accumulated = new Dictionary<MineralType, double>();

        foreach (var item in _items)
        {
            foreach (var mineral in item.CalculatedMinerals)
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

    public double GetMineralAmount(MineralType mineralType)
    {
        var minerals = CalculateTotalMinerals();
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
