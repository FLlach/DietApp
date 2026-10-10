using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace DietApp.Infrastructure.Supabase.Models;

/// <summary>
/// Como funciona: Modelo de persistencia para PostgREST que mapea la tabla `foods` en Supabase PostgreSQL.
/// Permite serializar y deserializar los alimentos del catalogo remoto y sincronizarlos con la entidad de dominio FoodItem.
/// Por que se tomo esta decision: Desacopla las convenciones snake_case de PostgreSQL de las propiedades
/// en PascalCase del dominio C#, permitiendo consultas directas sobre la API REST de Supabase.
/// </summary>
[Table("foods")]
public class SupabaseFoodModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("fdc_id")]
    public int FdcId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("reference_grams")]
    public double ReferenceGrams { get; set; } = 100.0;

    [Column("calories")]
    public double Calories { get; set; }

    [Column("protein_grams")]
    public double ProteinGrams { get; set; }

    [Column("phosphorus_mg")]
    public double PhosphorusMg { get; set; }

    [Column("potassium_mg")]
    public double PotassiumMg { get; set; }

    [Column("sodium_mg")]
    public double SodiumMg { get; set; }

    [Column("calcium_mg")]
    public double CalciumMg { get; set; }

    [Column("magnesium_mg")]
    public double MagnesiumMg { get; set; }

    [Column("iron_mg")]
    public double IronMg { get; set; }

    [Column("zinc_mg")]
    public double ZincMg { get; set; }

    [Column("barcode")]
    public string? Barcode { get; set; }

    [Column("is_custom")]
    public bool IsCustom { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }

    public FoodItem ToDomain()
    {
        Guid.TryParse(Id, out var parsedId);

        var minerals = new List<MineralAmount>
        {
            new(MineralType.Phosphorus, PhosphorusMg),
            new(MineralType.Potassium, PotassiumMg),
            new(MineralType.Sodium, SodiumMg),
            new(MineralType.Calcium, CalciumMg),
            new(MineralType.Magnesium, MagnesiumMg),
            new(MineralType.Iron, IronMg),
            new(MineralType.Zinc, ZincMg)
        };

        return new FoodItem(
            parsedId == Guid.Empty ? Guid.NewGuid() : parsedId,
            Name,
            Category,
            ReferenceGrams > 0 ? ReferenceGrams : 100.0,
            minerals,
            Calories,
            ProteinGrams,
            Barcode);
    }

    public static SupabaseFoodModel FromDomain(FoodItem domain, string? userId = null, bool isCustom = false)
    {
        return new SupabaseFoodModel
        {
            Id = domain.Id.ToString(),
            Name = domain.Name,
            Category = domain.Category,
            ReferenceGrams = domain.ReferenceGrams,
            Calories = domain.Calories,
            ProteinGrams = domain.ProteinGrams,
            Barcode = domain.Barcode,
            IsCustom = isCustom,
            UserId = userId,
            PhosphorusMg = domain.GetMineralMilligrams(MineralType.Phosphorus),
            PotassiumMg = domain.GetMineralMilligrams(MineralType.Potassium),
            SodiumMg = domain.GetMineralMilligrams(MineralType.Sodium),
            CalciumMg = domain.GetMineralMilligrams(MineralType.Calcium),
            MagnesiumMg = domain.GetMineralMilligrams(MineralType.Magnesium),
            IronMg = domain.GetMineralMilligrams(MineralType.Iron),
            ZincMg = domain.GetMineralMilligrams(MineralType.Zinc)
        };
    }
}
