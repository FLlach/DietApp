using DietApp.Domain.Enums;

namespace DietApp.Application.DTOs;

/// <summary>
/// Como funciona: DTO que transporta los datos de un ingrediente utilizado en una receta culinaria,
/// conteniendo los gramos requeridos y el desglose de calorias y minerales aportados.
/// Por que se tomo esta decision: Permite a las vistas de detalle de receta mostrar claramente
/// la cantidad en gramos y el aporte energetico individual de cada ingrediente del plato.
/// </summary>
public class RecipeIngredientDto
{
    public Guid Id { get; set; }
    public Guid FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Grams { get; set; }
    public double CalculatedCalories { get; set; }
    public double CalculatedProtein { get; set; }
    public List<MineralAmountDto> CalculatedMinerals { get; set; } = new();

    private string _imagePath = string.Empty;

    /// <summary>
    /// Como funciona: Ruta de la fotografia del ingrediente para exhibicion en la cuadricula 2x2.
    /// Si no se asigno una ruta explicita, infiere la imagen gastronomica correspondiente segun el nombre.
    /// Por que se tomo esta decision: Permite alinear la interfaz con el mockup visual de detalle de receta.
    /// </summary>
    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_imagePath)) return _imagePath;
            if (FoodName.Contains("Papa", StringComparison.OrdinalIgnoreCase) || FoodName.Contains("Potato", StringComparison.OrdinalIgnoreCase))
                return "food_potatoes.jpg";
            if (FoodName.Contains("Arroz", StringComparison.OrdinalIgnoreCase) || FoodName.Contains("Rice", StringComparison.OrdinalIgnoreCase))
                return "food_rice.jpg";
            if (FoodName.Contains("Brocol", StringComparison.OrdinalIgnoreCase) || FoodName.Contains("Broccol", StringComparison.OrdinalIgnoreCase))
                return "food_broccoli.jpg";
            if (FoodName.Contains("Pollo", StringComparison.OrdinalIgnoreCase) || FoodName.Contains("Chicken", StringComparison.OrdinalIgnoreCase))
                return "food_chicken.jpg";
            return "food_chicken.jpg";
        }
        set => _imagePath = value;
    }

    /// <summary>
    /// Como funciona: Formato estandarizado de peso en gramos para las tarjetas visuales (ej. '200 gr.').
    /// Por que se tomo esta decision: Replica fielmente la composicion tipografica de la Vista A en mockupBase.jpeg.
    /// </summary>
    public string GramsDisplay => $"{Grams:F0} gr.";

    public string DisplayText => $"{FoodName}: {Grams:F0}g ({CalculatedCalories:F0} kcal, {CalculatedProtein:F1}g prot.)";

    public string MineralsSummary
    {
        get
        {
            var parts = CalculatedMinerals
                .Where(m => m.Milligrams > 0)
                .Select(m => $"{m.MineralName}: {m.Milligrams:F0}mg");
            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Como funciona: Retorna la cantidad en miligramos de un mineral especifico aportado por este ingrediente.
    /// Por que se tomo esta decision: Facilita el ordenamiento de los ingredientes en la pantalla de detalle.
    /// </summary>
    public double GetMineralAmount(MineralType mineralType)
    {
        var item = CalculatedMinerals.FirstOrDefault(m => m.Type == mineralType);
        return item?.Milligrams ?? 0.0;
    }
}
