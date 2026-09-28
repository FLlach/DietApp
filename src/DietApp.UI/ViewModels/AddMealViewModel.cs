using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para el Compositor de Comidas (AddMealPage) basado en el diseno Stitch.
/// Permite seleccionar el momento del dia (Desayuno, Almuerzo, Cena, Snack), incorporar alimentos y recetas,
/// ajustar porciones y gramajes en tiempo real con proyeccion inmediata del balance de macronutrientes
/// y los 7 minerales criticos (K, P, Na, Ca, Mg, Fe, Zn), y confirmar la ingesta diaria en SQLite.
/// Por que se tomo esta decision: Centraliza el calculo reactivo de la carga metabolica y garantiza
/// una experiencia de usuario ergonomica de una sola mano.
/// </summary>
public partial class AddMealViewModel : ObservableObject
{
    private readonly IMealTrackingService _mealTrackingService;
    private readonly IFoodCatalogService _catalogService;
    private readonly IRecipeService _recipeService;
    private readonly IProteinGoalService _proteinGoalService;

    private List<FoodItemDto> _cachedFoods = new();
    private List<RecipeDto> _cachedRecipes = new();

    public AddMealViewModel(
        IMealTrackingService mealTrackingService,
        IFoodCatalogService catalogService,
        IRecipeService recipeService,
        IProteinGoalService proteinGoalService)
    {
        _mealTrackingService = mealTrackingService;
        _catalogService = catalogService;
        _recipeService = recipeService;
        _proteinGoalService = proteinGoalService;

        SetMealType(MealType.Lunch);
        UpdateFormattedDateTime();
    }

    [ObservableProperty]
    private MealType _selectedMealType = MealType.Lunch;

    [ObservableProperty]
    private bool _isBreakfastSelected;

    [ObservableProperty]
    private bool _isLunchSelected = true;

    [ObservableProperty]
    private bool _isDinnerSelected;

    [ObservableProperty]
    private bool _isSnackSelected;

    [ObservableProperty]
    private string _formattedDateTime = string.Empty;

    [ObservableProperty]
    private string _clinicalNotes = string.Empty;

    [ObservableProperty]
    private int _incorporatedItemsCount;

    [ObservableProperty]
    private bool _hasDraftItems;

    [ObservableProperty]
    private double _projectedCalories;

    [ObservableProperty]
    private double _projectedProtein;

    [ObservableProperty]
    private double _caloriesProgress;

    [ObservableProperty]
    private double _proteinProgress;

    [ObservableProperty]
    private string _formattedCaloriesTarget = "0% del objetivo (2000 kcal)";

    [ObservableProperty]
    private string _formattedProteinTarget = "0% meta diaria (75g)";

    [ObservableProperty]
    private double _potassiumMg;

    [ObservableProperty]
    private double _potassiumProgress;

    [ObservableProperty]
    private string _formattedPotassium = "0 mg";

    [ObservableProperty]
    private double _phosphorusMg;

    [ObservableProperty]
    private double _phosphorusProgress;

    [ObservableProperty]
    private string _formattedPhosphorus = "0 mg";

    [ObservableProperty]
    private double _sodiumMg;

    [ObservableProperty]
    private double _sodiumProgress;

    [ObservableProperty]
    private string _formattedSodium = "0 mg";

    [ObservableProperty]
    private string _formattedCalcium = "0 mg";

    [ObservableProperty]
    private string _formattedMagnesium = "0 mg";

    [ObservableProperty]
    private string _formattedIron = "0 mg";

    [ObservableProperty]
    private string _formattedZinc = "0 mg";

    [ObservableProperty]
    private bool _isPickerOpen;

    [ObservableProperty]
    private string _pickerTitle = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<MealComposerItemModel> DraftItems { get; } = new();
    public ObservableCollection<PickerOptionModel> PickerItems { get; } = new();

    public async Task InitializeAsync()
    {
        await LoadCatalogCacheAsync();
        UpdateFormattedDateTime();
        RecalculateProjections();
    }

    [RelayCommand]
    private void SelectMealType(string typeStr)
    {
        if (Enum.TryParse<MealType>(typeStr, true, out var type))
        {
            SetMealType(type);
        }
    }

    private void SetMealType(MealType type)
    {
        SelectedMealType = type;
        IsBreakfastSelected = type == MealType.Breakfast;
        IsLunchSelected = type == MealType.Lunch;
        IsDinnerSelected = type == MealType.Dinner;
        IsSnackSelected = type == MealType.Snack;
    }

    private void UpdateFormattedDateTime()
    {
        var now = DateTime.Now;
        FormattedDateTime = $"Hoy, {now:HH:mm}";
    }

