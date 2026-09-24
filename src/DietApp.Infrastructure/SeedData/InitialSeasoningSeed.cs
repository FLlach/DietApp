using DietApp.Domain.Entities;

namespace DietApp.Infrastructure.SeedData;

/// <summary>
/// Como funciona: Provee un catalogo inicial de alinos y condimentos habituales (vinagretas, aderezos y mezclas).
/// Resuelve los alimentos disponibles en el catalogo para armar composiciones representativas con gramajes definidos.
/// Por que se tomo esta decision: Permite al usuario contar de inmediato con alinos utilizables para acelerar
/// la creacion de recetas desde la primera ejecucion de la aplicacion.
/// </summary>
public static class InitialSeasoningSeed
{
    public static List<Seasoning> GetPreloadedSeasonings(IReadOnlyList<FoodItem> availableFoods)
    {
        var seasonings = new List<Seasoning>();
        if (availableFoods == null || availableFoods.Count == 0)
        {
            return seasonings;
        }

        // Buscar alimentos para condimentos o utilizar los primeros disponibles
        var oilOrFat = availableFoods.FirstOrDefault(f => f.Name.Contains("Oil", StringComparison.OrdinalIgnoreCase) ||
                                                          f.Name.Contains("Aceite", StringComparison.OrdinalIgnoreCase) ||
                                                          f.Name.Contains("Butter", StringComparison.OrdinalIgnoreCase))
                       ?? availableFoods[0];

        var citrusOrAcid = availableFoods.FirstOrDefault(f => f.Name.Contains("Lemon", StringComparison.OrdinalIgnoreCase) ||
                                                              f.Name.Contains("Limon", StringComparison.OrdinalIgnoreCase) ||
                                                              f.Name.Contains("Vinegar", StringComparison.OrdinalIgnoreCase) ||
                                                              f.Name.Contains("Juice", StringComparison.OrdinalIgnoreCase))
                          ?? (availableFoods.Count > 1 ? availableFoods[1] : availableFoods[0]);

        var herbOrSpice = availableFoods.FirstOrDefault(f => f.Name.Contains("Pepper", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.Contains("Garlic", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.Contains("Ajo", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.Contains("Salt", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.Contains("Herb", StringComparison.OrdinalIgnoreCase))
                         ?? (availableFoods.Count > 2 ? availableFoods[2] : availableFoods[0]);

        // Alino 1: Vinagreta Clasica Mediterranea
        var seasoning1 = new Seasoning(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Vinagreta Clasica de Limon y Oliva",
            "Aderezo tradicional balanceado para ensaladas frescas, vegetales cocidos y pescados.");
        seasoning1.AddItem(SeasoningItem.FromFoodItem(oilOrFat, 20.0));
        seasoning1.AddItem(SeasoningItem.FromFoodItem(citrusOrAcid, 10.0));
        seasoning1.AddItem(SeasoningItem.FromFoodItem(herbOrSpice, 2.0));
        seasonings.Add(seasoning1);

        // Alino 2: Condimento Aromatico para Guisos y Carnes
        var seasoning2 = new Seasoning(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Mezcla Aromatica de Hierbas y Aceite",
            "Combinacion aromatica para marinar pechuga de pollo, carnes magras o verduras al horno.");
        seasoning2.AddItem(SeasoningItem.FromFoodItem(oilOrFat, 15.0));
        seasoning2.AddItem(SeasoningItem.FromFoodItem(herbOrSpice, 5.0));
        seasonings.Add(seasoning2);

        return seasonings;
    }
}
