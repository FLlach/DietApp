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
        ["Tab_Seasonings"] = "Alinos",
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
        ["DailyTracking_FoodsIngested"] = "Alimentos ingeridos:",
        ["DailyTracking_MineralsProvided"] = "Aporte de minerales en esta comida:",

        // Pantalla Recetas
        ["Recipes_Title"] = "Recetario Nutricional",
        ["Recipes_NewRecipe"] = "+ Nueva Receta",
        ["Recipes_SearchPlaceholder"] = "Buscar recetas por titulo o descripcion...",
        ["Recipes_Empty"] = "No hay recetas registradas aun.",
        ["Recipes_EmptyAction"] = "Crear la primera receta",
        ["Recipes_ViewSteps"] = "Ver Pasos e Ingredientes",
        ["Recipes_SortTitle"] = "Ordenar recetas por aporte mineral:",

        // Ordenamiento por Minerales
        ["Sort_ByMineral"] = "Ordenar por mineral:",
        ["Sort_Direction"] = "Direccion:",
        ["Sort_Default"] = "Por defecto (original)",
        ["Sort_Descending"] = "Mayor a menor",
        ["Sort_Ascending"] = "Menor a mayor",
        ["RecipeDetail_SortIngredients"] = "Ordenar ingredientes por mineral:",

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
        ["RecipeDetail_PortionWarningTitle"] = "Advertencia de Limite de Minerales",
        ["RecipeDetail_PortionWarningSubtitle"] = "Atencion: al consumir esta receta se superara o estara cerca de superar el limite de los siguientes minerales:",

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
        ["AddRecipe_AddSeasoningTitle"] = "O Incorporar Alino Preconfigurado",
        ["AddRecipe_SelectSeasoning"] = "Seleccionar Alino o Condimento",
        ["AddRecipe_SelectSeasoningPlaceholder"] = "Elige un alino guardado...",
        ["AddRecipe_ApplySeasoning"] = "+ Incorporar Alino a la Receta",
        ["AddRecipe_SeasoningAppliedNotice"] = "Se incorporaron los ingredientes del alino a la receta.",

        // Pantalla Alinos y Condimentos
        ["Seasonings_Title"] = "Alinos y Condimentos",
        ["Seasonings_Subtitle"] = "Mezclas de condimentos e ingredientes listas para incorporar directamente a tus recetas:",
        ["Seasonings_NewSeasoning"] = "+ Nuevo Alino",
        ["Seasonings_CreateHeader"] = "Crear Nuevo Alino o Condimento",
        ["Seasonings_Name"] = "Nombre del Alino o Condimento",
        ["Seasonings_NamePlaceholder"] = "Ej: Vinagreta de limon, Adobo para carnes...",
        ["Seasonings_Description"] = "Descripcion o notas de uso",
        ["Seasonings_DescriptionPlaceholder"] = "Breve explicacion del tipo de plato o preparacion...",
        ["Seasonings_IngredientsTitle"] = "Ingredientes del Alino",
        ["Seasonings_SelectFood"] = "Seleccionar Alimento o Condimento del Catalogo",
        ["Seasonings_SelectFoodPlaceholder"] = "Toca para elegir...",
        ["Seasonings_Grams"] = "Gramos",
        ["Seasonings_AddIngredient"] = "+ Agregar Ingrediente al Alino",
        ["Seasonings_SaveSeasoning"] = "Guardar Alino",
        ["Seasonings_Cancel"] = "Cancelar",
        ["Seasonings_Empty"] = "No hay alinos ni condimentos registrados aun.",
        ["Seasonings_Delete"] = "Eliminar",
        ["Seasonings_IngredientsIncluded"] = "Ingredientes incluidos:",
        ["Seasonings_MineralBalance"] = "Aporte total de minerales:",
        ["Seasonings_SavedSuccess"] = "Alino guardado exitosamente.",
        ["Seasonings_DeletedSuccess"] = "Alino eliminado.",
        ["Seasonings_ValidationRequired"] = "Completa el nombre y agrega al menos un ingrediente al alino.",

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
        ["Settings_MineralAlertsTitle"] = "Alertas de Limite Maximo de Minerales",
        ["Settings_MineralAlertsSubtitle"] = "Fija la cantidad maxima diaria (en mg) para recibir advertencias al registrar o revisar tu consumo:",
        ["Settings_MineralAlertsSave"] = "Guardar Limites de Alerta",
        ["Settings_MineralAlertsReset"] = "Restablecer Todos los Limites",
        ["Settings_MineralAlertsSavedNotice"] = "Limites de alerta actualizados y activados correctamente.",
        ["Settings_MineralAlertsClearedNotice"] = "Se han desactivado todos los limites de alerta.",
        ["Settings_WarningThresholdTitle"] = "Umbral Porcentual de Aviso Preventivo",
        ["Settings_WarningThresholdSubtitle"] = "Define el porcentaje del limite diario (50% a 95%) para emitir una advertencia antes de superarlo:",
        ["DailyTracking_AlertsTitle"] = "Alertas y Advertencias de Minerales",
        ["DailyTracking_AlertsSubtitle"] = "Atencion: algunos minerales han superado su limite diario o estan proximos a alcanzarlo:",
        ["Alert_Severity_Exceeded"] = "LIMITE SUPERADO",
        ["Alert_Severity_NearLimit"] = "AVISO PREVENTIVO",
        ["Settings_AboutTitle"] = "Acerca de DietApp",
        ["Settings_AboutDescription"] = "Aplicacion especializada en control de minerales y recetas con persistencia SQLite y .NET MAUI.",
        ["Common_Select"] = "Seleccionar",
        ["Common_Remove"] = "Quitar",
        ["FoodCatalog_Mineral"] = "Mineral",
        ["FoodCatalog_MinMg"] = "Min (mg)",
        ["FoodCatalog_MaxMg"] = "Max (mg)",
        ["FoodCatalog_NewFood"] = "+ Nuevo Alimento",
        ["DailyTracking_AddMeal"] = "+ Registrar Comida",
        ["AddMeal_ChooseFoodPlaceholder"] = "Toca para elegir alimento...",
        ["AddMeal_ChooseRecipePlaceholder"] = "Toca para elegir receta..."
    };

    private static readonly Dictionary<string, string> EnglishStrings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Navigation and Tabs
        ["Tab_DailyTracking"] = "Daily Tracking",
        ["Tab_Recipes"] = "Recipes",
        ["Tab_Seasonings"] = "Seasonings",
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
        ["DailyTracking_FoodsIngested"] = "Foods consumed:",
        ["DailyTracking_MineralsProvided"] = "Minerals provided in this meal:",

        // Recipes Screen
        ["Recipes_Title"] = "Nutritional Recipe Book",
        ["Recipes_NewRecipe"] = "+ New Recipe",
        ["Recipes_SearchPlaceholder"] = "Search recipes by title or description...",
        ["Recipes_Empty"] = "No recipes recorded yet.",
        ["Recipes_EmptyAction"] = "Create the first recipe",
        ["Recipes_ViewSteps"] = "View Steps & Ingredients",
        ["Recipes_SortTitle"] = "Sort recipes by mineral amount:",

        // Sorting by Minerals
        ["Sort_ByMineral"] = "Sort by mineral:",
        ["Sort_Direction"] = "Direction:",
        ["Sort_Default"] = "Default (original)",
        ["Sort_Descending"] = "Highest to lowest",
        ["Sort_Ascending"] = "Lowest to highest",
        ["RecipeDetail_SortIngredients"] = "Sort ingredients by mineral:",

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
        ["RecipeDetail_PortionWarningTitle"] = "Mineral Limit Warning",
        ["RecipeDetail_PortionWarningSubtitle"] = "Attention: consuming this recipe will exceed or approach your daily limit for the following minerals:",

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
        ["AddRecipe_AddSeasoningTitle"] = "Or Add Pre-configured Seasoning",
        ["AddRecipe_SelectSeasoning"] = "Select Seasoning or Dressing",
        ["AddRecipe_SelectSeasoningPlaceholder"] = "Choose a saved seasoning...",
        ["AddRecipe_ApplySeasoning"] = "+ Incorporate Seasoning into Recipe",
        ["AddRecipe_SeasoningAppliedNotice"] = "Seasoning ingredients were incorporated into the recipe.",

        // Seasonings & Condiments Screen
        ["Seasonings_Title"] = "Dressings & Seasonings",
        ["Seasonings_Subtitle"] = "Pre-configured seasoning and dressing blends ready to incorporate directly into your recipes:",
        ["Seasonings_NewSeasoning"] = "+ New Seasoning",
        ["Seasonings_CreateHeader"] = "Create New Dressing or Seasoning Blend",
        ["Seasonings_Name"] = "Seasoning / Dressing Name",
        ["Seasonings_NamePlaceholder"] = "e.g. Lemon vinaigrette, Steak herb rub...",
        ["Seasonings_Description"] = "Description or usage notes",
        ["Seasonings_DescriptionPlaceholder"] = "Brief description or recommended dishes...",
        ["Seasonings_IngredientsTitle"] = "Seasoning Ingredients",
        ["Seasonings_SelectFood"] = "Select Food or Condiment from Catalog",
        ["Seasonings_SelectFoodPlaceholder"] = "Tap to choose...",
        ["Seasonings_Grams"] = "Grams",
        ["Seasonings_AddIngredient"] = "+ Add Ingredient to Seasoning",
        ["Seasonings_SaveSeasoning"] = "Save Seasoning",
        ["Seasonings_Cancel"] = "Cancel",
        ["Seasonings_Empty"] = "No dressings or seasonings recorded yet.",
        ["Seasonings_Delete"] = "Delete",
        ["Seasonings_IngredientsIncluded"] = "Included ingredients:",
        ["Seasonings_MineralBalance"] = "Total minerals provided:",
        ["Seasonings_SavedSuccess"] = "Seasoning saved successfully.",
        ["Seasonings_DeletedSuccess"] = "Seasoning deleted.",
        ["Seasonings_ValidationRequired"] = "Please provide a name and add at least one ingredient to the seasoning.",

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
        ["Settings_MineralAlertsTitle"] = "Maximum Mineral Limit Alerts",
        ["Settings_MineralAlertsSubtitle"] = "Set the maximum daily amount (in mg) to receive warnings when logging or reviewing your intake:",
        ["Settings_MineralAlertsSave"] = "Save Alert Limits",
        ["Settings_MineralAlertsReset"] = "Reset All Limits",
        ["Settings_MineralAlertsSavedNotice"] = "Alert limits updated and activated successfully.",
        ["Settings_MineralAlertsClearedNotice"] = "All alert limits have been deactivated.",
        ["Settings_WarningThresholdTitle"] = "Early Warning Percentage Threshold",
        ["Settings_WarningThresholdSubtitle"] = "Set the percentage of daily limit (50% to 95%) to trigger a warning before exceeding it:",
        ["DailyTracking_AlertsTitle"] = "Mineral Alerts and Warnings",
        ["DailyTracking_AlertsSubtitle"] = "Attention: some minerals have exceeded their daily limit or are approaching it:",
        ["Alert_Severity_Exceeded"] = "LIMIT EXCEEDED",
        ["Alert_Severity_NearLimit"] = "EARLY WARNING",
        ["Settings_AboutTitle"] = "About DietApp",
        ["Settings_AboutDescription"] = "Specialized application for mineral tracking and recipes with SQLite and .NET MAUI persistence.",
        ["Common_Select"] = "Select",
        ["Common_Remove"] = "Remove",
        ["FoodCatalog_Mineral"] = "Mineral",
        ["FoodCatalog_MinMg"] = "Min (mg)",
        ["FoodCatalog_MaxMg"] = "Max (mg)",
        ["FoodCatalog_NewFood"] = "+ New Food",
        ["DailyTracking_AddMeal"] = "+ Log Meal",
        ["AddMeal_ChooseFoodPlaceholder"] = "Tap to choose food...",
        ["AddMeal_ChooseRecipePlaceholder"] = "Tap to choose recipe..."
    };
}
