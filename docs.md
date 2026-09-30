# Documentacion Tecnica - DietApp

## 1. Descripcion General
DietApp es una aplicacion movil y de escritorio construida con **C# y .NET MAUI** orientada al registro, control y seguimiento de la ingesta de alimentos con un enfoque especializado en el contenido de **minerales** (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc), **proteinas** (g) y valor calorico (kcal).

Permite:
* Filtrar alimentos de forma avanzada por umbrales maximos y minimos de minerales y proteinas (de especial utilidad en dietas renales, cardiovasculares o deportivas).
* Ordenar el catalogo de alimentos y las recetas por densidad proteica (mayor a menor o menor a mayor).
* Cuantificar la cantidad de proteina por alimento en porciones base de referencia y calcular su aporte escalado en recetas y comidas consumidas.
* Calcular el aporte acumulado de minerales, calorias y proteinas totales por cada comida y a nivel diario.
* **Modulo de Recetas Nutricionales**: Crear y consultar preparaciones culinarias con titulo, fotografia del plato final capturable directamente con la camara del dispositivo o seleccionable desde la galeria, subtitulo automatico de minerales, calorias y proteina por porcion calculado con base en los ingredientes, e instrucciones en pasos numerados con soporte de captura fotografica en tiempo real con camara o explorador de galeria en cada etapa de preparacion.
* **Registro de Recetas en la Ingesta Diaria**: Capacidad de registrar porciones de recetas directamente en el conteo diario de comidas (tanto desde la pantalla de detalle de la receta como desde el compositor de comidas), escalando proporcionalmente el aporte de minerales, proteinas y calorias consumidos.
* **Escaneo y Persistencia de Codigos de Barra**: Posibilidad de escanear mediante camara o ingresar manualmente el codigo de barras (EAN/UPC/QR) al momento de registrar un nuevo alimento personalizado en la base de datos local SQLite, con campo estrictamente opcional.
* **Persistencia Relacional en SQLite**: Almacenamiento local de alto rendimiento con indices B-Tree en columnas de minerales y proteinas, con precarga y backfill de nutrientes de la base oficial USDA FoodData Central Foundation Foods (incluyendo codigo de nutriente 203 de proteinas) normalizados a gramos.

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
* **`Entities/FoodItem.cs`**: Entidad que representa un alimento en el catalogo con su perfil de minerales, calorias por porcion base normalizada de 100 gramos, y codigo de barras opcional (`Barcode`) para identificacion rapida de productos comerciales.
* **`Entities/MealItem.cs`**: Representa la ingesta de un item en una comida con su instantanea calculada de minerales. Incluye metodos de fabricacion `FromFoodItem(food, grams)` y `FromRecipe(recipe, servingsConsumed)` que escalan el aporte nutricional de forma exacta.
* **`Entities/Meal.cs`**: Raiz de agregado (Aggregate Root) que agrupa los alimentos consumidos y calcula la sumatoria consolidada de minerales con `CalculateTotalMinerals()`.
* **`Entities/Recipe.cs`**: Raiz de agregado para recetas culinarias con ingredientes dosificados, pasos numerados, porciones, imagen final y calculo nutricional por porcion.
* **`Entities/RecipeIngredient.cs`**: Ingrediente dosificado con calculo de calorias y minerales.
* **`Entities/RecipeStep.cs`**: Paso numerado con instruccion e imagen ilustrativa de la etapa.
* **`Services/DailyMineralAggregatorService.cs`**: Servicio de dominio que consolida la suma total de minerales entre multiples comidas del dia.
* **`Repositories/`**: Contratos `IFoodRepository.cs` (con soporte para `GetByBarcodeAsync`), `IMealRepository.cs` e `IRecipeRepository.cs`.

---

### 3.2. Capa de Aplicacion (`DietApp.Application`)
Orquesta los casos de uso del sistema:

* **`DTOs/`**: `FoodItemDto.cs` (incluye propiedad opcional `Barcode`), `MealDto.cs`, `MealItemDto.cs`, `MineralAmountDto.cs`, `MineralFilterCriteriaDto.cs`, `RecipeDto.cs` (con subtitulo nutricional por porcion y coleccion de minerales por porcion), `RecipeIngredientDto.cs` (con resumen de minerales aportados por ingrediente), `RecipeStepDto.cs`.
* **`Mapping/DomainDtoMapper.cs`**: Funciones puras de extension para transformar entidades a DTOs con traduccion de nombres a espanol y preservacion del codigo de barras.
* **`Services/FoodCatalogService.cs`**: Casos de uso de consulta, busqueda y filtrado por rangos de minerales, persistencia de alimentos con codigo de barras y consulta especializada por `GetFoodByBarcodeAsync`.
* **`Services/MealTrackingService.cs`**: Casos de uso de registro de comidas y balance diario de minerales. Soporta registro combinado de alimentos y recetas (`RecordMealWithMixedItemsAsync`) y registro rapido de recetas consumidas (`RecordRecipeInMealAsync`).
* **`Services/RecipeService.cs`**: Casos de uso de creacion, consulta y busqueda de recetas culinarias.
* **`Services/IProteinGoalStorage.cs` y `Services/IProteinGoalService.cs` (`ProteinGoalService.cs`)**: Gestion, validacion y notificacion reactiva de la meta diaria de proteina en gramos para balance nitrogenado y control en enfermedad renal cronica.

