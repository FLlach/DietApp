using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `food_portions` en Supabase PostgreSQL.
/// Permite recuperar porciones estandarizadas asociadas a alimentos del catalogo remoto.
/// Por que se tomo esta decision: Desacopla la persistencia de PostgreSQL del modelo interno de SQLite y la aplicacion.
/// </summary>
[Table("food_portions")]
public class SupabaseFoodPortionModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("food_id")]
    public string FoodId { get; set; } = string.Empty;

    [Column("measure_unit_name")]
    public string MeasureUnitName { get; set; } = string.Empty;

    [Column("amount")]
    public double Amount { get; set; }

    [Column("gram_weight")]
    public double GramWeight { get; set; }

    [Column("modifier")]
    public string Modifier { get; set; } = string.Empty;
}
