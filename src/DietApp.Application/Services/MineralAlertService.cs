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

    public IReadOnlyList<RecipePortionWarningDto> CheckRecipePortionWarnings(
        IEnumerable<MineralAmountDto> portionMinerals,
        IEnumerable<MineralAmountDto>? currentDailyMinerals = null,
        double servings = 1.0)
    {
        if (portionMinerals == null) return Array.Empty<RecipePortionWarningDto>();
        if (servings <= 0) servings = 1.0;

        var warnings = new List<RecipePortionWarningDto>();
        bool isEnglish = _localizationService.CurrentLanguage == "en";
        string servingsText = servings == 1.0
            ? (isEnglish ? "1 serving" : "1 porcion")
            : $"{servings:0.##} {(isEnglish ? "servings" : "porciones")}";

        var dailyMap = currentDailyMinerals != null
            ? currentDailyMinerals.ToDictionary(m => m.Type, m => m.Milligrams)
            : new Dictionary<MineralType, double>();

        foreach (var item in portionMinerals)
        {
            if (_thresholds.TryGetValue(item.Type, out var maxThreshold) && maxThreshold > 0)
            {
                double portionAmount = item.Milligrams * servings;
                dailyMap.TryGetValue(item.Type, out double currentDailyAmount);
                double projectedTotal = currentDailyAmount + portionAmount;
                double warningThreshold = maxThreshold * (_warningPercentage / 100.0);
                string mineralDisplayName = _localizationService.GetMineralName(item.Type);

                if (portionAmount >= maxThreshold)
                {
                    double excess = portionAmount - maxThreshold;
                    string badge = isEnglish ? "LIMIT EXCEEDED" : "LIMITE SUPERADO";
                    string message = isEnglish
                        ? $"{mineralDisplayName}: Consuming {servingsText} provides {portionAmount:F0} mg, which by itself exceeds your daily maximum limit of {maxThreshold:F0} mg (Excess: {excess:F0} mg)."
                        : $"{mineralDisplayName}: Consumir {servingsText} aportara {portionAmount:F0} mg, lo cual por si solo supera tu limite diario maximo de {maxThreshold:F0} mg (Exceso: {excess:F0} mg).";

                    warnings.Add(new RecipePortionWarningDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        PortionMilligrams = portionAmount,
                        CurrentDailyMilligrams = currentDailyAmount,
                        ProjectedTotalMilligrams = projectedTotal,
                        MaxDailyMilligrams = maxThreshold,
                        Severity = MineralAlertSeverity.ExceededLimit,
                        SeverityBadgeText = badge,
                        WarningMessage = message
                    });
                }
                else if (projectedTotal >= maxThreshold)
                {
                    double excess = projectedTotal - maxThreshold;
                    string badge = isEnglish ? "LIMIT EXCEEDED" : "LIMITE SUPERADO";
                    string message = isEnglish
                        ? $"{mineralDisplayName}: Consuming {servingsText} will push your daily total to {projectedTotal:F0} mg, exceeding your limit of {maxThreshold:F0} mg (Consumed today: {currentDailyAmount:F0} mg, recipe: {portionAmount:F0} mg, excess: {excess:F0} mg)."
                        : $"{mineralDisplayName}: Consumir {servingsText} hara que tu total diario alcance {projectedTotal:F0} mg, superando tu limite de {maxThreshold:F0} mg (Consumido hoy: {currentDailyAmount:F0} mg, aporte receta: {portionAmount:F0} mg, exceso: {excess:F0} mg).";

                    warnings.Add(new RecipePortionWarningDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        PortionMilligrams = portionAmount,
                        CurrentDailyMilligrams = currentDailyAmount,
                        ProjectedTotalMilligrams = projectedTotal,
                        MaxDailyMilligrams = maxThreshold,
                        Severity = MineralAlertSeverity.ExceededLimit,
                        SeverityBadgeText = badge,
                        WarningMessage = message
                    });
                }
                else if (projectedTotal >= warningThreshold)
                {
                    double percentage = (projectedTotal / maxThreshold) * 100.0;
                    string badge = isEnglish ? "EARLY WARNING" : "AVISO PREVENTIVO";
                    string message = isEnglish
                        ? $"{mineralDisplayName}: Early warning: Consuming {servingsText} will reach {percentage:F0}% of your daily limit ({projectedTotal:F0} mg of {maxThreshold:F0} mg)."
                        : $"{mineralDisplayName}: Aviso preventivo: Consumir {servingsText} alcanzara el {percentage:F0}% de tu limite diario ({projectedTotal:F0} mg de {maxThreshold:F0} mg).";

                    warnings.Add(new RecipePortionWarningDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        PortionMilligrams = portionAmount,
                        CurrentDailyMilligrams = currentDailyAmount,
                        ProjectedTotalMilligrams = projectedTotal,
                        MaxDailyMilligrams = maxThreshold,
                        Severity = MineralAlertSeverity.NearLimit,
                        SeverityBadgeText = badge,
                        WarningMessage = message
                    });
                }
            }
        }

        return warnings
            .OrderByDescending(w => w.Severity)
            .ThenByDescending(w => w.PercentageOfLimit)
            .ToList();
    }
}
