using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la creacion y registro de comidas consumidas. Permite seleccionar
/// alimentos del catalogo, especificar la porcion exacta en gramos para cada uno, y ver la lista
/// preliminar antes de persistir la comida completa.
/// Por que se tomo esta decision: Emplea propiedades parciales de C# 13 con CommunityToolkit.Mvvm,
/// asegurando compatibilidad WinRT y AOT mientras desacopla la vista de la capa de aplicacion.
/// </summary>
public partial class AddMealViewModel : ObservableObject
{
    private readonly IFoodCatalogService _foodCatalogService;
    private readonly IRecipeService _recipeService;
    private readonly IMealTrackingService _mealTrackingService;

    [ObservableProperty]
    public partial DateTime MealDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string SelectedMealTypeName { get; set; } = "Almuerzo";

    [ObservableProperty]
    public partial string Note { get; set; } = string.Empty;

    [ObservableProperty]
    public partial FoodItemDto? SelectedFood { get; set; }

    [ObservableProperty]
    public partial string PortionGramsText { get; set; } = "100";

    [ObservableProperty]
    public partial RecipeDto? SelectedRecipe { get; set; }

    [ObservableProperty]
    public partial string RecipeServingsText { get; set; } = "1";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();
    public ObservableCollection<RecipeDto> AvailableRecipes { get; } = new();
    public ObservableCollection<string> MealTypeOptions { get; } = new();
    public ObservableCollection<MealDraftItemModel> SelectedItems { get; } = new();

    public AddMealViewModel(
        IFoodCatalogService foodCatalogService,
        IRecipeService recipeService,
        IMealTrackingService mealTrackingService)
    {
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));

        MealTypeOptions.Add("Desayuno");
        MealTypeOptions.Add("Almuerzo");
        MealTypeOptions.Add("Cena");
        MealTypeOptions.Add("Merienda / Colacion");
        MealTypeOptions.Add("Otro");
    }

    [RelayCommand]
    public async Task LoadAvailableFoodsAndRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var foodsTask = _foodCatalogService.GetAllFoodsAsync();
            var recipesTask = _recipeService.GetAllRecipesAsync();

            await Task.WhenAll(foodsTask, recipesTask);

            AvailableFoods.Clear();
            foreach (var food in await foodsTask)
            {
                AvailableFoods.Add(food);
            }

            AvailableRecipes.Clear();
            foreach (var recipe in await recipesTask)
            {
                AvailableRecipes.Add(recipe);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public Task LoadAvailableFoodsAsync() => LoadAvailableFoodsAndRecipesAsync();

    [RelayCommand]
    public void AddFoodToMeal()
    {
        if (SelectedFood == null)
        {
            StatusMessage = "Selecciona un alimento de la lista.";
            return;
        }

        if (!double.TryParse(PortionGramsText, out double grams) || grams <= 0)
        {
            StatusMessage = "Ingresa una porcion valida en gramos (mayor a 0).";
            return;
        }

        SelectedItems.Add(new MealDraftItemModel
        {
            ItemId = SelectedFood.Id,
            ItemName = SelectedFood.Name,
            Quantity = grams,
            IsRecipe = false
        });

        StatusMessage = $"Agregado: {SelectedFood.Name} ({grams:F0}g)";
        PortionGramsText = "100";
    }

    [RelayCommand]
    public void AddRecipeToMeal()
    {
        if (SelectedRecipe == null)
        {
            StatusMessage = "Selecciona una receta de la lista.";
            return;
        }

        if (!double.TryParse(RecipeServingsText, out double servings) || servings <= 0)
        {
            StatusMessage = "Ingresa una cantidad valida de porciones (mayor a 0).";
            return;
        }

        string servingWord = servings == 1 ? "1 porcion" : $"{servings:0.##} porciones";
        SelectedItems.Add(new MealDraftItemModel
        {
            ItemId = SelectedRecipe.Id,
            ItemName = $"{SelectedRecipe.Title} (Receta)",
            Quantity = servings,
            IsRecipe = true
        });

        StatusMessage = $"Receta agregada: {SelectedRecipe.Title} ({servingWord})";
        RecipeServingsText = "1";
    }

    [RelayCommand]
    public void RemoveItem(MealDraftItemModel item)
    {
        if (item != null)
        {
            SelectedItems.Remove(item);
        }
    }

    [RelayCommand]
    public async Task SaveMealAsync()
    {
        if (SelectedItems.Count == 0)
        {
            StatusMessage = "Agrega al menos un alimento o receta a la comida.";
            return;
        }

        try
        {
            IsBusy = true;

            var mealType = SelectedMealTypeName switch
            {
                "Desayuno" => MealType.Breakfast,
                "Almuerzo" => MealType.Lunch,
                "Cena" => MealType.Dinner,
                "Merienda / Colacion" => MealType.Snack,
                _ => MealType.Other
            };

            var itemsToSave = SelectedItems.Select(i => (i.ItemId, i.Quantity, i.IsRecipe)).ToList();
            await _mealTrackingService.RecordMealWithMixedItemsAsync(MealDate, mealType, Note, itemsToSave);

            StatusMessage = "Comida registrada correctamente con el conteo de minerales actualizado.";
            SelectedItems.Clear();
            Note = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
