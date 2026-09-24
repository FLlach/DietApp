using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel responsable de la gestion del listado y busqueda de recetas culinarias.
/// Muestra cada receta con su titulo, imagen final y subtitulo de minerales y calorias por porcion.
/// Por que se tomo esta decision: En el patron MVVM, aísla la carga y navegacion hacia los detalles
/// de la receta, manteniendo la vista declarativa y reactiva mediante CommunityToolkit.Mvvm.
/// </summary>
public partial class RecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial RecipeDto? SelectedRecipe { get; set; }

    public ObservableCollection<RecipeDto> Recipes { get; } = new();

    public RecipesViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
    }

    [RelayCommand]
    public async Task LoadRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var items = await _recipeService.GetAllRecipesAsync();
            Recipes.Clear();
            foreach (var item in items)
            {
                Recipes.Add(item);
            }
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
            Recipes.Clear();
            foreach (var item in items)
            {
                Recipes.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
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
}
