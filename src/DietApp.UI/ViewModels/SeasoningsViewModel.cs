using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la seccion de alinos y condimentos. Administra la consulta del recetario de alinos,
/// la creacion interactiva de nuevas mezclas con alimentos del catalogo y su eliminacion.
/// Por que se tomo esta decision: En el patron MVVM, centraliza la logica de presentacion de alinos,
/// asegurando reactividad, gestion de estado y desacoplamiento con respecto a los servicios de datos.
/// </summary>
public partial class SeasoningsViewModel : ObservableObject
{
    private readonly ISeasoningService _seasoningService;
    private readonly IFoodCatalogService _foodCatalogService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsCreatePanelVisible { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial FoodItemDto? SelectedFood { get; set; }

    [ObservableProperty]
    public partial string GramsText { get; set; } = "15";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    public partial string StatusMessage { get; set; } = string.Empty;

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    [ObservableProperty]
    public partial bool HasSeasonings { get; set; }

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = "Aliños";

    public ObservableCollection<SeasoningDto> Seasonings { get; } = new();
    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();
    public ObservableCollection<SeasoningDraftItemModel> DraftItems { get; } = new();
    public ObservableCollection<MealModuleSummaryModel> SummaryModules { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<ComponentSelectionItemModel> SelectableSeasonings { get; } = new();

    public SeasoningsViewModel(
        ISeasoningService seasoningService,
        IFoodCatalogService foodCatalogService,
        ILocalizationService localizationService)
    {
        _seasoningService = seasoningService ?? throw new ArgumentNullException(nameof(seasoningService));
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        InitializeMockupModules();
    }

    private void InitializeMockupModules()
    {
        SummaryModules.Clear();
        SummaryModules.Add(new MealModuleSummaryModel
        {
            Title = "Receta ABCD",
            PortionText = "1 porcion",
            BadgeIcon = "!",
            IsAlertBadge = true,
            NutrientPills = new List<NutrientPillItemModel>
            {
                new() { Text = "0,3 Sodio", IsHighlighted = false },
                new() { Text = "150 Potasio", IsHighlighted = true },
                new() { Text = "0,1 Fosforo", IsHighlighted = false }
            }
        });

        SummaryModules.Add(new MealModuleSummaryModel
        {
            Title = "Postre",
            PortionText = "1 porcion",
            BadgeIcon = "✓",
            IsAlertBadge = false,
            ImagePath = "module_dessert.jpg",
            NutrientPills = new List<NutrientPillItemModel>
            {
                new() { Text = "100 Fibra", IsHighlighted = false },
                new() { Text = "200 Azucar", IsHighlighted = false }
            }
        });

        SummaryModules.Add(new MealModuleSummaryModel
        {
            Title = "Liquido",
            PortionText = "Min. 200 ml",
            BadgeIcon = "✓",
            IsAlertBadge = false,
            ImagePath = "module_liquid.jpg",
            NutrientPills = new List<NutrientPillItemModel>
            {
                new() { Text = "Agua Mineral", IsHighlighted = false },
                new() { Text = "Jugo Natural", IsHighlighted = false }
            }
        });

        Categories.Clear();
        Categories.Add("Base");
        Categories.Add("Alinos");
        Categories.Add("Postre");
        Categories.Add("Liquido");

        SelectableSeasonings.Clear();
        SelectableSeasonings.Add(new ComponentSelectionItemModel
        {
            Id = "custom",
            Title = "#Personalizado",
            ImagePath = "seasoning_custom.jpg",
            IsSelected = true
        });
        SelectableSeasonings.Add(new ComponentSelectionItemModel
        {
            Id = "olive_oil",
            Title = "Aceite de Oliva",
            ImagePath = "seasoning_olive_oil.jpg",
            IsSelected = false
        });
        SelectableSeasonings.Add(new ComponentSelectionItemModel
        {
            Id = "oregano",
            Title = "Oregano",
            ImagePath = "seasoning_oregano.jpg",
            IsSelected = false
        });
        SelectableSeasonings.Add(new ComponentSelectionItemModel
        {
            Id = "canola_oil",
            Title = "Aceite de Canola",
            ImagePath = "seasoning_canola_oil.jpg",
            IsSelected = false
        });
    }

    [RelayCommand]
    public void SelectCategory(string category)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            SelectedCategory = category;
        }
    }

