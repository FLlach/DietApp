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
/// Como funciona: ViewModel para el formulario de Alta de Alimento Personalizado (AddFoodPage)
/// segun prototipo Stitch. Captura datos generales, porcion de referencia, densidad calórica,
/// macronutrientes principales, perfil cuantitativo de los 7 minerales diana (K, P, Na, Ca, Mg, Fe, Zn),
/// dictamen de dieta renal (KDOQI) y notas dietoterapéuticas, persistiendo la entidad a través de IFoodCatalogService.
/// Por que se tomo esta decision: Permite enriquecer la base de datos clinica local con alimentos adaptados
/// a las recetas y habitos de cada paciente sin alterar la estructura estandarizada de dominio.
/// </summary>
public partial class AddFoodViewModel : ObservableObject
{
    private readonly IFoodCatalogService _catalogService;

    public AddFoodViewModel(IFoodCatalogService catalogService)
    {
        _catalogService = catalogService;
        InitializeClinicalPills();
    }

    // Datos Generales
    [ObservableProperty]
    private string _foodName = "Merluza al horno casera";

    public List<string> Categories { get; } = new()
    {
        "Pescados y Mariscos",
        "Carnes y Aves",
        "Verduras y Hortalizas",
        "Frutas Frescas",
        "Cereales y Granos",
        "Lácteos y Derivados",
        "Legumbres",
        "Semillas y Frutos Secos",
        "Aceites y Grasas",
        "Otros"
    };

    [ObservableProperty]
    private int _selectedCategoryIndex = 0;

    [ObservableProperty]
    private string _referenceGramsText = "100";

    public List<string> Units { get; } = new()
    {
        "gramos (g)",
        "mililitros (ml)",
        "porción ración"
    };

    [ObservableProperty]
    private int _selectedUnitIndex = 0;

    // Codigo de Barras (Opcional)
    [ObservableProperty]
    private string _barcodeText = string.Empty;

    [ObservableProperty]
    private bool _hasBarcode;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isTorchOn;

    partial void OnBarcodeTextChanged(string value)
    {
        HasBarcode = !string.IsNullOrWhiteSpace(value);
    }

    // Macronutrientes Principales
    [ObservableProperty]
    private string _caloriesText = "112";

    [ObservableProperty]
    private string _proteinGramsText = "16.5";

    [ObservableProperty]
    private string _carbohydratesGramsText = "1.2";

    [ObservableProperty]
    private string _fatsGramsText = "2.8";

    // 7 Minerales Críticos Cuantitativos
    [ObservableProperty]
    private string _potassiumMgText = "280";

    [ObservableProperty]
    private string _phosphorusMgText = "190";

    [ObservableProperty]
    private string _sodiumMgText = "68";

    [ObservableProperty]
    private string _calciumMgText = "32";

    [ObservableProperty]
    private string _magnesiumMgText = "24";

    [ObservableProperty]
    private string _ironMgText = "0.9";

    [ObservableProperty]
    private string _zincMgText = "0.5";

    // Dictamen Clínico y Advertencias
    [ObservableProperty]
    private bool _isKdoqiRenalDietCompliant = true;

    public ObservableCollection<ClinicalTagPillModel> ClinicalPills { get; } = new();

    [ObservableProperty]
    private string _clinicalObservations = "Excelente ratio fósforo-proteína. Preparación al horno sin adición de salmueras.";

    // Estado Visual
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private string? _imageSourcePath;

    private void InitializeClinicalPills()
    {
        ClinicalPills.Clear();
        ClinicalPills.Add(new ClinicalTagPillModel { Name = "Bajo en fósforo orgánico", IsSelected = true });
        ClinicalPills.Add(new ClinicalTagPillModel { Name = "Requiere remojo doble", IsSelected = false });
        ClinicalPills.Add(new ClinicalTagPillModel { Name = "Sin sal añadida", IsSelected = false });
        ClinicalPills.Add(new ClinicalTagPillModel { Name = "Hervido con pérdida de K", IsSelected = false });
    }