---

### 3.3. Capa de Infraestructura (`DietApp.Infrastructure`)
Implementa el acceso a datos mediante **SQLite**:

* **`Data/DietAppDbContext.cs`**: Administrador de la conexion SQLite asincrona (`SQLiteAsyncConnection`), responsable de la creacion de tablas relacionales indexadas y de la inicializacion automatica de datos.
* **`Data/FoodDataCentralImporter.cs`**: Lector de alto rendimiento basado en `System.Text.Json.JsonDocument` que procesa el dataset oficial de USDA FoodData Central Foundation Foods (`FoodData_Central_foundation_food_json_2026-04-30.json`).
  * Normaliza la base de nutrientes y minerales a 100 gramos de referencia.
  * Extrae las porciones caseras (tazas, cucharadas, rebanadas) y las normaliza a su peso exacto en gramos (`gramWeight`).
  * Inserta 363 alimentos y 383 porciones en una sola transaccion atomica.
* **`Data/Models/`**:
  * `FoodEntity.cs`: Tabla relacional con indices en `PhosphorusMg`, `PotassiumMg`, `SodiumMg` y `Barcode`.
  * `FoodPortionEntity.cs`: Tabla de porciones caseras normalizadas a gramos.
  * `MealEntity.cs` y `MealItemEntity.cs`: Tablas de comidas e items con instantanea JSON.
  * `RecipeEntity.cs`, `RecipeIngredientEntity.cs` y `RecipeStepEntity.cs`: Tablas relacionales para recetas.
* **`Repositories/`**:
  * `SqliteFoodRepository.cs`: Consultas SQL optimizadas por indices B-Tree, implementando `GetByBarcodeAsync`.
  * `SqliteMealRepository.cs`: Transacciones de comidas e items consumidos.
  * `SqliteRecipeRepository.cs`: Persistencia relacional de recetas, ingredientes y pasos.

---

### 3.4. Capa de Presentacion (`DietApp.UI`)
Construida con .NET MAUI y **CommunityToolkit.Mvvm**:

* **Navegacion (`AppShell.xaml`)**:
  * Pestanas en `TabBar` (optimizadas a 4 pestanas nucleares para ergonomia movil):
    1. **Conteo Diario** (`MealTrackingPage`): Totales diarios de minerales, cumplimiento porcentual dinamico del plan diario (iniciado en 0% al no haber ingestas y recalculado reactivamente con cada comida registrada), banner reactivo de advertencias si se superan los limites maximos fijados por el usuario, detalle por comida y boton de accion rapida "+ Registrar Comida".
    2. **Recetas** (`RecipesPage`): Catalogo de recetas con buscador de texto, selector interactivo para ordenar por cantidad de cualquier mineral por porcion (ascendente o descendente), tarjeta con imagen final, subtitulo y badges visuales con el aporte de los 7 minerales por porcion organizados en 2 columnas con solo simbolo quimico para legibilidad movil, y apartado integrado para gestionar y abrir los **Alinos y Marinadas** (`SeasoningsPage`).
    3. **Catalogo y Filtro** (`FoodCatalogPage`): Filtrado avanzado por umbrales minimos y maximos de minerales y boton de accion "+ Nuevo Alimento".
    4. **Ajustes** (`SettingsPage`): Selector de idioma (Espanol / Ingles), selector de tema visual (Claro / Oscuro / Sistema), configuracion de la meta diaria cuantitativa de proteina (en gramos), calibracion del umbral preventivo y limites maximos diarios de minerales con activacion de alertas.
  * Rutas registradas:
    * `RecipeDetailPage`: Detalle de receta con imagen final, panel de los 7 minerales clinicos por porcion en cuadricula de 2 columnas con simbolo quimico, selector para ordenar ingredientes segun el mineral aportado, pasos numerados con imagenes y modulo interactivo para registrar el consumo en la ingesta diaria.
    * `AddRecipePage`: Compositor clinico de recetas con inicializacion limpia (formulario vacio con estados vacios para ingredientes, pasos y fotografia), dosificacion en tiempo real de alimentos y alinos, captura directa con camara (`MediaPicker.CapturePhotoAsync`) o seleccion desde galeria tanto para la foto del plato como para cada paso de elaboracion, previsualizacion con descarte y proyecciones instantaneas de nutrientes por porcion.
    * `SeasoningsPage`: Vista de administracion de alinos y marinadas, accesible modularmente desde la seccion de recetas.
    * `AddMealPage`: Composicion de comidas con soporte mixto de alimentos (en gramos) y recetas culinarias (en porciones), invocable contextualmente desde el seguimiento diario.
    * `AddFoodPage`: Formulario para registrar alimentos adicionales al catalogo SQLite con apertura en estado limpio (sin datos de prueba precargados ni textos de relleno en marcas de agua), incorporando captura opcional de codigo de barras con visor de camara nativo (`CameraView` de `BarcodeScanning.Native.Maui`), reticula de alineacion, control de linterna y fallback a entrada manual.
