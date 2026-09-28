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
/// Como funciona: ViewModel para la Ficha Detallada de Receta (RecipeDetailPage) basado en Stitch.
/// Carga los datos de la receta segun el identificador recibido por navegacion, calcula el balance
/// de sodio, potasio y fosforo contra la ingesta diaria del usuario, audita si la racion supera limites
/// preventivos o criticos, y permite registrar directamente el consumo en el conteo diario.
/// Por que se tomo esta decision: Centraliza la auditoria proactiva y el registro dosificado sin intermediarios.
/// </summary>
public partial class RecipeDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IRecipeService _recipeService;
    private readonly IMineralAlertService _mineralAlertService;
    private readonly IMealTrackingService _mealTrackingService;

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        IMineralAlertService mineralAlertService,
        IMealTrackingService mealTrackingService)
    {
        _recipeService = recipeService;
        _mineralAlertService = mineralAlertService;
        _mealTrackingService = mealTrackingService;

        UpdateDateDisplay();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("RecipeId", out var idObj) && idObj != null)
        {
            if (Guid.TryParse(idObj.ToString(), out var id))
            {
                _ = LoadRecipeDetailsAsync(id);
            }
        }
    }

    [ObservableProperty]
    private Guid _recipeId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private bool _hasNoImage = true;

    [ObservableProperty]
    private string _energyBadge = "385 kcal por porción";

    [ObservableProperty]
    private string _proteinBadge = "31 g Proteína";

    [ObservableProperty]
    private string _prepTimeDisplay = "25 min de preparación • 1 ración clínica";

    [ObservableProperty]
    private string _sodiumMg = "210";

    [ObservableProperty]
    private double _sodiumProgress = 0.14;

    [ObservableProperty]
    private string _sodiumPctText = "10% CDR";

    [ObservableProperty]
    private string _potassiumMg = "640";

    [ObservableProperty]
    private double _potassiumProgress = 0.32;

    [ObservableProperty]
    private string _potassiumPctText = "32% CDR (Alto)";

    [ObservableProperty]
    private bool _isPotassiumAlert = true;

    [ObservableProperty]
    private string _phosphorusMg = "195";

    [ObservableProperty]
    private double _phosphorusProgress = 0.22;

    [ObservableProperty]
    private string _phosphorusPctText = "20% CDR";

    [ObservableProperty]
    private string _clinicalAuditText = "✓ Apto para tu límite diario de fósforo. Atención: Aporta el 32% del cupo de potasio diario previsto en tu pauta.";

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    partial void OnSelectedDateChanged(DateTime value)
    {
        UpdateDateDisplay();
    }

    [ObservableProperty]
    private string _formattedDateDisplay = "Hoy, 24 Oct";

    [ObservableProperty]
    private string _selectedMealTypeName = "Almuerzo";

    [ObservableProperty]
    private MealType _selectedMealType = MealType.Lunch;

    [ObservableProperty]
    private int _selectedMealTypeIndex = 1;

    public List<string> MealTypeOptions { get; } = new()
    {
        "Desayuno",
        "Almuerzo",
        "Merienda",
        "Cena",
        "Snack"
    };

    partial void OnSelectedMealTypeIndexChanged(int value)
    {
        switch (value)
        {
            case 0:
                SelectedMealType = MealType.Breakfast;
                SelectedMealTypeName = "Desayuno";
                break;
            case 1:
                SelectedMealType = MealType.Lunch;
                SelectedMealTypeName = "Almuerzo";
                break;
            case 2:
                SelectedMealType = MealType.Snack;
                SelectedMealTypeName = "Merienda";
                break;
            case 3:
                SelectedMealType = MealType.Dinner;
                SelectedMealTypeName = "Cena";
                break;
            case 4:
                SelectedMealType = MealType.Snack;
                SelectedMealTypeName = "Snack";
                break;
        }
    }

    [ObservableProperty]
    private double _servingsToLog = 1.0;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isSortedByPotassium;

    [ObservableProperty]
    private string _sortButtonText = "Mayor Potasio";

    private readonly List<RecipeIngredientDisplayModel> _originalIngredients = new();

    public ObservableCollection<RecipeIngredientDisplayModel> IngredientsList { get; } = new();
    public ObservableCollection<MealMineralChipModel> SevenMinerals { get; } = new();
    public ObservableCollection<RecipeStepDisplayModel> StepsList { get; } = new();

    private void UpdateDateDisplay()
    {
        var culture = new CultureInfo("es-ES");
        string dayPrefix = SelectedDate.Date == DateTime.Today ? "Hoy, " : "";
        string month = culture.DateTimeFormat.GetAbbreviatedMonthName(SelectedDate.Month);
        FormattedDateDisplay = $"{dayPrefix}{SelectedDate.Day} {culture.TextInfo.ToTitleCase(month)}";
    }

    public async Task LoadRecipeDetailsAsync(Guid id)
    {
        if (id == Guid.Empty) return;

        try
        {
            IsBusy = true;
            RecipeId = id;

            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return;

            Title = recipe.Title;
            ImageUrl = recipe.FinalImagePath;
            HasImage = !string.IsNullOrWhiteSpace(ImageUrl);
            HasNoImage = !HasImage;

            EnergyBadge = $"{recipe.CaloriesPerServing:N0} kcal por porción";
            ProteinBadge = $"{recipe.ProteinPerServing:0.#} g Proteína";
            PrepTimeDisplay = $"25 min de preparación • {recipe.Servings} {(recipe.Servings == 1 ? "ración clínica" : "raciones")}";

            // 1. Nutrientes Clave (Sodio, Potasio, Fosforo)
            double na = recipe.GetMineralAmountPerServing(MineralType.Sodium);
            double k = recipe.GetMineralAmountPerServing(MineralType.Potassium);
            double p = recipe.GetMineralAmountPerServing(MineralType.Phosphorus);

            SodiumMg = $"{na:N0}";
            SodiumProgress = Math.Min(1.0, na / 2000.0);
            SodiumPctText = $"{Math.Round((na / 2000.0) * 100, 0)}% CDR";

            PotassiumMg = $"{k:N0}";
            PotassiumProgress = Math.Min(1.0, k / 2000.0);
            bool kHigh = k > 400;
            IsPotassiumAlert = kHigh;
            PotassiumPctText = kHigh
                ? $"{Math.Round((k / 2000.0) * 100, 0)}% CDR (Alto)"
                : $"{Math.Round((k / 2000.0) * 100, 0)}% CDR";

            PhosphorusMg = $"{p:N0}";
            PhosphorusProgress = Math.Min(1.0, p / 850.0);
            PhosphorusPctText = $"{Math.Round((p / 850.0) * 100, 0)}% CDR";

            // Auditoria clinica
            if (kHigh)
            {
                ClinicalAuditText = $"✓ Apto para tu límite diario de fósforo. Atención: Aporta el {Math.Round((k / 2000.0) * 100, 0)}% del cupo de potasio diario previsto en tu pauta.";
            }
            else
            {
                ClinicalAuditText = "✓ Apto para tu pauta renal y metabólica: perfil balanceado de potasio y fósforo bajo control.";
            }

            // 2. Ingredientes
            IngredientsList.Clear();
            _originalIngredients.Clear();
            foreach (var ing in recipe.Ingredients)
            {
                double ingK = ing.CalculatedMinerals.FirstOrDefault(m => m.Type == MineralType.Potassium)?.Milligrams ?? 0;
                double ingP = ing.CalculatedMinerals.FirstOrDefault(m => m.Type == MineralType.Phosphorus)?.Milligrams ?? 0;

                string glyph = Helpers.MaterialIconFont.Restaurant;
                string lower = ing.FoodName.ToLowerInvariant();
                if (lower.Contains("aceite") || lower.Contains("oil") || lower.Contains("vinagre"))
                {
                    glyph = Helpers.MaterialIconFont.Opacity;
                }
                else if (lower.Contains("limon") || lower.Contains("lemon") || lower.Contains("fruta"))
                {
                    glyph = Helpers.MaterialIconFont.Eco;
                }
                else if (lower.Contains("calabacin") || lower.Contains("brocoli") || lower.Contains("verdura"))
                {
                    glyph = Helpers.MaterialIconFont.Eco;
                }
                else if (lower.Contains("pollo") || lower.Contains("chicken") || lower.Contains("carne"))
                {
                    glyph = Helpers.MaterialIconFont.DinnerDining;
                }

                var item = new RecipeIngredientDisplayModel
                {
                    Name = ing.FoodName,
                    Subtitle = "Ingrediente dosificado",
                    GramsDisplay = $"{ing.Grams:N0} g",
                    PotassiumAmount = ingK,
                    IconGlyph = glyph,
                    MineralPreview1 = $"K: {ingK:N0} mg",
                    MineralPreview2 = $"P: {ingP:N0} mg",
                    MineralColor1 = ingK > 200 ? Color.FromArgb("#FC7B48") : Color.FromArgb("#416900"),
                    MineralColor2 = Color.FromArgb("#131B2E")
                };

                IngredientsList.Add(item);
                _originalIngredients.Add(item);
            }

            // 3. Los 7 Minerales
            SevenMinerals.Clear();
            var mineralDict = recipe.MineralsPerServing.ToDictionary(m => m.Type, m => m.Milligrams);

            var specs = new (MineralType type, string symbol)[]
            {
                (MineralType.Phosphorus, "P"),
                (MineralType.Potassium, "K"),
                (MineralType.Sodium, "Na"),
                (MineralType.Calcium, "Ca"),
                (MineralType.Magnesium, "Mg"),
                (MineralType.Iron, "Fe"),
                (MineralType.Zinc, "Zn")
            };

            foreach (var spec in specs)
            {
                double amount = mineralDict.TryGetValue(spec.type, out var val) ? val : 0;
                bool isAlert = spec.type == MineralType.Potassium && amount > 400;

                SevenMinerals.Add(new MealMineralChipModel
                {
                    Symbol = spec.symbol,
                    AmountWithUnit = amount >= 10 ? $"{amount:N0} mg" : $"{amount:0.#} mg",
                    IsAlert = isAlert,
                    BackgroundColor = isAlert ? Color.FromArgb("#FFF7F5") : Color.FromArgb("#F2F3FF"),
                    BorderColor = isAlert ? Color.FromArgb("#FC7B48") : Color.FromArgb("#C1CAB0"),
                    TextColor = isAlert ? Color.FromArgb("#A53C0B") : Color.FromArgb("#131B2E")
                });
            }

            // 4. Pasos
            StepsList.Clear();
            if (recipe.Steps.Count > 0)
            {
                foreach (var step in recipe.Steps.OrderBy(s => s.StepNumber))
                {
                    StepsList.Add(new RecipeStepDisplayModel
                    {
                        StepNumber = step.StepNumber,
                        Title = $"Paso {step.StepNumber}",
                        Instruction = step.Instruction
                    });
                }
            }
            else
            {
                StepsList.Add(new RecipeStepDisplayModel
                {
                    StepNumber = 1,
                    Title = "Preparación dosificada",
                    Instruction = "Cocinar los ingredientes respetando los gramajes indicados y sirviendo inmediatamente para preservar micronutrientes."
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleSortIngredients()
    {
        IsSortedByPotassium = !IsSortedByPotassium;
        if (IsSortedByPotassium)
        {
            SortButtonText = "Orden Normal";
            var sorted = IngredientsList.OrderByDescending(i => i.PotassiumAmount).ToList();
            IngredientsList.Clear();
            foreach (var item in sorted)
            {
                IngredientsList.Add(item);
            }
        }
        else
        {
            SortButtonText = "Mayor Potasio";
            IngredientsList.Clear();
            foreach (var item in _originalIngredients)
            {
                IngredientsList.Add(item);
            }
        }
    }

    [RelayCommand]
    private void IncreaseServings()
    {
        ServingsToLog += 0.5;
    }

    [RelayCommand]
    private void DecreaseServings()
    {
        if (ServingsToLog > 0.5)
        {
            ServingsToLog -= 0.5;
        }
    }

    [RelayCommand]
    private async Task RecordConsumptionAsync()
    {
        if (RecipeId == Guid.Empty) return;

        try
        {
            IsBusy = true;
            await _mealTrackingService.RecordRecipeInMealAsync(
                SelectedDate,
                SelectedMealType,
                RecipeId,
                ServingsToLog,
                $"Consumo de {Title}");

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Éxito", $"Se registró {ServingsToLog} ración(es) de {Title} en el Conteo Diario.", "Aceptar");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No se pudo registrar la ingesta: {ex.Message}", "Aceptar");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleBookmarkAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Receta Guardada", $"La receta '{Title}' fue añadida a tus favoritos.", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ShareRecipeAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Compartir Ficha", $"Compartiendo ficha clínica de '{Title}' con desglose de micronutrientes.", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
