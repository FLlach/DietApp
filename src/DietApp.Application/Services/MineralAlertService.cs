using DietApp.Application.DTOs;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Administra los umbrales maximos diarios de minerales configurados por el usuario,
/// persiste la configuracion mediante IMineralAlertStorage y evalua ingestas de minerales para
/// detectar superaciones de limites, generando mensajes de advertencia contextualizados.
/// Por que se tomo esta decision: En DDD, encapsula la logica de alertas preventivas en la capa
/// de aplicacion, permitiendo que tanto el monitoreo diario como los registros de comidas alerten
/// de inmediato al usuario sin duplicar logica de comparacion numerica.
/// </summary>
public class MineralAlertService : IMineralAlertService
{
    private readonly IMineralAlertStorage _storage;
    private readonly ILocalizationService _localizationService;
    private readonly Dictionary<MineralType, double> _thresholds;

    public event EventHandler? ThresholdsChanged;

    public MineralAlertService(
        IMineralAlertStorage storage,
        ILocalizationService localizationService)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _thresholds = _storage.GetAlertThresholds() ?? new Dictionary<MineralType, double>();
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

    public IReadOnlyList<MineralAlertExceededDto> CheckExceededThresholds(IEnumerable<MineralAmountDto> dailyMinerals)
    {
        if (dailyMinerals == null) return Array.Empty<MineralAlertExceededDto>();

        var exceededList = new List<MineralAlertExceededDto>();

        foreach (var item in dailyMinerals)
        {
            if (_thresholds.TryGetValue(item.Type, out var maxThreshold) && maxThreshold > 0)
            {
                if (item.Milligrams >= maxThreshold)
                {
                    double excess = Math.Round(item.Milligrams - maxThreshold, 1);
                    string mineralDisplayName = _localizationService.GetMineralName(item.Type);

                    string warning = _localizationService.CurrentLanguage == "en"
                        ? $"Limit exceeded: {mineralDisplayName} reached {item.Milligrams:F0} mg (Max: {maxThreshold:F0} mg, Excess: {excess:F0} mg)"
                        : $"Limite superado: {mineralDisplayName} alcanzo {item.Milligrams:F0} mg (Maximo: {maxThreshold:F0} mg, Exceso: {excess:F0} mg)";

                    exceededList.Add(new MineralAlertExceededDto
                    {
                        Mineral = item.Type,
                        MineralName = mineralDisplayName,
                        CurrentMilligrams = item.Milligrams,
                        MaxMilligrams = maxThreshold,
                        WarningMessage = warning
                    });
                }
            }
        }

        return exceededList;
    }
}
