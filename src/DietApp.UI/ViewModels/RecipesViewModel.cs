using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel responsable de la gestion del listado, busqueda y ordenamiento de recetas culinarias.
/// Permite al usuario filtrar por texto y ordenar dinamicamente las recetas segun la cantidad de un mineral
/// especifico por porcion (en orden ascendente o descendente).
/// Por que se tomo esta decision: En el patron MVVM con DDD, centraliza la logica de ordenamiento y filtrado
/// en memoria manteniendo la reactividad de la vista y adaptando las opciones segun el idioma activo sin duplicar consultas.
/// </summary>
public partial class RecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly ILocalizationService _localizationService;
    private List<RecipeDto> _rawLoadedRecipes = new();

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial RecipeDto? SelectedRecipe { get; set; }

    [ObservableProperty]
    public partial MineralSortOption? SelectedMineralOption { get; set; }

    [ObservableProperty]
    public partial SortDirectionOption? SelectedDirectionOption { get; set; }

    public ObservableCollection<RecipeDto> Recipes { get; } = new();
    public ObservableCollection<MineralSortOption> MineralOptions { get; } = new();
    public ObservableCollection<SortDirectionOption> DirectionOptions { get; } = new();

    public RecipesViewModel(
        IRecipeService recipeService,
        ILocalizationService localizationService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        InitializeSortOptions();
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        InitializeSortOptions();
        ApplyFilterAndSort();
    }

    private void InitializeSortOptions()
    {
        var previousMineral = SelectedMineralOption?.Mineral;
        var previousDescending = SelectedDirectionOption?.IsDescending ?? true;

        MineralOptions.Clear();
        MineralOptions.Add(new MineralSortOption
        {
            Mineral = null,
            DisplayName = _localizationService["Sort_Default"]
        });

        foreach (MineralType mineral in Enum.GetValues<MineralType>())
        {
            MineralOptions.Add(new MineralSortOption
            {
                Mineral = mineral,
                DisplayName = _localizationService.GetMineralName(mineral)
            });
        }

        DirectionOptions.Clear();
        DirectionOptions.Add(new SortDirectionOption
        {
            IsDescending = true,
            DisplayName = _localizationService["Sort_Descending"]
        });
        DirectionOptions.Add(new SortDirectionOption
        {
            IsDescending = false,
            DisplayName = _localizationService["Sort_Ascending"]
        });

        SelectedMineralOption = MineralOptions.FirstOrDefault(o => o.Mineral == previousMineral) ?? MineralOptions[0];
        SelectedDirectionOption = DirectionOptions.FirstOrDefault(d => d.IsDescending == previousDescending) ?? DirectionOptions[0];
    }

    partial void OnSelectedMineralOptionChanged(MineralSortOption? value)
    {
        ApplyFilterAndSort();
    }

    partial void OnSelectedDirectionOptionChanged(SortDirectionOption? value)
    {
        ApplyFilterAndSort();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilterAndSort();
    }

    [RelayCommand]
    public async Task LoadRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _recipeService.GetAllRecipesAsync();
            _rawLoadedRecipes = items.ToList();
            ApplyFilterAndSort();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _recipeService.SearchRecipesAsync(SearchText);
            _rawLoadedRecipes = items.ToList();
            ApplyFilterAndSort();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ResetSort()
    {
        SelectedMineralOption = MineralOptions.FirstOrDefault(o => o.Mineral == null) ?? MineralOptions[0];
        SelectedDirectionOption = DirectionOptions.FirstOrDefault(d => d.IsDescending) ?? DirectionOptions[0];
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        IEnumerable<RecipeDto> query = _rawLoadedRecipes;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(recipe =>
                recipe.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                recipe.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedMineralOption?.Mineral is MineralType mineral)
        {
            bool isDescending = SelectedDirectionOption?.IsDescending ?? true;
            query = isDescending
                ? query.OrderByDescending(recipe => recipe.GetMineralAmountPerServing(mineral)).ThenBy(recipe => recipe.Title)
                : query.OrderBy(recipe => recipe.GetMineralAmountPerServing(mineral)).ThenBy(recipe => recipe.Title);
        }

        Recipes.Clear();
        foreach (var recipe in query)
        {
            Recipes.Add(recipe);
        }
    }

    [RelayCommand]
    public async Task SelectRecipeAsync(RecipeDto recipe)
    {
        if (recipe == null) return;

        SelectedRecipe = recipe;
        await Shell.Current.GoToAsync($"RecipeDetailPage?RecipeId={recipe.Id}");
    }

    [RelayCommand]
    public async Task NavigateToAddRecipeAsync()
    {
        await Shell.Current.GoToAsync("AddRecipePage");
    }

    /// <summary>
    /// Como funciona: Navega de forma modal a la pantalla de gestion de alinos y condimentos (SeasoningsPage).
    /// Por que se tomo esta decision: Consolida los alinos como una opcion y subseccion modular dependiente
    /// del recetario nutricional, liberando espacio en la barra de navegacion inferior (TabBar) para ergonomia movil.
    /// </summary>
    [RelayCommand]
    public async Task NavigateToSeasoningsAsync()
    {
        await Shell.Current.GoToAsync("SeasoningsPage");
    }
}