* **Inyeccion de Dependencias (`MauiProgram.cs`)**:
  * Registra `DietAppDbContext`, conecta los repositorios SQLite, inicializa `.UseBarcodeScanning()` y registra los servicios de localizacion (`ILanguagePreferenceStorage`, `ILocalizationService`) y alertas de minerales (`IMineralAlertStorage`, `IMineralAlertService`) en el contenedor IoC.

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

La aplicacion permite al usuario definir limites maximos diarios para controlar la ingesta de compuestos especificos (e.g. restringir sodio en dietas hipertensas o controlar fosforo y potasio en pacientes renales), emitiendo tanto avisos preventivos al aproximarse al limite como alarmas criticas al superarlo:

### 6.1. Configuracion en Ajustes (`SettingsPage` / `SettingsViewModel`)
* **Umbral Porcentual de Aviso Preventivo**:
  * Control deslizante (`Slider`) interactivo con rango del 50% al 95% (80% por defecto) y badge visual en tiempo real.
  * Permite al usuario decidir a que porcentaje de cercania a su limite diario desea recibir una advertencia temprana.
* **Limites Diarios por Mineral**:
  * Lista interactiva de los 7 minerales cuantificables (`MineralAlertConfigModel`).
  * Cada mineral cuenta con un campo numerico para ingresar el limite maximo en miligramos (`ThresholdText`) y un conmutador (`IsEnabled`) para activar o desactivar la alerta de forma independiente.
* **Persistencia Integral**:
  * Botones para guardar los limites y el porcentaje de advertencia de forma persistente o restablecerlos a sus valores por defecto.

### 6.2. Persistencia y Evaluacion en Capa de Aplicacion
* **`IMineralAlertStorage`**: Contrato desacoplado en `DietApp.Application.Services` para guardar y recuperar la coleccion de umbrales y el porcentaje de advertencia.
* **`MauiPreferencesMineralAlertStorage`**: Implementacion nativa en `DietApp.UI.Services` que serializa los umbrales en JSON y persiste el porcentaje en `Preferences`.
* **`IMineralAlertService` / `MineralAlertService`**:
  * Centraliza la logica para comparar los totales consumidos en el dia (`DailyMinerals`) contra los limites activos y el porcentaje preventivo.
  * Clasifica las alertas segun su severidad (`MineralAlertSeverity`):
    * **`NearLimit` (Aviso preventivo)**: Se activa cuando el consumo diario alcanza o supera el umbral porcentual configurado pero no ha rebasado el 100% del maximo.
    * **`ExceededLimit` (Limite superado)**: Se activa cuando el consumo diario alcanza o supera el 100% del maximo fijado.
  * Genera objetos `MineralAlertExceededDto` con cantidad actual, maximo permitido, exceso/margen, porcentaje alcanzado, nivel de severidad y formato visual (colores y etiquetas).

### 6.3. Disparo Visual de Alertas en Seguimiento Diario (`MealTrackingPage` / `MealTrackingViewModel`)
* Al consultar cualquier fecha en el seguimiento diario, se evaluan los totales acumulados.
* Si uno o varios minerales se encuentran proximos al limite o lo han superado, se despliega el banner de advertencia destacado.
* Cada alerta se diferencia visualmente segun su nivel de severidad:
  * **Avisos preventivos (`NearLimit`)**: Estilizados en tonos ambar/dorado con la insignia "AVISO PREVENTIVO" / "EARLY WARNING", indicando el porcentaje consumido y el limite objetivo.
  * **Limites superados (`ExceededLimit`)**: Estilizados en tonos rojo/coral con la insignia "LIMITE SUPERADO" / "LIMIT EXCEEDED", detallando el exceso en miligramos y el porcentaje total alcanzado.

### 6.4. Advertencias Preventivas al Consultar Recetas (`RecipeDetailPage` / `RecipeDetailViewModel`)
* Al entrar en la vista de detalle de cualquier receta culinaria, el sistema audita de forma proactiva el contenido mineral por porcion contra los limites maximos fijados por el usuario.
* El metodo `CheckRecipePortionWarnings` en `IMineralAlertService` evalua:
  * Si consumir una sola porcion supera de forma individual el limite diario maximo fijado (`Portion >= Max`).
  * Si consumir la porcion sumada al consumo ya acumulado en la fecha seleccionada supera el limite diario (`CurrentDaily + Portion >= Max`).
  * Si consumir la porcion alcanza o supera el umbral preventivo configurado (`Projected >= Max * WarningPercentage`).
* Si se detecta un exceso o aviso preventivo, se despliega de inmediato un banner destacado en la parte superior de la receta con badges coloreados (`LIMITE SUPERADO` / `AVISO PREVENTIVO`), desglosando el aporte de la receta, el consumo del dia y el margen de exceso en miligramos.
* La evaluacion se actualiza de manera reactiva si el usuario modifica el numero de porciones a registrar o cambia la fecha de ingesta.

---

## 7. Modulo de Alinos y Condimentos

