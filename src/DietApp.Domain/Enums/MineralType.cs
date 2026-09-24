namespace DietApp.Domain.Enums;

/// <summary>
/// Como funciona: Define los tipos de minerales cuantificables en los alimentos.
/// Por que se tomo esta decision: El uso de una enumeracion fuertemente tipada
/// previene errores tipograficos por cadenas magicas, optimiza el filtrado
/// en memoria y bases de datos, y centraliza el catalogo de minerales soportados.
/// </summary>
public enum MineralType
{
    Phosphorus = 1,
    Potassium = 2,
    Sodium = 3,
    Calcium = 4,
    Magnesium = 5,
    Iron = 6,
    Zinc = 7
}