    private async Task LoadCatalogCacheAsync()
    {
        try
        {
            var foodsTask = _catalogService.GetAllFoodsAsync();
            var recipesTask = _recipeService.GetAllRecipesAsync();
            await Task.WhenAll(foodsTask, recipesTask);

            _cachedFoods = (await foodsTask).ToList();
            _cachedRecipes = (await recipesTask).ToList();
        }
        catch
        {
            // Resistencia ante fallos de conexion inicial
        }
    }

    [RelayCommand]
    private async Task OpenFoodPickerAsync()
    {
        if (_cachedFoods.Count == 0)
        {
            await LoadCatalogCacheAsync();
        }

        PickerTitle = "Buscar Alimento";
        SearchQuery = string.Empty;
        FilterPickerItems(false);
        IsPickerOpen = true;
    }

    [RelayCommand]
    private async Task OpenRecipePickerAsync()
    {
        if (_cachedRecipes.Count == 0)
        {
            await LoadCatalogCacheAsync();
        }

        PickerTitle = "Añadir de Recetas";
        SearchQuery = string.Empty;
        FilterPickerItems(true);
        IsPickerOpen = true;
    }

    [RelayCommand]
    private void ClosePicker()
    {
        IsPickerOpen = false;
        SearchQuery = string.Empty;
    }

    [RelayCommand]
    private void SearchPicker(string query)
    {
        SearchQuery = query;
        bool isRecipePicker = PickerTitle.Contains("Receta", StringComparison.OrdinalIgnoreCase);
        FilterPickerItems(isRecipePicker);
    }

