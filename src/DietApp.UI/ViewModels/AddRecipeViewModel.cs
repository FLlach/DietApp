using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la creacion interactiva de recetas culinarias completas.
/// Permite capturar titulo, descripcion, porciones, imagen final de la receta terminada,
/// agregar ingredientes del catalogo con gramaje especifico y armar los pasos numerados
/// con soporte para incorporar imagenes por cada paso mediante el selector de archivos nativo.
/// Por que se tomo esta decision: Centraliza la construccion del borrador de receta, orquesta
/// el selector de imagenes multiplataforma de MAUI y valida todos los campos antes de delegar
/// la persistencia al RecipeService.
/// </summary>
public partial class AddRecipeViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IFoodCatalogService _foodCatalogService;
    private readonly ISeasoningService _seasoningService;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServingsText { get; set; } = "2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFinalImage))]
    public partial string FinalImagePath { get; set; } = string.Empty;

    public bool HasFinalImage => !string.IsNullOrWhiteSpace(FinalImagePath);

    [ObservableProperty]
    public partial FoodItemDto? SelectedFood { get; set; }

    [ObservableProperty]
    public partial SeasoningDto? SelectedSeasoning { get; set; }

    [ObservableProperty]
    public partial string IngredientGramsText { get; set; } = "100";

    [ObservableProperty]
    public partial string CurrentStepInstruction { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentStepImagePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();
    public ObservableCollection<SeasoningDto> AvailableSeasonings { get; } = new();
    public ObservableCollection<RecipeIngredientDraftModel> Ingredients { get; } = new();
    public ObservableCollection<RecipeStepDraftModel> Steps { get; } = new();

    public AddRecipeViewModel(
        IRecipeService recipeService,
        IFoodCatalogService foodCatalogService,
        ISeasoningService seasoningService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
        _seasoningService = seasoningService ?? throw new ArgumentNullException(nameof(seasoningService));
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

            var seasonings = await _seasoningService.GetAllSeasoningsAsync();
            AvailableSeasonings.Clear();
            foreach (var seasoning in seasonings)
            {
                AvailableSeasonings.Add(seasoning);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ApplySeasoning()
    {
        if (SelectedSeasoning == null)
        {
            StatusMessage = "Selecciona un alino o condimento para incorporar a la receta.";
            return;
        }

        if (SelectedSeasoning.Items == null || SelectedSeasoning.Items.Count == 0)
        {
            StatusMessage = "El alino seleccionado no contiene ingredientes.";
            return;
        }

        int addedCount = 0;
        foreach (var item in SelectedSeasoning.Items)
        {
            Ingredients.Add(new RecipeIngredientDraftModel
            {
                FoodId = item.FoodItemId,
                FoodName = item.FoodName,
                Grams = item.Grams
            });
            addedCount++;
        }

        StatusMessage = $"Se incorporaron {addedCount} ingredientes del alino '{SelectedSeasoning.Name}' a la receta.";
    }

    [RelayCommand]
    public async Task PickFinalImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona la imagen final de la receta terminada",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                FinalImagePath = result.FullPath;
                StatusMessage = "Imagen final de receta seleccionada.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar la imagen: {ex.Message}";
        }
    }

    [RelayCommand]
    public void AddIngredient()
    {
        if (SelectedFood == null)
        {
            StatusMessage = "Selecciona un alimento de la lista para el ingrediente.";
            return;
        }

        if (!double.TryParse(IngredientGramsText, out double grams) || grams <= 0)
        {
            StatusMessage = "Ingresa una cantidad valida en gramos (mayor a cero).";
            return;
        }

        Ingredients.Add(new RecipeIngredientDraftModel
        {
            FoodId = SelectedFood.Id,
            FoodName = SelectedFood.Name,
            Grams = grams
        });

        StatusMessage = $"Ingrediente agregado: {SelectedFood.Name} ({grams:F0}g)";
        IngredientGramsText = "100";
    }

    [RelayCommand]
    public void RemoveIngredient(RecipeIngredientDraftModel item)
    {
        if (item != null)
        {
            Ingredients.Remove(item);
        }
    }

    [RelayCommand]
    public async Task PickStepImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona la imagen ilustrativa del paso",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                CurrentStepImagePath = result.FullPath;
                StatusMessage = "Imagen del paso seleccionada.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar la imagen del paso: {ex.Message}";
        }
    }

    [RelayCommand]
    public void AddStep()
    {
        if (string.IsNullOrWhiteSpace(CurrentStepInstruction))
        {
            StatusMessage = "Escribe la instruccion del paso antes de agregarlo.";
            return;
        }

        int nextNumber = Steps.Count + 1;
        Steps.Add(new RecipeStepDraftModel
        {
            StepNumber = nextNumber,
            Instruction = CurrentStepInstruction.Trim(),
            ImagePath = CurrentStepImagePath.Trim()
        });

        StatusMessage = $"Paso {nextNumber} agregado correctamente.";
        CurrentStepInstruction = string.Empty;
        CurrentStepImagePath = string.Empty;
    }

    [RelayCommand]
    public void RemoveStep(RecipeStepDraftModel step)
    {
        if (step != null)
        {
            Steps.Remove(step);
            // Renumerar pasos restantes
            for (int i = 0; i < Steps.Count; i++)
            {
                Steps[i].StepNumber = i + 1;
            }
        }
    }

    [RelayCommand]
    public async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            StatusMessage = "El titulo de la receta es obligatorio.";
            return;
        }

        if (!int.TryParse(ServingsText, out int servings) || servings <= 0)
        {
            StatusMessage = "El numero de porciones debe ser un entero mayor a cero.";
            return;
        }

        if (Ingredients.Count == 0)
        {
            StatusMessage = "Agrega al menos un ingrediente a la receta.";
            return;
        }

        if (Steps.Count == 0)
        {
            StatusMessage = "Agrega al menos un paso secuencial a la receta.";
            return;
        }

        try
        {
            IsBusy = true;

            var ingredientsToSave = Ingredients.Select(i => (i.FoodId, i.Grams)).ToList();
            var stepsToSave = Steps.Select(s => (s.Instruction, s.ImagePath)).ToList();

            await _recipeService.CreateRecipeAsync(
                Title,
                Description,
                servings,
                FinalImagePath,
                ingredientsToSave,
                stepsToSave);

            StatusMessage = "Receta guardada exitosamente con el calculo nutricional por porcion.";

            // Limpiar formulario tras guardar
            Title = string.Empty;
            Description = string.Empty;
            FinalImagePath = string.Empty;
            Ingredients.Clear();
            Steps.Clear();
            CurrentStepInstruction = string.Empty;
            CurrentStepImagePath = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
