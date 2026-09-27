using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using DietApp.UI.Models;

namespace DietApp.UI.ViewModels;

/// <summary>
/// Como funciona: ViewModel para la pantalla de configuracion general. Administra tanto la seleccion de idioma
/// como la configuracion de alertas y limites maximos de minerales diarios fijados por el usuario.
/// Por que se tomo esta decision: En el patron MVVM, centraliza las preferencias de la aplicacion
/// garantizando que las modificaciones se guarden inmediatamente y se propaguen a las demas pantallas.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IMineralAlertService _mineralAlertService;
    private readonly IThemeService _themeService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpanishBorderColor))]
    [NotifyPropertyChangedFor(nameof(SpanishBorderThickness))]
    public partial bool IsSpanishSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnglishBorderColor))]
    [NotifyPropertyChangedFor(nameof(EnglishBorderThickness))]
    public partial bool IsEnglishSelected { get; set; }

    public Color SpanishBorderColor => IsSpanishSelected ? Color.FromArgb("#ef713f") : Color.FromArgb("#CBD5E1");
    public Color EnglishBorderColor => IsEnglishSelected ? Color.FromArgb("#ef713f") : Color.FromArgb("#CBD5E1");

    public double SpanishBorderThickness => IsSpanishSelected ? 2.5 : 1.0;
    public double EnglishBorderThickness => IsEnglishSelected ? 2.5 : 1.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LightBorderColor))]
    [NotifyPropertyChangedFor(nameof(LightBorderThickness))]
    public partial bool IsLightSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DarkBorderColor))]
    [NotifyPropertyChangedFor(nameof(DarkBorderThickness))]
    public partial bool IsDarkSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemBorderColor))]
    [NotifyPropertyChangedFor(nameof(SystemBorderThickness))]
    public partial bool IsSystemSelected { get; set; }

    public Color LightBorderColor => IsLightSelected ? Color.FromArgb("#ef713f") : Color.FromArgb("#CBD5E1");
    public Color DarkBorderColor => IsDarkSelected ? Color.FromArgb("#ef713f") : Color.FromArgb("#CBD5E1");
    public Color SystemBorderColor => IsSystemSelected ? Color.FromArgb("#ef713f") : Color.FromArgb("#CBD5E1");

    public double LightBorderThickness => IsLightSelected ? 2.5 : 1.0;
    public double DarkBorderThickness => IsDarkSelected ? 2.5 : 1.0;
    public double SystemBorderThickness => IsSystemSelected ? 2.5 : 1.0;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ThemeStatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AlertsStatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double WarningPercentage { get; set; } = 80.0;

    [ObservableProperty]
    public partial string WarningPercentageDisplay { get; set; } = "80%";

    public ObservableCollection<MineralAlertConfigModel> MineralAlerts { get; } = new();

    public SettingsViewModel(
        ILocalizationService localizationService,
        IMineralAlertService mineralAlertService,
        IThemeService themeService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _mineralAlertService = mineralAlertService ?? throw new ArgumentNullException(nameof(mineralAlertService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));

        UpdateSelectionState();
        UpdateThemeSelectionState();
        InitializeMineralAlerts();

        _localizationService.LanguageChanged += OnLanguageChanged;
        _themeService.ThemeChanged += OnThemeChanged;
    }

    partial void OnWarningPercentageChanged(double value)
    {
        WarningPercentageDisplay = $"{value:F0}%";
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateSelectionState();
        UpdateMineralAlertNames();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        UpdateThemeSelectionState();
    }

    private void UpdateThemeSelectionState()
    {
        var current = _themeService.CurrentTheme;
        IsLightSelected = current == ThemeMode.Light;
        IsDarkSelected = current == ThemeMode.Dark;
        IsSystemSelected = current == ThemeMode.System;
    }

    private void UpdateSelectionState()
    {
        var lang = _localizationService.CurrentLanguage;
        IsSpanishSelected = lang == "es";
        IsEnglishSelected = lang == "en";
    }

    [RelayCommand]
    public void SelectLight()
    {
        _themeService.SetTheme(ThemeMode.Light);
        UpdateThemeSelectionState();
        ThemeStatusMessage = _localizationService.GetString("Settings_ThemeNotice");
    }

    [RelayCommand]
    public void SelectDark()
    {
        _themeService.SetTheme(ThemeMode.Dark);
        UpdateThemeSelectionState();
        ThemeStatusMessage = _localizationService.GetString("Settings_ThemeNotice");
    }

    [RelayCommand]
    public void SelectSystem()
    {
        _themeService.SetTheme(ThemeMode.System);
        UpdateThemeSelectionState();
        ThemeStatusMessage = _localizationService.GetString("Settings_ThemeNotice");
    }

    private void InitializeMineralAlerts()
    {
        WarningPercentage = _mineralAlertService.WarningPercentage;
        WarningPercentageDisplay = $"{WarningPercentage:F0}%";

        MineralAlerts.Clear();
        var existingThresholds = _mineralAlertService.GetThresholds();

        foreach (MineralType mineral in Enum.GetValues<MineralType>())
        {
            bool hasThreshold = existingThresholds.TryGetValue(mineral, out var maxVal) && maxVal > 0;

            MineralAlerts.Add(new MineralAlertConfigModel
            {
                Mineral = mineral,
                MineralName = _localizationService.GetMineralName(mineral),
                IsEnabled = hasThreshold,
                ThresholdText = hasThreshold ? maxVal.ToString("F0") : string.Empty,
                Unit = "mg"
            });
        }
    }

    private void UpdateMineralAlertNames()
    {
        foreach (var item in MineralAlerts)
        {
            item.MineralName = _localizationService.GetMineralName(item.Mineral);
        }
    }

    [RelayCommand]
    public void SelectSpanish()
    {
        _localizationService.SetLanguage("es");
        UpdateSelectionState();
        StatusMessage = _localizationService.GetString("Settings_CurrentLanguageNotice");
    }

    [RelayCommand]
    public void SelectEnglish()
    {
        _localizationService.SetLanguage("en");
        UpdateSelectionState();
        StatusMessage = _localizationService.GetString("Settings_CurrentLanguageNotice");
    }

    [RelayCommand]
    public void SaveAlerts()
    {
        _mineralAlertService.SetWarningPercentage(WarningPercentage);

        var dict = new Dictionary<MineralType, double?>();
        foreach (var alert in MineralAlerts)
        {
            dict[alert.Mineral] = alert.GetValidThreshold();
        }

        _mineralAlertService.SaveThresholds(dict);
        AlertsStatusMessage = _localizationService.GetString("Settings_MineralAlertsSavedNotice");
    }

    [RelayCommand]
    public void ResetAlerts()
    {
        WarningPercentage = 80.0;
        WarningPercentageDisplay = "80%";
        _mineralAlertService.SetWarningPercentage(80.0);

        foreach (var alert in MineralAlerts)
        {
            alert.IsEnabled = false;
            alert.ThresholdText = string.Empty;
        }

        _mineralAlertService.SaveThresholds(new Dictionary<MineralType, double?>());
        AlertsStatusMessage = _localizationService.GetString("Settings_MineralAlertsClearedNotice");
    }
}
