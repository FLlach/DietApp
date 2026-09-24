using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla en SQLite para almacenar porciones habituales de los alimentos
/// con su peso exacto normalizado en gramos (gramWeight).
/// Por que se tomo esta decision: Permite a los usuarios seleccionar porciones cotidianas
/// (cucharadas, tazas, rebanadas) traduciendolas automaticamente al gramaje equivalente para calcular
/// de forma precisa los minerales ingeridos.
/// </summary>
[Table("FoodPortions")]
public class FoodPortionEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid FoodId { get; set; }

    public string MeasureUnitName { get; set; } = string.Empty;

    public double Amount { get; set; }

    public double GramWeight { get; set; }

    public string Modifier { get; set; } = string.Empty;

    public string DescriptionDisplay
    {
        get
        {
            string modifierText = string.IsNullOrWhiteSpace(Modifier) ? "" : $" ({Modifier})";
            return $"{Amount} {MeasureUnitName}{modifierText} - {GramWeight:F1} g";
        }
    }
}
