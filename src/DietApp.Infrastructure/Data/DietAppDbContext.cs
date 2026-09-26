using DietApp.Domain.Entities;
using DietApp.Domain.Repositories;
using DietApp.Infrastructure.Data.Models;
using DietApp.Infrastructure.SeedData;
using SQLite;

namespace DietApp.Infrastructure.Data;

/// <summary>
/// Como funciona: Administrador central de la conexion SQLite asincrona (`SQLiteAsyncConnection`).
/// Crea las tablas relacionales y asegura que la base de datos se inicialice automaticamente,
/// importando el archivo JSON de FoodData Central si la tabla de alimentos se encuentra vacia
/// o copiando una base pre-inicializada si esta disponible.
/// Por que se tomo esta decision: Centraliza el ciclo de vida de SQLite en la capa de Infraestructura,
/// garantizando que la inicializacion y migracion sea transparente para el dominio y la presentacion.
/// </summary>
public class DietAppDbContext
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly string _databasePath;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized = false;

    public SQLiteAsyncConnection Connection => _connection;

    public DietAppDbContext(string databasePath)
    {
        _databasePath = databasePath;

        // Si la base en AppDataDirectory no existe todavia, pero existe una precompilada en el directorio de trabajo, copiarla
        try
        {
            if (!File.Exists(databasePath))
            {
                string localDb = Path.Combine(Directory.GetCurrentDirectory(), "dietapp.db3");
                if (File.Exists(localDb))
                {
                    string parent = Path.GetDirectoryName(databasePath) ?? "";
                    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }
                    File.Copy(localDb, databasePath, overwrite: false);
                }
            }
        }
        catch
        {
            // Continuar con creacion normal si no se puede copiar
        }

        var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        _connection = new SQLiteAsyncConnection(databasePath, flags);
    }

    public async Task InitializeAsync(
        string? jsonSeedFilePath = null,
        Func<Task<Stream?>>? assetStreamProvider = null)
    {
        if (_isInitialized) return;

        await _initializationLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            // Creacion de tablas relacionales indexadas
            await _connection.CreateTableAsync<FoodEntity>();
            await _connection.CreateTableAsync<FoodPortionEntity>();
            await _connection.CreateTableAsync<MealEntity>();
            await _connection.CreateTableAsync<MealItemEntity>();
            await _connection.CreateTableAsync<RecipeEntity>();
            await _connection.CreateTableAsync<RecipeIngredientEntity>();
            await _connection.CreateTableAsync<RecipeStepEntity>();
            await _connection.CreateTableAsync<SeasoningEntity>();
            await _connection.CreateTableAsync<SeasoningItemEntity>();

            // Validar si la tabla de alimentos esta vacia para importar datos
            int foodCount = await _connection.Table<FoodEntity>().CountAsync();
            if (foodCount == 0)
            {
                bool importedFromStream = false;
                if (assetStreamProvider != null)
                {
                    try
                    {
                        using var stream = await assetStreamProvider();
                        if (stream != null)
                        {
                            await FoodDataCentralImporter.ImportFromJsonStreamAsync(stream, _connection);
                            importedFromStream = true;
                        }
                    }
                    catch
                    {
                        // Si falla la lectura del stream de recursos, continuar con busqueda de archivo en disco
                    }
                }

                if (!importedFromStream)
                {
                    // Buscar el archivo JSON en rutas tentativas comunes
                    string candidatePath = jsonSeedFilePath ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(candidatePath) || !File.Exists(candidatePath))
                    {
                        string[] possiblePaths =
                        {
                            "FoodData_Central_foundation_food_json_2026-04-30.json",
                            Path.Combine(AppContext.BaseDirectory, "FoodData_Central_foundation_food_json_2026-04-30.json"),
                            Path.Combine(Directory.GetCurrentDirectory(), "FoodData_Central_foundation_food_json_2026-04-30.json"),
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FoodData_Central_foundation_food_json_2026-04-30.json")
                        };

                        foreach (var path in possiblePaths)
                        {
                            if (File.Exists(path))
                            {
                                candidatePath = path;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(candidatePath) && File.Exists(candidatePath))
                    {
                        await FoodDataCentralImporter.ImportFromJsonFileAsync(candidatePath, _connection);
                    }
                    else
                    {
                        // Fallback a semillas basicas si el archivo json no esta en la ruta local
                        var initialFoods = InitialFoodCatalogSeed.GetPreloadedFoods();
                        var entities = initialFoods.Select(FoodEntity.FromDomain).ToList();
                        await _connection.InsertAllAsync(entities);
                    }
                }
            }
            else
            {
                // Actualizar valores de proteina si la base de datos ya existia pero tiene ceros
                int zeroProteinCount = await _connection.Table<FoodEntity>().Where(f => f.ProteinGrams == 0).CountAsync();
                if (zeroProteinCount > 0)
                {
                    var preloadedFoods = InitialFoodCatalogSeed.GetPreloadedFoods();
                    foreach (var seed in preloadedFoods)
                    {
                        await _connection.ExecuteAsync("UPDATE Foods SET ProteinGrams = ? WHERE Id = ? AND (ProteinGrams = 0 OR ProteinGrams IS NULL)", seed.ProteinGrams, seed.Id);
                    }

                    if (assetStreamProvider != null)
                    {
                        try
                        {
                            using var stream = await assetStreamProvider();
                            if (stream != null)
                            {
                                await FoodDataCentralImporter.BackfillProteinIfMissingAsync(stream, _connection);
                            }
                        }
                        catch { }
                    }

                    string candidatePath = jsonSeedFilePath ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(candidatePath) || !File.Exists(candidatePath))
                    {
                        string[] possiblePaths =
                        {
                            "FoodData_Central_foundation_food_json_2026-04-30.json",
                            Path.Combine(AppContext.BaseDirectory, "FoodData_Central_foundation_food_json_2026-04-30.json"),
                            Path.Combine(Directory.GetCurrentDirectory(), "FoodData_Central_foundation_food_json_2026-04-30.json"),
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FoodData_Central_foundation_food_json_2026-04-30.json")
                        };

                        foreach (var path in possiblePaths)
                        {
                            if (File.Exists(path))
                            {
                                candidatePath = path;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(candidatePath) && File.Exists(candidatePath))
                    {
                        await FoodDataCentralImporter.BackfillProteinIfMissingFromFileAsync(candidatePath, _connection);
                    }
                }
            }

            // Sincronizar proteinas calculadas en ingredientes y comidas existentes
            var zeroProteinIngredients = await _connection.Table<RecipeIngredientEntity>().Where(i => i.CalculatedProtein == 0).ToListAsync();
            if (zeroProteinIngredients.Count > 0)
            {
                foreach (var ingredient in zeroProteinIngredients)
                {
                    var food = await _connection.Table<FoodEntity>().FirstOrDefaultAsync(f => f.Id == ingredient.FoodItemId);
                    if (food != null && food.ProteinGrams > 0)
                    {
                        double calculatedProtein = (ingredient.Grams / (food.ReferenceGrams > 0 ? food.ReferenceGrams : 100.0)) * food.ProteinGrams;
                        await _connection.ExecuteAsync("UPDATE RecipeIngredients SET CalculatedProtein = ? WHERE Id = ?", calculatedProtein, ingredient.Id);
                    }
                }
            }

            var zeroProteinMealItems = await _connection.Table<MealItemEntity>().Where(m => m.CalculatedProtein == 0).ToListAsync();
            if (zeroProteinMealItems.Count > 0)
            {
                foreach (var mealItem in zeroProteinMealItems)
                {
                    var food = await _connection.Table<FoodEntity>().FirstOrDefaultAsync(f => f.Id == mealItem.FoodItemId);
                    if (food != null && food.ProteinGrams > 0)
                    {
                        double calculatedProtein = (mealItem.PortionInGrams / (food.ReferenceGrams > 0 ? food.ReferenceGrams : 100.0)) * food.ProteinGrams;
                        await _connection.ExecuteAsync("UPDATE MealItems SET CalculatedProtein = ? WHERE Id = ?", calculatedProtein, mealItem.Id);
                    }
                }
            }

            // Validar si la tabla de recetas esta vacia para precargar recetas de ejemplo
            int recipeCount = await _connection.Table<RecipeEntity>().CountAsync();
            if (recipeCount == 0)
            {
                var foods = await _connection.Table<FoodEntity>().Take(50).ToListAsync();
                var domainFoods = foods.Select(f => f.ToDomain()).ToList();
                var seedRecipes = InitialRecipeSeed.GetPreloadedRecipes(domainFoods);

                foreach (var recipe in seedRecipes)
                {
                    await _connection.InsertAsync(RecipeEntity.FromDomain(recipe));

                    foreach (var ing in recipe.Ingredients)
                    {
                        await _connection.InsertAsync(RecipeIngredientEntity.FromDomain(recipe.Id, ing));
                    }

                    foreach (var step in recipe.Steps)
                    {
                        await _connection.InsertAsync(RecipeStepEntity.FromDomain(recipe.Id, step));
                    }
                }
            }

            // Validar si la tabla de alinos esta vacia para precargar alinos de ejemplo
            int seasoningCount = await _connection.Table<SeasoningEntity>().CountAsync();
            if (seasoningCount == 0)
            {
                var foods = await _connection.Table<FoodEntity>().Take(50).ToListAsync();
                var domainFoods = foods.Select(f => f.ToDomain()).ToList();
                var seedSeasonings = InitialSeasoningSeed.GetPreloadedSeasonings(domainFoods);

                foreach (var seasoning in seedSeasonings)
                {
                    await _connection.InsertAsync(SeasoningEntity.FromDomain(seasoning));

                    foreach (var item in seasoning.Items)
                    {
                        await _connection.InsertAsync(SeasoningItemEntity.FromDomain(seasoning.Id, item));
                    }
                }
            }

            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}
