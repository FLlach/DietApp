using System.ComponentModel;
using System.Globalization;
using DietApp.Domain.Enums;

namespace DietApp.Application.Services;

/// <summary>
/// Como funciona: Administra los diccionarios de traduccion entre ingles y espanol, coordina la
/// actualizacion de CultureInfo del hilo de ejecucion, persiste la seleccion mediante ILanguagePreferenceStorage
/// y notifica cambios a traves de INotifyPropertyChanged y del evento LanguageChanged.
/// Por que se tomo esta decision: Permite una localizacion reactiva e inmediata en toda la aplicacion,
/// garantizando que al alternar el idioma, todos los enlaces compilados en las vistas y los textos
/// generados por DTOs se actualicen instantaneamente sin reiniciar la app.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ILanguagePreferenceStorage? _preferenceStorage;
    private string _currentLanguage = "es";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    public string CurrentLanguage => _currentLanguage;

    public string this[string key] => GetString(key);

    public LocalizationService(ILanguagePreferenceStorage? preferenceStorage = null)
    {
        _preferenceStorage = preferenceStorage;

        var saved = _preferenceStorage?.GetSavedLanguage();
        if (!string.IsNullOrWhiteSpace(saved) && (saved == "en" || saved == "es"))
        {
            _currentLanguage = saved;
        }
        else
        {
            var systemCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            _currentLanguage = systemCulture == "en" ? "en" : "es";
        }

        ApplyCulture(_currentLanguage);
    }

    public IReadOnlyList<string> GetSupportedLanguages()
    {
        return new List<string> { "es", "en" };
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return;

        var normalized = languageCode.Trim().ToLowerInvariant();
        if (normalized != "es" && normalized != "en")
        {
            normalized = "es";
        }

        if (_currentLanguage == normalized) return;

        _currentLanguage = normalized;
        _preferenceStorage?.SaveLanguage(_currentLanguage);
        ApplyCulture(_currentLanguage);

        LanguageChanged?.Invoke(this, EventArgs.Empty);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = new CultureInfo(languageCode == "en" ? "en-US" : "es-ES");
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var dict = _currentLanguage == "en" ? EnglishStrings : SpanishStrings;
        if (dict.TryGetValue(key, out var translation))
        {
            return translation;
        }

        if (SpanishStrings.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public string GetMineralName(MineralType mineralType)
    {
        if (_currentLanguage == "en")
        {
            return mineralType switch
            {
                MineralType.Phosphorus => "Phosphorus",
                MineralType.Potassium => "Potassium",
                MineralType.Sodium => "Sodium",
                MineralType.Calcium => "Calcium",
                MineralType.Magnesium => "Magnesium",
                MineralType.Iron => "Iron",
                MineralType.Zinc => "Zinc",
                _ => mineralType.ToString()
            };
        }

        return mineralType switch
        {
            MineralType.Phosphorus => "Fosforo",
            MineralType.Potassium => "Potasio",
            MineralType.Sodium => "Sodio",
            MineralType.Calcium => "Calcio",
            MineralType.Magnesium => "Magnesio",
            MineralType.Iron => "Hierro",
            MineralType.Zinc => "Zinc",
            _ => mineralType.ToString()
        };
    }

    public string GetMealTypeName(MealType mealType)
    {
        if (_currentLanguage == "en")
        {
            return mealType switch
            {
                MealType.Breakfast => "Breakfast",
                MealType.Lunch => "Lunch",
                MealType.Dinner => "Dinner",
                MealType.Snack => "Snack / Collation",
                MealType.Other => "Other",
                _ => mealType.ToString()
            };
        }

        return mealType switch
        {
            MealType.Breakfast => "Desayuno",
            MealType.Lunch => "Almuerzo",
            MealType.Dinner => "Cena",
            MealType.Snack => "Merienda / Colacion",
            MealType.Other => "Otro",
            _ => mealType.ToString()
        };
    }

    private static readonly Dictionary<string, string> SpanishStrings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Navegacion y Pestanas
        ["Tab_DailyTracking"] = "Conteo Diario",
        ["Tab_Recipes"] = "Recetas",
        ["Tab_Catalog"] = "Catalogo y Filtro",
        ["Tab_AddMeal"] = "Registrar Comida",
        ["Tab_AddFood"] = "Nuevo Alimento",
        ["Tab_Settings"] = "Ajustes",

        // Pantalla Conteo Diario
        ["DailyTracking_Title"] = "Registro Diario de Ingesta y Minerales",
        ["DailyTracking_PreviousDay"] = "< Dia Anterior",
        ["DailyTracking_NextDay"] = "Dia Siguiente >",
        ["DailyTracking_DailyTotal"] = "Total Consumido del Dia",
        ["DailyTracking_DailyTotalSubtitle"] = "Suma consolidada de todos los compuestos minerales ingeridos hoy:",
        ["DailyTracking_MealsDetail"] = "Detalle de Comidas del Dia",
        ["DailyTracking_EmptyMeals"] = "No hay comidas registradas para esta fecha.",
        ["DailyTracking_EmptyAction"] = "Registrar una comida ahora",
        ["DailyTracking_Food"] = "Alimento",
        ["DailyTracking_Portion"] = "Porcion",
        ["DailyTracking_Delete"] = "Eliminar",

        // Pantalla Recetas
        ["Recipes_Title"] = "Recetario Nutricional",
        ["Recipes_NewRecipe"] = "+ Nueva Receta",
        ["Recipes_SearchPlaceholder"] = "Buscar recetas por titulo o descripcion...",
        ["Recipes_Empty"] = "No hay recetas registradas aun.",
        ["Recipes_EmptyAction"] = "Crear la primera receta",
        ["Recipes_ViewSteps"] = "Ver Pasos e Ingredientes",

        // Pantalla Detalle de Receta
        ["RecipeDetail_Title"] = "Detalle de la Receta",
        ["RecipeDetail_NutritionPerServing"] = "Aporte Nutricional por Porcion:",
        ["RecipeDetail_MineralsPerServingTitle"] = "Minerales Aportados por Porcion",
        ["RecipeDetail_MineralsPerServingSubtitle"] = "Balance mineral calculado automaticamente segun los ingredientes de la receta:",
        ["RecipeDetail_IntakePanelTitle"] = "Registrar en la Ingesta Diaria",
        ["RecipeDetail_IntakePanelSubtitle"] = "Agrega esta receta a tu registro de comidas para contabilizar sus minerales y calorias:",
        ["RecipeDetail_IntakeDate"] = "Fecha de Ingesta",
        ["RecipeDetail_MealMoment"] = "Momento de Comida",
        ["RecipeDetail_ServingsConsumed"] = "Porciones Consumidas",
        ["RecipeDetail_LogAction"] = "Registrar en mi Comida",
        ["RecipeDetail_IngredientsTitle"] = "Ingredientes de la Receta",
        ["RecipeDetail_StepsTitle"] = "Pasos de Elaboracion Numerados",

        // Minerales individuales
        ["Mineral_Phosphorus"] = "Fosforo (mg)",
        ["Mineral_Potassium"] = "Potasio (mg)",
        ["Mineral_Sodium"] = "Sodio (mg)",
        ["Mineral_Calcium"] = "Calcio (mg)",
        ["Mineral_Magnesium"] = "Magnesio (mg)",
        ["Mineral_Iron"] = "Hierro (mg)",
        ["Mineral_Zinc"] = "Zinc (mg)",

        // Pantalla Agregar Alimento
        ["AddFood_Title"] = "Agregar Nuevo Alimento",
        ["AddFood_Header"] = "Nuevo Alimento con Perfil Mineral",
        ["AddFood_Subtitle"] = "Ingresa los datos del alimento y la cantidad de minerales presentes por porcion de referencia (por ejemplo 100g):",
        ["AddFood_Name"] = "Nombre del Alimento",
        ["AddFood_NamePlaceholder"] = "Ej: Palta Hass, Avena tradicional",
        ["AddFood_Category"] = "Categoria",
        ["AddFood_CategoryPlaceholder"] = "Ej: Frutas, Cereales, Pescados",
        ["AddFood_PortionGrams"] = "Porcion base (Gramos)",
        ["AddFood_MineralsTitle"] = "Contenido de Minerales en la Porcion Base (mg)",
        ["AddFood_Calories"] = "Calorias (kcal por 100g)",
        ["AddFood_SaveButton"] = "Guardar Alimento en Catalogo",

        // Pantalla Crear Receta
        ["AddRecipe_Title"] = "Crear Nueva Receta",
        ["AddRecipe_Header"] = "Crear Receta con Perfil Nutricional",
        ["AddRecipe_GeneralData"] = "Datos Generales",
        ["AddRecipe_RecipeTitle"] = "Titulo de la Receta",
        ["AddRecipe_RecipeTitlePlaceholder"] = "Ej: Cazuela ligera de pollo y verduras",
        ["AddRecipe_Servings"] = "Numero de Porciones",
        ["AddRecipe_Description"] = "Descripcion",
        ["AddRecipe_DescriptionPlaceholder"] = "Breve descripcion o caracteristicas del plato...",
        ["AddRecipe_FinalImage"] = "Imagen Final de la Receta Terminada",
        ["AddRecipe_FinalImageSubtitle"] = "Selecciona una imagen desde tu dispositivo o ingresa la ruta/nombre del archivo:",
        ["AddRecipe_ImagePathPlaceholder"] = "Ruta o nombre de archivo...",
        ["AddRecipe_BrowseButton"] = "Examinar...",
        ["AddRecipe_SelectImage"] = "Seleccionar Imagen Final",
        ["AddRecipe_Ingredients"] = "Ingredientes de la Receta",
        ["AddRecipe_SelectFood"] = "Seleccionar Alimento del Catalogo",
        ["AddRecipe_SelectFoodPlaceholder"] = "Toca para elegir ingrediente...",
        ["AddRecipe_GramsRequired"] = "Gramos requeridos",
        ["AddRecipe_AddIngredient"] = "+ Agregar Ingrediente",
        ["AddRecipe_IngredientsListTitle"] = "Ingredientes incorporados:",
        ["AddRecipe_EmptyIngredients"] = "Aun no has agregado ingredientes.",
        ["AddRecipe_Remove"] = "Quitar",
        ["AddRecipe_PreparationSteps"] = "Pasos Numerados de Elaboracion",
        ["AddRecipe_StepInstruction"] = "Instruccion del Paso",
        ["AddRecipe_StepInstructionPlaceholder"] = "Describe detalladamente que hacer en este paso...",
        ["AddRecipe_StepImage"] = "Imagen del Paso (Opcional)",
        ["AddRecipe_SelectStepImage"] = "Seleccionar Imagen del Paso",
        ["AddRecipe_AddStep"] = "+ Agregar Paso a la Receta",
        ["AddRecipe_StepsListTitle"] = "Pasos de la receta:",
        ["AddRecipe_EmptySteps"] = "Aun no has agregado pasos a la receta.",
        ["AddRecipe_SaveRecipe"] = "Guardar Receta Completa",

        // Pantalla Catalogo de Alimentos
        ["FoodCatalog_Title"] = "Catalogo de Alimentos",
        ["FoodCatalog_FilterTitle"] = "Filtros por Minerales (mg por 100g)",
        ["FoodCatalog_SearchName"] = "Buscar por nombre...",
        ["FoodCatalog_ApplyFilters"] = "Aplicar Filtros",
        ["FoodCatalog_ResetFilters"] = "Limpiar Filtros",
        ["FoodCatalog_Empty"] = "No se encontraron alimentos con los criterios especificados.",
        ["FoodCatalog_Per100g"] = "Valores por 100g base:",

        // Pantalla Registrar Comida
        ["AddMeal_Title"] = "Registrar Nueva Comida",
        ["AddMeal_Date"] = "Fecha",
        ["AddMeal_Moment"] = "Momento",
        ["AddMeal_Note"] = "Nota u Observacion (opcional)",
        ["AddMeal_NotePlaceholder"] = "Ej: Almuerzo liviano post entrenamiento",
        ["AddMeal_AddFoodTitle"] = "Agregar Alimento del Catalogo",
        ["AddMeal_SelectFood"] = "Seleccionar Alimento del Catalogo",
        ["AddMeal_GramsConsumed"] = "Gramos consumidos",
        ["AddMeal_AddFoodButton"] = "+ Agregar Alimento",
        ["AddMeal_AddRecipeTitle"] = "O Agregar Receta Culinaria",
        ["AddMeal_SelectRecipe"] = "Seleccionar Receta",
        ["AddMeal_ServingsConsumed"] = "Porciones consumidas",
        ["AddMeal_AddRecipeButton"] = "+ Agregar Receta",
        ["AddMeal_ItemsInMeal"] = "Items en esta Comida:",
        ["AddMeal_EmptyItems"] = "Aun no has agregado alimentos ni recetas a esta comida.",
        ["AddMeal_SaveButton"] = "Guardar Comida y Calcular Minerales",

        // Pantalla Ajustes
        ["Settings_Title"] = "Ajustes y Configuracion",
        ["Settings_LanguageTitle"] = "Idioma de la Aplicacion",
        ["Settings_LanguageSubtitle"] = "Selecciona tu idioma preferido para la interfaz, nombres de minerales y momentos de comida:",
        ["Settings_Spanish"] = "Espanol",
        ["Settings_English"] = "Ingles (English)",
        ["Settings_CurrentLanguageNotice"] = "El idioma se guarda y aplica inmediatamente en toda la aplicacion.",
        ["Settings_AboutTitle"] = "Acerca de DietApp",
        ["Settings_AboutDescription"] = "Aplicacion especializada en control de minerales y recetas con persistencia SQLite y .NET MAUI."
    };

    private static readonly Dictionary<string, string> EnglishStrings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Navigation and Tabs
        ["Tab_DailyTracking"] = "Daily Tracking",
        ["Tab_Recipes"] = "Recipes",
        ["Tab_Catalog"] = "Catalog & Filter",
        ["Tab_AddMeal"] = "Log Meal",
        ["Tab_AddFood"] = "New Food",
        ["Tab_Settings"] = "Settings",

        // Daily Tracking Screen
        ["DailyTracking_Title"] = "Daily Intake and Mineral Tracking",
        ["DailyTracking_PreviousDay"] = "< Previous Day",
        ["DailyTracking_NextDay"] = "Next Day >",
        ["DailyTracking_DailyTotal"] = "Daily Total Consumed",
        ["DailyTracking_DailyTotalSubtitle"] = "Consolidated sum of all mineral compounds ingested today:",
        ["DailyTracking_MealsDetail"] = "Daily Meals Detail",
        ["DailyTracking_EmptyMeals"] = "No meals recorded for this date.",
        ["DailyTracking_EmptyAction"] = "Log a meal now",
        ["DailyTracking_Food"] = "Food",
        ["DailyTracking_Portion"] = "Portion",
        ["DailyTracking_Delete"] = "Delete",

        // Recipes Screen
        ["Recipes_Title"] = "Nutritional Recipe Book",
        ["Recipes_NewRecipe"] = "+ New Recipe",
        ["Recipes_SearchPlaceholder"] = "Search recipes by title or description...",
        ["Recipes_Empty"] = "No recipes recorded yet.",
        ["Recipes_EmptyAction"] = "Create the first recipe",
        ["Recipes_ViewSteps"] = "View Steps & Ingredients",

        // Recipe Detail Screen
        ["RecipeDetail_Title"] = "Recipe Details",
        ["RecipeDetail_NutritionPerServing"] = "Nutritional Intake per Serving:",
        ["RecipeDetail_MineralsPerServingTitle"] = "Minerals Provided per Serving",
        ["RecipeDetail_MineralsPerServingSubtitle"] = "Mineral balance automatically calculated from recipe ingredients:",
        ["RecipeDetail_IntakePanelTitle"] = "Record in Daily Intake",
        ["RecipeDetail_IntakePanelSubtitle"] = "Add this recipe to your meal log to count its minerals and calories:",
        ["RecipeDetail_IntakeDate"] = "Intake Date",
        ["RecipeDetail_MealMoment"] = "Meal Moment",
        ["RecipeDetail_ServingsConsumed"] = "Servings Consumed",
        ["RecipeDetail_LogAction"] = "Record in my Meal",
        ["RecipeDetail_IngredientsTitle"] = "Recipe Ingredients",
        ["RecipeDetail_StepsTitle"] = "Numbered Preparation Steps",

        // Individual minerals
        ["Mineral_Phosphorus"] = "Phosphorus (mg)",
        ["Mineral_Potassium"] = "Potassium (mg)",
        ["Mineral_Sodium"] = "Sodium (mg)",
        ["Mineral_Calcium"] = "Calcium (mg)",
        ["Mineral_Magnesium"] = "Magnesium (mg)",
        ["Mineral_Iron"] = "Iron (mg)",
        ["Mineral_Zinc"] = "Zinc (mg)",

        // Add Food Screen
        ["AddFood_Title"] = "Add New Food",
        ["AddFood_Header"] = "New Food with Mineral Profile",
        ["AddFood_Subtitle"] = "Enter food details and mineral quantities present in the reference portion (e.g. 100g):",
        ["AddFood_Name"] = "Food Name",
        ["AddFood_NamePlaceholder"] = "e.g. Avocado, Rolled oats",
        ["AddFood_Category"] = "Category",
        ["AddFood_CategoryPlaceholder"] = "e.g. Fruits, Grains, Fish",
        ["AddFood_PortionGrams"] = "Base Portion (Grams)",
        ["AddFood_MineralsTitle"] = "Mineral Content in Base Portion (mg)",
        ["AddFood_Calories"] = "Calories (kcal per 100g)",
        ["AddFood_SaveButton"] = "Save Food to Catalog",

        // Add Recipe Screen
        ["AddRecipe_Title"] = "Create New Recipe",
        ["AddRecipe_Header"] = "Create Recipe with Nutritional Profile",
        ["AddRecipe_GeneralData"] = "General Information",
        ["AddRecipe_RecipeTitle"] = "Recipe Title",
        ["AddRecipe_RecipeTitlePlaceholder"] = "e.g. Light chicken and vegetable stew",
        ["AddRecipe_Servings"] = "Number of Servings",
        ["AddRecipe_Description"] = "Description",
        ["AddRecipe_DescriptionPlaceholder"] = "Brief description or dish characteristics...",
        ["AddRecipe_FinalImage"] = "Final Image of Finished Recipe",
        ["AddRecipe_FinalImageSubtitle"] = "Select an image from your device or enter the file path/name:",
        ["AddRecipe_ImagePathPlaceholder"] = "File path or name...",
        ["AddRecipe_BrowseButton"] = "Browse...",
        ["AddRecipe_SelectImage"] = "Select Final Image",
        ["AddRecipe_Ingredients"] = "Recipe Ingredients",
        ["AddRecipe_SelectFood"] = "Select Food from Catalog",
        ["AddRecipe_SelectFoodPlaceholder"] = "Tap to choose ingredient...",
        ["AddRecipe_GramsRequired"] = "Grams required",
        ["AddRecipe_AddIngredient"] = "+ Add Ingredient",
        ["AddRecipe_IngredientsListTitle"] = "Added ingredients:",
        ["AddRecipe_EmptyIngredients"] = "No ingredients added yet.",
        ["AddRecipe_Remove"] = "Remove",
        ["AddRecipe_PreparationSteps"] = "Numbered Preparation Steps",
        ["AddRecipe_StepInstruction"] = "Step Instruction",
        ["AddRecipe_StepInstructionPlaceholder"] = "Describe in detail what to do in this step...",
        ["AddRecipe_StepImage"] = "Step Image (Optional)",
        ["AddRecipe_SelectStepImage"] = "Select Step Image",
        ["AddRecipe_AddStep"] = "+ Add Step to Recipe",
        ["AddRecipe_StepsListTitle"] = "Recipe steps:",
        ["AddRecipe_EmptySteps"] = "No steps added to the recipe yet.",
        ["AddRecipe_SaveRecipe"] = "Save Complete Recipe",

        // Food Catalog Screen
        ["FoodCatalog_Title"] = "Food Catalog",
        ["FoodCatalog_FilterTitle"] = "Mineral Filters (mg per 100g)",
        ["FoodCatalog_SearchName"] = "Search by name...",
        ["FoodCatalog_ApplyFilters"] = "Apply Filters",
        ["FoodCatalog_ResetFilters"] = "Reset Filters",
        ["FoodCatalog_Empty"] = "No foods found matching the specified criteria.",
        ["FoodCatalog_Per100g"] = "Values per 100g base:",

        // Add Meal Screen
        ["AddMeal_Title"] = "Record New Meal",
        ["AddMeal_Date"] = "Date",
        ["AddMeal_Moment"] = "Meal Moment",
        ["AddMeal_Note"] = "Note or Observation (optional)",
        ["AddMeal_NotePlaceholder"] = "e.g. Light post-workout lunch",
        ["AddMeal_AddFoodTitle"] = "Add Food from Catalog",
        ["AddMeal_SelectFood"] = "Select Food from Catalog",
        ["AddMeal_GramsConsumed"] = "Grams consumed",
        ["AddMeal_AddFoodButton"] = "+ Add Food",
        ["AddMeal_AddRecipeTitle"] = "Or Add Culinary Recipe",
        ["AddMeal_SelectRecipe"] = "Select Recipe",
        ["AddMeal_ServingsConsumed"] = "Servings consumed",
        ["AddMeal_AddRecipeButton"] = "+ Add Recipe",
        ["AddMeal_ItemsInMeal"] = "Items in this Meal:",
        ["AddMeal_EmptyItems"] = "No foods or recipes added to this meal yet.",
        ["AddMeal_SaveButton"] = "Save Meal & Calculate Minerals",

        // Settings Screen
        ["Settings_Title"] = "Settings & Configuration",
        ["Settings_LanguageTitle"] = "Application Language",
        ["Settings_LanguageSubtitle"] = "Select your preferred language for interface, mineral names, and meal moments:",
        ["Settings_Spanish"] = "Spanish",
        ["Settings_English"] = "English",
        ["Settings_CurrentLanguageNotice"] = "The selected language is saved and applied immediately across the entire application.",
        ["Settings_AboutTitle"] = "About DietApp",
        ["Settings_AboutDescription"] = "Specialized application for mineral tracking and recipes with SQLite and .NET MAUI persistence."
    };
}