La aplicacion incluye una seccion especializada para la gestion de alinos, vinagretas, aderezos y mezclas de condimentos (`SeasoningsPage`), los cuales pueden incorporarse con un solo toque en la creacion de recetas para agilizar sustancialmente el proceso culinario:

### 7.1. Gestion de Alinos (`SeasoningsPage` / `SeasoningsViewModel`)
* **Listado de Alinos**: Muestra las preparaciones guardadas con su peso total en gramos, calorias acumuladas, chips con los ingredientes incluidos y el balance consolidado de minerales.
* **Creacion Interactiva**:
  * Panel desplegable para ingresar nombre, descripcion de uso y agregar ingredientes dosificados en gramos a partir del catalogo de alimentos.
  * Valida y calcula de forma inmediata los minerales y calorias de la mezcla completa.
  * Permite eliminar ingredientes del borrador antes de guardar.
* **Eliminacion**: Boton directo para borrar alinos existentes.

### 7.2. Incorporacion en Creacion de Recetas (`AddRecipePage` / `AddRecipeViewModel`)
* En el asistente de creacion de recetas, dentro de la seccion de ingredientes, se ofrece la opcion **"Incorporar Alino o Condimento Preconfigurado"**.
* El usuario selecciona cualquier alino guardado (e.g. "Vinagreta Clasica de Limon y Oliva") y presiona el boton **"+ Incorporar Alino a la Receta"**.
* El sistema expande y anade automaticamente todos los componentes del alino a la lista de ingredientes de la receta con sus gramajes exactos, evitando tener que agregar aceites, sales, limones y especias uno por uno.
* Los ingredientes incorporados se integran de inmediato al calculo automatico de minerales y calorias por porcion de la receta.

### 7.3. Arquitectura y Persistencia
* **`Seasoning` y `SeasoningItem`**: Entidades y raiz de agregado en `DietApp.Domain.Entities` que encapsulan el calculo matematico de minerales y calorias de los condimentos.
* **`ISeasoningRepository` y `SqliteSeasoningRepository`**: Contrato e implementacion SQLite con almacenamiento relacional en las tablas `Seasonings` y `SeasoningItems`.
* **`ISeasoningService` y `SeasoningService`**: Servicio de aplicacion en `DietApp.Application.Services` que orquesta la creacion, conversion a DTOs y persistencia.
* **`InitialSeasoningSeed`**: Semillas predefinidas que se precargan automaticamente en la base de datos si la tabla se encuentra vacia.

---

---

## 8. Modulo de Cuantificacion y Seguimiento de Proteinas

La aplicacion incluye integracion integral de **proteinas (g)** a traves de las cuatro capas de la arquitectura DDD:

### 8.1. Logica de Dominio (`DietApp.Domain`)
* **`FoodItem`**: Modela la propiedad `ProteinGrams` (gramos de proteina presentes en la porcion base de referencia, por ejemplo 100g). Incluye el metodo puro `CalculateProteinForPortion(double grams)` que calcula el aporte escalado para cualquier gramaje consumido: `(ProteinGrams * grams) / ReferenceGrams`.
* **`RecipeIngredient` y `SeasoningItem`**: Calculan y almacenan la instantanea inmutable `CalculatedProtein` al instanciarse a partir de un alimento base dosificado en gramos.
* **`Recipe`**: Como raiz de agregado culinario, calcula la proteina total sumando sus ingredientes (`CalculateTotalProtein()`) y la densidad por porcion individual (`CalculateProteinPerServing() = TotalProtein / Servings`).
* **`MealItem`**: Registra la cantidad de proteina consumida tanto si proviene directamente de un alimento (`FromFoodItem`) como si proviene de una racion de receta (`FromRecipe`), preservando fidelidad matematica.
* **`Meal`**: Suma la proteina de todos los alimentos y recetas ingeridas en la comida con `CalculateTotalProtein()`.
* **`DailyMineralAggregatorService`**: Consolida la ingesta total de proteina del dia mediante `AggregateProtein(IEnumerable<Meal>)`.
* **`IFoodRepository`**: Define el contrato `FilterByProteinRangeAsync(double minimumGrams, double maximumGrams)` para filtrado por densidad proteica.

### 8.2. Casos de Uso y Contratos (`DietApp.Application`)
* **DTOs Enriquecidos**:
  * `FoodItemDto`: Expone `ProteinGrams`, `SubtitleSummary` con indicacion de proteinas y la propiedad formateada `ProteinBadgeText` (`"{ProteinGrams:F1} g proteina"`).
  * `RecipeDto`: Expone `ProteinPerServing`, `TotalProtein`, `ProteinPerServingDisplay` (`"{ProteinPerServing:F1} g proteina / porcion"`) y actualiza `NutritionSubtitle` con el aporte proteico por porcion.
  * `RecipeIngredientDto`: Expone `CalculatedProtein` y resume el aporte en `DisplayText`.
  * `MealDto` y `MealItemDto`: Exponen `CalculatedProtein`, `TotalProtein` y el resumen nutricional `NutritionSummary`.
  * `MineralFilterCriteriaDto`: Incorpora `FilterByProtein`, `MinimumProteinGrams`, `MaximumProteinGrams` y la opcion de ordenamiento `SortBy` (`ProteinDesc`, `ProteinAsc`).
