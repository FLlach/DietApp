using CommunityToolkit.Mvvm.ComponentModel;
using DietApp.Domain.Enums;

namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de presentacion reactivo para cada item en el compositor de comidas (AddMealPage).
/// Permite modificar la cantidad o racion, calcula calorias, proteinas y minerales en tiempo real,
/// y expone los chips visuales segun el sistema Stitch.
/// Por que se tomo esta decision: Soporta actualizacion inmediata del panel de proyeccion bento
/// al aumentar o disminuir porciones sin recargar la pantalla.
/// </summary>
public partial class MealComposerItemModel : ObservableObject
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public bool IsRecipe { get; set; }
    public string TypeBadgeText { get; set; } = "Alimento base";
    public string Subtitle { get; set; } = string.Empty;

    public Color TypeBadgeBackground { get; set; } = Color.FromArgb("#EAEDFF");
    public Color TypeBadgeTextColor { get; set; } = Color.FromArgb("#131B2E");

    [ObservableProperty]
    private double _quantity = 1.0;

    [ObservableProperty]
    private string _quantityDisplay = "1.0 ración";

    [ObservableProperty]
    private double _calculatedCalories;

    [ObservableProperty]
    private double _calculatedProtein;

    public double BaseCaloriesPerUnit { get; set; }
    public double BaseProteinPerUnit { get; set; }
    public Dictionary<MineralType, double> BaseMineralsPerUnit { get; set; } = new();

    public List<MealMineralChipModel> MineralChips { get; set; } = new();

    public void Recalculate()
    {
        CalculatedCalories = Math.Round(BaseCaloriesPerUnit * Quantity, 0);
        CalculatedProtein = Math.Round(BaseProteinPerUnit * Quantity, 1);

        QuantityDisplay = IsRecipe
            ? (Quantity == 1 ? "1.0 ración" : $"{Quantity:0.#} raciones")
            : $"{Quantity:F0} g";

        MineralChips.Clear();
        if (BaseMineralsPerUnit.TryGetValue(MineralType.Potassium, out var k))
        {
            MineralChips.Add(new MealMineralChipModel
            {
                Symbol = "K",
                AmountWithUnit = $"{Math.Round(k * Quantity, 0)}mg",
                TextColor = Color.FromArgb("#416900")
            });
        }
        if (BaseMineralsPerUnit.TryGetValue(MineralType.Sodium, out var na))
        {
            MineralChips.Add(new MealMineralChipModel
            {
                Symbol = "Na",
                AmountWithUnit = $"{Math.Round(na * Quantity, 1)}mg",
                TextColor = Color.FromArgb("#A53C0B")
            });
        }
        if (BaseMineralsPerUnit.TryGetValue(MineralType.Phosphorus, out var p))
        {
            MineralChips.Add(new MealMineralChipModel
            {
                Symbol = "P",
                AmountWithUnit = $"{Math.Round(p * Quantity, 0)}mg",
                TextColor = Color.FromArgb("#855300")
            });
        }
    }
}
