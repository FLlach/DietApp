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

    public ObservableCollection<SeasoningDto> Seasonings { get; } = new();
    public ObservableCollection<FoodItemDto> AvailableFoods { get; } = new();
    public ObservableCollection<SeasoningDraftItemModel> DraftItems { get; } = new();

    public SeasoningsViewModel(
        ISeasoningService seasoningService,
        IFoodCatalogService foodCatalogService,
        ILocalizationService localizationService)
    {
        _seasoningService = seasoningService ?? throw new ArgumentNullException(nameof(seasoningService));
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
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