    [RelayCommand]
    public void ToggleSelectableItem(ComponentSelectionItemModel item)
    {
        if (item != null)
        {
            item.IsSelected = !item.IsSelected;
            if (item.IsSelected && item.Id == "custom")
            {
                IsCreatePanelVisible = true;
            }
        }
    }

    [RelayCommand]
    public void CompleteSelection()
    {
        var selectedNames = SelectableSeasonings.Where(s => s.IsSelected).Select(s => s.Title).ToList();
        string summary = selectedNames.Count > 0 ? string.Join(", ", selectedNames) : "Alino base";
        StatusMessage = $"Seleccion completada ({summary}). Balance de componentes y alinos consolidado.";
    }

    [RelayCommand]
    public async Task LoadSeasoningsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var seasonings = await _seasoningService.GetAllSeasoningsAsync();
            Seasonings.Clear();
            foreach (var seasoning in seasonings)
            {
                Seasonings.Add(seasoning);
            }
            HasSeasonings = Seasonings.Count > 0;

            if (AvailableFoods.Count == 0)
            {
                var foods = await _foodCatalogService.GetAllFoodsAsync();
                AvailableFoods.Clear();
                foreach (var food in foods)
                {
                    AvailableFoods.Add(food);
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ToggleCreatePanel()
    {
        IsCreatePanelVisible = !IsCreatePanelVisible;
        if (!IsCreatePanelVisible)
        {
            ResetCreateForm();
        }
    }

    [RelayCommand]
    public void AddDraftItem()
    {
        if (SelectedFood == null)
        {
            StatusMessage = _localizationService.GetString("Seasonings_SelectFoodPlaceholder");
            return;
        }

        if (!double.TryParse(GramsText, out double grams) || grams <= 0)
        {
            StatusMessage = "Ingresa una cantidad valida en gramos (mayor a 0).";
            return;
        }

        DraftItems.Add(new SeasoningDraftItemModel
        {
            FoodId = SelectedFood.Id,
            FoodName = SelectedFood.Name,
            Grams = grams
        });

        StatusMessage = $"{SelectedFood.Name} ({grams:F0}g) agregado.";
        GramsText = "15";
    }

    [RelayCommand]
    public void RemoveDraftItem(SeasoningDraftItemModel item)
    {
        if (item != null)
        {
            DraftItems.Remove(item);
        }
    }

    [RelayCommand]
    public async Task SaveSeasoningAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = _localizationService.GetString("Seasonings_ValidationRequired");
            return;
        }

        if (DraftItems.Count == 0)
        {
            StatusMessage = _localizationService.GetString("Seasonings_ValidationRequired");
            return;
        }

        try
        {
            IsBusy = true;

            var items = DraftItems.Select(d => (d.FoodId, d.Grams)).ToList();
            await _seasoningService.CreateSeasoningAsync(Name, Description, items);

            StatusMessage = _localizationService.GetString("Seasonings_SavedSuccess");
            ResetCreateForm();
            IsCreatePanelVisible = false;

            await LoadSeasoningsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteSeasoningAsync(Guid id)
    {
        try
        {
            IsBusy = true;
            await _seasoningService.DeleteSeasoningAsync(id);
            StatusMessage = _localizationService.GetString("Seasonings_DeletedSuccess");
            await LoadSeasoningsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetCreateForm()
    {
        Name = string.Empty;
        Description = string.Empty;
        DraftItems.Clear();
        SelectedFood = null;
        GramsText = "15";
    }
}
