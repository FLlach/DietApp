using DietApp.Domain.Entities;
using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Infrastructure.SeedData;

/// <summary>
/// Como funciona: Provee un catalogo inicial con alimentos representativos y su contenido
/// de minerales en miligramos (mg) por cada 100 gramos de porcion de referencia.
/// Por que se tomo esta decision: Permite probar de inmediato las funciones de filtrado
/// y conteo de fosforo, potasio y sodio sin requerir carga manual inicial por parte del usuario,
/// reflejando valores nutricionales reales basados en tablas de composicion de alimentos.
/// </summary>
public static class InitialFoodCatalogSeed
{
    public static List<FoodItem> GetPreloadedFoods()
    {
        return new List<FoodItem>
        {
            new FoodItem(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Platano maduro",
                "Frutas",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Potassium, 358.0),
                    new(MineralType.Phosphorus, 22.0),
                    new(MineralType.Sodium, 1.0),
                    new(MineralType.Magnesium, 27.0),
                    new(MineralType.Calcium, 5.0)
                }),

            new FoodItem(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Espinaca fresca cruda",
                "Verduras",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Potassium, 558.0),
                    new(MineralType.Phosphorus, 49.0),
                    new(MineralType.Sodium, 79.0),
                    new(MineralType.Calcium, 99.0),
                    new(MineralType.Iron, 2.7),
                    new(MineralType.Magnesium, 79.0)
                }),

            new FoodItem(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Pechuga de pollo cocida",
                "Carnes y Aves",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Phosphorus, 228.0),
                    new(MineralType.Potassium, 256.0),
                    new(MineralType.Sodium, 74.0),
                    new(MineralType.Zinc, 1.0),
                    new(MineralType.Magnesium, 29.0)
                }),

            new FoodItem(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                "Lentejas cocidas",
                "Legumbres",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Potassium, 369.0),
                    new(MineralType.Phosphorus, 180.0),
                    new(MineralType.Sodium, 2.0),
                    new(MineralType.Iron, 3.3),
                    new(MineralType.Zinc, 1.3),
                    new(MineralType.Magnesium, 36.0)
                }),

            new FoodItem(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                "Salmon fresco a la plancha",
                "Pescados",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Phosphorus, 252.0),
                    new(MineralType.Potassium, 384.0),
                    new(MineralType.Sodium, 59.0),
                    new(MineralType.Magnesium, 29.0),
                    new(MineralType.Calcium, 12.0)
                }),

            new FoodItem(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                "Queso parmesano",
                "Lacteos",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Sodium, 1529.0),
                    new(MineralType.Calcium, 1184.0),
                    new(MineralType.Phosphorus, 694.0),
                    new(MineralType.Potassium, 92.0),
                    new(MineralType.Zinc, 2.7)
                }),

            new FoodItem(
                Guid.Parse("77777777-7777-7777-7777-777777777777"),
                "Papa hervida sin cascara",
                "Tuberculos",
                100.0,
                new List<MineralAmount>
                {
                    new(MineralType.Potassium, 379.0),
                    new(MineralType.Phosphorus, 50.0),
                    new(MineralType.Sodium, 4.0),
                    new(MineralType.Magnesium, 20.0),
                    new(MineralType.Iron, 0.3)
                })
        };
    }
}
