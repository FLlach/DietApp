# Documentacion Tecnica - DietApp

## 1. Descripcion General
DietApp es una aplicacion movil y de escritorio construida con **C# y .NET MAUI** orientada al registro, control y seguimiento de la ingesta de alimentos con un enfoque especializado en el contenido de **minerales** (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc) y valor calorico (kcal).

Permite:
* Filtrar alimentos de forma avanzada por umbrales maximos y minimos de minerales (de especial utilidad en dietas renales, cardiovasculares o deportivas).
* Calcular el aporte acumulado de compuestos por cada comida y a nivel diario.
* **Modulo de Recetas Nutricionales**: Crear y consultar preparaciones culinarias con titulo, imagen final de la receta terminada, subtitulo automatico de minerales y calorias por porcion calculado con base en los ingredientes, e instrucciones en pasos numerados con opcion de adjuntar imagenes en cada etapa.
* **Registro de Recetas en la Ingesta Diaria**: Capacidad de registrar porciones de recetas directamente en el conteo diario de comidas (tanto desde la pantalla de detalle de la receta como desde el compositor de comidas), escalando proporcionalmente el aporte de minerales y calorias consumidos.
* **Persistencia Relacional en SQLite**: Almacenamiento local de alto rendimiento con indices B-Tree en columnas de minerales y precarga de 363 alimentos de la base oficial USDA FoodData Central Foundation Foods normalizados a gramos.

---

## 2. Arquitectura del Proyecto (Domain-Driven Design)
La solucion esta estructurada en cuatro capas desacopladas con dependencias unidireccionales:

```
DietApp.slnx
├── src/
│   ├── DietApp.Domain/          # Nucleo puro de negocio (sin dependencias externas)
│   ├── DietApp.Application/     # Casos de uso, DTOs, mapeos e interfaces de servicio
│   ├── DietApp.Infrastructure/  # Persistencia SQLite, modelos relacionales y FoodData Central
│   └── DietApp.UI/              # Vistas XAML, ViewModels (MVVM) y configuracion MAUI
```

### Reglas de Diseno Aplicadas:
* **Bajo acoplamiento y alta cohesion**: La capa de dominio no depende de bases de datos ni de frameworks visuales.
* **Inversion de dependencias**: La aplicacion y la UI se comunican mediante abstracciones e interfaces (`IFoodRepository`, `IMealRepository`, `IRecipeRepository`, `IFoodCatalogService`, `IMealTrackingService`, `IRecipeService`).
* **Sin cadenas magicas ni valores arbitrarios**: Se utilizan enumeraciones fuertemente tipadas y objetos de valor inmutables.
* **Sin emojis**: Todo el codigo, comentarios y documentacion mantienen estilo profesional formal.

---

## 3. Descripcion de las Capas y Componentes

### 3.1. Capa de Dominio (`DietApp.Domain`)
Contiene las entidades, enumeraciones, objetos de valor y servicios del dominio:

* **`Enums/MineralType.cs`**: Catalogo fuertemente tipado de minerales cuantificables (`Phosphorus`, `Potassium`, `Sodium`, `Calcium`, `Magnesium`, `Iron`, `Zinc`).
* **`Enums/MealType.cs`**: Momentos de ingesta (`Breakfast`, `Lunch`, `Dinner`, `Snack`, `Other`).
* **`ValueObjects/MineralAmount.cs`**: Objeto de valor inmutable que encapsula el tipo de mineral y su cantidad en miligramos (mg). Cuenta con atributos de serializacion JSON (`[JsonConstructor]`, `[JsonPropertyName]`, accesores `init`) para asegurar reconstruccion fiel desde campos JSON en SQLite.
* **`Entities/FoodItem.cs`**: Entidad que representa un alimento en el catalogo con su perfil de minerales y calorias por porcion base normalizada de 100 gramos.
* **`Entities/MealItem.cs`**: Representa la ingesta de un item en una comida con su instantanea calculada de minerales. Incluye metodos de fabricacion `FromFoodItem(food, grams)` y `FromRecipe(recipe, servingsConsumed)` que escalan el aporte nutricional de forma exacta.
* **`Entities/Meal.cs`**: Raiz de agregado (Aggregate Root) que agrupa los alimentos consumidos y calcula la sumatoria consolidada de minerales con `CalculateTotalMinerals()`.
* **`Entities/Recipe.cs`**: Raiz de agregado para recetas culinarias con ingredientes dosificados, pasos numerados, porciones, imagen final y calculo nutricional por porcion.
* **`Entities/RecipeIngredient.cs`**: Ingrediente dosificado con calculo de calorias y minerales.
* **`Entities/RecipeStep.cs`**: Paso numerado con instruccion e imagen ilustrativa de la etapa.
* **`Services/DailyMineralAggregatorService.cs`**: Servicio de dominio que consolida la suma total de minerales entre multiples comidas del dia.
* **`Repositories/`**: Contratos `IFoodRepository.cs`, `IMealRepository.cs` e `IRecipeRepository.cs`.

