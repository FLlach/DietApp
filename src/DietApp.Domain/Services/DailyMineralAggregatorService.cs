using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Services;

/// <summary>
/// Como funciona: Servicio de dominio que recibe un conjunto de comidas y consolida la suma
/// de todos los minerales consumidos a lo largo de un periodo (ej. un dia completo).
/// Por que se tomo esta decision: En DDD, las operaciones que involucran multiples agregados
/// (varias instancias de Meal) y no pertenecen naturalmente a una sola entidad deben residir
/// en un Domain Service, manteniendo la alta cohesion y evitando acoplar entidades entre si.
/// </summary>
public class DailyMineralAggregatorService
{
    /// <summary>
    /// Calcula el total consolidado de minerales para una coleccion de comidas.
    /// </summary>
    public IReadOnlyList<MineralAmount> AggregateMinerals(IEnumerable<Meal> meals)
    {
        if (meals == null)
        {
            return Array.Empty<MineralAmount>();
        }

        var accumulatedMinerals = new Dictionary<MineralType, double>();

        foreach (var meal in meals)
        {
            var mealTotals = meal.CalculateTotalMinerals();
            foreach (var mineral in mealTotals)
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

        var results = new List<MineralAmount>(accumulatedMinerals.Count);
        foreach (var entry in accumulatedMinerals)
        {
            results.Add(new MineralAmount(entry.Key, entry.Value));
        }

        return results;
    }
}
