namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Implementa el servicio de administracion de la meta de proteina diaria,
/// gestionando la carga inicial desde almacenamiento persistente, la actualizacion de la cifra cuantitativa
/// y la emision de eventos para sincronizacion en tiempo real con las vistas de la aplicacion.
/// Por que se tomo esta decision: En DDD, mantiene la logica de metas nutricionales en la capa de aplicacion,
/// garantizando que tanto la pantalla de ajustes como las pantallas de seguimiento y composicion de comidas
/// consuman un unico punto de verdad coherente y reactivo.
/// </summary>
public class ProteinGoalService : IProteinGoalService
{
    public const double DefaultProteinGoalGrams = 60.0;

    private readonly IProteinGoalStorage _storage;
    private double _dailyProteinGoalGrams;
    private bool _isProteinGoalEnabled;

    public event EventHandler? ProteinGoalChanged;

    public double DailyProteinGoalGrams => _dailyProteinGoalGrams;
    public bool IsProteinGoalEnabled => _isProteinGoalEnabled;

    public ProteinGoalService(IProteinGoalStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dailyProteinGoalGrams = _storage.GetDailyProteinGoalGrams();
        if (_dailyProteinGoalGrams <= 0)
        {
            _dailyProteinGoalGrams = DefaultProteinGoalGrams;
        }
        _isProteinGoalEnabled = _storage.IsProteinGoalEnabled();
    }

    public void SetDailyProteinGoal(double grams, bool isEnabled = true)
    {
        double normalizedGrams = Math.Max(0.0, grams);
        _dailyProteinGoalGrams = normalizedGrams;
        _isProteinGoalEnabled = isEnabled;

        _storage.SaveDailyProteinGoalGrams(_dailyProteinGoalGrams);
        _storage.SaveProteinGoalEnabled(_isProteinGoalEnabled);

        ProteinGoalChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToDefault()
    {
        SetDailyProteinGoal(DefaultProteinGoalGrams, true);
    }
}