---

### 3.2. Capa de Aplicacion (`DietApp.Application`)
Orquesta los casos de uso del sistema:

* **`DTOs/`**: `FoodItemDto.cs`, `MealDto.cs`, `MealItemDto.cs`, `MineralAmountDto.cs`, `MineralFilterCriteriaDto.cs`, `RecipeDto.cs` (con subtitulo nutricional por porcion y coleccion de minerales por porcion), `RecipeIngredientDto.cs` (con resumen de minerales aportados por ingrediente), `RecipeStepDto.cs`.
* **`Mapping/DomainDtoMapper.cs`**: Funciones puras de extension para transformar entidades a DTOs con traduccion de nombres a espanol.
* **`Services/FoodCatalogService.cs`**: Casos de uso de consulta, busqueda y filtrado por rangos de minerales.
* **`Services/MealTrackingService.cs`**: Casos de uso de registro de comidas y balance diario de minerales. Soporta registro combinado de alimentos y recetas (`RecordMealWithMixedItemsAsync`) y registro rapido de recetas consumidas (`RecordRecipeInMealAsync`).
* **`Services/RecipeService.cs`**: Casos de uso de creacion, consulta y busqueda de recetas culinarias.

---

### 3.3. Capa de Infraestructura (`DietApp.Infrastructure`)
Implementa el acceso a datos mediante **SQLite**:

* **`Data/DietAppDbContext.cs`**: Administrador de la conexion SQLite asincrona (`SQLiteAsyncConnection`), responsable de la creacion de tablas relacionales indexadas y de la inicializacion automatica de datos.
* **`Data/FoodDataCentralImporter.cs`**: Lector de alto rendimiento basado en `System.Text.Json.JsonDocument` que procesa el dataset oficial de USDA FoodData Central Foundation Foods (`FoodData_Central_foundation_food_json_2026-04-30.json`).
  * Normaliza la base de nutrientes y minerales a 100 gramos de referencia.
  * Extrae las porciones caseras (tazas, cucharadas, rebanadas) y las normaliza a su peso exacto en gramos (`gramWeight`).
  * Inserta 363 alimentos y 383 porciones en una sola transaccion atomica.
* **`Data/Models/`**:
  * `FoodEntity.cs`: Tabla relacional con indices en `PhosphorusMg`, `PotassiumMg` y `SodiumMg`.
  * `FoodPortionEntity.cs`: Tabla de porciones caseras normalizadas a gramos.
  * `MealEntity.cs` y `MealItemEntity.cs`: Tablas de comidas e items con instantanea JSON.
  * `RecipeEntity.cs`, `RecipeIngredientEntity.cs` y `RecipeStepEntity.cs`: Tablas relacionales para recetas.
* **`Repositories/`**:
  * `SqliteFoodRepository.cs`: Consultas SQL optimizadas por indices B-Tree.
  * `SqliteMealRepository.cs`: Transacciones de comidas e items consumidos.
  * `SqliteRecipeRepository.cs`: Persistencia relacional de recetas, ingredientes y pasos.

---

### 3.4. Capa de Presentacion (`DietApp.UI`)
Construida con .NET MAUI y **CommunityToolkit.Mvvm**:

