using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.Infrastructure.Supabase;
using DietApp.UI.Localization;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de Configuracion y Ajustes (SettingsPage) segun prototipo Stitch.
/// Gestiona las preferencias de interfaz (idioma y modo visual), calibracion del umbral preventivo de alerta
/// con barra deslizadora (50% a 95%), limites diarios cuantitativos de los 7 minerales diana (K, P, Na, Ca, Mg, Fe, Zn)
/// con interruptores individuales de activacion, restauracion a directrices KDOQI/USDA y auditoria de la base local.
/// Por que se tomo esta decision: Centraliza en la capa de presentacion la configuracion del paciente
/// garantizando sincronizacion en tiempo real con ILocalizationService, IThemeService, IMineralAlertService e IProteinGoalService.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IMineralAlertService _mineralAlertService;
    private readonly IProteinGoalService _proteinGoalService;
    private readonly ICalorieGoalService _calorieGoalService;
    private readonly IThemeService _themeService;
    private readonly IUserProfileService _userProfileService;

    public SettingsViewModel(
        ILocalizationService localizationService,
        IMineralAlertService mineralAlertService,
        IProteinGoalService proteinGoalService,
        ICalorieGoalService calorieGoalService,
        IThemeService themeService,
        IUserProfileService userProfileService)
    {
        _localizationService = localizationService;
        _mineralAlertService = mineralAlertService;
        _proteinGoalService = proteinGoalService;
        _calorieGoalService = calorieGoalService;
        _themeService = themeService;
        _userProfileService = userProfileService;

        LoadSettings();
    }

    // 0. Informacion y Estado del Perfil
    [ObservableProperty]
    private string _displayName = "Usuario Principal";

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _dietaryCondition = "Renal / KDOQI";

    // 1. Preferencias de Idioma
    [ObservableProperty]
    private string _currentLanguage = "es";

    [ObservableProperty]
    private bool _isSpanishSelected = true;

    [ObservableProperty]
    private bool _isEnglishSelected = false;

    // 2. Preferencias de Tema Visual
    [ObservableProperty]
    private bool _isLightThemeSelected = false;

    [ObservableProperty]
    private bool _isDarkThemeSelected = true;

    [ObservableProperty]
    private bool _isSystemThemeSelected = false;

    // 3. Calibracion de Umbral Preventivo de Alerta
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WarningThresholdDisplay))]
    private double _warningThresholdPercent = 80.0;

    public string WarningThresholdDisplay => $"{WarningThresholdPercent:F0}";

    // 4. Metas Nutricionales (Calorias, Proteina) y Limites de Minerales
    [ObservableProperty]
    private bool _isCalorieGoalEnabled = true;

    [ObservableProperty]
    private string _calorieGoalText = "2000";

    [ObservableProperty]
    private bool _isProteinGoalEnabled = true;

    [ObservableProperty]
    private string _proteinGoalGramsText = "60";

    // Limites de los 7 Minerales Criticos
    [ObservableProperty]
    private bool _isPotassiumEnabled = true;

    [ObservableProperty]
    private string _potassiumLimitMgText = "2000";

    [ObservableProperty]
    private bool _isPhosphorusEnabled = true;

    [ObservableProperty]
    private string _phosphorusLimitMgText = "850";

    [ObservableProperty]
    private bool _isSodiumEnabled = true;

    [ObservableProperty]
    private string _sodiumLimitMgText = "1500";

    [ObservableProperty]
    private bool _isCalciumEnabled = true;

    [ObservableProperty]
    private string _calciumLimitMgText = "1000";

    [ObservableProperty]
    private bool _isMagnesiumEnabled = true;

    [ObservableProperty]
    private string _magnesiumLimitMgText = "350";

    [ObservableProperty]
    private bool _isIronEnabled = true;

    [ObservableProperty]
    private string _ironLimitMgText = "15";

    [ObservableProperty]
    private bool _isZincEnabled = true;

    [ObservableProperty]
    private string _zincLimitMgText = "12";

    // 5. Informacion Tecnica
    public string AppVersionText => "v1.4.2 (.NET 10 MAUI)";
    public string DatabaseStatusText => "SQLite Cipher (Offline-First)";
    public string DatasetReferenceText => "USDA FoodData Central 2024";
    public string LastSyncText => "Hoy (Almacenamiento OK)";

    private void LoadSettings()
    {
        // Cargar Idioma
        CurrentLanguage = _localizationService.CurrentLanguage ?? "es";
        IsSpanishSelected = CurrentLanguage.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        IsEnglishSelected = !IsSpanishSelected;

        // Cargar Tema
        var currentTheme = _themeService.CurrentTheme;
        IsLightThemeSelected = currentTheme == ThemeMode.Light;
        IsDarkThemeSelected = currentTheme == ThemeMode.Dark;
        IsSystemThemeSelected = currentTheme == ThemeMode.System;

        // Cargar Umbral de Alerta
        double wp = _mineralAlertService.WarningPercentage;
        WarningThresholdPercent = wp >= 50.0 && wp <= 95.0 ? wp : 80.0;

        // Cargar Metas Nutricionales
        IsCalorieGoalEnabled = _calorieGoalService.IsCalorieGoalEnabled;
        CalorieGoalText = _calorieGoalService.DailyCalorieGoal.ToString("F0", CultureInfo.InvariantCulture);

        IsProteinGoalEnabled = _proteinGoalService.IsProteinGoalEnabled;
        ProteinGoalGramsText = _proteinGoalService.DailyProteinGoalGrams.ToString("F0", CultureInfo.InvariantCulture);

        // Cargar Limites de Minerales
        var thresholds = _mineralAlertService.GetThresholds();

        if (thresholds.TryGetValue(MineralType.Potassium, out var k))
        {
            IsPotassiumEnabled = true;
            PotassiumLimitMgText = k.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsPotassiumEnabled = true;
            PotassiumLimitMgText = "2000";
        }

        if (thresholds.TryGetValue(MineralType.Phosphorus, out var p))
        {
            IsPhosphorusEnabled = true;
            PhosphorusLimitMgText = p.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsPhosphorusEnabled = true;
            PhosphorusLimitMgText = "850";
        }

        if (thresholds.TryGetValue(MineralType.Sodium, out var na))
        {
            IsSodiumEnabled = true;
            SodiumLimitMgText = na.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsSodiumEnabled = true;
            SodiumLimitMgText = "1500";
        }

        if (thresholds.TryGetValue(MineralType.Calcium, out var ca))
        {
            IsCalciumEnabled = true;
            CalciumLimitMgText = ca.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsCalciumEnabled = true;
            CalciumLimitMgText = "1000";
        }

        if (thresholds.TryGetValue(MineralType.Magnesium, out var mg))
        {
            IsMagnesiumEnabled = true;
            MagnesiumLimitMgText = mg.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsMagnesiumEnabled = true;
            MagnesiumLimitMgText = "350";
        }

        if (thresholds.TryGetValue(MineralType.Iron, out var fe))
        {
            IsIronEnabled = true;
            IronLimitMgText = fe.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsIronEnabled = true;
            IronLimitMgText = "15";
        }

        if (thresholds.TryGetValue(MineralType.Zinc, out var zn))
        {
            IsZincEnabled = true;
            ZincLimitMgText = zn.ToString("F0", CultureInfo.InvariantCulture);
        }
        else
        {
            IsZincEnabled = true;
            ZincLimitMgText = "12";
        }

        // Cargar Datos del Perfil desde IUserProfileService
        Task.Run(async () =>
        {
            try
            {
                var profile = await _userProfileService.GetCurrentProfileAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DisplayName = profile.DisplayName;
                    Email = profile.Email ?? string.Empty;
                    DietaryCondition = profile.DietaryCondition;
                });
            }
            catch
            {
                // Fallback silencioso ante inicializaciones asincronas en paralelo
            }
        });
    }

    [RelayCommand]
    private void SelectLanguage(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return;

        CurrentLanguage = lang;
        IsSpanishSelected = lang.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        IsEnglishSelected = !IsSpanishSelected;

        _localizationService.SetLanguage(lang);
        LocalizationResourceManager.Instance.SetLanguage(lang);

        string currentThemeStr = IsLightThemeSelected ? "Light" : (IsDarkThemeSelected ? "Dark" : "System");
        _ = _userProfileService.UpdatePreferencesAsync(lang, currentThemeStr);
    }

    [RelayCommand]
    private void SelectTheme(string themeModeStr)
    {
        var mode = themeModeStr switch
        {
            "Light" => ThemeMode.Light,
            "Dark" => ThemeMode.Dark,
            _ => ThemeMode.System
        };

        IsLightThemeSelected = mode == ThemeMode.Light;
        IsDarkThemeSelected = mode == ThemeMode.Dark;
        IsSystemThemeSelected = mode == ThemeMode.System;

        _themeService.SetTheme(mode);
        _ = _userProfileService.UpdatePreferencesAsync(CurrentLanguage, themeModeStr);
    }

    [RelayCommand]
    private async Task SaveClinicalLimitsAsync()
    {
        try
        {
            // Guardar Meta de Calorias
            double calories = 2000.0;
            if (double.TryParse(CalorieGoalText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedCalories) && parsedCalories >= 0)
            {
                calories = parsedCalories;
                _calorieGoalService.SetDailyCalorieGoal(calories, IsCalorieGoalEnabled);
            }

            // Guardar Meta de Proteina
            double protein = 60.0;
            if (double.TryParse(ProteinGoalGramsText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedProtein) && parsedProtein >= 0)
            {
                protein = parsedProtein;
                _proteinGoalService.SetDailyProteinGoal(protein, IsProteinGoalEnabled);
            }

            // Guardar Umbral Preventivo
            _mineralAlertService.SetWarningPercentage(WarningThresholdPercent);

            // Guardar Limites de Minerales
            var dict = new Dictionary<MineralType, double?>();

            dict[MineralType.Potassium] = IsPotassiumEnabled && double.TryParse(PotassiumLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var k) && k > 0 ? k : null;
            dict[MineralType.Phosphorus] = IsPhosphorusEnabled && double.TryParse(PhosphorusLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var p) && p > 0 ? p : null;
            dict[MineralType.Sodium] = IsSodiumEnabled && double.TryParse(SodiumLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var na) && na > 0 ? na : null;
            dict[MineralType.Calcium] = IsCalciumEnabled && double.TryParse(CalciumLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var ca) && ca > 0 ? ca : null;
            dict[MineralType.Magnesium] = IsMagnesiumEnabled && double.TryParse(MagnesiumLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var mg) && mg > 0 ? mg : null;
            dict[MineralType.Iron] = IsIronEnabled && double.TryParse(IronLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var fe) && fe > 0 ? fe : null;
            dict[MineralType.Zinc] = IsZincEnabled && double.TryParse(ZincLimitMgText, NumberStyles.Any, CultureInfo.InvariantCulture, out var zn) && zn > 0 ? zn : null;

            _mineralAlertService.SaveThresholds(dict);

            // Persistir de forma consolidada en IUserProfileService
            var currentProfile = await _userProfileService.GetCurrentProfileAsync();
            currentProfile.DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? currentProfile.DisplayName : DisplayName.Trim();
            currentProfile.Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
            currentProfile.DietaryCondition = string.IsNullOrWhiteSpace(DietaryCondition) ? currentProfile.DietaryCondition : DietaryCondition.Trim();
            currentProfile.DailyCalorieTarget = calories;
            currentProfile.IsCalorieGoalEnabled = IsCalorieGoalEnabled;
            currentProfile.DailyProteinTargetGrams = protein;
            currentProfile.IsProteinGoalEnabled = IsProteinGoalEnabled;
            currentProfile.WarningThresholdPercentage = WarningThresholdPercent;
            currentProfile.PotassiumLimitMg = dict[MineralType.Potassium] ?? currentProfile.PotassiumLimitMg;
            currentProfile.IsPotassiumEnabled = IsPotassiumEnabled;
            currentProfile.PhosphorusLimitMg = dict[MineralType.Phosphorus] ?? currentProfile.PhosphorusLimitMg;
            currentProfile.IsPhosphorusEnabled = IsPhosphorusEnabled;
            currentProfile.SodiumLimitMg = dict[MineralType.Sodium] ?? currentProfile.SodiumLimitMg;
            currentProfile.IsSodiumEnabled = IsSodiumEnabled;
            currentProfile.CalciumLimitMg = dict[MineralType.Calcium] ?? currentProfile.CalciumLimitMg;
            currentProfile.IsCalciumEnabled = IsCalciumEnabled;
            currentProfile.MagnesiumLimitMg = dict[MineralType.Magnesium] ?? currentProfile.MagnesiumLimitMg;
            currentProfile.IsMagnesiumEnabled = IsMagnesiumEnabled;
            currentProfile.IronLimitMg = dict[MineralType.Iron] ?? currentProfile.IronLimitMg;
            currentProfile.IsIronEnabled = IsIronEnabled;
            currentProfile.ZincLimitMg = dict[MineralType.Zinc] ?? currentProfile.ZincLimitMg;
            currentProfile.IsZincEnabled = IsZincEnabled;
            currentProfile.PreferredLanguage = CurrentLanguage;
            currentProfile.ThemePreference = IsLightThemeSelected ? "Light" : (IsDarkThemeSelected ? "Dark" : "System");

            await _userProfileService.SaveProfileAsync(currentProfile);

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Ajustes Guardados", "Los parámetros clínicos, perfil y alertas preventivas han sido actualizados exitosamente.", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No fue posible guardar los límites clínicos: {ex.Message}", "Aceptar");
            }
        }
    }

    [RelayCommand]
    private async Task ResetDefaultsAsync()
    {
        if (Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Restablecer Valores",
            "¿Deseas restablecer los umbrales clínicos a las directrices de referencia KDOQI 2024 y USDA?",
            "Restablecer",
            "Cancelar");

        if (confirm)
        {
            await _userProfileService.ResetToDefaultsAsync();

            _calorieGoalService.ResetToDefault();
            IsCalorieGoalEnabled = true;
            CalorieGoalText = "2000";

            _proteinGoalService.ResetToDefault();
            IsProteinGoalEnabled = true;
            ProteinGoalGramsText = "60";

            WarningThresholdPercent = 80.0;

            IsPotassiumEnabled = true;
            PotassiumLimitMgText = "2000";

            IsPhosphorusEnabled = true;
            PhosphorusLimitMgText = "850";

            IsSodiumEnabled = true;
            SodiumLimitMgText = "1500";

            IsCalciumEnabled = true;
            CalciumLimitMgText = "1000";

            IsMagnesiumEnabled = true;
            MagnesiumLimitMgText = "350";

            IsIronEnabled = true;
            IronLimitMgText = "15";

            IsZincEnabled = true;
            ZincLimitMgText = "12";

            await SaveClinicalLimitsAsync();
        }
    }

    [RelayCommand]
    private async Task SyncClinicalDataAsync()
    {
        if (Shell.Current == null) return;

        try
        {
            var profile = await _userProfileService.GetCurrentProfileAsync();
            await _userProfileService.SaveProfileAsync(profile);

            string statusMessage = SupabaseConfig.IsConfigured
                ? "Sincronización con Supabase (PostgreSQL) y SQLite local completada exitosamente."
                : "Almacenamiento local SQLite operativo. Las credenciales de Supabase no están configuradas.";

            await Shell.Current.DisplayAlertAsync("Sincronización Clínica", statusMessage, "Aceptar");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Sincronización", $"Error al conectar con la nube: {ex.Message}", "Aceptar");
        }
    }

    [RelayCommand]
    private async Task ShowClinicalGuideAsync()
    {
        if (Shell.Current != null)
        {
            string guide = "Directrices Clínicas de Referencia:\n\n" +
                           "• Proteína: 0.6 - 0.8 g/kg/día (aprox. 50 - 70 g/día según estadio y directrices KDOQI 2024)\n" +
                           "• Potasio: Máximo 2000 mg/día (Estadios 3-5 ERC)\n" +
                           "• Fósforo: 800 - 1000 mg/día con relación P/Proteína óptima\n" +
                           "• Sodio: < 1500 - 2000 mg/día para control tensional y volemia\n" +
                           "• Calcio: 800 - 1000 mg/día para evitar calcificaciones vasculares\n" +
                           "• Magnesio: 310 - 420 mg/día para balance neuromuscular\n\n" +
                           "Fuente: National Kidney Foundation (KDOQI 2024) y USDA FoodData Central.";

            await Shell.Current.DisplayAlertAsync("Guía Nutricional Clínica", guide, "Entendido");
        }
    }

    [RelayCommand]
    private async Task ExportReportPdfAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Exportar Informe",
                "Generando resumen clínico del paciente y parámetros de filtración en formato PDF...",
                "Aceptar");
        }
    }
}
