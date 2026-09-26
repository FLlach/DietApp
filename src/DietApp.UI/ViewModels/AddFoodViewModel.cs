using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la captura de nuevos alimentos con sus cantidades especificas
/// de minerales (fosforo, potasio, sodio, etc.) por porcion base.
/// Por que se tomo esta decision: Encapsula la validacion de entrada de datos numericos de minerales
/// y construye el DTO correspondiente para ser guardado por el servicio de aplicacion, evitando
/// logica de validacion en la vista XAML.
/// </summary>
public partial class AddFoodViewModel : ObservableObject
{
    private readonly IFoodCatalogService _foodCatalogService;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Category { get; set; } = "General";

    [ObservableProperty]
    public partial string ReferenceGramsText { get; set; } = "100";

    [ObservableProperty]
    public partial string CaloriesText { get; set; } = "0";

    [ObservableProperty]
    public partial string ProteinText { get; set; } = "0";

    [ObservableProperty]
    public partial string PhosphorusText { get; set; } = "0";

    [ObservableProperty]
    public partial string PotassiumText { get; set; } = "0";

    [ObservableProperty]
    public partial string SodiumText { get; set; } = "0";

    [ObservableProperty]
    public partial string CalciumText { get; set; } = "0";

    [ObservableProperty]
    public partial string MagnesiumText { get; set; } = "0";

    [ObservableProperty]
    public partial string IronText { get; set; } = "0";

    [ObservableProperty]
    public partial string ZincText { get; set; } = "0";

    [ObservableProperty]
    public partial string FeedbackMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public AddFoodViewModel(IFoodCatalogService foodCatalogService)
    {
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
    }

    [RelayCommand]
    public async Task SaveFoodAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            FeedbackMessage = "El nombre del alimento es obligatorio.";
            return;
        }

        if (!double.TryParse(ReferenceGramsText, out double referenceGrams) || referenceGrams <= 0)
        {
            FeedbackMessage = "La porcion base debe ser un numero mayor a cero.";
            return;
        }

        try
        {
            IsBusy = true;
            FeedbackMessage = string.Empty;

            double calories = 0;
            if (double.TryParse(CaloriesText, out double parsedCalories) && parsedCalories >= 0)
            {
                calories = parsedCalories;
            }

            double proteinGrams = 0;
            if (double.TryParse(ProteinText, out double parsedProtein) && parsedProtein >= 0)
            {
                proteinGrams = parsedProtein;
            }

            var minerals = new List<MineralAmountDto>();

            AddMineralIfValid(minerals, MineralType.Phosphorus, "Fosforo", PhosphorusText);
            AddMineralIfValid(minerals, MineralType.Potassium, "Potasio", PotassiumText);
            AddMineralIfValid(minerals, MineralType.Sodium, "Sodio", SodiumText);
            AddMineralIfValid(minerals, MineralType.Calcium, "Calcio", CalciumText);
            AddMineralIfValid(minerals, MineralType.Magnesium, "Magnesio", MagnesiumText);
            AddMineralIfValid(minerals, MineralType.Iron, "Hierro", IronText);
            AddMineralIfValid(minerals, MineralType.Zinc, "Zinc", ZincText);

            var foodDto = new FoodItemDto
            {
                Id = Guid.NewGuid(),
                Name = Name.Trim(),
                Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim(),
                ReferenceGrams = referenceGrams,
                Calories = calories,
                ProteinGrams = proteinGrams,
                Minerals = minerals
            };

            await _foodCatalogService.SaveFoodAsync(foodDto);
            FeedbackMessage = "Alimento guardado exitosamente.";

            // Limpiar formulario tras guardar
            Name = string.Empty;
            CaloriesText = "0";
            ProteinText = "0";
            PhosphorusText = "0";
            PotassiumText = "0";
            SodiumText = "0";
            CalciumText = "0";
            MagnesiumText = "0";
            IronText = "0";
            ZincText = "0";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void AddMineralIfValid(
        List<MineralAmountDto> list,
        MineralType type,
        string name,
        string textValue)
    {
        if (double.TryParse(textValue, out double milligrams) && milligrams >= 0)
        {
            list.Add(new MineralAmountDto
            {
                Type = type,
                MineralName = name,
                Milligrams = milligrams,
                Unit = "mg"
            });
        }
    }
}