* **Servicios de Aplicacion**:
  * `FoodCatalogService`: Aplica filtros por umbral de proteina y ordenamiento descendente o ascendente segun el criterio del usuario. Guarda y persiste el valor de proteina al ingresar nuevos alimentos.
  * `MealTrackingService`: Ofrece `GetDailyTotalProteinAsync(DateTime date)` para consultar el acumulado proteico de cualquier fecha.

### 8.3. Persistencia y Base de Datos SQLite (`DietApp.Infrastructure`)
* **Modelos Relacionales**:
  * `FoodEntity`: Incorpora la columna `ProteinGrams` con atributo `[Indexed]` para acelerar consultas y ordenamientos por rangos.
  * `MealItemEntity`, `RecipeIngredientEntity`, `SeasoningItemEntity`: Almacenan `CalculatedProtein`.
* **Dataset Oficial USDA FoodData Central**:
  * `FoodDataCentralImporter` extrae de forma automatizada el codigo de nutriente oficial `"203"` (Protein en gramos con unidad `"g"`) de cada alimento de la fundacion USDA Foundation Foods.
  * Incluye la rutina `BackfillProteinIfMissingAsync` para rellenar retroactivamente valores de proteinas en bases de datos que ya contenian alimentos precargados.
* **Semillas y Migraciones**:
  * `InitialFoodCatalogSeed`: Incluye los valores reales de proteina de cada alimento semilla precargado.
  * `DietAppDbContext`: Aplica rutinas automaticas en `InitializeAsync` para sincronizar y actualizar el esquema y poblar valores de proteina en tablas relacionales existentes.

### 8.4. Interfaz de Usuario y Experiencia (`DietApp.UI`)
* **Catalogo de Alimentos (`FoodCatalogPage` / `FoodCatalogViewModel`)**:
  * Selector desplegable para filtrar por "Proteina (g)" con campos numericos de minimo y maximo.
  * Selector de ordenamiento con opciones: "Proteina (Mayor a menor)", "Proteina (Menor a mayor)" y "Nombre (A-Z)".
  * Badge visual estilizado en cada tarjeta de alimento con la cantidad de proteina (`ProteinBadgeText`).
* **Formulario de Nuevo Alimento (`AddFoodPage` / `AddFoodViewModel`)**:
  * Campos dedicados para ingresar calorias (kcal) y proteina (g por porcion de referencia).
* **Seguimiento Diario (`MealTrackingPage` / `MealTrackingViewModel`)**:
  * Bloque de resumen diario con badge destacado que muestra la proteina total consumida en la fecha (`DailyProteinDisplay`).
  * Desglose de macronutrientes en cada tarjeta de comida registrada (`NutritionSummary`).
* **Recetario y Detalle de Receta (`RecipesPage`, `RecipeDetailPage`)**:
  * Ordenamiento de recetas e ingredientes por contenido de proteina a traves de `MineralSortOption` (`IsProtein`).
  * Badge visual del aporte de proteina por porcion en las tarjetas de recetas y en la seccion superior de la receta.

---

## 9. Instrucciones de Compilacion y Ejecucion

### Ejecucion en Windows (Modo Rapido):
Para compilar y ejecutar en Windows directamente desde la terminal:
```powershell
dotnet build -t:Run -f net10.0-windows10.0.19041.0 src/DietApp.UI/DietApp.UI.csproj
```
```powershell
dotnet run --project src/DietApp.UI/DietApp.UI.csproj -f net10.0-windows10.0.19041.0
```

### Ejecucion en Android:
1. Abrir la solucion `DietApp.slnx` en Visual Studio.
2. Seleccionar como proyecto de inicio `DietApp.UI`.
3. Seleccionar el emulador de Android o un dispositivo fisico conectado en la barra superior.
4. Presionar `F5` para iniciar la depuracion.

---

## 10. Sistema Visual y Experiencia de Usuario (Alineacion Stitch Clinical Nutrition Ergonomics)

### 10.1. Paleta Oficial Stitch y Recursos Semanticos
* **Colores Principales**:
  * **Verde Lima Clinico (`#84cc16`)**: Color primario de marca, empleado en botones principales de accion (`Guardar`, `Completado`, `Registrar`), badges de proteina destacada y bordes de seleccion activa.
  * **Verde Bosque Profundo (`#416900`)**: Primario oscuro para estados presionados y alto contraste.
  * **Naranja Coral (`#ef713f`)**: Color semantico de alerta clinica, limites superados, pildora de potasio prioritario y acciones destructivas de eliminacion.
  * **Ambar Dorado (`#f59e0b`)**: Color de aviso preventivo cuando un mineral alcanza el umbral de advertencia configurado.
  * **Superficie Clinica Calida (`#faf8ff`)**: Fondo general de pantallas en tema claro.
  * **Contenedor Bento (`#ffffff`)**: Fondo de tarjetas bento e ingredientes con borde normado (`#c1cab0`).
  * **Contenedor Oscuro (`#1c1c20`)** y Fondo Oscuro (`#121214`): Superficie para el modo oscuro con bordes (`#43483e`).
