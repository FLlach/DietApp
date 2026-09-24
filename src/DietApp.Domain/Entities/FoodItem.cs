using DietApp.Domain.Enums;
using DietApp.Domain.ValueObjects;

namespace DietApp.Domain.Entities;

/// <summary>
/// Como funciona: Entidad que representa un alimento registrado en el catalogo nutricional.
/// Almacena su composicion de minerales referenciada a una porcion base (por defecto 100 gramos)
/// y provee metodos para calcular la cantidad real de minerales para cualquier porcion consumida.
/// Por que se tomo esta decision: En DDD, FoodItem posee identidad unica (Id). Al desacoplar
/// la definicion base del alimento (por 100g) del consumo real del usuario, se evita duplicar
/// datos y se garantiza precision en el calculo de totales por porcion.
/// </summary>
public class FoodItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public double ReferenceGrams { get; private set; }
    public IReadOnlyList<MineralAmount> Minerals { get; private set; }

    public FoodItem(
        Guid id,
        string name,
        string category,
        double referenceGrams,
        IEnumerable<MineralAmount> minerals)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del alimento no puede estar vacio.", nameof(name));
        }

        if (referenceGrams <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(referenceGrams),
                "La porcion de referencia en gramos debe ser mayor a cero.");
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
        ReferenceGrams = referenceGrams;
        Minerals = minerals?.ToList() ?? new List<MineralAmount>();
    }

    /// <summary>
    /// Obtiene la cantidad de miligramos de un mineral especifico en la porcion de referencia.
    /// </summary>
    public double GetMineralMilligrams(MineralType mineralType)
    {
        for (int index = 0; index < Minerals.Count; index++)
        {
            if (Minerals[index].Type == mineralType)
            {
                return Minerals[index].Milligrams;
            }
        }

        return 0.0;
    }

    /// <summary>
    /// Calcula el contenido de minerales proyectado para una porcion arbitraria en gramos.
    /// </summary>
    /// <param name="grams">Peso consumido en gramos.</param>
    public IReadOnlyList<MineralAmount> CalculateMineralsForPortion(double grams)
    {
        if (grams < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grams),
                "La cantidad de gramos consumida no puede ser negativa.");
        }

        double ratio = grams / ReferenceGrams;
        var scaledMinerals = new List<MineralAmount>(Minerals.Count);

        for (int index = 0; index < Minerals.Count; index++)
        {
            scaledMinerals.Add(Minerals[index].Scale(ratio));
        }

        return scaledMinerals;
    }
}