* **Navegacion (`AppShell.xaml`)**:
  * Pestanas en `TabBar`:
    1. **Conteo Diario** (`MealTrackingPage`): Totales diarios de minerales, banner reactivo de advertencias si se superan los limites maximos fijados por el usuario y detalle por comida formateado por tipo y fecha limpia.
    2. **Recetas** (`RecipesPage`): Catalogo de recetas con buscador de texto, selector interactivo para ordenar por cantidad de cualquier mineral por porcion (ascendente o descendente), tarjeta con imagen final, subtitulo y badges visuales con el aporte de minerales por porcion.
    3. **Catalogo y Filtro** (`FoodCatalogPage`): Filtrado avanzado por umbrales minimos y maximos de minerales.
    4. **Registrar Comida** (`AddMealPage`): Composicion de comidas con soporte mixto de alimentos (en gramos) y recetas culinarias (en porciones).
    5. **Nuevo Alimento** (`AddFoodPage`): Formulario para ingresar alimentos adicionales al catalogo SQLite.
    6. **Ajustes** (`SettingsPage`): Selector de idioma (Espanol / Ingles) y configuracion personalizada de limites maximos diarios de minerales con activacion de alertas.
  * Rutas registradas:
    * `RecipeDetailPage`: Detalle de receta con imagen final, panel completo de minerales por porcion, selector para ordenar ingredientes segun el mineral aportado, pasos numerados con imagenes y modulo interactivo para registrar el consumo en la ingesta diaria.
    * `AddRecipePage`: Formulario para crear recetas con selector de imagenes por paso y final.
* **Inyeccion de Dependencias (`MauiProgram.cs`)**:
  * Registra `DietAppDbContext`, conecta los repositorios SQLite y registra los servicios de localizacion (`ILanguagePreferenceStorage`, `ILocalizationService`) y alertas de minerales (`IMineralAlertStorage`, `IMineralAlertService`) en el contenedor IoC.

---

## 4. Sistema de Internacionalizacion y Cambio de Idioma

La aplicacion soporta alternancia reactiva y dinamica entre **Espanol** e **Ingles**:

### 4.1. Arquitectura de Localizacion
* **Abstraccion de Preferencias (`ILanguagePreferenceStorage`)**: Ubicada en `DietApp.Application.Services` para mantener la capa de aplicacion libre de dependencias de plataforma (respetando DDD).
* **Implementacion en UI (`MauiPreferencesLanguageStorage`)**: Ubicada en `DietApp.UI.Services`, utiliza `Microsoft.Maui.Storage.Preferences` para persistir la seleccion del usuario entre inicios de sesion.
* **Servicio de Localizacion (`ILocalizationService` / `LocalizationService`)**:
  * Gestiona diccionarios completos de terminos para Espanol e Ingles.
  * Modifica `CultureInfo.CurrentCulture`, `CultureInfo.CurrentUICulture`, `CultureInfo.DefaultThreadCurrentCulture` y `CultureInfo.DefaultThreadCurrentUICulture`.
  * Traduce de forma reactiva los nombres de minerales (`MineralType`) y momentos de comida (`MealType`).
  * Emite el evento `LanguageChanged` y `PropertyChanged` con nombre de propiedad nulo para forzar la reevaluacion de todos los bindings compilados.
* **Puente XAML (`LocalizationResourceManager`)**: Singleton que expone el indexador `this[string key]` y suscribe al servicio de localizacion para notificar a la infraestructura de MAUI.
* **Markup Extension (`TranslateExtension`)**: Permite en XAML escribir `{loc:Translate KeyName}` con enlace reactivo a `LocalizationResourceManager.Instance`.

### 4.2. Vistas Localizadas
Todas las vistas de la aplicacion implementan traduccion instantanea:
1. `AppShell`: Pestanas de navegacion traducidas al vuelo.
2. `MealTrackingPage`: Titulos, subtitulos, botones de navegacion de fechas y estados vacios.
3. `RecipesPage`: Buscador, controles de ordenamiento, botones y textos descriptivos.
4. `RecipeDetailPage`: Titulos, desglose de aportes, controles de ordenamiento de ingredientes, formulario de ingesta y pasos.
5. `FoodCatalogPage`: Filtros de minerales y tablas de resultados.
6. `AddMealPage`: Selectores y listas de alimentos y recetas.
7. `AddFoodPage`: Formulario de alimentos y nombres de minerales.
8. `AddRecipePage`: Formularios, listas de pasos e ingredientes.
9. `SettingsPage`: Tarjetas para alternar entre Espanol e Ingles y configurar limites maximos de minerales.

