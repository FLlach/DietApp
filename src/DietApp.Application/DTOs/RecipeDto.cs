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
    public List<MineralAmountDto> MineralsPerServing { get; set; } = new();
    public List<MineralAmountDto> TotalMinerals { get; set; } = new();
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    public List<RecipeStepDto> Steps { get; set; } = new();

    /// <summary>
    /// Subtitulo resumen que detalla calorias y minerales por porcion.
    /// </summary>
    public string NutritionSubtitle
    {
        get
        {
            var mineralsParts = MineralsPerServing
                .Take(4)
                .Select(m => $"{m.MineralName}: {m.Milligrams:F0}mg");

            string mineralsJoined = string.Join(", ", mineralsParts);
            string mineralsSummary = string.IsNullOrWhiteSpace(mineralsJoined) ? "Sin minerales registrados" : mineralsJoined;

            return $"Por porcion ({Servings} {(Servings == 1 ? "porcion" : "porciones")}): {CaloriesPerServing:F0} kcal | {mineralsSummary}";
        }
    }
}
