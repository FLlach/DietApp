namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Define el contrato de persistencia para la meta diaria de calorias
/// (en kilocalorias) y su estado de activacion, abstrayendo el mecanismo de almacenamiento del dispositivo.
/// Por que se tomo esta decision: Sigue estrictamente Domain-Driven Design (DDD) con inversion
/// de dependencias, desacoplando la logica de metas energeticas de implementaciones de plataforma.
/// </summary>
public interface ICalorieGoalStorage
{
    double GetDailyCalorieGoal();
    void SaveDailyCalorieGoal(double calories);
    bool IsCalorieGoalEnabled();
    void SaveCalorieGoalEnabled(bool isEnabled);
}
