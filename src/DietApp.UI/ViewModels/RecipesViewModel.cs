using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para el Catalogo de Recetas Culinarias (RecipesPage) basado en Stitch.
/// Carga las recetas de la base de datos SQLite, permite busqueda reactiva por texto o ingredientes,
/// ordenamiento clinico por minerales (Potasio, Sodio, Fosforo, Proteina, Calorias) y filtrado rápido por pildoras.
/// Por que se tomo esta decision: Orquesta la recuperacion y transformacion de recetas hacia el modelo visual
/// manteniendo desacoplada la interfaz de los repositorios y servicios de aplicacion.
/// </summary>
public partial class RecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IUserProfileService _userProfileService;
    private List<RecipeDto> _allRecipes = new();

    public RecipesViewModel(
        IRecipeService recipeService,
        IUserProfileService userProfileService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));

        SortOptions = new List<string>
        {
            "Menor Potasio",
            "Menor Sodio",
            "Menor Fósforo",
            "Mayor Proteína",
            "Menor Calorías"
        };
        _selectedSortOption = SortOptions[0];

        DirectionOptions = new List<string>
        {
            "Menor a Mayor (Restricción)",
            "Mayor a Menor (Refuerzo)"
        };
        _selectedDirectionOption = DirectionOptions[0];
    }

    public List<string> SortOptions { get; }
    public List<string> DirectionOptions { get; }

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedSortOption;

    [ObservableProperty]
    private string _selectedDirectionOption;

    [ObservableProperty]
    private string _selectedFilterChip = "Todos";

    [ObservableProperty]
    private string _catalogSummaryText = "Catálogo Dosificado (0 preparaciones)";

    [ObservableProperty]
    private bool _hasRecipes;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<RecipeCardDisplayModel> Recipes { get; } = new();

    public async Task InitializeAsync()
    {
        await LoadRecipesAsync();
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var profile = await _userProfileService.GetCurrentProfileAsync();
            string userId = profile?.UserId ?? "local_user";

            var favList = await _recipeService.GetFavoriteRecipesAsync(userId);
            var favIds = new HashSet<Guid>(favList.Select(f => f.Id));

            IReadOnlyList<RecipeDto> list;
            if (SelectedFilterChip == "Globales")
            {
                list = await _recipeService.GetGlobalRecipesAsync();
            }
            else if (SelectedFilterChip == "Favoritos")
            {
                list = favList;
            }
            else
            {
                list = await _recipeService.GetAllRecipesAsync();
            }

            _allRecipes = list.Select(r =>
            {
                r.IsFavorite = favIds.Contains(r.Id);
                return r;
            }).ToList();

            ApplyFilterAndSort();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchRecipesAsync(string query)
    {
        SearchQuery = query;

        if (SelectedFilterChip == "Globales" && !string.IsNullOrWhiteSpace(query))
        {
            try
            {
                IsBusy = true;
                var profile = await _userProfileService.GetCurrentProfileAsync();
                string userId = profile?.UserId ?? "local_user";
                var favList = await _recipeService.GetFavoriteRecipesAsync(userId);
                var favIds = new HashSet<Guid>(favList.Select(f => f.Id));

                var globalResults = await _recipeService.SearchGlobalRecipesAsync(query);
                _allRecipes = globalResults.Select(r =>
                {
                    r.IsFavorite = favIds.Contains(r.Id);
                    return r;
                }).ToList();

                ApplyFilterAndSort();
            }
            finally
            {
                IsBusy = false;
            }
            return;
        }

        ApplyFilterAndSort();
    }

    [RelayCommand]
    private async Task SelectFilterChipAsync(string chipName)
    {
        SelectedFilterChip = (SelectedFilterChip == chipName) ? "Todos" : chipName;
        await LoadRecipesAsync();
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(RecipeCardDisplayModel card)
    {
        if (card == null) return;

        var profile = await _userProfileService.GetCurrentProfileAsync();
        string userId = profile?.UserId ?? "local_user";

        bool isFav = await _recipeService.ToggleFavoriteAsync(userId, card.Id);
        card.IsFavorite = isFav;

        var match = _allRecipes.FirstOrDefault(r => r.Id == card.Id);
        if (match != null)
        {
            match.IsFavorite = isFav;
        }

        if (SelectedFilterChip == "Favoritos" && !isFav)
        {
            Recipes.Remove(card);
            CatalogSummaryText = $"Catálogo Dosificado ({Recipes.Count} preparaciones)";
            HasRecipes = Recipes.Count > 0;
        }
    }

    partial void OnSelectedSortOptionChanged(string value) => ApplyFilterAndSort();
    partial void OnSelectedDirectionOptionChanged(string value) => ApplyFilterAndSort();

    private void ApplyFilterAndSort()
    {
        var query = _allRecipes.AsEnumerable();

        // 1. Filtrado por busqueda
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            query = query.Where(r =>
                r.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                r.Ingredients.Any(i => i.FoodName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));
        }

        // 2. Filtrado por chip rapido
        if (SelectedFilterChip == "Mis Recetas")
        {
            query = query.Where(r => !r.IsGlobal);
        }
        else if (SelectedFilterChip == "Globales")
        {
            query = query.Where(r => r.IsGlobal);
        }
        else if (SelectedFilterChip == "Favoritos")
        {
            query = query.Where(r => r.IsFavorite);
        }
        else if (SelectedFilterChip == "Bajo en Potasio (<300mg)")
        {
            query = query.Where(r => r.GetMineralAmountPerServing(MineralType.Potassium) < 300);
        }
        else if (SelectedFilterChip == "Bajo en Sodio (<200mg)")
        {
            query = query.Where(r => r.GetMineralAmountPerServing(MineralType.Sodium) < 200);
        }
        else if (SelectedFilterChip == "Aporte Proteico Magro")
        {
            query = query.Where(r => r.ProteinPerServing >= 20);
        }
        else if (SelectedFilterChip == "Renal Apto")
        {
            query = query.Where(r => r.GetMineralAmountPerServing(MineralType.Potassium) < 400 && r.GetMineralAmountPerServing(MineralType.Phosphorus) < 250);
        }

        // 3. Ordenamiento
        bool isDescending = SelectedDirectionOption.StartsWith("Mayor", StringComparison.OrdinalIgnoreCase);

        query = SelectedSortOption switch
        {
            "Menor Potasio" => isDescending
                ? query.OrderByDescending(r => r.GetMineralAmountPerServing(MineralType.Potassium))
                : query.OrderBy(r => r.GetMineralAmountPerServing(MineralType.Potassium)),

            "Menor Sodio" => isDescending
                ? query.OrderByDescending(r => r.GetMineralAmountPerServing(MineralType.Sodium))
                : query.OrderBy(r => r.GetMineralAmountPerServing(MineralType.Sodium)),

            "Menor Fósforo" => isDescending
                ? query.OrderByDescending(r => r.GetMineralAmountPerServing(MineralType.Phosphorus))
                : query.OrderBy(r => r.GetMineralAmountPerServing(MineralType.Phosphorus)),

            "Mayor Proteína" => isDescending
                ? query.OrderBy(r => r.ProteinPerServing)
                : query.OrderByDescending(r => r.ProteinPerServing),

            "Menor Calorías" => isDescending
                ? query.OrderByDescending(r => r.CaloriesPerServing)
                : query.OrderBy(r => r.CaloriesPerServing),

            _ => query
        };

        var filteredList = query.ToList();

        Recipes.Clear();
        foreach (var r in filteredList)
        {
            Recipes.Add(BuildCardModel(r));
        }

        CatalogSummaryText = $"Catálogo Dosificado ({Recipes.Count} preparaciones)";
        HasRecipes = Recipes.Count > 0;
    }

    private RecipeCardDisplayModel BuildCardModel(RecipeDto recipe)
    {
        double potasio = recipe.GetMineralAmountPerServing(MineralType.Potassium);
        double sodio = recipe.GetMineralAmountPerServing(MineralType.Sodium);
        double fosforo = recipe.GetMineralAmountPerServing(MineralType.Phosphorus);
        double calcio = recipe.GetMineralAmountPerServing(MineralType.Calcium);
        double magnesio = recipe.GetMineralAmountPerServing(MineralType.Magnesium);
        double hierro = recipe.GetMineralAmountPerServing(MineralType.Iron);
        double zinc = recipe.GetMineralAmountPerServing(MineralType.Zinc);

        var card = new RecipeCardDisplayModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            ImageUrl = recipe.FinalImagePath,
            EnergyBadge = $"{recipe.CaloriesPerServing:N0} kcal / porción",
            ProteinBadge = $"{recipe.ProteinPerServing:0.#} g Proteína",
            PrepTimeDisplay = "25 min",
            ServingsDisplay = recipe.Servings == 1 ? "1 ración clínica" : $"{recipe.Servings} raciones",
            IsGlobal = recipe.IsGlobal,
            IsFavorite = recipe.IsFavorite
        };

        // Estado clinico
        if (sodio < 200 && fosforo < 200)
        {
            card.ClinicalStatusBadge = "Apto Bajo Sodio y Fósforo";
            card.ClinicalStatusBackground = Color.FromArgb("#F0FDF4");
            card.ClinicalStatusBorderColor = Color.FromArgb("#86EFAC");
            card.ClinicalStatusTextColor = Color.FromArgb("#166534");
        }
        else if (potasio < 300)
        {
            card.ClinicalStatusBadge = "Bajo en Potasio (< 300 mg)";
            card.ClinicalStatusBackground = Color.FromArgb("#F7FEE7");
            card.ClinicalStatusBorderColor = Color.FromArgb("#84CC16");
            card.ClinicalStatusTextColor = Color.FromArgb("#315200");
        }
        else
        {
            card.ClinicalStatusBadge = "Apto Renal Controlado";
            card.ClinicalStatusBackground = Color.FromArgb("#F2F3FF");
            card.ClinicalStatusBorderColor = Color.FromArgb("#C1CAB0");
            card.ClinicalStatusTextColor = Color.FromArgb("#131B2E");
        }

        // 7 Minerales
        card.MineralChips.Add(new MealMineralChipModel { Symbol = "P", AmountWithUnit = $"{fosforo:N0} mg" });

        bool isPotasioHigh = potasio > 500;
        card.MineralChips.Add(new MealMineralChipModel
        {
            Symbol = "K",
            AmountWithUnit = isPotasioHigh ? $"{potasio:N0} mg (!)" : $"{potasio:N0} mg",
            IsAlert = isPotasioHigh,
            BackgroundColor = isPotasioHigh ? Color.FromArgb("#FFF7F5") : Color.FromArgb("#F2F3FF"),
            BorderColor = isPotasioHigh ? Color.FromArgb("#FC7B48") : Color.FromArgb("#C1CAB0"),
            TextColor = isPotasioHigh ? Color.FromArgb("#A53C0B") : Color.FromArgb("#131B2E")
        });

        card.MineralChips.Add(new MealMineralChipModel { Symbol = "Na", AmountWithUnit = $"{sodio:N0} mg" });
        card.MineralChips.Add(new MealMineralChipModel { Symbol = "Ca", AmountWithUnit = $"{calcio:N0} mg" });
        card.MineralChips.Add(new MealMineralChipModel { Symbol = "Mg", AmountWithUnit = $"{magnesio:N0} mg" });
        card.MineralChips.Add(new MealMineralChipModel { Symbol = "Fe", AmountWithUnit = $"{hierro:0.#} mg" });
        card.MineralChips.Add(new MealMineralChipModel { Symbol = "Zn", AmountWithUnit = $"{zinc:0.#} mg" });

        return card;
    }

    [RelayCommand]
    private async Task NavigateToAddRecipeAsync()
    {
        await Shell.Current.GoToAsync("AddRecipePage");
    }

    [RelayCommand]
    private async Task NavigateToSeasoningsAsync()
    {
        await Shell.Current.GoToAsync("SeasoningsPage");
    }

    [RelayCommand]
    private async Task NavigateToRecipeDetailAsync(Guid recipeId)
    {
        if (recipeId != Guid.Empty)
        {
            await Shell.Current.GoToAsync($"RecipeDetailPage?RecipeId={recipeId}");
        }
    }
}
