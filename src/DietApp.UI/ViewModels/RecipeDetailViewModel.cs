using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de detalle completo de una receta culinaria.
/// Expone la receta, los pasos numerados, permite agregar las porciones consumidas a la ingesta diaria,
/// y permite ordenar interactivamente los ingredientes de la receta segun la cantidad de un mineral elegido.
/// Por que se tomo esta decision: Habilita tanto el registro inmediato de la comida como el analisis
/// nutricional granular para identificar cuales ingredientes aportan mayor o menor cantidad de un compuesto.
/// </summary>
[QueryProperty(nameof(RecipeIdString), "RecipeId")]
public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IMealTrackingService _mealTrackingService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string RecipeIdString { get; set; } = string.Empty;

    [ObservableProperty]
    public partial RecipeDto? Recipe { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial DateTime IntakeDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string SelectedMealTypeName { get; set; } = "Almuerzo";

    [ObservableProperty]
    public partial string ServingsConsumedText { get; set; } = "1";

    [ObservableProperty]
    public partial string IntakeMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial MineralSortOption? SelectedIngredientMineralOption { get; set; }

    [ObservableProperty]
    public partial SortDirectionOption? SelectedIngredientDirectionOption { get; set; }

    public ObservableCollection<string> MealTypeOptions { get; } = new();
    public ObservableCollection<MineralSortOption> IngredientMineralOptions { get; } = new();
    public ObservableCollection<SortDirectionOption> IngredientDirectionOptions { get; } = new();
    public ObservableCollection<RecipeIngredientDto> DisplayedIngredients { get; } = new();

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        IMealTrackingService mealTrackingService,
        ILocalizationService localizationService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        InitializeOptions();
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        InitializeOptions();
        ApplyIngredientSort();
    }

    private void InitializeOptions()
    {
        // Momentos de comida localizados
        MealTypeOptions.Clear();
        MealTypeOptions.Add(_localizationService.GetMealTypeName(MealType.Breakfast));
        MealTypeOptions.Add(_localizationService.GetMealTypeName(MealType.Lunch));
        MealTypeOptions.Add(_localizationService.GetMealTypeName(MealType.Dinner));
        MealTypeOptions.Add(_localizationService.GetMealTypeName(MealType.Snack));
        MealTypeOptions.Add(_localizationService.GetMealTypeName(MealType.Other));

        SelectedMealTypeName = MealTypeOptions.Count > 1 ? MealTypeOptions[1] : string.Empty;

        // Opciones de ordenamiento para ingredientes
        var previousMineral = SelectedIngredientMineralOption?.Mineral;
        var previousDescending = SelectedIngredientDirectionOption?.IsDescending ?? true;

        IngredientMineralOptions.Clear();
        IngredientMineralOptions.Add(new MineralSortOption
        {
            Mineral = null,
            DisplayName = _localizationService["Sort_Default"]
        });

        foreach (MineralType mineral in Enum.GetValues<MineralType>())
        {
            IngredientMineralOptions.Add(new MineralSortOption
            {
                Mineral = mineral,
                DisplayName = _localizationService.GetMineralName(mineral)
            });
        }

        IngredientDirectionOptions.Clear();
        IngredientDirectionOptions.Add(new SortDirectionOption
        {
            IsDescending = true,
            DisplayName = _localizationService["Sort_Descending"]
        });
        IngredientDirectionOptions.Add(new SortDirectionOption
        {
            IsDescending = false,
            DisplayName = _localizationService["Sort_Ascending"]
        });

        SelectedIngredientMineralOption = IngredientMineralOptions.FirstOrDefault(o => o.Mineral == previousMineral) ?? IngredientMineralOptions[0];
        SelectedIngredientDirectionOption = IngredientDirectionOptions.FirstOrDefault(d => d.IsDescending == previousDescending) ?? IngredientDirectionOptions[0];
    }

    partial void OnSelectedIngredientMineralOptionChanged(MineralSortOption? value)
    {
        ApplyIngredientSort();
    }

    partial void OnSelectedIngredientDirectionOptionChanged(SortDirectionOption? value)
    {
        ApplyIngredientSort();
    }

    [RelayCommand]
    public async Task LoadRecipeAsync()
    {
        if (Guid.TryParse(RecipeIdString, out Guid id))
        {
            try
            {
                IsBusy = true;
                Recipe = await _recipeService.GetRecipeByIdAsync(id);
                IntakeMessage = string.Empty;
                ApplyIngredientSort();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private void ApplyIngredientSort()
    {
        if (Recipe == null)
        {
            DisplayedIngredients.Clear();
            return;
        }

        IEnumerable<RecipeIngredientDto> query = Recipe.Ingredients;

        if (SelectedIngredientMineralOption?.Mineral is MineralType mineral)
        {
            bool isDescending = SelectedIngredientDirectionOption?.IsDescending ?? true;
            query = isDescending
                ? query.OrderByDescending(i => i.GetMineralAmount(mineral)).ThenBy(i => i.FoodName)
                : query.OrderBy(i => i.GetMineralAmount(mineral)).ThenBy(i => i.FoodName);
        }

        DisplayedIngredients.Clear();
        foreach (var ingredient in query)
        {
            DisplayedIngredients.Add(ingredient);
        }
    }

    [RelayCommand]
    public async Task AddToDailyIntakeAsync()
    {
        if (Recipe == null)
        {
            IntakeMessage = "No hay receta activa para registrar.";
            return;
        }

        if (!double.TryParse(ServingsConsumedText, out double servings) || servings <= 0)
        {
            IntakeMessage = "Ingresa una cantidad valida de porciones (mayor a cero).";
            return;
        }

        try
        {
            IsBusy = true;

            var mealType = MealType.Lunch;
            if (SelectedMealTypeName == _localizationService.GetMealTypeName(MealType.Breakfast) || SelectedMealTypeName == "Desayuno" || SelectedMealTypeName == "Breakfast")
            {
                mealType = MealType.Breakfast;
            }
            else if (SelectedMealTypeName == _localizationService.GetMealTypeName(MealType.Lunch) || SelectedMealTypeName == "Almuerzo" || SelectedMealTypeName == "Lunch")
            {
                mealType = MealType.Lunch;
            }
            else if (SelectedMealTypeName == _localizationService.GetMealTypeName(MealType.Dinner) || SelectedMealTypeName == "Cena" || SelectedMealTypeName == "Dinner")
            {
                mealType = MealType.Dinner;
            }
            else if (SelectedMealTypeName == _localizationService.GetMealTypeName(MealType.Snack) || SelectedMealTypeName == "Merienda / Colacion" || SelectedMealTypeName == "Snack / Collation")
            {
                mealType = MealType.Snack;
            }
            else
            {
                mealType = MealType.Other;
            }

            await _mealTrackingService.RecordRecipeInMealAsync(
                IntakeDate,
                mealType,
                Recipe.Id,
                servings);

            string portionWord = servings == 1 ? "1 porcion" : $"{servings:0.##} porciones";
            IntakeMessage = $"Agregado: {Recipe.Title} ({portionWord}) a {SelectedMealTypeName} del {IntakeDate:dd/MM/yyyy}. Balance de minerales actualizado.";
        }
        catch (Exception ex)
        {
            IntakeMessage = $"Error al registrar en la ingesta: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