    private void FilterPickerItems(bool forRecipes)
    {
        PickerItems.Clear();

        if (forRecipes)
        {
            var query = _cachedRecipes.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(r => r.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var r in query.Take(30))
            {
                PickerItems.Add(new PickerOptionModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Subtitle = $"{r.Servings} porciones estándar",
                    TagText = "Receta clínica",
                    IsRecipe = true,
                    Calories = Math.Round(r.CaloriesPerServing, 0),
                    Protein = Math.Round(r.ProteinPerServing, 1)
                });
            }
        }
        else
        {
            var query = _cachedFoods.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(f => f.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var f in query.Take(30))
            {
                PickerItems.Add(new PickerOptionModel
                {
                    Id = f.Id,
                    Title = f.Name,
                    Subtitle = "Por 100g estándar",
                    TagText = "Alimento base",
                    IsRecipe = false,
                    Calories = Math.Round(f.Calories, 0),
                    Protein = Math.Round(f.ProteinGrams, 1)
                });
            }
        }
    }

    [RelayCommand]
    private void SelectPickerItem(PickerOptionModel item)
    {
        if (item == null)
        {
            return;
        }

        if (item.IsRecipe)
        {
            var recipe = _cachedRecipes.FirstOrDefault(r => r.Id == item.Id);
            if (recipe != null)
            {
                var composerItem = new MealComposerItemModel
                {
                    ItemId = recipe.Id,
                    ItemName = recipe.Title,
                    IsRecipe = true,
                    TypeBadgeText = "Receta clínica",
                    Subtitle = "1 ración estándar",
                    TypeBadgeBackground = Color.FromArgb("#FFDBCF"),
                    TypeBadgeTextColor = Color.FromArgb("#671F00"),
                    Quantity = 1.0,
                    BaseCaloriesPerUnit = recipe.CaloriesPerServing,
                    BaseProteinPerUnit = recipe.ProteinPerServing
                };

                foreach (var m in recipe.MineralsPerServing)
                {
                    composerItem.BaseMineralsPerUnit[m.Type] = m.Milligrams;
                }

                composerItem.Recalculate();
                DraftItems.Add(composerItem);
            }
        }
        else
        {
            var food = _cachedFoods.FirstOrDefault(f => f.Id == item.Id);
            if (food != null)
            {
                var composerItem = new MealComposerItemModel
                {
                    ItemId = food.Id,
                    ItemName = food.Name,
                    IsRecipe = false,
                    TypeBadgeText = "Alimento base",
                    Subtitle = "Cocido / Hervido",
                    TypeBadgeBackground = Color.FromArgb("#EAEDFF"),
                    TypeBadgeTextColor = Color.FromArgb("#131B2E"),
                    Quantity = 100, // 100 gramos por defecto
                    BaseCaloriesPerUnit = food.Calories / 100.0,
                    BaseProteinPerUnit = food.ProteinGrams / 100.0
                };

                foreach (var m in food.Minerals)
                {
                    composerItem.BaseMineralsPerUnit[m.Type] = m.Milligrams / 100.0;
                }

                composerItem.Recalculate();
                DraftItems.Add(composerItem);
            }
        }

        ClosePicker();
        RecalculateProjections();
    }

    [RelayCommand]
    private void IncreaseQuantity(MealComposerItemModel item)
    {
        if (item == null) return;

        if (item.IsRecipe)
        {
            item.Quantity += 0.5;
        }
        else
        {
            item.Quantity += 25; // Salto de 25 gramos
        }

        item.Recalculate();
        RecalculateProjections();
    }

    [RelayCommand]
    private void DecreaseQuantity(MealComposerItemModel item)
    {
        if (item == null) return;

        if (item.IsRecipe)
        {
            if (item.Quantity > 0.5)
            {
                item.Quantity -= 0.5;
            }
        }
        else
        {
            if (item.Quantity > 25)
            {
                item.Quantity -= 25;
            }
        }

        item.Recalculate();
        RecalculateProjections();
    }

    [RelayCommand]
    private void RemoveDraftItem(MealComposerItemModel item)
    {
        if (item != null)
        {
            DraftItems.Remove(item);
            RecalculateProjections();
        }
    }

    private void RecalculateProjections()
    {
        IncorporatedItemsCount = DraftItems.Count;
        HasDraftItems = DraftItems.Count > 0;

        double cal = 0;
        double prot = 0;
        var minerals = new Dictionary<MineralType, double>();

        foreach (var item in DraftItems)
        {
            cal += item.CalculatedCalories;
            prot += item.CalculatedProtein;

            foreach (var kvp in item.BaseMineralsPerUnit)
            {
                if (!minerals.ContainsKey(kvp.Key)) minerals[kvp.Key] = 0;
                minerals[kvp.Key] += (kvp.Value * item.Quantity);
            }
        }

        ProjectedCalories = Math.Round(cal, 0);
        ProjectedProtein = Math.Round(prot, 1);

        // Metas nutricionales diarias
        double targetCalories = 2000;
        double targetProtein = _proteinGoalService.IsProteinGoalEnabled && _proteinGoalService.DailyProteinGoalGrams > 0
            ? _proteinGoalService.DailyProteinGoalGrams
            : 60.0;

        double calPct = targetCalories > 0 ? (ProjectedCalories / targetCalories) * 100 : 0;
        CaloriesProgress = Math.Min(1.0, calPct / 100.0);
        FormattedCaloriesTarget = $"{Math.Round(calPct, 0)}% del objetivo (2000 kcal)";

        double protPct = targetProtein > 0 ? (ProjectedProtein / targetProtein) * 100 : 0;
        ProteinProgress = Math.Min(1.0, protPct / 100.0);
        FormattedProteinTarget = $"{Math.Round(protPct, 0)}% meta diaria ({targetProtein:0.#}g)";

        // Minerales
        PotassiumMg = minerals.TryGetValue(MineralType.Potassium, out var k) ? Math.Round(k, 1) : 0;
        PotassiumProgress = Math.Min(1.0, PotassiumMg / 2000.0);
        FormattedPotassium = $"{PotassiumMg:N0} mg";

        PhosphorusMg = minerals.TryGetValue(MineralType.Phosphorus, out var p) ? Math.Round(p, 1) : 0;
        PhosphorusProgress = Math.Min(1.0, PhosphorusMg / 850.0);
        FormattedPhosphorus = $"{PhosphorusMg:N0} mg";

        SodiumMg = minerals.TryGetValue(MineralType.Sodium, out var na) ? Math.Round(na, 1) : 0;
        SodiumProgress = Math.Min(1.0, SodiumMg / 2000.0);
        FormattedSodium = $"{SodiumMg:N0} mg";

        double ca = minerals.TryGetValue(MineralType.Calcium, out var caVal) ? caVal : 0;
        FormattedCalcium = $"{ca:N0} mg";

        double mg = minerals.TryGetValue(MineralType.Magnesium, out var mgVal) ? mgVal : 0;
        FormattedMagnesium = $"{mg:N0} mg";

        double fe = minerals.TryGetValue(MineralType.Iron, out var feVal) ? feVal : 0;
        FormattedIron = $"{fe:0.#} mg";

        double zn = minerals.TryGetValue(MineralType.Zinc, out var znVal) ? znVal : 0;
        FormattedZinc = $"{zn:0.#} mg";
    }

    [RelayCommand]
    private async Task ConfirmAndSaveAsync()
    {
        if (DraftItems.Count == 0)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Añada al menos un alimento o receta para registrar la comida.", "Entendido");
            }
            return;
        }

        try
        {
            IsBusy = true;

            var itemsToSave = DraftItems.Select(d => (id: d.ItemId, quantity: d.Quantity, isRecipe: d.IsRecipe));
            await _mealTrackingService.RecordMealWithMixedItemsAsync(
                DateTime.Today,
                SelectedMealType,
                ClinicalNotes,
                itemsToSave);

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar la comida: {ex.Message}", "Aceptar");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DiscardAndCancelAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