* **Matriz Cromatica Especializada para los 7 Minerales**:
  * Potasio (K): `#ef713f` (Naranja Coral).
  * Fosforo (P): `#8b5cf6` (Amatista Real).
  * Sodio (Na): `#2563eb` (Azul Cobalto).
  * Calcio (Ca): `#06b6d4` (Cian Turquesa).
  * Magnesio (Mg): `#ec4899` (Rosa Pizarra).
  * Hierro (Fe): `#f97316` (Terracota Calido).
  * Zinc (Zn): `#64748b` (Acero Industrial).
* **Archivos Clave**:
  * [Colors.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Resources/Styles/Colors.xaml): Definicion de colores y pinceles semanticos Stitch (`Primary`, `PrimaryDark`, `Secondary`, `Tertiary`, `SurfaceLight`, `SurfaceContainerLight`, `MineralPill*Brush`).
  * [Styles.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Resources/Styles/Styles.xaml): Estilos base de pildoras (`StitchPillStyle`, `PillCoralStyle`), tarjetas bento (`StitchCardStyle`), botones ergonomicos de 48dp (`StitchPrimaryButtonStyle`, `StitchSecondaryButtonStyle`, `StitchNeutralButtonStyle`) y barra de navegacion `Shell`.

### 10.2. Vista A: Ficha de Detalle de Receta (`RecipeDetailPage`)
* **Hero Visual y Overlay**:
  * Contenedor superior con imagen culinaria real (`recipe_hero_dish.jpg`) de 260dp de altura.
  * Boton circular translucido flotante en la esquina superior izquierda (48dp x 48dp, radio 24dp) con icono de cierre ('✕') que ejecuta `CloseCommand` para retornar (`Shell.Current.GoToAsync("..")`).
  * Barra inferior flotante oscura superpuesta sobre el borde de la fotografia con el titulo de la receta y badge energetico en verde lima clinico (`#84cc16`).
* **Pildoras de Minerales Criticos**:
  * Fila horizontal que resalta el balance clinico con pastillas nutricionales (`Sodio` en azul cobalto `#2563eb`, `Potasio` en naranja coral `#ef713f`, `Fosforo` en amatista `#8b5cf6`).
* **Cuadricula 2x2 de Ingredientes**:
  * Tarjetas bento individuales con fondo blanco, bordes finos calidos, imagen fotografica, nombre del alimento y gramaje formateado.
* **Preservacion Clinica**:
  * Incluye avisos preventivos de limites recomendados, seleccion de fecha para registro en ingesta diaria con controles de 48dp de altura y pasos de elaboracion culinaria en tarjetas bento numeradas.

### 10.3. Vista B: Seleccion de Componentes y Alinos (`SeasoningsPage`)
* **Carrusel Superior de Modulos Nutricionales**:
  * Fila de modulos con tarjetas de balance: modulo principal con pildoras de aporte de nutrientes y badge de atencion, modulo de postre con indicador de check, y modulo de liquido con indicador de check.
* **Selector de Categorias**:
  * Pestanas limpias de navegacion interna (`Base`, `Alinos`, `Postre`, `Liquido`).
* **Cuadricula 2x2 de Seleccion**:
  * Tarjetas interactivas de alinos (`#Personalizado`, `Aceite de Oliva`, `Oregano`, `Aceite de Canola`) con imagenes fotograficas y badge circular de check coral en esquina superior derecha para el elemento activo.
* **Barra Inferior de Accion Fija**:
  * Contenedor inferior oscuro con botones ergonomicos de 48dp: `Agregar` (estilo secundario con borde fino) y `Completado` (boton verde lima clinico `#84cc16` con texto oscuro).
* **Constructor Personalizado**:
  * Mantiene el panel de creacion de mezclas de condimentos a medida con calculo instantaneo de minerales.

### 10.4. Fotografia Culinaria Integrada
* Fotografia culinaria real ubicada en `src/DietApp.UI/Resources/Images/`:
  * `recipe_hero_dish.jpg`, `food_potatoes.jpg`, `food_rice.jpg`, `food_broccoli.jpg`, `food_chicken.jpg`.
  * `seasoning_custom.jpg`, `seasoning_olive_oil.jpg`, `seasoning_oregano.jpg`, `seasoning_canola_oil.jpg`.
  * `module_dessert.jpg`, `module_liquid.jpg`.
* Migracion transparente en `DietAppDbContext` para sustituir referencias residuales en bases de datos SQLite locales existentes.

### 10.5. Soporte Integral de Modo Claro y Modo Oscuro
* **Arquitectura de Temas (DDD)**:
  * `ThemeMode` en `DietApp.Domain.Enums` define los estados `Light`, `Dark` y `System`.
  * `IThemeService` en `DietApp.Application.Services` define el contrato para consultar, conmutar y persistir el tema de la aplicacion.
  * `AppThemeService` en `DietApp.UI.Services` implementa el servicio mediante `Preferences` y conmuta `Application.Current.UserAppTheme` en el hilo principal (`MainThread`).
  * `App.xaml.cs` inicializa el servicio al arrancar, fijando por defecto el modo oscuro si no existe configuracion previa.
