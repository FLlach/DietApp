using DietApp.Application.Services;
using Microsoft.Maui.Storage;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Implementa IProteinGoalStorage utilizando Preferences de .NET MAUI para
/// guardar y recuperar de forma persistente la meta diaria de proteina (en gramos) y su estado de activacion.
/// Por que se tomo esta decision: Permite persistencia liviana entre sesiones del usuario sin requerir
/// modificaciones ni migraciones en las tablas de SQLite, desacoplando los datos de configuracion personal.
/// </summary>
public class MauiPreferencesProteinGoalStorage : IProteinGoalStorage
{
    private const string ProteinGoalGramsKey = "DietApp_ProteinGoalGrams";
    private const string ProteinGoalEnabledKey = "DietApp_ProteinGoalEnabled";
    private const double DefaultGoalGrams = 60.0;

    public double GetDailyProteinGoalGrams()
    {
        try
        {
            double value = Preferences.Default.Get<double>(ProteinGoalGramsKey, DefaultGoalGrams);
            return value > 0 ? value : DefaultGoalGrams;
        }
        catch
        {
            return DefaultGoalGrams;
        }
    }

    public void SaveDailyProteinGoalGrams(double grams)
    {
        try
        {
            Preferences.Default.Set(ProteinGoalGramsKey, grams);
        }
        catch
        {
            // Silencioso ante fallas de hardware para no interrumpir el flujo del usuario
        }
    }

    public bool IsProteinGoalEnabled()
    {
        try
        {
            return Preferences.Default.Get<bool>(ProteinGoalEnabledKey, true);
        }
        catch
        {
            return true;
        }
    }

    public void SaveProteinGoalEnabled(bool isEnabled)
    {
        try
        {
            Preferences.Default.Set(ProteinGoalEnabledKey, isEnabled);
        }
        catch
        {
            // Silencioso ante fallas de hardware para no interrumpir el flujo del usuario
        }
    }
}
