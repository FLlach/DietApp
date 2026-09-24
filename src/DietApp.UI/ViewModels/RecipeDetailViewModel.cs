using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de detalle completo de una receta culinaria.
/// Ademas de exponer la receta, los ingredientes y los pasos numerados, permite agregar directamente
/// la receta consumida (o porciones de ella) a la ingesta diaria de alimentos en cualquier fecha y comida.
/// Por que se tomo esta decision: Habilita un flujo de usuario directo y sin friccion: quien consulta una receta
/// puede registrar de inmediato su consumo en el historial diario con un solo toque.
/// </summary>
[QueryProperty(nameof(RecipeIdString), "RecipeId")]
public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IMealTrackingService _mealTrackingService;

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

    public ObservableCollection<string> MealTypeOptions { get; } = new();

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        IMealTrackingService mealTrackingService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _mealTrackingService = mealTrackingService ?? throw new ArgumentNullException(nameof(mealTrackingService));

        MealTypeOptions.Add("Desayuno");
        MealTypeOptions.Add("Almuerzo");
        MealTypeOptions.Add("Cena");
        MealTypeOptions.Add("Merienda / Colacion");
        MealTypeOptions.Add("Otro");
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
            }
            finally
            {
                IsBusy = false;
            }
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

            var mealType = SelectedMealTypeName switch
            {
                "Desayuno" => MealType.Breakfast,
                "Almuerzo" => MealType.Lunch,
                "Cena" => MealType.Dinner,
                "Merienda / Colacion" => MealType.Snack,
                _ => MealType.Other
            };

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
