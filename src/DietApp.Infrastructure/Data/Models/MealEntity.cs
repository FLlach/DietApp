using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para el registro de comidas consumidas.
/// Por que se tomo esta decision: Permite consultar el historial de comidas por rango de fechas
/// de forma indexada y asociar sus items correspondientes.
/// </summary>
[Table("Meals")]
public class MealEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public DateTime Date { get; set; }

    public int MealTypeValue { get; set; }

    public string Note { get; set; } = string.Empty;

    public Meal ToDomain(IEnumerable<MealItem> items)
    {
        var meal = new Meal(Id, Date, (MealType)MealTypeValue, Note);
        if (items != null)
        {
            foreach (var item in items)
            {
                meal.AddItem(item);
            }
        }
        return meal;
    }

    public static MealEntity FromDomain(Meal meal)
    {
        return new MealEntity
        {
            Id = meal.Id,
            Date = meal.Date,
            MealTypeValue = (int)meal.Type,
            Note = meal.Note
        };
    }
}
