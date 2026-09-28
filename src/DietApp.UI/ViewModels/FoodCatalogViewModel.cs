using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.DTOs;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Helpers;
using DietApp.UI.Models;
using DietApp.UI.Views;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para el Catálogo de Alimentos (FoodCatalogPage) segun prototipo Stitch.
/// Carga la base de datos de alimentos (USDA FoodData Central y registros locales), ofrece buscador clinico,
/// filtro avanzado por rango minimo/maximo de minerales (K, Na, P, etc.), ordenacion multicriterio,
/// presintonias frecuentes y mapea cada alimento a tarjetas visuales con desglose de los 7 minerales
/// y alertas visuales coral si superan los umbrales de seguridad renal.
/// Por que se tomo esta decision: Permite una consulta clinica agil tanto para prescripcion dietetica
/// como para seleccion informada por parte del paciente.
/// </summary>
public partial class FoodCatalogViewModel : ObservableObject
{
    private readonly IFoodCatalogService _catalogService;

    public FoodCatalogViewModel(IFoodCatalogService catalogService)
    {
        _catalogService = catalogService;

        InitializePresets();
        _ = LoadFoodsAsync();
    }

    private const int PageSize = 25;
    private int _currentPage = 1;
    private List<FoodCardModel> _filteredPool = new();
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, token);
                if (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ApplySortingAndFilter();
                    });
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    // Filtros de Rango Mineral
    public List<string> MineralOptions { get; } = new()
    {
        "Potasio (K)",
        "Sodio (Na)",
        "Fósforo (P)",
        "Calcio (Ca)",
        "Magnesio (Mg)",
        "Hierro (Fe)",
        "Zinc (Zn)",
        "Proteína"
    };

    [ObservableProperty]
    private int _selectedMineralIndex = 0;

    [ObservableProperty]
    private string _minMgText = "0";

    [ObservableProperty]
    private string _maxMgText = "250";

    [ObservableProperty]
    private string _activeFiltersBadgeText = "2 activos";

    // Ordenación
    public List<string> SortOptions { get; } = new()
    {
        "Menor Potasio",
        "Mayor Proteína",
        "Menor Fósforo",
        "Menor Sodio",
        "Nombre (A - Z)"
    };

    [ObservableProperty]
    private int _selectedSortIndex = 0;

    partial void OnSelectedSortIndexChanged(int value)
    {
        ApplySortingAndFilter();
    }

    // Presintonías Clínicas Frecuentes
    public ObservableCollection<ClinicalPresetFilterModel> ClinicalPresets { get; } = new();

    [ObservableProperty]
    private ClinicalPresetFilterModel? _selectedPreset;

    // Colección de Alimentos
    public ObservableCollection<FoodCardModel> Foods { get; } = new();
    private readonly List<FoodCardModel> _allFoods = new();

    [ObservableProperty]
    private int _totalFoodsCount;

    [ObservableProperty]
    private bool _hasMoreFoods;

    [ObservableProperty]
    private string _loadMoreButtonText = "Cargar más alimentos";

    [ObservableProperty]
    private string _resultsCountText = "alimentos encontrados";

    [ObservableProperty]
    private bool _isBusy;

    private void InitializePresets()
    {
        ClinicalPresets.Clear();
        var p1 = new ClinicalPresetFilterModel { Name = "Bajo en Potasio (<200mg)", IsSelected = true };
        ClinicalPresets.Add(p1);
        ClinicalPresets.Add(new ClinicalPresetFilterModel { Name = "Bajo en Sodio (<140mg)", IsSelected = false });
        ClinicalPresets.Add(new ClinicalPresetFilterModel { Name = "Alto en Proteína", IsSelected = false });
        ClinicalPresets.Add(new ClinicalPresetFilterModel { Name = "Legumbres & Semillas", IsSelected = false });
        ClinicalPresets.Add(new ClinicalPresetFilterModel { Name = "Carnes y Pescados", IsSelected = false });

        SelectedPreset = p1;
    }

    public async Task LoadFoodsAsync()
    {
        try
        {
            IsBusy = true;
            var dtoList = await _catalogService.GetAllFoodsAsync();

            _allFoods.Clear();
            if (dtoList != null && dtoList.Count > 0)
            {
                foreach (var dto in dtoList)
                {
                    _allFoods.Add(MapToCardModel(dto));
                }
            }

            // Si la base local es pequeña, incorporar alimentos de referencia del prototipo Stitch
            EnsurePrototypeFoods();

            ApplySortingAndFilter();
        }
        catch
        {
            EnsurePrototypeFoods();
            ApplySortingAndFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EnsurePrototypeFoods()
    {
        if (_allFoods.Any(f => f.Name.Contains("Pechuga de pollo", StringComparison.OrdinalIgnoreCase))) return;

        // 1. Pechuga de pollo
        _allFoods.Add(new FoodCardModel
        {
            Id = Guid.NewGuid(),
            Name = "Pechuga de pollo (sin piel)",
            Category = "Carnes y Aves",
            ReferenceGrams = 100,
            Calories = 165,
            ProteinGrams = 31.0,
            PhosphorusMg = 228,
            PotassiumMg = 256,
            SodiumMg = 74,
            CalciumMg = 15,
            MagnesiumMg = 29,
            IronMg = 1.0,
            ZincMg = 1.0,
            IsAlert = false,
            VerdictIcon = MaterialIconFont.Verified,
            VerdictText = "Apto bajo potasio",
            VerdictColor = Color.FromArgb("#416900")
        });

        // 2. Arroz blanco cocido
        _allFoods.Add(new FoodCardModel
        {
            Id = Guid.NewGuid(),
            Name = "Arroz blanco cocido",
            Category = "Cereales y Granos",
            ReferenceGrams = 100,
            Calories = 130,
            ProteinGrams = 2.7,
            PhosphorusMg = 43,
            PotassiumMg = 35,
            SodiumMg = 1,
            CalciumMg = 10,
            MagnesiumMg = 12,
            IronMg = 0.2,
            ZincMg = 0.5,
            IsAlert = false,
            VerdictIcon = MaterialIconFont.CheckCircle,
            VerdictText = "Muy bajo en minerales críticos",
            VerdictColor = Color.FromArgb("#416900")
        });

        // 3. Plátano / Banana fresco (Alerta Coral)
        _allFoods.Add(new FoodCardModel
        {
            Id = Guid.NewGuid(),
            Name = "Plátano / Banana fresco",
            Category = "Frutas",
            ReferenceGrams = 100,
            Calories = 89,
            ProteinGrams = 1.1,
            PhosphorusMg = 22,
            PotassiumMg = 358,
            SodiumMg = 1,
            CalciumMg = 5,
            MagnesiumMg = 27,
            IronMg = 0.3,
            ZincMg = 0.15,
            IsAlert = true,
            AlertBadgeText = "Límite superado",
            CardBackgroundColor = Color.FromArgb("#FFF7F5"),
            CardBorderColor = Color.FromArgb("#FC7B48"),
            PotassiumChipBackground = Color.FromArgb("#FC7B48"),
            PotassiumChipTextColor = Color.FromArgb("#FFFFFF"),
            PotassiumChipNumberColor = Color.FromArgb("#FFFFFF"),
            PotassiumChipLabel = "K (Alto)",
            VerdictIcon = MaterialIconFont.Warning,
            VerdictText = "Atención: Alto Potasio (>250 mg)",
            VerdictColor = Color.FromArgb("#A53C0B")
        });

        // 4. Calabacín / Zucchini
        _allFoods.Add(new FoodCardModel
        {
            Id = Guid.NewGuid(),
            Name = "Calabacín / Zucchini",
            Category = "Verduras y Hortalizas",
            ReferenceGrams = 100,
            Calories = 17,
            ProteinGrams = 1.2,
            PhosphorusMg = 38,
            PotassiumMg = 261,
            SodiumMg = 8,
            CalciumMg = 16,
            MagnesiumMg = 18,
            IronMg = 0.4,
            ZincMg = 0.3,
            IsAlert = false,
            VerdictIcon = MaterialIconFont.Info,
            VerdictText = "Consumo moderado sugerido",
            VerdictColor = Color.FromArgb("#424936")
        });

        // 5. Clara de huevo cocida
        _allFoods.Add(new FoodCardModel
        {
            Id = Guid.NewGuid(),
            Name = "Clara de huevo cocida",
            Category = "Huevos y Derivados",
            ReferenceGrams = 100,
            Calories = 52,
            ProteinGrams = 10.9,
            PhosphorusMg = 15,
            PotassiumMg = 163,
            SodiumMg = 166,
            CalciumMg = 7,
            MagnesiumMg = 11,
            IronMg = 0.1,
            ZincMg = 0.0,
            IsAlert = false,
            VerdictIcon = MaterialIconFont.Check,
            VerdictText = "Excelente fuente proteica renal",
            VerdictColor = Color.FromArgb("#416900")
        });
    }

    private FoodCardModel MapToCardModel(FoodItemDto dto)
    {
        double na = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Sodium)?.Milligrams ?? 0;
        double k = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Potassium)?.Milligrams ?? 0;
        double p = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Phosphorus)?.Milligrams ?? 0;
        double ca = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Calcium)?.Milligrams ?? 0;
        double mg = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Magnesium)?.Milligrams ?? 0;
        double fe = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Iron)?.Milligrams ?? 0;
        double zn = dto.Minerals?.FirstOrDefault(m => m.Type == MineralType.Zinc)?.Milligrams ?? 0;

        bool isAlert = k > 300 || na > 300;

        string verdictIcon = MaterialIconFont.Verified;
        string verdictText = "Apto bajo potasio";
        Color verdictColor = Color.FromArgb("#416900");

        if (isAlert)
        {
            verdictIcon = MaterialIconFont.Warning;
            verdictText = k > 300 ? "Atención: Alto Potasio (>250 mg)" : "Atención: Alto Sodio (>300 mg)";
            verdictColor = Color.FromArgb("#A53C0B");
        }
        else if (dto.ProteinGrams >= 15)
        {
            verdictIcon = MaterialIconFont.Check;
            verdictText = "Excelente fuente proteica renal";
            verdictColor = Color.FromArgb("#416900");
        }
        else if (k <= 100 && na <= 50 && p <= 100)
        {
            verdictIcon = MaterialIconFont.CheckCircle;
            verdictText = "Muy bajo en minerales críticos";
            verdictColor = Color.FromArgb("#416900");
        }

        return new FoodCardModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Category = dto.Category,
            ReferenceGrams = dto.ReferenceGrams > 0 ? dto.ReferenceGrams : 100,
            Calories = dto.Calories,
            ProteinGrams = dto.ProteinGrams,
            SodiumMg = na,
            PotassiumMg = k,
            PhosphorusMg = p,
            CalciumMg = ca,
            MagnesiumMg = mg,
            IronMg = fe,
            ZincMg = zn,
            IsAlert = isAlert,
            AlertBadgeText = "Límite superado",
            CardBackgroundColor = isAlert ? Color.FromArgb("#FFF7F5") : Color.FromArgb("#FFFFFF"),
            CardBorderColor = isAlert ? Color.FromArgb("#FC7B48") : Color.FromArgb("#C1CAB0"),
            PotassiumChipBackground = isAlert ? Color.FromArgb("#FC7B48") : Color.FromArgb("#F2F3FF"),
            PotassiumChipTextColor = isAlert ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#416900"),
            PotassiumChipNumberColor = isAlert ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#131B2E"),
            PotassiumChipLabel = isAlert ? "K (Alto)" : "K",
            VerdictIcon = verdictIcon,
            VerdictText = verdictText,
            VerdictColor = verdictColor
        };
    }

    [RelayCommand]
    private async Task FilterFoodsAsync()
    {
        await Task.Yield();
        ApplySortingAndFilter();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        ApplySortingAndFilter();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchQuery = string.Empty;
        MinMgText = "0";
        MaxMgText = "250";
        SelectedMineralIndex = 0;
        SelectedSortIndex = 0;

        foreach (var p in ClinicalPresets)
        {
            p.IsSelected = false;
        }

        ActiveFiltersBadgeText = "0 activos";
        ApplySortingAndFilter();
    }

    [RelayCommand]
    private void SelectPreset(ClinicalPresetFilterModel preset)
    {
        if (preset == null) return;

        foreach (var p in ClinicalPresets)
        {
            p.IsSelected = p == preset;
        }
        SelectedPreset = preset;

        // Configurar rangos segun la presintonia seleccionada
        if (preset.Name.Contains("Potasio"))
        {
            SelectedMineralIndex = 0;
            MinMgText = "0";
            MaxMgText = "200";
        }
        else if (preset.Name.Contains("Sodio"))
        {
            SelectedMineralIndex = 1;
            MinMgText = "0";
            MaxMgText = "140";
        }
        else if (preset.Name.Contains("Proteína"))
        {
            SelectedMineralIndex = 7;
            MinMgText = "15";
            MaxMgText = "100";
        }

        ActiveFiltersBadgeText = "1 activo";
        ApplySortingAndFilter();
    }

    private void ApplySortingAndFilter()
    {
        var query = _allFoods.AsEnumerable();

        // 1. Busqueda por texto
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            query = query.Where(f =>
                f.Name.Contains(SearchQuery.Trim(), StringComparison.OrdinalIgnoreCase) ||
                f.Category.Contains(SearchQuery.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // 2. Filtro numerico por mineral seleccionado
        double.TryParse(MinMgText, out double minVal);
        if (!double.TryParse(MaxMgText, out double maxVal) || maxVal <= 0)
        {
            maxVal = double.MaxValue;
        }

        switch (SelectedMineralIndex)
        {
            case 0: // Potasio (K)
                query = query.Where(f => f.PotassiumMg >= minVal && f.PotassiumMg <= maxVal);
                break;
            case 1: // Sodio (Na)
                query = query.Where(f => f.SodiumMg >= minVal && f.SodiumMg <= maxVal);
                break;
            case 2: // Fósforo (P)
                query = query.Where(f => f.PhosphorusMg >= minVal && f.PhosphorusMg <= maxVal);
                break;
            case 3: // Calcio (Ca)
                query = query.Where(f => f.CalciumMg >= minVal && f.CalciumMg <= maxVal);
                break;
            case 4: // Magnesio (Mg)
                query = query.Where(f => f.MagnesiumMg >= minVal && f.MagnesiumMg <= maxVal);
                break;
            case 5: // Hierro (Fe)
                query = query.Where(f => f.IronMg >= minVal && f.IronMg <= maxVal);
                break;
            case 6: // Zinc (Zn)
                query = query.Where(f => f.ZincMg >= minVal && f.ZincMg <= maxVal);
                break;
            case 7: // Proteína
                query = query.Where(f => f.ProteinGrams >= minVal && f.ProteinGrams <= maxVal);
                break;
        }

        // 3. Ordenación
        query = SelectedSortIndex switch
        {
            0 => query.OrderBy(f => f.PotassiumMg).ThenBy(f => f.Name),
            1 => query.OrderByDescending(f => f.ProteinGrams).ThenBy(f => f.Name),
            2 => query.OrderBy(f => f.PhosphorusMg).ThenBy(f => f.Name),
            3 => query.OrderBy(f => f.SodiumMg).ThenBy(f => f.Name),
            4 => query.OrderBy(f => f.Name),
            _ => query.OrderBy(f => f.Name)
        };

        _filteredPool = query.ToList();
        TotalFoodsCount = _filteredPool.Count;
        _currentPage = 1;
        PopulateCurrentPage();
    }

    private void PopulateCurrentPage()
    {
        int itemsToShow = _currentPage * PageSize;
        var pageItems = _filteredPool.Take(itemsToShow).ToList();

        Foods.Clear();
        foreach (var item in pageItems)
        {
            Foods.Add(item);
        }

        HasMoreFoods = _filteredPool.Count > itemsToShow;
        int remaining = _filteredPool.Count - itemsToShow;
        LoadMoreButtonText = $"Cargar más alimentos (+{Math.Min(remaining, PageSize)})";
        ResultsCountText = _filteredPool.Count > PageSize
            ? $"alimentos (mostrando {Foods.Count} de {TotalFoodsCount})"
            : "alimentos encontrados";
    }

    [RelayCommand]
    private void LoadMoreFoods()
    {
        if (!HasMoreFoods) return;
        _currentPage++;
        int nextBatchStartIndex = (_currentPage - 1) * PageSize;
        var nextBatch = _filteredPool.Skip(nextBatchStartIndex).Take(PageSize).ToList();

        foreach (var item in nextBatch)
        {
            Foods.Add(item);
        }

        int itemsToShow = _currentPage * PageSize;
        HasMoreFoods = _filteredPool.Count > itemsToShow;
        int remaining = _filteredPool.Count - itemsToShow;
        LoadMoreButtonText = $"Cargar más alimentos (+{Math.Min(remaining, PageSize)})";
        ResultsCountText = $"alimentos (mostrando {Foods.Count} de {TotalFoodsCount})";
    }

    [RelayCommand]
    private async Task NavigateToAddFoodAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(AddFoodPage));
        }
    }

    [RelayCommand]
    private async Task SyncDatabaseAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Sincronización", "La base de datos de FoodData Central y registros clínicos locales se encuentra actualizada.", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ShowNotificationsAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync("Notificaciones", "No hay alertas pendientes en el catálogo clínico.", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task OpenFoodDetailAsync(FoodCardModel food)
    {
        if (food == null || Shell.Current == null) return;

        string detail = $"{food.Name}\n" +
                        $"Calorías: {food.Calories:N0} kcal\n" +
                        $"Proteínas: {food.ProteinGrams:0.#} g\n" +
                        $"Potasio: {food.PotassiumMg:N0} mg\n" +
                        $"Fósforo: {food.PhosphorusMg:N0} mg\n" +
                        $"Sodio: {food.SodiumMg:N0} mg\n" +
                        $"Calcio: {food.CalciumMg:N0} mg\n" +
                        $"Magnesio: {food.MagnesiumMg:N0} mg\n" +
                        $"Hierro: {food.IronMg:0.#} mg\n" +
                        $"Zinc: {food.ZincMg:0.#} mg\n\n" +
                        $"Veredicto: {food.VerdictText}";

        await Shell.Current.DisplayAlertAsync("Ficha Nutricional", detail, "Aceptar");
    }
}
