namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Contrato para consultar, actualizar y notificar cambios en la meta
/// nutricional diaria de proteina del paciente (en gramos).
/// Por que se tomo esta decision: Centraliza en la capa de aplicacion las reglas de negocio
/// referentes al objetivo proteico diario, desacoplando los ViewModels y permitiendo sincronizacion reactiva.
/// </summary>
public interface IProteinGoalService
{
    double DailyProteinGoalGrams { get; }
    bool IsProteinGoalEnabled { get; }
    void SetDailyProteinGoal(double grams, bool isEnabled = true);
    void ResetToDefault();
    event EventHandler? ProteinGoalChanged;
}
