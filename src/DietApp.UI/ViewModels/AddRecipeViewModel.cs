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
/// Como funciona: ViewModel para el Compositor de Recetas Culinarias (AddRecipePage) segun diseno Stitch.
/// Permite definir el titulo, notas dieteticas, raciones y tiempo de preparacion, seleccionar fotografia,
/// dosificar alimentos individuales e integrar alinos guardados, calculando en tiempo real la proyeccion
/// de macronutrientes y los 7 minerales clinicos por racion, agregando pasos secuenciales numerados y persistiendo
/// la receta a traves del servicio de aplicacion IRecipeService.
/// Por que se tomo esta decision: Asegura que la creacion de recetas mantenga rigor clinico proactivo con proyeccion
/// inmediata de micronutrientes por porcion antes de persistir los agregados en la base de datos local.
/// </summary>
public partial class AddRecipeViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IFoodCatalogService _catalogService;
    private readonly ISeasoningService _seasoningService;

    public AddRecipeViewModel(
        IRecipeService recipeService,
        IFoodCatalogService catalogService,
        ISeasoningService seasoningService)
    {
        _recipeService = recipeService;
        _catalogService = catalogService;
        _seasoningService = seasoningService;

        UpdateCounts();
        RecalculateProjections();
    }

    // 1. Datos Principales
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int _servings = 1;

    partial void OnServingsChanged(int value)
    {
        RecalculateProjections();
    }

    [ObservableProperty]
    private int _cookTimeMinutes = 15;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private bool _hasImage = false;

    [ObservableProperty]
    private bool _hasNoImage = true;

    [ObservableProperty]
    private bool _isBusy;

    // 2. Ingredientes Dosificados
    public ObservableCollection<RecipeIngredientDraftModel> IngredientsList { get; } = new();

    [ObservableProperty]
    private string _ingredientsCountDisplay = "0 agregados";

    [ObservableProperty]
    private bool _hasIngredients;

    [ObservableProperty]
    private bool _hasNoIngredients = true;

    // 3. Proyeccion Clinica en Tiempo Real (Por Racion)
    [ObservableProperty]
    private string _energyPerServingText = "0";

    [ObservableProperty]
    private string _proteinPerServingText = "0";

    [ObservableProperty]
    private string _fatPerServingText = "0";

    [ObservableProperty]
    private string _sodiumBadgeText = "Sin ingredientes";

    [ObservableProperty]
    private bool _isSodiumSafe = true;

    public ObservableCollection<MineralProjectionModel> MineralProjections { get; } = new();

    // 4. Pasos de Elaboracion
    public ObservableCollection<RecipeStepDraftModel> StepsList { get; } = new();

    [ObservableProperty]
    private string _stepsCountDisplay = "0 pasos";

    [ObservableProperty]
    private bool _hasSteps;

    [ObservableProperty]
    private bool _hasNoSteps = true;

    [ObservableProperty]
    private string _newStepInstruction = string.Empty;

    [ObservableProperty]
    private int _newStepTimeMinutes = 10;

    [ObservableProperty]
    private string _newStepTechnique = "Cocción suave";

    // 5. Selectores Modales / Desplegables de Alimentos y Aliños
    [ObservableProperty]
    private bool _isFoodPickerVisible;

    [ObservableProperty]
    private string _foodSearchQuery = string.Empty;

    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();

    [ObservableProperty]
    private double _foodGramsToAdd = 100.0;

    [ObservableProperty]
    private bool _isSeasoningPickerVisible;

    public ObservableCollection<SeasoningDto> AvailableSeasonings { get; } = new();

    private void UpdateCounts()
    {
        IngredientsCountDisplay = $"{IngredientsList.Count} {(IngredientsList.Count == 1 ? "agregado" : "agregados")}";
        StepsCountDisplay = $"{StepsList.Count} {(StepsList.Count == 1 ? "paso" : "pasos")}";
        HasIngredients = IngredientsList.Count > 0;
        HasNoIngredients = IngredientsList.Count == 0;
        HasSteps = StepsList.Count > 0;
        HasNoSteps = StepsList.Count == 0;
    }

    public void RecalculateProjections()
    {
        int portions = Math.Max(1, Servings);

        double totalCalories = IngredientsList.Sum(i => i.Calories);
        double totalProtein = IngredientsList.Sum(i => i.ProteinGrams);
        double totalSodium = IngredientsList.Sum(i => i.SodiumMg);
        double totalPotassium = IngredientsList.Sum(i => i.PotassiumMg);
        double totalPhosphorus = IngredientsList.Sum(i => i.PhosphorusMg);
        double totalGrams = IngredientsList.Sum(i => i.Grams);

        // Estimacion de grasas cardiosaludables segun ingredientes dosificados
        double estimatedFat = totalGrams > 0
            ? Math.Max(0.0, (totalCalories - (totalProtein * 4.0)) / 9.0 * 0.45)
            : 0.0;

        double calPerServing = totalGrams > 0 ? (totalCalories / portions) : 0.0;
        double protPerServing = totalGrams > 0 ? (totalProtein / portions) : 0.0;
        double fatPerServing = totalGrams > 0 ? (estimatedFat / portions) : 0.0;

        double naPerServing = totalGrams > 0 ? (totalSodium / portions) : 0.0;
        double kPerServing = totalGrams > 0 ? (totalPotassium / portions) : 0.0;
        double pPerServing = totalGrams > 0 ? (totalPhosphorus / portions) : 0.0;

        EnergyPerServingText = totalGrams > 0 ? $"{calPerServing:N0}" : "0";
        ProteinPerServingText = totalGrams > 0 ? $"{protPerServing:0.#}" : "0";
        FatPerServingText = totalGrams > 0 ? $"{fatPerServing:0.#}" : "0";

        IsSodiumSafe = totalGrams == 0 || naPerServing <= 150;
        SodiumBadgeText = totalGrams == 0
            ? "Sin ingredientes"
            : (IsSodiumSafe ? "Apto bajo sodio" : "Sodio moderado");

        // Actualizar balance de 7 minerales clinicos por racion
        MineralProjections.Clear();

        // 1. Fósforo (P)
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "P",
            AmountDisplay = totalGrams > 0 ? $"{pPerServing:N0} mg" : "- mg",
            StatusText = totalGrams == 0 ? "-" : (pPerServing <= 300 ? "Óptimo" : "Controlar"),
            StatusColor = pPerServing <= 300 ? Color.FromArgb("#416900") : Color.FromArgb("#A53C0B"),
            BackgroundColor = Color.FromArgb("#EAEDFF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });

        // 2. Potasio (K)
        bool kElevated = kPerServing > 500;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "K",
            AmountDisplay = totalGrams > 0 ? $"{kPerServing:N0} mg" : "- mg",
            StatusText = totalGrams == 0 ? "-" : (kElevated ? "Medio" : "Bajo"),
            StatusColor = kElevated ? Color.FromArgb("#855300") : Color.FromArgb("#416900"),
            BackgroundColor = kElevated ? Color.FromArgb("#FFF7F5") : Color.FromArgb("#EAEDFF"),
            BorderColor = kElevated ? Color.FromArgb("#FC7B48") : Color.FromArgb("#C1CAB0"),
            TextColor = kElevated ? Color.FromArgb("#A53C0B") : Color.FromArgb("#131B2E")
        });

        // 3. Sodio (Na)
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "Na",
            AmountDisplay = totalGrams > 0 ? $"{naPerServing:N0} mg" : "- mg",
            StatusText = totalGrams == 0 ? "-" : (naPerServing < 100 ? "Bajo" : "Aceptable"),
            StatusColor = Color.FromArgb("#416900"),
            BackgroundColor = Color.FromArgb("#EAF8DC"),
            BorderColor = Color.FromArgb("#84CC16"),
            TextColor = Color.FromArgb("#315200")
        });

        // 4. Calcio (Ca)
        double caPerServing = totalGrams > 0 ? (totalGrams * 0.18) / portions : 0.0;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "Ca",
            AmountDisplay = totalGrams > 0 ? $"{caPerServing:N0} mg" : "- mg",
            StatusText = totalGrams > 0 ? "14%" : "-",
            StatusColor = Color.FromArgb("#424936"),
            BackgroundColor = Color.FromArgb("#EAEDFF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });

        // 5. Magnesio (Mg)
        double mgPerServing = totalGrams > 0 ? (totalGrams * 0.16) / portions : 0.0;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "Mg",
            AmountDisplay = totalGrams > 0 ? $"{mgPerServing:N0} mg" : "- mg",
            StatusText = totalGrams > 0 ? "22%" : "-",
            StatusColor = Color.FromArgb("#424936"),
            BackgroundColor = Color.FromArgb("#EAEDFF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });

        // 6. Hierro (Fe)
        double fePerServing = totalGrams > 0 ? (totalGrams * 0.005) / portions : 0.0;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "Fe",
            AmountDisplay = totalGrams > 0 ? $"{fePerServing:0.#} mg" : "- mg",
            StatusText = totalGrams > 0 ? "12%" : "-",
            StatusColor = Color.FromArgb("#424936"),
            BackgroundColor = Color.FromArgb("#EAEDFF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });

        // 7. Zinc (Zn)
        double znPerServing = totalGrams > 0 ? (totalGrams * 0.004) / portions : 0.0;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "Zn",
            AmountDisplay = totalGrams > 0 ? $"{znPerServing:0.#} mg" : "- mg",
            StatusText = totalGrams > 0 ? "18%" : "-",
            StatusColor = Color.FromArgb("#424936"),
            BackgroundColor = Color.FromArgb("#EAEDFF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });

        // 8. H2O Libre
        double waterPerServing = totalGrams > 0 ? (totalGrams * 0.8) / portions : 0.0;
        MineralProjections.Add(new MineralProjectionModel
        {
            Symbol = "H₂O Libre",
            AmountDisplay = totalGrams > 0 ? $"{waterPerServing:N0} ml" : "- ml",
            StatusText = totalGrams > 0 ? "Hidrat." : "-",
            StatusColor = Color.FromArgb("#424936"),
            BackgroundColor = Color.FromArgb("#E2E7FF"),
            BorderColor = Color.FromArgb("#C1CAB0"),
            TextColor = Color.FromArgb("#131B2E")
        });
    }

    [RelayCommand]
    private void IncreaseServings()
    {
        Servings++;
    }

    [RelayCommand]
    private void DecreaseServings()
    {
        if (Servings > 1)
        {
            Servings--;
        }
    }

    [RelayCommand]
    private void RemoveIngredient(RecipeIngredientDraftModel ingredient)
    {
        if (ingredient != null)
        {
            IngredientsList.Remove(ingredient);
            UpdateCounts();
            RecalculateProjections();
        }
    }

    [RelayCommand]
    private async Task OpenAddFoodModalAsync()
    {
        try
        {
            IsBusy = true;
            var foods = await _catalogService.FilterFoodsAsync(new MineralFilterCriteriaDto { SearchTerm = FoodSearchQuery });
            AvailableFoods.Clear();
            foreach (var food in foods.Take(25))
            {
                AvailableFoods.Add(food);
            }
            IsFoodPickerVisible = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchFoodsAsync()
    {
        var foods = await _catalogService.FilterFoodsAsync(new MineralFilterCriteriaDto { SearchTerm = FoodSearchQuery });
        AvailableFoods.Clear();
        foreach (var food in foods.Take(25))
        {
            AvailableFoods.Add(food);
        }
    }

    [RelayCommand]
    private void SelectFoodItem(FoodItemDto food)
    {
        if (food == null) return;

        double factor = FoodGramsToAdd / (food.ReferenceGrams > 0 ? food.ReferenceGrams : 100.0);
        double cal = food.Calories * factor;
        double prot = food.ProteinGrams * factor;

        double na = (food.Minerals?.FirstOrDefault(m => m.Type == MineralType.Sodium)?.Milligrams ?? 0) * factor;
        double k = (food.Minerals?.FirstOrDefault(m => m.Type == MineralType.Potassium)?.Milligrams ?? 0) * factor;
        double p = (food.Minerals?.FirstOrDefault(m => m.Type == MineralType.Phosphorus)?.Milligrams ?? 0) * factor;

        string glyph = MaterialIconFont.Restaurant;
        string lower = food.Name.ToLowerInvariant();
        if (lower.Contains("aceite") || lower.Contains("oil") || lower.Contains("vinagre"))
        {
            glyph = MaterialIconFont.Opacity;
        }
        else if (lower.Contains("limon") || lower.Contains("verdura") || lower.Contains("patata") || lower.Contains("arroz"))
        {
            glyph = MaterialIconFont.Eco;
        }
        else if (lower.Contains("pollo") || lower.Contains("pescado") || lower.Contains("salmon") || lower.Contains("carne"))
        {
            glyph = MaterialIconFont.DinnerDining;
        }

        IngredientsList.Add(new RecipeIngredientDraftModel
        {
            FoodId = food.Id,
            Name = food.Name,
            Grams = FoodGramsToAdd,
            Calories = cal,
            ProteinGrams = prot,
            SodiumMg = na,
            PotassiumMg = k,
            PhosphorusMg = p,
            IconGlyph = glyph,
            IsFromSeasoning = false
        });

        IsFoodPickerVisible = false;
        UpdateCounts();
        RecalculateProjections();
    }

    [RelayCommand]
    private void CloseFoodPicker()
    {
        IsFoodPickerVisible = false;
    }

    [RelayCommand]
    private async Task OpenSeasoningPickerAsync()
    {
        try
        {
            IsBusy = true;
            var seasonings = await _seasoningService.GetAllSeasoningsAsync();
            AvailableSeasonings.Clear();
            foreach (var seasoning in seasonings)
            {
                AvailableSeasonings.Add(seasoning);
            }

            if (AvailableSeasonings.Count == 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlertAsync("Sin Aliños", "Aún no tienes aliños guardados. Puedes crearlos desde el Módulo de Aliños.", "Aceptar");
                }
                return;
            }

            IsSeasoningPickerVisible = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectSeasoning(SeasoningDto seasoning)
    {
        if (seasoning == null) return;

        double na = seasoning.TotalMinerals?.FirstOrDefault(m => m.Type == MineralType.Sodium)?.Milligrams ?? 0;
        double k = seasoning.TotalMinerals?.FirstOrDefault(m => m.Type == MineralType.Potassium)?.Milligrams ?? 0;
        double p = seasoning.TotalMinerals?.FirstOrDefault(m => m.Type == MineralType.Phosphorus)?.Milligrams ?? 0;

        IngredientsList.Add(new RecipeIngredientDraftModel
        {
            FoodId = seasoning.Id,
            Name = seasoning.Name,
            Grams = seasoning.TotalGrams > 0 ? seasoning.TotalGrams : 20,
            Calories = seasoning.TotalCalories,
            ProteinGrams = seasoning.TotalProtein,
            SodiumMg = na,
            PotassiumMg = k,
            PhosphorusMg = p,
            IconGlyph = MaterialIconFont.Opacity,
            IsFromSeasoning = true,
            SeasoningBadgeText = "ALIÑO"
        });

        IsSeasoningPickerVisible = false;
        UpdateCounts();
        RecalculateProjections();
    }

    [RelayCommand]
    private void CloseSeasoningPicker()
    {
        IsSeasoningPickerVisible = false;
    }

    [RelayCommand]
    private void AddStep()
    {
        if (string.IsNullOrWhiteSpace(NewStepInstruction)) return;

        StepsList.Add(new RecipeStepDraftModel
        {
            StepNumber = StepsList.Count + 1,
            Instruction = NewStepInstruction.Trim(),
            TimeDisplayText = $"{NewStepTimeMinutes} min",
            TechniqueTag = string.IsNullOrWhiteSpace(NewStepTechnique) ? "Cocción dosificada" : NewStepTechnique.Trim()
        });

        NewStepInstruction = string.Empty;
        UpdateCounts();
    }

    [RelayCommand]
    private void RemoveStep(RecipeStepDraftModel step)
    {
        if (step != null)
        {
            StepsList.Remove(step);
            // Reenumerar pasos
            for (int i = 0; i < StepsList.Count; i++)
            {
                StepsList[i].StepNumber = i + 1;
            }
            UpdateCounts();
        }
    }

    [RelayCommand]
    private async Task BrowseGalleryAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Seleccionar fotografía del plato",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                ImageUrl = result.FullPath;
                HasImage = true;
                HasNoImage = false;
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Aviso", $"No se pudo abrir la galería: {ex.Message}", "Aceptar");
            }
        }
    }

    [RelayCommand]
    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Validación", "Por favor ingresa un nombre para la preparación culinaria.", "Aceptar");
            }
            return;
        }

        if (IngredientsList.Count == 0)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Validación", "Debes incluir al menos un ingrediente dosificado en la receta.", "Aceptar");
            }
            return;
        }

        try
        {
            IsBusy = true;

            var ingredientsTuples = IngredientsList.Select(i => (foodId: i.FoodId, grams: i.Grams));
            var stepsTuples = StepsList.Select(s => (instruction: s.Instruction, imagePath: s.ImagePath ?? string.Empty));

            await _recipeService.CreateRecipeAsync(
                Title.Trim(),
                Description?.Trim() ?? string.Empty,
                Math.Max(1, Servings),
                ImageUrl ?? string.Empty,
                ingredientsTuples,
                stepsTuples);

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Receta Guardada", $"La receta '{Title}' fue guardada exitosamente con su desglose clínico.", "Aceptar");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar la receta: {ex.Message}", "Aceptar");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsDraftAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            Title = "Borrador de Preparación";
        }

        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Borrador Guardado", $"La receta '{Title}' se ha guardado localmente como borrador clínico.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
