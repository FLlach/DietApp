namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Implementa el servicio de administracion de la meta calorica diaria,
/// gestionando la carga inicial desde almacenamiento persistente, la actualizacion de la cifra cuantitativa
/// y la emision de eventos para sincronizacion en tiempo real con las vistas de la aplicacion.
/// Por que se tomo esta decision: En DDD, mantiene la logica de metas nutricionales en la capa de aplicacion,
/// garantizando que tanto la pantalla de ajustes como las pantallas de seguimiento y composicion de comidas
/// consuman un unico punto de verdad coherente y reactivo.
/// </summary>
public class CalorieGoalService : ICalorieGoalService
{
    public const double DefaultCalorieGoal = 2000.0;

    private readonly ICalorieGoalStorage _storage;
    private double _dailyCalorieGoal;
    private bool _isCalorieGoalEnabled;

    public event EventHandler? CalorieGoalChanged;

    public double DailyCalorieGoal => _dailyCalorieGoal;
    public bool IsCalorieGoalEnabled => _isCalorieGoalEnabled;

    public CalorieGoalService(ICalorieGoalStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dailyCalorieGoal = _storage.GetDailyCalorieGoal();
        if (_dailyCalorieGoal <= 0)
        {
            _dailyCalorieGoal = DefaultCalorieGoal;
        }
        _isCalorieGoalEnabled = _storage.IsCalorieGoalEnabled();
    }

    public void SetDailyCalorieGoal(double calories, bool isEnabled = true)
    {
        double normalizedCalories = Math.Max(0.0, calories);
        _dailyCalorieGoal = normalizedCalories;
        _isCalorieGoalEnabled = isEnabled;

        _storage.SaveDailyCalorieGoal(_dailyCalorieGoal);
        _storage.SaveCalorieGoalEnabled(_isCalorieGoalEnabled);

        CalorieGoalChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToDefault()
    {
        SetDailyCalorieGoal(DefaultCalorieGoal, true);
    }
}
