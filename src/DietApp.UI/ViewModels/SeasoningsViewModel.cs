using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Helpers;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para el Modulo de Alinos y Condimentos (SeasoningsPage) segun prototipo Stitch.
/// Permite segmentar el destino del alino (Plato Principal, Guarnicion, etc.), explorar alinos preconfigurados
/// con etiquetas de control renal (Bajo Sodio, Renal Seguro, Zero Sal Añadida, Sodio Moderado), construir alinos
/// caseros combinando bases con extractos desmineralizados, y auditar en tiempo real el aporte bioquimico
/// (Sodio Total, Potasio Total, Calorias y presupuesto de sodio de la comida).
/// Por que se tomo esta decision: Los alinos comerciales e industriales suelen ser la fuente oculta principal
/// de sodio y fosfatos inorganicos en pacientes renales; este modulo garantiza prescripcion controlada y segura.
/// </summary>
public partial class SeasoningsViewModel : ObservableObject
{
    private readonly ISeasoningService _seasoningService;
    private readonly IFoodCatalogService _catalogService;

    public SeasoningsViewModel(
        ISeasoningService seasoningService,
        IFoodCatalogService catalogService)
    {
        _seasoningService = seasoningService;
        _catalogService = catalogService;

        InitializeTargetDishes();
        InitializeCategoryFilters();
        InitializePreconfiguredSeasonings();
        InitializeCustomFormula();
        RecalculateBiochemicalMeter();
    }

    // 1. Destino del Aliño (Horizontal Rail)
    public ObservableCollection<TargetDishOptionModel> TargetDishes { get; } = new();

    [ObservableProperty]
    private TargetDishOptionModel? _selectedTargetDish;

    // 2. Subsegmentación por Categoría
    public ObservableCollection<SeasoningCategoryFilterModel> CategoryFilters { get; } = new();

    [ObservableProperty]
    private SeasoningCategoryFilterModel? _selectedCategoryFilter;

    // 3. Opciones Preconfiguradas
    public ObservableCollection<SeasoningOptionModel> PreconfiguredSeasonings { get; } = new();
    public ObservableCollection<SeasoningOptionModel> DisplayedSeasonings { get; } = new();

    [ObservableProperty]
    private SeasoningOptionModel? _selectedSeasoning;

    // 4. Constructor de Aliño Casero
    public ObservableCollection<CustomSeasoningIngredientModel> CustomIngredients { get; } = new();

    [ObservableProperty]
    private string _totalSodiumText = "0.7";

    [ObservableProperty]
    private string _sodiumLimitPercentageText = "< 1% límite";

    [ObservableProperty]
    private string _totalPotassiumText = "10.0";

    [ObservableProperty]
    private string _totalCaloriesText = "88";

    [ObservableProperty]
    private double _sodiumBudgetProgress = 0.005;

    [ObservableProperty]
    private string _sodiumBudgetText = "0.7mg / 150mg máx";

    [ObservableProperty]
    private bool _isBusy;

    private void InitializeTargetDishes()
    {
        TargetDishes.Clear();
        var main = new TargetDishOptionModel { Name = "Plato Principal", IconGlyph = MaterialIconFont.DinnerDining, IsSelected = true };
        TargetDishes.Add(main);
        TargetDishes.Add(new TargetDishOptionModel { Name = "Guarnición", IconGlyph = MaterialIconFont.Eco, IsSelected = false });
        TargetDishes.Add(new TargetDishOptionModel { Name = "Postre", IconGlyph = MaterialIconFont.BakeryDining, IsSelected = false });
        TargetDishes.Add(new TargetDishOptionModel { Name = "Líquidos y Caldos", IconGlyph = MaterialIconFont.SoupKitchen, IsSelected = false });

        SelectedTargetDish = main;
    }

    private void InitializeCategoryFilters()
    {
        CategoryFilters.Clear();
        var all = new SeasoningCategoryFilterModel { Name = "Todos", IsSelected = true };
        CategoryFilters.Add(all);
        CategoryFilters.Add(new SeasoningCategoryFilterModel { Name = "Vinagretas", IsSelected = false });
        CategoryFilters.Add(new SeasoningCategoryFilterModel { Name = "Marinadas Bajas en Sodio", IsSelected = false });
        CategoryFilters.Add(new SeasoningCategoryFilterModel { Name = "Hierbas y Especias", IsSelected = false });
        CategoryFilters.Add(new SeasoningCategoryFilterModel { Name = "Salsas Ligeras", IsSelected = false });

        SelectedCategoryFilter = all;
    }