    [RelayCommand]
    private void ToggleClinicalPill(ClinicalTagPillModel pill)
    {
        if (pill == null) return;
        pill.IsSelected = !pill.IsSelected;
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    ImageSourcePath = photo.FullPath;
                    HasImage = true;
                }
            }
        }
        catch
        {
            // Omitir si la plataforma o permisos impiden seleccion de foto
        }
    }

    [RelayCommand]
    private void ClearBarcode()
    {
        BarcodeText = string.Empty;
    }

    [RelayCommand]
    private void StartScan()
    {
        IsScanning = true;
        IsTorchOn = false;
    }

    [RelayCommand]
    private void StopScan()
    {
        IsScanning = false;
        IsTorchOn = false;
    }

    [RelayCommand]
    private void ToggleTorch()
    {
        IsTorchOn = !IsTorchOn;
    }

    /// <summary>
    /// Procesa el resultado de lectura exitosa emitido por el escaner de codigo de barras.
    /// Por que se tomo esta decision: Centraliza la recepcion del codigo leido, desactiva la camara
    /// para liberar hardware y emite vibracion haptica para confirmacion sensorial.
    /// </summary>
    public void OnBarcodeDetected(string detectedBarcode)
    {
        if (string.IsNullOrWhiteSpace(detectedBarcode)) return;

        BarcodeText = detectedBarcode.Trim();
        IsScanning = false;
        IsTorchOn = false;

        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            // Omitir si la plataforma o dispositivo no soporta retroalimentacion haptica
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

    [RelayCommand]
    private async Task DiscardAsync()
    {
        if (Shell.Current != null)
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Descartar Alimento",
                "¿Deseas salir sin guardar los cambios del nuevo alimento?",
                "Sí, Descartar",
                "Cancelar");

            if (confirm)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    [RelayCommand]
    private async Task SaveFoodAsync()
    {
        if (string.IsNullOrWhiteSpace(FoodName))
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Campo Requerido", "Por favor introduce el nombre del alimento.", "Aceptar");
            }
            return;
        }

        try
        {
            IsBusy = true;

            double refGrams = ParseDouble(ReferenceGramsText, 100.0);
            double cals = ParseDouble(CaloriesText, 0.0);
            double protein = ParseDouble(ProteinGramsText, 0.0);
            double k = ParseDouble(PotassiumMgText, 0.0);
            double p = ParseDouble(PhosphorusMgText, 0.0);
            double na = ParseDouble(SodiumMgText, 0.0);
            double ca = ParseDouble(CalciumMgText, 0.0);
            double mg = ParseDouble(MagnesiumMgText, 0.0);
            double fe = ParseDouble(IronMgText, 0.0);
            double zn = ParseDouble(ZincMgText, 0.0);

            string category = Categories.ElementAtOrDefault(SelectedCategoryIndex) ?? "Otros";
            string? barcode = string.IsNullOrWhiteSpace(BarcodeText) ? null : BarcodeText.Trim();

            var foodDto = new FoodItemDto
            {
                Id = Guid.NewGuid(),
                Name = FoodName.Trim(),
                Category = category,
                ReferenceGrams = refGrams > 0 ? refGrams : 100.0,
                Calories = cals,
                ProteinGrams = protein,
                Barcode = barcode,
                Minerals = new List<MineralAmountDto>
                {
                    new() { Type = MineralType.Potassium, MineralName = "Potasio", Milligrams = k, Unit = "mg" },
                    new() { Type = MineralType.Phosphorus, MineralName = "Fósforo", Milligrams = p, Unit = "mg" },
                    new() { Type = MineralType.Sodium, MineralName = "Sodio", Milligrams = na, Unit = "mg" },
                    new() { Type = MineralType.Calcium, MineralName = "Calcio", Milligrams = ca, Unit = "mg" },
                    new() { Type = MineralType.Magnesium, MineralName = "Magnesio", Milligrams = mg, Unit = "mg" },
                    new() { Type = MineralType.Iron, MineralName = "Hierro", Milligrams = fe, Unit = "mg" },
                    new() { Type = MineralType.Zinc, MineralName = "Zinc", Milligrams = zn, Unit = "mg" }
                }
            };

            await _catalogService.SaveFoodAsync(foodDto);

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Alimento Guardado", $"'{foodDto.Name}' ha sido registrado exitosamente en el catálogo clínico.", "Aceptar");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No fue posible guardar el alimento: {ex.Message}", "Aceptar");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static double ParseDouble(string? text, double fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;

        string normalized = text.Replace(',', '.').Trim();
        if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return fallback;
    }
}
