namespace DietApp.Domain.Enums;

/// <summary>
/// Como funciona: Define las modalidades de tema visual disponibles en la aplicacion (Claro, Oscuro o Sistema).
/// Por que se tomo esta decision: Permite independizar la configuracion estetica en el modelo de dominio,
/// permitiendo su seleccion, serializacion y persistencia sin acoplamiento a librerias de interfaz grafica.
/// </summary>
public enum ThemeMode
{
    Light = 1,
    Dark = 2,
    System = 3
}
