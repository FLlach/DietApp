using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `meals` en Supabase PostgreSQL.
/// Permite almacenar cabeceras de comidas consumidas por cada usuario autenticado.
/// Por que se tomo esta decision: Permite sincronizar las comidas del paciente en la nube con aislamiento RLS por `user_id`.
/// </summary>
[Table("meals")]
public class SupabaseMealModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("meal_type_value")]
    public int MealTypeValue { get; set; }

    [Column("note")]
    public string Note { get; set; } = string.Empty;

    public Meal ToDomain(IEnumerable<MealItem>? items = null)
    {
        Guid.TryParse(Id, out var parsedId);
        var meal = new Meal(parsedId == Guid.Empty ? Guid.NewGuid() : parsedId, Date, (MealType)MealTypeValue, Note);
        if (items != null)
        {
            foreach (var item in items)
            {
                meal.AddItem(item);
            }
        }
        return meal;
    }

    public static SupabaseMealModel FromDomain(Meal meal, string? userId = null)
    {
        return new SupabaseMealModel
        {
            Id = meal.Id.ToString(),
            UserId = userId,
            Date = meal.Date,
            MealTypeValue = (int)meal.Type,
            Note = meal.Note
        };
    }
}
