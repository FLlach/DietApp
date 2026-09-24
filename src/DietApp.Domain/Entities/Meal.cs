using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Raiz de Agregado (Aggregate Root) que representa una comida (desayuno, almuerzo, etc.)
/// registrada en una fecha especifica, compuesta por uno o varios alimentos consumidos.
/// Provee metodos para agregar alimentos y totalizar los minerales consumidos en esa sesion.
/// Por que se tomo esta decision: En DDD, el agregado asegura la consistencia de sus elementos hijos.
/// La suma total de compuestos y minerales debe ser calculada y validada por la entidad Meal
/// para garantizar coherencia en los reportes y conteos sin delegar logica contable a la interfaz.
/// </summary>
public class Meal
{
    private readonly List<MealItem> _items;

    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public MealType Type { get; private set; }
    public string Note { get; private set; }
    public IReadOnlyList<MealItem> Items => _items.AsReadOnly();

    public Meal(Guid id, DateTime date, MealType type, string note = "")
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Date = date;
        Type = type;
        Note = note?.Trim() ?? string.Empty;
        _items = new List<MealItem>();
    }

    /// <summary>
    /// Agrega un alimento consumido a la comida.
    /// </summary>
    public void AddItem(MealItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _items.Add(item);
    }

    /// <summary>
    /// Remueve un alimento consumido por su identificador.
    /// </summary>
    public bool RemoveItem(Guid itemId)
    {
        int index = _items.FindIndex(mealItem => mealItem.Id == itemId);
        if (index >= 0)
        {
            _items.RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calcula y consolida la suma total de cada mineral presente en todos los alimentos de la comida.
    /// </summary>
    public IReadOnlyList<MineralAmount> CalculateTotalMinerals()
    {
        var accumulatedMinerals = new Dictionary<MineralType, double>();

        foreach (var item in _items)
        {
            foreach (var mineral in item.CalculatedMinerals)
            {
                if (accumulatedMinerals.ContainsKey(mineral.Type))
                {
                    accumulatedMinerals[mineral.Type] += mineral.Milligrams;
                }
                else
                {
                    accumulatedMinerals[mineral.Type] = mineral.Milligrams;
                }
            }
        }

        var totals = new List<MineralAmount>(accumulatedMinerals.Count);
        foreach (var entry in accumulatedMinerals)
        {
            totals.Add(new MineralAmount(entry.Key, entry.Value));
        }

        return totals;
    }
}
