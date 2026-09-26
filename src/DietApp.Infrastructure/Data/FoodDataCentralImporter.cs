using System.Text.Json;
using DietApp.Infrastructure.Data.Models;
using SQLite;

namespace DietApp.Infrastructure.Data;

/// <summary>
/// Como funciona: Servicio que lee el archivo JSON oficial de USDA FoodData Central Foundation Foods,
/// extrae los 395 alimentos base, normaliza las medidas a gramos (base referencial de 100g para nutrientes
/// y peso exacto gramWeight para porciones caseras) y los inserta de forma transaccional en SQLite.
/// Por que se tomo esta decision: Al admitir tanto Stream (para MauiAssets empaquetados en Android/iOS)
/// como ruta de archivo directo, permite importar los datos de manera transparente en cualquier plataforma
/// protegiendo contra propiedades nulas en el arbol JSON.
/// </summary>
public static class FoodDataCentralImporter
{
    public static async Task ImportFromJsonFileAsync(
        string jsonFilePath,
        SQLiteAsyncConnection databaseConnection)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException("No se encontro el archivo JSON de FoodData Central.", jsonFilePath);
        }

        using var fileStream = File.OpenRead(jsonFilePath);
        await ImportFromJsonStreamAsync(fileStream, databaseConnection);
    }

    public static async Task ImportFromJsonStreamAsync(
        Stream jsonStream,
        SQLiteAsyncConnection databaseConnection)
    {
        if (jsonStream == null)
        {
            throw new ArgumentNullException(nameof(jsonStream));
        }

        using var document = await JsonDocument.ParseAsync(jsonStream);

        var root = document.RootElement;
        if (!root.TryGetProperty("FoundationFoods", out var foodsElement) || foodsElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("El archivo JSON no contiene un arreglo valido en 'FoundationFoods'.");
        }

        var foodsToInsert = new List<FoodEntity>();
        var portionsToInsert = new List<FoodPortionEntity>();

        foreach (var foodElement in foodsElement.EnumerateArray())
        {
            if (foodElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var foodId = Guid.NewGuid();

            int fdcId = foodElement.TryGetProperty("fdcId", out var fdcProp) && 
                        fdcProp.ValueKind == JsonValueKind.Number && 
                        fdcProp.TryGetInt32(out int idVal)
                ? idVal
                : 0;

            string name = foodElement.TryGetProperty("description", out var descProp) && 
                          descProp.ValueKind == JsonValueKind.String
                ? descProp.GetString() ?? "Sin nombre"
                : "Sin nombre";

            string category = "General";
            if (foodElement.TryGetProperty("foodCategory", out var categoryProp) &&
                categoryProp.ValueKind == JsonValueKind.Object &&
                categoryProp.TryGetProperty("description", out var catDescProp) &&
                catDescProp.ValueKind == JsonValueKind.String)
            {
                category = catDescProp.GetString() ?? "General";
            }

            double calories = 0.0;
            double protein = 0.0;
            double phosphorus = 0.0;
            double potassium = 0.0;
            double sodium = 0.0;
            double calcium = 0.0;
            double magnesium = 0.0;
            double iron = 0.0;
            double zinc = 0.0;

            if (foodElement.TryGetProperty("foodNutrients", out var nutrientsProp) &&
                nutrientsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var nutrientItem in nutrientsProp.EnumerateArray())
                {
                    if (nutrientItem.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    double amount = 0.0;
                    if (nutrientItem.TryGetProperty("amount", out var amountProp) &&
                        amountProp.ValueKind == JsonValueKind.Number &&
                        amountProp.TryGetDouble(out double parsedAmount))
                    {
                        amount = parsedAmount;
                    }

                    if (nutrientItem.TryGetProperty("nutrient", out var nutrientObj) &&
                        nutrientObj.ValueKind == JsonValueKind.Object)
                    {
                        string nutrientNumber = nutrientObj.TryGetProperty("number", out var numProp) &&
                                                numProp.ValueKind == JsonValueKind.String
                            ? numProp.GetString() ?? ""
                            : "";

                        switch (nutrientNumber)
                        {
                            case "208": // Energia / Calorias (kcal)
                                calories = amount;
                                break;
                            case "203": // Proteina (g)
                                protein = amount;
                                break;
                            case "305": // Fosforo (P)
                                phosphorus = amount;
                                break;
                            case "306": // Potasio (K)
                                potassium = amount;
                                break;
                            case "307": // Sodio (Na)
                                sodium = amount;
                                break;
                            case "301": // Calcio (Ca)
                                calcium = amount;
                                break;
                            case "304": // Magnesio (Mg)
                                magnesium = amount;
                                break;
                            case "303": // Hierro (Fe)
                                iron = amount;
                                break;
                            case "309": // Zinc (Zn)
                                zinc = amount;
                                break;
                        }
                    }
                }
            }

            var foodEntity = new FoodEntity
            {
                Id = foodId,
                FdcId = fdcId,
                Name = name.Trim(),
                Category = category.Trim(),
                ReferenceGrams = 100.0, // Normalizado a 100 gramos de referencia oficial
                Calories = calories,
                ProteinGrams = protein,
                PhosphorusMg = phosphorus,
                PotassiumMg = potassium,
                SodiumMg = sodium,
                CalciumMg = calcium,
                MagnesiumMg = magnesium,
                IronMg = iron,
                ZincMg = zinc
            };

            foodsToInsert.Add(foodEntity);

            // Normalizar porciones habituales con peso en gramos
            if (foodElement.TryGetProperty("foodPortions", out var portionsProp) &&
                portionsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var portionItem in portionsProp.EnumerateArray())
                {
                    if (portionItem.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    double gramWeight = 0.0;
                    if (portionItem.TryGetProperty("gramWeight", out var gwProp) &&
                        gwProp.ValueKind == JsonValueKind.Number &&
                        gwProp.TryGetDouble(out double parsedGw))
                    {
                        gramWeight = parsedGw;
                    }

                    if (gramWeight <= 0)
                    {
                        continue;
                    }

                    double amount = 1.0;
                    if (portionItem.TryGetProperty("amount", out var amtProp) &&
                        amtProp.ValueKind == JsonValueKind.Number &&
                        amtProp.TryGetDouble(out double parsedAmt))
                    {
                        amount = parsedAmt;
                    }
                    else if (portionItem.TryGetProperty("value", out var valProp) &&
                             valProp.ValueKind == JsonValueKind.Number &&
                             valProp.TryGetDouble(out double parsedVal))
                    {
                        amount = parsedVal;
                    }

                    string unitName = "porcion";
                    if (portionItem.TryGetProperty("measureUnit", out var unitProp) &&
                        unitProp.ValueKind == JsonValueKind.Object &&
                        unitProp.TryGetProperty("name", out var unitNameProp) &&
                        unitNameProp.ValueKind == JsonValueKind.String)
                    {
                        unitName = unitNameProp.GetString() ?? "porcion";
                    }

                    string modifier = portionItem.TryGetProperty("modifier", out var modProp) &&
                                      modProp.ValueKind == JsonValueKind.String
                        ? modProp.GetString() ?? ""
                        : "";

                    portionsToInsert.Add(new FoodPortionEntity
                    {
                        Id = Guid.NewGuid(),
                        FoodId = foodId,
                        Amount = amount,
                        MeasureUnitName = unitName,
                        GramWeight = gramWeight,
                        Modifier = modifier
                    });
                }
            }
        }

        // Insercion masiva en transaccion unica
        await databaseConnection.RunInTransactionAsync(conn =>
        {
            conn.InsertAll(foodsToInsert);
            conn.InsertAll(portionsToInsert);
        });
    }

    /// <summary>
    /// Como funciona: Recorre el stream JSON de FoodData Central y actualiza la columna ProteinGrams
    /// para aquellos alimentos que tengan valor cero en la base de datos existente.
    /// Por que se tomo esta decision: Permite una migracion automatica y transparente de bases de datos
    /// SQLite preexistentes sin requerir borrar la base de datos ni perder registros del usuario.
    /// </summary>
    public static async Task BackfillProteinIfMissingAsync(
        Stream jsonStream,
        SQLiteAsyncConnection databaseConnection)
    {
        if (jsonStream == null) return;

        using var document = await JsonDocument.ParseAsync(jsonStream);
        var root = document.RootElement;
        if (!root.TryGetProperty("FoundationFoods", out var foodsElement) || foodsElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var fdcProteinMap = new Dictionary<int, double>();

        foreach (var foodElement in foodsElement.EnumerateArray())
        {
            if (foodElement.ValueKind != JsonValueKind.Object) continue;

            if (foodElement.TryGetProperty("fdcId", out var fdcProp) &&
                fdcProp.ValueKind == JsonValueKind.Number &&
                fdcProp.TryGetInt32(out int fdcId) && fdcId > 0)
            {
                if (foodElement.TryGetProperty("foodNutrients", out var nutrientsProp) &&
                    nutrientsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var nutrientItem in nutrientsProp.EnumerateArray())
                    {
                        if (nutrientItem.ValueKind != JsonValueKind.Object) continue;

                        if (nutrientItem.TryGetProperty("nutrient", out var nutrientObj) &&
                            nutrientObj.ValueKind == JsonValueKind.Object &&
                            nutrientObj.TryGetProperty("number", out var numProp) &&
                            numProp.GetString() == "203")
                        {
                            if (nutrientItem.TryGetProperty("amount", out var amtProp) &&
                                amtProp.TryGetDouble(out double amount))
                            {
                                fdcProteinMap[fdcId] = amount;
                            }
                            break;
                        }
                    }
                }
            }
        }

        if (fdcProteinMap.Count > 0)
        {
            await databaseConnection.RunInTransactionAsync(conn =>
            {
                foreach (var (fdcId, proteinGrams) in fdcProteinMap)
                {
                    conn.Execute("UPDATE Foods SET ProteinGrams = ? WHERE FdcId = ? AND (ProteinGrams = 0 OR ProteinGrams IS NULL)", proteinGrams, fdcId);
                }
            });
        }
    }

    public static async Task BackfillProteinIfMissingFromFileAsync(
        string jsonFilePath,
        SQLiteAsyncConnection databaseConnection)
    {
        if (!File.Exists(jsonFilePath)) return;

        using var fileStream = File.OpenRead(jsonFilePath);
        await BackfillProteinIfMissingAsync(fileStream, databaseConnection);
    }
}
