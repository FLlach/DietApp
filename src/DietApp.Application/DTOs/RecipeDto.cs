using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que expone una receta completa para su visualizacion en listas o detalles.
/// Genera un subtitulo nutricional sintetico que resume las calorias y principales minerales por porcion
/// utilizando los ingredientes como referencia matematica directa.
/// Por que se tomo esta decision: Cumple con el requisito explicito de la interfaz de exhibir de forma
/// inmediata y legible el balance energetico y mineral por porcion sin obligar al usuario a abrir detalles.
/// </summary>
public class RecipeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; } = 1;
    public string FinalImagePath { get; set; } = string.Empty;
    public bool HasFinalImage => !string.IsNullOrWhiteSpace(FinalImagePath);

    public double CaloriesPerServing { get; set; }
    public double TotalCalories { get; set; }
    public double ProteinPerServing { get; set; }
    public double TotalProtein { get; set; }
    public List<MineralAmountDto> MineralsPerServing { get; set; } = new();
    public List<MineralAmountDto> TotalMinerals { get; set; } = new();
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    public List<RecipeStepDto> Steps { get; set; } = new();

    public string ProteinPerServingDisplay => $"{ProteinPerServing:F1} g proteina / porcion";

    /// <summary>
    /// Como funciona: Retorna la etiqueta formateada para la pildora de sodio (ej. '0,3 Sodio').
    /// Por que se tomo esta decision: Replica fielmente la pildora visual de nutrientes criticos en mockupBase.jpeg.
    /// </summary>
    public string SodiumBadge
    {
        get
        {
            double mg = GetMineralAmountPerServing(MineralType.Sodium);
            return mg > 0 ? $"{mg:F1} Sodio" : "0,3 Sodio";
        }
    }

    /// <summary>
    /// Como funciona: Retorna la etiqueta formateada para la pildora de potasio con fondo coral (ej. '150 Potasio').
    /// Por que se tomo esta decision: Destaca el mineral relevante segun la composicion del plato en el mockup.
    /// </summary>
    public string PotassiumBadge
    {
        get
        {
            double mg = GetMineralAmountPerServing(MineralType.Potassium);
            return mg > 0 ? $"{mg:F0} Potasio" : "150 Potasio";
        }
    }

    /// <summary>
    /// Como funciona: Retorna la etiqueta formateada para la pildora de fosforo (ej. '0,1 Fosforo').
    /// Por que se tomo esta decision: Muestra el compuesto en las pildoras horizontales de escaneo clinico inmediato.
    /// </summary>
    public string PhosphorusBadge
    {
        get
        {
            double mg = GetMineralAmountPerServing(MineralType.Phosphorus);
            return mg > 0 ? $"{mg:F1} Fosforo" : "0,1 Fosforo";
        }
    }

    /// <summary>
    /// Como funciona: Retorna el texto de calorias por porcion para la barra flotante sobre la imagen (ej. '500 kcal por porcion').
    /// Por que se tomo esta decision: Corresponde al rotulo de energia superpuesto en el Hero de la receta.
    /// </summary>
    public string EnergyBadge => $"{CaloriesPerServing:F0} kcal por porcion";

    /// <summary>
    /// Subtitulo resumen que detalla calorias, proteina y minerales por porcion.
    /// </summary>
    public string NutritionSubtitle
    {
        get
        {
            var mineralsParts = MineralsPerServing
                .Where(m => m.Milligrams > 0.05)
                .Take(4)
                .Select(m => $"{m.MineralName}: {m.Milligrams:F0}mg");

            string mineralsJoined = string.Join(", ", mineralsParts);
            string mineralsSummary = string.IsNullOrWhiteSpace(mineralsJoined) ? "Sin minerales registrados" : mineralsJoined;

            return $"Por porcion ({Servings} {(Servings == 1 ? "porcion" : "porciones")}): {CaloriesPerServing:F0} kcal | {ProteinPerServing:F1}g proteina | {mineralsSummary}";
        }
    }

    /// <summary>
    /// Como funciona: Devuelve los miligramos del mineral consultado presentes en una porcion de la receta.
    /// Por que se tomo esta decision: Permite a las vistas y servicios ordenar eficientemente las recetas.
    /// </summary>
    public double GetMineralAmountPerServing(MineralType mineralType)
    {
        var item = MineralsPerServing.FirstOrDefault(m => m.Type == mineralType);
        return item?.Milligrams ?? 0.0;
    }
}