* **Selector en Pantalla de Ajustes (`SettingsPage` / `SettingsViewModel`)**:
  * Tarjetas interactivas con tres opciones de 48dp de altura minima: `Modo Claro` (superficie clinica `#faf8ff`, tarjetas blancas `#ffffff` y tipografia profunda `#191c1e`), `Modo Oscuro` (fondo oscuro `#121214`, tarjetas bento `#1c1c20` y textos en `#e2e2e6`) y `Sistema` (sincronizacion automatica con Android).
  * Conmutacion reactiva instantanea de toda la interfaz y soporte bilingue (espanol/ingles).
* **Armonizacion de Superficies y Bordes**:
  * Pinceles de borde dinamicos `AppThemeBinding Light={StaticResource OutlineLightBrush}, Dark={StaticResource OutlineDarkBrush}` aplicados en tarjetas, modulos y listas.

---

## 11. Reconstruccion Integral de Interfaz UI segun Prototipo Stitch

La interfaz grafica de usuario de DietApp fue reconstruida desde cero para alinearse al prototipo de Stitch (`projects/2919043587307730423`).

### 11.1. Catalogo Completo de Pantallas Reconstruidas
1. **`MealTrackingPage` (Conteo Diario)**: Monitoreo ergonomico del dia con balance metabolico, energia calorica, meta proteica, cumplimiento del plan diario, panel de trazabilidad cuantitativa de los 7 minerales criticos en columna unica a ancho completo con barras de progreso y badges clinicos (K, P, Na, Ca, Mg, Fe, Zn), banners reactivos de advertencia si se supera el umbral preventivo y desglose de las tomas registradas.
2. **`AddMealPage` (Compositor de Comidas)**: Composicion de platos con seleccion combinada de alimentos de la base de datos y recetas, selector de momento de ingesta y auditoria preventiva previa al registro.
3. **`RecipesPage` (Catalogo de Recetas)**: Recetario clinico con buscador en tiempo real, ordenamiento multicriterio por densidad de minerales o proteinas, badges macro flotantes y acceso directo al modulo de alinos.
4. **`RecipeDetailPage` (Ficha de Receta)**: Fotografia heroica culinaria, barras de sodio, potasio y fosforo, auditoria preventiva proactiva, cuadricula dosificada de ingredientes en 2 columnas, matriz de los 7 minerales cuantitativos, pasos tecnicos numerados y modulo de registro de porciones consumidas.
5. **`AddRecipePage` (Compositor de Recetas)**: Formulario de creacion de recetas con selector de imagen, proyeccion nutricional reactiva por porcion, integracion con alinos personalizados y pasos secuenciales.
6. **`SeasoningsPage` (Modulo de Alinos)**: Riel horizontal de platos diana, tira de advertencia de seguridad de sodio, filtros por categoria, 4 alinos preconfigurados y estudio de formulacion personalizada con medidor bioquimico instantaneo.
7. **`FoodCatalogPage` (Catalogo de Alimentos)**: Buscador clinico, filtro avanzado por rangos minimos y maximos de minerales (mg), presintonias clinicas frecuentes en riel horizontal y tarjetas de alimentos con desglose de los 7 minerales y distincion coral para alertas de potasio.
8. **`AddFoodPage` (Alta de Alimento)**: Formulario modal con selector de imagen/icono, datos generales, porcion de referencia dividida, macronutrientes, matriz cuantitativa de los 7 minerales diana, selector de dieta renal KDOQI, pastillas de advertencia y notas dietoterapeuticas.
9. **`SettingsPage` (Ajustes y Configuracion)**: Preferencias bilingues (Espanol/Ingles), selector de tema (Claro/Oscuro/Sistema), barra deslizadora interactiva para calibrar el umbral preventivo de alerta (50% a 95%), administracion interactiva de la meta diaria de proteina (en gramos) con interruptor de activacion y valor cuantitativo ajustable, interruptores y limites diarios para los 7 minerales, boton de restauracion a estandares KDOQI/USDA y auditoria tecnica de la base SQLite.