---

## 5. Ordenamiento Nutricional por Minerales

La aplicacion permite al usuario ordenar tanto el recetario como los ingredientes de cada plato segun cualquier mineral cuantificable:

### 5.1. Ordenamiento de Recetas (`RecipesPage` / `RecipesViewModel`)
* Permite seleccionar cualquier mineral soportado (`Phosphorus`, `Potassium`, `Sodium`, `Calcium`, `Magnesium`, `Iron`, `Zinc`) o volver al orden por defecto.
* Soporta alternancia de direccion: **Mayor a menor** (descendente) o **Menor a mayor** (ascendente).
* El ordenamiento evalua la cantidad en miligramos aportada por porcion (`MineralsPerServing`), combinandose de forma reactiva con el filtro de busqueda por texto.

### 5.2. Ordenamiento de Ingredientes (`RecipeDetailPage` / `RecipeDetailViewModel`)
* Permite analizar que ingredientes del plato aportan mayor o menor cantidad de un compuesto determinado.
* El usuario selecciona el mineral de interes y el sentido de ordenamiento, actualizando la lista de ingredientes en tiempo real (`DisplayedIngredients`).
* Cada ingrediente conserva su desglose completo de gramos, calorias y balance de minerales.

---

## 6. Sistema de Alertas y Limites Maximos de Minerales

La aplicacion permite al usuario definir limites maximos diarios para controlar la ingesta de compuestos especificos (e.g. restringir sodio en dietas hipertensas o controlar fosforo y potasio en pacientes renales):

### 6.1. Configuracion en Ajustes (`SettingsPage` / `SettingsViewModel`)
* Lista interactiva de los 7 minerales cuantificables (`MineralAlertConfigModel`).
* Cada mineral cuenta con un campo numerico para ingresar el limite maximo en miligramos (`ThresholdText`) y un conmutador (`IsEnabled`) para activar o desactivar la alerta de forma independiente.
* Botones para guardar los limites de forma persistente o restablecerlos a cero.

### 6.2. Persistencia y Evaluacion en Capa de Aplicacion
* **`IMineralAlertStorage`**: Contrato desacoplado en `DietApp.Application.Services` para guardar y recuperar la coleccion de umbrales.
* **`MauiPreferencesMineralAlertStorage`**: Implementacion nativa en `DietApp.UI.Services` que serializa los umbrales en JSON mediante `Microsoft.Maui.Storage.Preferences`.
* **`IMineralAlertService` / `MineralAlertService`**:
  * Centraliza la logica para comparar los totales consumidos en el dia (`DailyMinerals`) contra los limites activos.
  * Genera objetos `MineralAlertExceededDto` con la cantidad actual, el maximo permitido, el exceso y el mensaje preventivo localizado.

### 6.3. Disparo Visual de Alertas (`MealTrackingPage` / `MealTrackingViewModel`)
* Al consultar cualquier fecha en el seguimiento diario, se evaluan los totales acumulados.
* Si uno o varios minerales superan el umbral maximo configurado por el usuario, se despliega un banner de advertencia destacado en color rojo/coral con el desglose exacto del exceso por mineral.

---

## 7. Instrucciones de Compilacion y Ejecucion

### Ejecucion en Windows (Modo Rapido):
Para compilar y ejecutar en Windows directamente desde la terminal:
```powershell
dotnet build -t:Run -f net10.0-windows10.0.19041.0 src/DietApp.UI/DietApp.UI.csproj
```

### Ejecucion en Android:
1. Abrir la solucion `DietApp.slnx` en Visual Studio.
2. Seleccionar como proyecto de inicio `DietApp.UI`.
3. Seleccionar el emulador de Android o un dispositivo fisico conectado en la barra superior.
4. Presionar `F5` para iniciar la depuracion.