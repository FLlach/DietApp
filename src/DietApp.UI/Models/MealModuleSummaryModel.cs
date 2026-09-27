namespace DietApp.UI.Models;

/// <summary>
/// Como funciona: Modelo de datos para las tarjetas modulares del carrusel superior (Vista B en mockupBase.jpeg).
/// Transporta el titulo, la porcion, la imagen opcional, el icono de estado (alerta '!' o check) y la coleccion de pildoras.
/// Por que se tomo esta decision: Replica con total fidelidad los modulos Receta, Postre y Liquido del mockup.
/// </summary>
public class MealModuleSummaryModel
{
    public string Title { get; set; } = string.Empty;
    public string PortionText { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);
    public string BadgeIcon { get; set; } = string.Empty;
    public bool IsAlertBadge { get; set; }
    public List<NutrientPillItemModel> NutrientPills { get; set; } = new();
}

/// <summary>
/// Como funciona: Representa una pildora de nutriente dentro de los modulos superiores con indicador de si es destacada.
/// Por que se tomo esta decision: Permite colorear en naranja coral las alertas o valores preponderantes como Potasio.
/// </summary>
public class NutrientPillItemModel
{
    public string Text { get; set; } = string.Empty;
    public bool IsHighlighted { get; set; }
}