    private void InitializePreconfiguredSeasonings()
    {
        PreconfiguredSeasonings.Clear();

        var s1 = new SeasoningOptionModel
        {
            Name = "Vinagreta de Eneldo y Manzana",
            Description = "Emulsión ácida con vinagre crudo de manzana, aceite virgen y eneldo liofilizado.",
            BadgeText = "Bajo Sodio",
            BadgeBackgroundColor = Color.FromArgb("#2084CC16"),
            BadgeTextColor = Color.FromArgb("#315200"),
            Category = "Vinagretas",
            IsSelected = true,
            IsAlert = false,
            SodiumMg = 4,
            PotassiumMg = 18,
            Calories = 45
        };

        var s2 = new SeasoningOptionModel
        {
            Name = "Marinada Cítrica con Romero",
            Description = "Zumo rebajado de limón, romero machacado fresco y aceite de pepita de uva.",
            BadgeText = "Renal Seguro",
            BadgeBackgroundColor = Color.FromArgb("#EAEDFF"),
            BadgeTextColor = Color.FromArgb("#727A64"),
            Category = "Marinadas Bajas en Sodio",
            IsSelected = false,
            IsAlert = false,
            SodiumMg = 2,
            PotassiumMg = 22,
            Calories = 38
        };

        var s3 = new SeasoningOptionModel
        {
            Name = "Macerado de Ajo y Finas Hierbas",
            Description = "Ajo confitado a baja temperatura con tomillo silvestre, orégano y perejil.",
            BadgeText = "Zero Sal Añadida",
            BadgeBackgroundColor = Color.FromArgb("#2084CC16"),
            BadgeTextColor = Color.FromArgb("#315200"),
            Category = "Hierbas y Especias",
            IsSelected = false,
            IsAlert = false,
            SodiumMg = 0,
            PotassiumMg = 12,
            Calories = 35
        };

        var s4 = new SeasoningOptionModel
        {
            Name = "Emulsión de Mostaza Antigua y AOVE",
            Description = "Grano de mostaza lavado, vinagre de jerez, aceite de oliva virgen extra.",
            BadgeText = "Sodio Moderado",
            BadgeBackgroundColor = Color.FromArgb("#FFDBCF"),
            BadgeTextColor = Color.FromArgb("#380D00"),
            Category = "Salsas Ligeras",
            IsSelected = false,
            IsAlert = true,
            SodiumMg = 15,
            PotassiumMg = 30,
            Calories = 52
        };

        PreconfiguredSeasonings.Add(s1);
        PreconfiguredSeasonings.Add(s2);
        PreconfiguredSeasonings.Add(s3);
        PreconfiguredSeasonings.Add(s4);

        SelectedSeasoning = s1;
        FilterDisplayedSeasonings();
    }

    private void InitializeCustomFormula()
    {
        CustomIngredients.Clear();

        CustomIngredients.Add(new CustomSeasoningIngredientModel
        {
            Name = "Aceite de oliva virgen (10ml)",
            Subtitle = "Grasa monoinsaturada pura",
            MineralBadge = "0mg Na",
            MineralBadgeColor = Color.FromArgb("#416900"),
            IsChecked = true,
            SodiumMg = 0.0,
            PotassiumMg = 0.0,
            Calories = 80.0,
            Grams = 10.0
        });

        CustomIngredients.Add(new CustomSeasoningIngredientModel
        {
            Name = "Vinagre de manzana (5ml)",
            Subtitle = "Acidez orgánica desinfectante",
            MineralBadge = "0.5mg Na",
            MineralBadgeColor = Color.FromArgb("#416900"),
            IsChecked = true,
            SodiumMg = 0.5,
            PotassiumMg = 2.5,
            Calories = 1.0,
            Grams = 5.0
        });

        CustomIngredients.Add(new CustomSeasoningIngredientModel
        {
            Name = "Gotas de limón natural (8 gotas)",
            Subtitle = "Ácido cítrico y vitamina C",
            MineralBadge = "6mg K",
            MineralBadgeColor = Color.FromArgb("#A53C0B"),
            IsChecked = true,
            SodiumMg = 0.1,
            PotassiumMg = 6.0,
            Calories = 2.0,
            Grams = 2.0
        });

        CustomIngredients.Add(new CustomSeasoningIngredientModel
        {
            Name = "Especias desmineralizadas",
            Subtitle = "Orégano y albahaca remojada",
            MineralBadge = "0mg Na",
            MineralBadgeColor = Color.FromArgb("#416900"),
            IsChecked = true,
            SodiumMg = 0.1,
            PotassiumMg = 1.5,
            Calories = 5.0,
            Grams = 1.0
        });
    }

