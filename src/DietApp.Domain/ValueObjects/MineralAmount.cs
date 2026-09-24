using DietApp.Domain.Enums;

namespace DietApp.Domain.ValueObjects;

/// <summary>
/// Como funciona: Modela la cantidad especifica de un mineral cuantificada en miligramos (mg).
/// Por que se tomo esta decision: En Domain-Driven Design, cantidades con unidad de medida
/// son Objetos de Valor (Value Objects). Son inmutables y dos instancias son iguales si su
/// tipo de mineral y valor numerico coinciden, garantizando integridad en operaciones aritmeticas.
/// </summary>
public readonly record struct MineralAmount
{
    public MineralType Type { get; }
    public double Milligrams { get; }

    public MineralAmount(MineralType type, double milligrams)
    {
        if (milligrams < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(milligrams),
                "La cantidad de mineral no puede ser negativa.");
        }

        Type = type;
        Milligrams = milligrams;
    }

    /// <summary>
    /// Calcula la cantidad proporcional de mineral en base a una razon de escala.
    /// </summary>
    /// <param name="scalingRatio">Factor multiplicador basado en la porcion consumida.</param>
    public MineralAmount Scale(double scalingRatio)
    {
        if (scalingRatio < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scalingRatio),
                "El factor de escala no puede ser negativo.");
        }

        return new MineralAmount(Type, Milligrams * scalingRatio);
    }
}
