using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel responsable de la gestion del estado y comandos de la vista de catalogo
/// de alimentos. Coordina la carga reactiva, busqueda por texto y filtrado especializado por
/// umbrales de minerales (Fosforo, Potasio, Sodio, etc.).
/// Por que se tomo esta decision: Utiliza las propiedades parciales generadas por CommunityToolkit.Mvvm
/// compatibles con AOT y WinRT en C# moderno, eliminando boilerplate y manteniendo enlace fuertemente tipado.
/// </summary>
public partial class FoodCatalogViewModel : ObservableObject
{
    private readonly IFoodCatalogService _foodCatalogService;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedMineralName { get; set; } = "Todos los nutrientes";

    [ObservableProperty]
    public partial string SelectedSortOption { get; set; } = "Por defecto";

    [ObservableProperty]
    public partial string MinimumMilligramsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MaximumMilligramsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<FoodItemDto> Foods { get; } = new();
    public ObservableCollection<string> MineralOptions { get; } = new();
    public ObservableCollection<string> SortOptions { get; } = new();

    public FoodCatalogViewModel(IFoodCatalogService foodCatalogService)
    {
        _foodCatalogService = foodCatalogService ?? throw new ArgumentNullException(nameof(foodCatalogService));

        InitializeMineralOptions();
        InitializeSortOptions();
    }

    private void InitializeMineralOptions()
    {
        MineralOptions.Add("Todos los nutrientes");
        MineralOptions.Add("Proteina (g)");
        MineralOptions.Add("Fosforo");
        MineralOptions.Add("Potasio");
        MineralOptions.Add("Sodio");
        MineralOptions.Add("Calcio");
        MineralOptions.Add("Magnesio");
        MineralOptions.Add("Hierro");
        MineralOptions.Add("Zinc");
    }

    private void InitializeSortOptions()
    {
        SortOptions.Add("Por defecto");
        SortOptions.Add("Proteina (Mayor a menor)");
        SortOptions.Add("Proteina (Menor a mayor)");
        SortOptions.Add("Nombre (A-Z)");
    }

    [RelayCommand]
    public async Task LoadFoodsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _foodCatalogService.GetAllFoodsAsync();
            Foods.Clear();
            foreach (var item in items)
            {
                Foods.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ApplyFilterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var criteria = new MineralFilterCriteriaDto
            {
                SearchTerm = SearchText
            };

            bool isProtein = SelectedMineralName == "Proteina (g)";
            criteria.FilterByProtein = isProtein;

            if (isProtein)
            {
                if (double.TryParse(MinimumMilligramsText, out double minProtein))
                {
                    criteria.MinimumProteinGrams = minProtein;
                }

                if (double.TryParse(MaximumMilligramsText, out double maxProtein))
                {
                    criteria.MaximumProteinGrams = maxProtein;
                }
            }
            else if (SelectedMineralName != "Todos los nutrientes" && SelectedMineralName != "Todos los minerales")
            {
                criteria.SelectedMineral = SelectedMineralName switch
                {
                    "Fosforo" => MineralType.Phosphorus,
                    "Potasio" => MineralType.Potassium,
                    "Sodio" => MineralType.Sodium,
                    "Calcio" => MineralType.Calcium,
                    "Magnesio" => MineralType.Magnesium,
                    "Hierro" => MineralType.Iron,
                    "Zinc" => MineralType.Zinc,
                    _ => null
                };

                if (double.TryParse(MinimumMilligramsText, out double minVal))
                {
                    criteria.MinimumMilligrams = minVal;
                }

                if (double.TryParse(MaximumMilligramsText, out double maxVal))
                {
                    criteria.MaximumMilligrams = maxVal;
                }
            }

            criteria.SortBy = SelectedSortOption switch
            {
                "Proteina (Mayor a menor)" => "ProteinDesc",
                "Proteina (Menor a mayor)" => "ProteinAsc",
                "Nombre (A-Z)" => "NameAsc",
                _ => null
            };

            var items = await _foodCatalogService.FilterFoodsAsync(criteria);
            Foods.Clear();
            foreach (var item in items)
            {
                Foods.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ClearFilterAsync()
    {
        SearchText = string.Empty;
        SelectedMineralName = "Todos los nutrientes";
        SelectedSortOption = "Por defecto";
        MinimumMilligramsText = string.Empty;
        MaximumMilligramsText = string.Empty;
        await LoadFoodsAsync();
    }

    [RelayCommand]
    public async Task NavigateToAddFoodAsync()
    {
        await Shell.Current.GoToAsync("AddFoodPage");
    }
}