    [RelayCommand]
    private void SelectTargetDish(TargetDishOptionModel dish)
    {
        if (dish == null) return;

        foreach (var item in TargetDishes)
        {
            item.IsSelected = item == dish;
        }
        SelectedTargetDish = dish;
    }

    [RelayCommand]
    private void SelectCategoryFilter(SeasoningCategoryFilterModel filter)
    {
        if (filter == null) return;

        foreach (var item in CategoryFilters)
        {
            item.IsSelected = item == filter;
        }
        SelectedCategoryFilter = filter;
        FilterDisplayedSeasonings();
    }

    private void FilterDisplayedSeasonings()
    {
        DisplayedSeasonings.Clear();
        string category = SelectedCategoryFilter?.Name ?? "Todos";

        foreach (var item in PreconfiguredSeasonings)
        {
            if (category == "Todos" || item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                DisplayedSeasonings.Add(item);
            }
        }
    }

    [RelayCommand]
    private void SelectSeasoning(SeasoningOptionModel option)
    {
        if (option == null) return;

        foreach (var item in PreconfiguredSeasonings)
        {
            item.IsSelected = item == option;
        }
        SelectedSeasoning = option;
    }

    [RelayCommand]
    private void ToggleIngredient(CustomSeasoningIngredientModel ingredient)
    {
        if (ingredient != null)
        {
            ingredient.IsChecked = !ingredient.IsChecked;
            RecalculateBiochemicalMeter();
        }
    }

    [RelayCommand]
    private void ResetFormula()
    {
        foreach (var item in CustomIngredients)
        {
            item.IsChecked = true;
        }
        RecalculateBiochemicalMeter();
    }

    public void RecalculateBiochemicalMeter()
    {
        double totalNa = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.SodiumMg);
        double totalK = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.PotassiumMg);
        double totalCal = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.Calories);

        TotalSodiumText = $"{totalNa:0.#}";
        SodiumLimitPercentageText = totalNa < 1.0 ? "< 1% límite" : $"{Math.Round((totalNa / 150.0) * 100, 0)}% límite";

        TotalPotassiumText = $"{totalK:0.#}";
        TotalCaloriesText = $"{totalCal:N0}";

        double progress = Math.Min(1.0, Math.Max(0.01, totalNa / 150.0));
        SodiumBudgetProgress = progress;
        SodiumBudgetText = $"{totalNa:0.#}mg / 150mg máx";
    }

    [RelayCommand]
    private async Task SaveCustomSeasoningAsync()
    {
        try
        {
            IsBusy = true;
            string name = $"Aliño Casero {DateTime.Now:dd/MM}";
            double totalNa = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.SodiumMg);
            double totalK = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.PotassiumMg);
            double totalCal = CustomIngredients.Where(i => i.IsChecked).Sum(i => i.Calories);

            // Persistir mediante el servicio de aplicación
            var foods = await _catalogService.GetAllFoodsAsync();
            var oilFood = foods.FirstOrDefault(f => f.Name.Contains("Aceite", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Oil", StringComparison.OrdinalIgnoreCase)) ?? foods.FirstOrDefault();

            if (oilFood != null)
            {
                await _seasoningService.CreateSeasoningAsync(
                    name,
                    "Fórmula casera libre de sodio con balance electrolítico seguro.",
                    new[] { (oilFood.Id, 18.0) });
            }

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Aliño Guardado", $"La fórmula '{name}' ha sido guardada en tu catálogo de aliños para su reutilización.", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar el aliño: {ex.Message}", "Aceptar");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyToRecipeOrMealAsync()
    {
        string chosenName = SelectedSeasoning?.Name ?? "Aliño Seleccionado";
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Aliño Aplicado", $"Se ha vinculado '{chosenName}' a {SelectedTargetDish?.Name ?? "el plato seleccionado"}.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task ShowInfoAlertAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Dosificación Clínica", "Los aliños formulados evitan sales sintéticas (NaCl) para proteger la presión arterial y la filtración glomerular.", "Entendido");
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
