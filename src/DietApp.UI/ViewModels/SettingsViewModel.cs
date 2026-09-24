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

    [ObservableProperty]
    public partial bool IsSpanishSelected { get; set; }

    [ObservableProperty]
    public partial bool IsEnglishSelected { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AlertsStatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double WarningPercentage { get; set; } = 80.0;

    [ObservableProperty]
    public partial string WarningPercentageDisplay { get; set; } = "80%";

    public ObservableCollection<MineralAlertConfigModel> MineralAlerts { get; } = new();

    public SettingsViewModel(
        ILocalizationService localizationService,
        IMineralAlertService mineralAlertService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _mineralAlertService = mineralAlertService ?? throw new ArgumentNullException(nameof(mineralAlertService));

        UpdateSelectionState();
        InitializeMineralAlerts();

        _localizationService.LanguageChanged += OnLanguageChanged;
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

    private void UpdateSelectionState()
    {
        var lang = _localizationService.CurrentLanguage;
        IsSpanishSelected = lang == "es";
        IsEnglishSelected = lang == "en";
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
