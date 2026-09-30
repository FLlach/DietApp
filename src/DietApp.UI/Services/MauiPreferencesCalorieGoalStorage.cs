using DietApp.Application.Services;
using Microsoft.Maui.Storage;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Implementa ICalorieGoalStorage utilizando Preferences de .NET MAUI para
/// guardar y recuperar de forma persistente la meta diaria de calorias (en kcal) y su estado de activacion.
/// Por que se tomo esta decision: Permite persistencia liviana entre sesiones del usuario sin requerir
/// modificaciones ni migraciones en las tablas de SQLite, desacoplando los datos de configuracion personal.
/// </summary>
public class MauiPreferencesCalorieGoalStorage : ICalorieGoalStorage
{
    private const string CalorieGoalKey = "DietApp_CalorieGoal";
    private const string CalorieGoalEnabledKey = "DietApp_CalorieGoalEnabled";
    private const double DefaultGoalCalories = 2000.0;

    public double GetDailyCalorieGoal()
    {
        try
        {
            double value = Preferences.Default.Get<double>(CalorieGoalKey, DefaultGoalCalories);
            return value > 0 ? value : DefaultGoalCalories;
        }
        catch
        {
            return DefaultGoalCalories;
        }
    }

    public void SaveDailyCalorieGoal(double calories)
    {
        try
        {
            Preferences.Default.Set(CalorieGoalKey, calories);
        }
        catch
        {
            // Silencioso ante fallas de hardware para no interrumpir el flujo del usuario
        }
    }

    public bool IsCalorieGoalEnabled()
    {
        try
        {
            return Preferences.Default.Get<bool>(CalorieGoalEnabledKey, true);
        }
        catch
        {
            return true;
        }
    }

    public void SaveCalorieGoalEnabled(bool isEnabled)
    {
        try
        {
            Preferences.Default.Set(CalorieGoalEnabledKey, isEnabled);
        }
        catch
        {
            // Silencioso ante fallas de hardware para no interrumpir el flujo del usuario
        }
    }
}
