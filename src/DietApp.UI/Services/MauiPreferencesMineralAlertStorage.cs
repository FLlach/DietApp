using System.Text.Json;
using DietApp.Application.Services;
using DietApp.Domain.Enums;
using Microsoft.Maui.Storage;

namespace DietApp.UI.Services;

/// <summary>
/// Como funciona: Implementa IMineralAlertStorage utilizando las preferencias nativas de la plataforma (Preferences)
/// serializadas en formato JSON liviano.
/// Por que se tomo esta decision: Permite almacenar de forma persistente y agil los limites de minerales
/// configurados por el usuario entre ejecuciones de la app, sin requerir migraciones de esquema en la base relacional.
/// </summary>
public class MauiPreferencesMineralAlertStorage : IMineralAlertStorage
{
    private const string StorageKey = "DietApp_MineralAlertThresholds";

    public Dictionary<MineralType, double> GetAlertThresholds()
    {
        try
        {
            var json = Preferences.Default.Get<string?>(StorageKey, null);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<MineralType, double>();
            }

            var rawDict = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
            if (rawDict == null) return new Dictionary<MineralType, double>();

            var result = new Dictionary<MineralType, double>();
            foreach (var kvp in rawDict)
            {
                if (Enum.TryParse<MineralType>(kvp.Key, true, out var mineralType))
                {
                    result[mineralType] = kvp.Value;
                }
            }

            return result;
        }
        catch
        {
            return new Dictionary<MineralType, double>();
        }
    }

    public void SaveAlertThresholds(IDictionary<MineralType, double> thresholds)
    {
        try
        {
            if (thresholds == null || thresholds.Count == 0)
            {
                Preferences.Default.Remove(StorageKey);
                return;
            }

            var serializableDict = new Dictionary<string, double>();
            foreach (var kvp in thresholds)
            {
                serializableDict[kvp.Key.ToString()] = kvp.Value;
            }

            var json = JsonSerializer.Serialize(serializableDict);
            Preferences.Default.Set(StorageKey, json);
        }
        catch
        {
            // Silencioso ante fallas de hardware para no interrumpir el flujo del usuario
        }
    }
}
