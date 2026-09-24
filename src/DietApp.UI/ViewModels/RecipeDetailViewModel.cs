using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de detalle completo de una receta culinaria.
/// Recibe el identificador mediante navegacion de Shell, carga la receta y expone el titulo,
/// la imagen final terminada, el subtitulo nutricional por porcion, los ingredientes y los pasos numerados.
/// Por que se tomo esta decision: Permite una navegacion limpia basada en rutas y URLs internas de MAUI Shell,
/// asegurando que el detalle se refresque con el agregado mas reciente de la capa de aplicacion.
/// </summary>
[QueryProperty(nameof(RecipeIdString), "RecipeId")]
public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    public partial string RecipeIdString { get; set; } = string.Empty;

    [ObservableProperty]
    public partial RecipeDto? Recipe { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public RecipeDetailViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
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
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
