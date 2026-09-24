using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para guardar y recuperar la configuracion de umbrales maximos diarios de minerales
/// y el porcentaje de advertencia preventiva definido por el usuario.
/// Por que se tomo esta decision: Desacopla la persistencia de configuraciones de plataforma (Preferences, archivos, SQLite)
/// de la logica de negocio en la capa de aplicacion, preservando los principios de Domain Driven Design.
/// </summary>
public interface IMineralAlertStorage
{
    Dictionary<MineralType, double> GetAlertThresholds();
    void SaveAlertThresholds(IDictionary<MineralType, double> thresholds);
    double GetWarningPercentage();
    void SaveWarningPercentage(double percentage);
}
