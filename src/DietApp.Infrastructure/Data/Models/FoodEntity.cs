using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;
using SQLite;

namespace DietApp.Infrastructure.Data.Models;

/// <summary>
/// Como funciona: Tabla relacional en SQLite que almacena los alimentos del catalogo,
/// sus calorias y los valores de cada mineral en miligramos normalizados a una porcion base de 100 gramos.
/// Por que se tomo esta decision: Al aplanar los minerales principales (fosforo, potasio, sodio) en columnas
/// indexadas individualmente, SQLite puede ejecutar consultas de filtrado por rango en microsegundos
/// sin necesidad de escanear la tabla completa ni deserializar blobs JSON.
/// </summary>
[Table("Foods")]
public class FoodEntity
{
    [PrimaryKey]
    public Guid Id { get; set; }

    public int FdcId { get; set; }

    [Indexed]
    public string Name { get; set; } = string.Empty;

    [Indexed]
    public string Category { get; set; } = string.Empty;

    public double ReferenceGrams { get; set; } = 100.0;

    public double Calories { get; set; }

    [Indexed]
    public double ProteinGrams { get; set; }

    [Indexed]
    public double PhosphorusMg { get; set; }

    [Indexed]
    public double PotassiumMg { get; set; }

    [Indexed]
    public double SodiumMg { get; set; }

    public double CalciumMg { get; set; }

    public double MagnesiumMg { get; set; }

    public double IronMg { get; set; }

    public double ZincMg { get; set; }

    public FoodItem ToDomain()
    {
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
            Id,
            Name,
            Category,
            ReferenceGrams > 0 ? ReferenceGrams : 100.0,
            minerals,
            Calories,
            ProteinGrams);
    }

    public static FoodEntity FromDomain(FoodItem domainItem)
    {
        return new FoodEntity
        {
            Id = domainItem.Id,
            Name = domainItem.Name,
            Category = domainItem.Category,
            ReferenceGrams = domainItem.ReferenceGrams,
            Calories = domainItem.Calories,
            ProteinGrams = domainItem.ProteinGrams,
            PhosphorusMg = domainItem.GetMineralMilligrams(MineralType.Phosphorus),
            PotassiumMg = domainItem.GetMineralMilligrams(MineralType.Potassium),
            SodiumMg = domainItem.GetMineralMilligrams(MineralType.Sodium),
            CalciumMg = domainItem.GetMineralMilligrams(MineralType.Calcium),
            MagnesiumMg = domainItem.GetMineralMilligrams(MineralType.Magnesium),
            IronMg = domainItem.GetMineralMilligrams(MineralType.Iron),
            ZincMg = domainItem.GetMineralMilligrams(MineralType.Zinc)
        };
    }
}
