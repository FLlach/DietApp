namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para consultar, actualizar y notificar cambios en la meta
/// energetica diaria del paciente (en kilocalorias).
/// Por que se tomo esta decision: Centraliza en la capa de aplicacion las reglas de negocio
/// del balance energetico diario, permitiendo sincronizacion reactiva entre la pantalla de ajustes,
/// el conteo diario y el compositor de ingesta.
/// </summary>
public interface ICalorieGoalService
{
    double DailyCalorieGoal { get; }
    bool IsCalorieGoalEnabled { get; }
    void SetDailyCalorieGoal(double calories, bool isEnabled = true);
    void ResetToDefault();
    event EventHandler? CalorieGoalChanged;
}