### 11.2. Resolucion de Restricciones Tecnicas entre Stitch HTML/CSS y .NET MAUI
* **Virtualizacion y Prevencion de Congelamiento en Catálogo Extenso (`FoodCatalogPage`)**:
  * *Problema Detectado*: El catalogo de alimentos incorpora cerca de 400 items con matrices completas de 7 minerales, badges e indicadores (mas de 45 controles visuales por tarjeta). El uso previo de `BindableLayout.ItemsSource` dentro de un `ScrollView` forzaba la creacion sincrona de aproximadamente 20.000 vistas en el hilo principal de Android al cargar o filtrar la lista, provocando bloqueos y congelamiento severo de la aplicacion (UI hang de mas de 10 segundos).
  * *Solucion Arquitectonica Implementada*:
    1. **Sustitucion por `CollectionView` Virtualizado**: Se elimino el `ScrollView` externo y se utilizo `CollectionView` con reciclado de vistas nativo de Android (`RecyclerView`), limitando el inflado en memoria a unicamente las ~6 a 8 tarjetas visibles en pantalla.
    2. **Estructura con `CollectionView.Header` y `CollectionView.Footer`**: Todo el panel bento de busqueda y filtros clinicos se ubico dentro de `<CollectionView.Header>`, y el boton interactivo de carga incremental dentro de `<CollectionView.Footer>`, preservando el desplazamiento vertical continuo sin anidacion de scrolls conflictivos.
    3. **Paginacion Progresiva en Memoria**: `FoodCatalogViewModel` particiona el conjunto filtrado en bloques de 25 alimentos (`PageSize = 25`) con el comando `LoadMoreFoodsCommand`, permitiendo una presentacion instantanea (< 5 ms) y carga fluida a demanda.
    4. **Debounce No Bloqueante en Busqueda**: La propiedad `SearchQuery` incorpora un retardo de 250 ms gestionado mediante `CancellationTokenSource` y `MainThread.BeginInvokeOnMainThread` para no disparar renderizados redundantes durante la digitacion del usuario.
* **Propiedad Padding en Controles Entry**: El analizador XAML de MAUI rechaza la propiedad `Padding` directamente en controles `Entry`. Se resolvio encapsulando cada campo de entrada dentro de un contenedor `Border` estilizado con `StrokeShape="RoundRectangle 8"`, fondo contrastado y `Padding` interno, manteniendo el `Entry` con fondo transparente (`BackgroundColor="Transparent"`).
* **Ausencia de Selectores Nativos Molestos**: Para preservar la estetica de tarjeta moderna de Stitch sin perder la ergonomia de selectores de fecha o desplegables nativos en dispositivos moviles, se utilizaron controles nativos superpuestos con opacidad minima o `Picker` estilizados integrados en cuadriculas con icono vectorizado indicador.
* **Resolucion de Recursos Estaticos y Convertidores Globales**: El inflador de XAML en Android requiere que todos los tokens referenciados mediante `StaticResource` esten explicitamente presentes en el diccionario de recursos consolidado. Se incorporaron los tokens faltantes para contenedores oscuros (`StitchDarkSurfaceContainerLow`, `StitchDarkSurfaceContainerHigh`), contraste de texto sobre elementos fijos (`StitchOnSecondaryFixed`, `StitchOnTertiaryFixed`), el recurso de sombra `StitchCardShadow` y el convertidor `InvertedBoolConverter` registrado a nivel global en `App.xaml`.
* **Cumplimiento Estricto de Cero Emojis**: Todos los iconos de la interfaz grafica fueron implementados mediante glifos vectoriales nitidos de Material Symbols Outlined a traves de la clase estatica [MaterialIconFont.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Helpers/MaterialIconFont.cs).

---

## 12. Sistema de Metas Nutricionales y Meta Diaria de Proteina

### 12.1. Requerimiento Clinico
En el tratamiento nutricional de pacientes con enfermedad renal cronica (ERC / CKD) segun las directrices KDOQI 2024, el control cuantitativo de la ingesta de proteina (habitualmente fijado en rangos de 0.6 a 0.8 g/kg/dia para estadios 3 a 5 sin dialisis) resulta tan determinante para preservar la tasa de filtracion glomerular como el control de fosforo y potasio.

### 12.2. Arquitectura de la Solucion (DDD)
* **Abstraccion de Almacenamiento (`IProteinGoalStorage`)**: Definida en `DietApp.Application.Services` para desacoplar el mecanismo de persistencia nativo de la capa de aplicacion.
* **Implementacion con Preferences (`MauiPreferencesProteinGoalStorage`)**: Definida en `DietApp.UI.Services`, almacena la meta diaria en gramos y su estado de habilitacion de forma persistente entre sesiones del usuario.
* **Servicio de Aplicacion (`IProteinGoalService` / `ProteinGoalService`)**: Centraliza la cifra meta (por defecto 60 g/dia), la activacion del objetivo, la restauracion a valores de referencia KDOQI y la emision de eventos `ProteinGoalChanged` para sincronizacion reactiva.
* **Integracion en Pantalla de Ajustes (`SettingsViewModel` y `SettingsPage.xaml`)**:
  * Incorpora tarjeta bento con distintivo verde lima `PROT` (`MineralProteinContainer` y `MineralProteinText`), titulo formal, subtitulo KDOQI, interruptor de activacion y campo numerico en gramos.
  * Se guarda de forma unificada al accionar "Guardar Limites Clinicos" y se restablece con "Restablecer Valores por Defecto".
* **Consumo Dinamico en Vistas**:
  * **Conteo Diario (`MealTrackingViewModel` / `MealTrackingPage.xaml`)**: El badge "Meta Proteica" refleja reactivamente la relacion de consumo contra el objetivo (ejemplo: `45.2 / 60 g` o `45.2 g` si la meta esta deshabilitada).
  * **Compositor de Comidas (`AddMealViewModel`)**: Calcula el porcentaje de cobertura proteica proactiva utilizando la meta configurada en lugar de valores fijos arbitrarios.