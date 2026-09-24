using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Administra los umbrales maximos diarios de minerales configurados por el usuario
/// y el porcentaje de aviso preventivo, persiste la configuracion mediante IMineralAlertStorage y
/// evalua ingestas de minerales para detectar advertencias tempranas y superaciones de limites.
/// Por que se tomo esta decision: En DDD, encapsula la logica de alertas preventivas en la capa
/// de aplicacion, permitiendo que tanto el monitoreo diario como los registros de comidas alerten
/// de inmediato al usuario sin duplicar logica de comparacion numerica ni formateo de mensajes.
/// </summary>
public class MineralAlertService : IMineralAlertService
{
    private readonly IMineralAlertStorage _storage;
    private readonly ILocalizationService _localizationService;
    private readonly Dictionary<MineralType, double> _thresholds;
    private double _warningPercentage;

    public event EventHandler? ThresholdsChanged;

    public double WarningPercentage => _warningPercentage;

    public MineralAlertService(
        IMineralAlertStorage storage,
        ILocalizationService localizationService)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _thresholds = _storage.GetAlertThresholds() ?? new Dictionary<MineralType, double>();
        _warningPercentage = _storage.GetWarningPercentage();
    }

    public IReadOnlyDictionary<MineralType, double> GetThresholds()
    {
        return new Dictionary<MineralType, double>(_thresholds);
    }

    public double? GetThreshold(MineralType mineral)
    {
        return _thresholds.TryGetValue(mineral, out var value) && value > 0 ? value : null;
    }

    public void SetThreshold(MineralType mineral, double? maxMilligrams)
    {
        if (maxMilligrams.HasValue && maxMilligrams.Value > 0)
        {
            _thresholds[mineral] = maxMilligrams.Value;
        }
        else
        {
            _thresholds.Remove(mineral);
        }

        _storage.SaveAlertThresholds(_thresholds);
        ThresholdsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveThresholds(IDictionary<MineralType, double?> thresholds)
    {
        if (thresholds == null) return;

        _thresholds.Clear();
        foreach (var pair in thresholds)
        {
            if (pair.Value.HasValue && pair.Value.Value > 0)
            {
                _thresholds[pair.Key] = pair.Value.Value;
            }
        }

        _storage.SaveAlertThresholds(_thresholds);
        ThresholdsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetWarningPercentage(double percentage)
    {
        _warningPercentage = Math.Clamp(percentage, 10.0, 99.0);
        _storage.SaveWarningPercentage(_warningPercentage);
        ThresholdsChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<MineralAlertExceededDto> CheckExceededThresholds(IEnumerable<MineralAmountDto> dailyMinerals)
    {
        if (dailyMinerals == null) return Array.Empty<MineralAlertExceededDto>();

        var alertsList = new List<MineralAlertExceededDto>();

        foreach (var item in dailyMinerals)
        {
            if (_thresholds.TryGetValue(item.Type, out var maxThreshold) && maxThreshold > 0)
            {
                double warningThreshold = maxThreshold * (_warningPercentage / 100.0);
                string mineralDisplayName = _localizationService.GetMineralName(item.Type);
                double percentageOfLimit = (item.Milligrams / maxThreshold) * 100.0;

                if (item.Milligrams >= maxThreshold)
                {
                    double excess = Math.Round(item.Milligrams - maxThreshold, 1);
                    string badge = _localizationService.GetString("Alert_Severity_Exceeded");
                    if (string.IsNullOrWhiteSpace(badge) || badge == "Alert_Severity_Exceeded")
                    {
                        badge = _localizationService.CurrentLanguage == "en" ? "LIMIT EXCEEDED" : "LIMITE SUPERADO";
                    }

                    string warning = _localizationService.CurrentLanguage == "en"
                        ? $"Limit exceeded: {mineralDisplayName} reached {item.Milligrams:F0} mg (Max: {maxThreshold:F0} mg, Excess: {excess:F0} mg - {percentageOfLimit:F0}%)"
                        : $"Limite superado: {mineralDisplayName} alcanzo {item.Milligrams:F0} mg (Maximo: {maxThreshold:F0} mg, Exceso: {excess:F0} mg - {percentageOfLimit:F0}%)";

                    alertsList.Add(new MineralAlertExceededDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        CurrentMilligrams = item.Milligrams,
                        MaxMilligrams = maxThreshold,
                        WarningMessage = warning,
                        Severity = MineralAlertSeverity.ExceededLimit,
                        SeverityBadgeText = badge
                    });
                }
                else if (item.Milligrams >= warningThreshold)
                {
                    string badge = _localizationService.GetString("Alert_Severity_NearLimit");
                    if (string.IsNullOrWhiteSpace(badge) || badge == "Alert_Severity_NearLimit")
                    {
                        badge = _localizationService.CurrentLanguage == "en" ? "EARLY WARNING" : "AVISO PREVENTIVO";
                    }

                    string warning = _localizationService.CurrentLanguage == "en"
                        ? $"Early warning: Close to limit of {mineralDisplayName} ({item.Milligrams:F0} mg of {maxThreshold:F0} mg - {percentageOfLimit:F0}%)"
                        : $"Aviso preventivo: Cerca del limite de {mineralDisplayName} ({item.Milligrams:F0} mg de {maxThreshold:F0} mg - {percentageOfLimit:F0}%)";

                    alertsList.Add(new MineralAlertExceededDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        CurrentMilligrams = item.Milligrams,
                        MaxMilligrams = maxThreshold,
                        WarningMessage = warning,
                        Severity = MineralAlertSeverity.NearLimit,
                        SeverityBadgeText = badge
                    });
                }
            }
        }

        return alertsList
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.PercentageOfLimit)
            .ToList();
    }
}
