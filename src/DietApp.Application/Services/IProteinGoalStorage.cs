namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Define el contrato de persistencia para la meta diaria de proteina
/// en gramos y su estado de activacion, abstrayendo el mecanismo de almacenamiento del dispositivo.
/// Por que se tomo esta decision: Permite bajo acoplamiento e inversion de dependencias segun DDD,
/// evitando acoplar la logica de negocio a librerias especificas de plataforma (Preferences, SQLite).
/// </summary>
public interface IProteinGoalStorage
{
    double GetDailyProteinGoalGrams();
    void SaveDailyProteinGoalGrams(double grams);
    bool IsProteinGoalEnabled();
    void SaveProteinGoalEnabled(bool isEnabled);
}
