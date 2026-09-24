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
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();
    public ObservableCollection<string> MealTypeOptions { get; } = new();
    public ObservableCollection<MealDraftItemModel> SelectedItems { get; } = new();

    public AddMealViewModel(
        IFoodCatalogService foodCatalogService,
        IMealTrackingService mealTrackingService)
    {
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));

        MealTypeOptions.Add("Desayuno");
        MealTypeOptions.Add("Almuerzo");
        MealTypeOptions.Add("Cena");
        MealTypeOptions.Add("Merienda / Colacion");
        MealTypeOptions.Add("Otro");
    }

    [RelayCommand]
    public async Task LoadAvailableFoodsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var foods = await _foodCatalogService.GetAllFoodsAsync();
            AvailableFoods.Clear();
            foreach (var food in foods)
            {
                AvailableFoods.Add(food);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

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
            FoodId = SelectedFood.Id,
            FoodName = SelectedFood.Name,
            Grams = grams
        });

        StatusMessage = $"Agregado: {SelectedFood.Name} ({grams:F0}g)";
        PortionGramsText = "100";
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
            StatusMessage = "Agrega al menos un alimento a la comida.";
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

            var itemsToSave = SelectedItems.Select(i => (i.FoodId, i.Grams)).ToList();
            await _mealTrackingService.RecordMealAsync(MealDate, mealType, Note, itemsToSave);

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
