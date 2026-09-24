# Documentacion Tecnica - DietApp

## 1. Descripcion General
DietApp es una aplicacion movil y de escritorio construida con **C# y .NET MAUI** orientada al registro, control y seguimiento de la ingesta de alimentos con un enfoque especializado en el contenido de **minerales** (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc) y valor calorico (kcal).

Permite:
* Filtrar alimentos de forma avanzada por umbrales maximos y minimos de minerales (de especial utilidad en dietas renales, cardiovasculares o deportivas).
* Calcular el aporte acumulado de compuestos por cada comida y a nivel diario.
* **Modulo de Recetas Nutricionales**: Crear y consultar preparaciones culinarias con titulo, imagen final de la receta terminada, subtitulo automatico de minerales y calorias por porcion calculado con base en los ingredientes, e instrucciones en pasos numerados con opcion de adjuntar imagenes en cada etapa.
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
* **`ValueObjects/MineralAmount.cs`**: Objeto de valor inmutable que encapsula el tipo de mineral y su cantidad en miligramos (mg).
* **`Entities/FoodItem.cs`**: Entidad que representa un alimento en el catalogo con su perfil de minerales y calorias por porcion base normalizada de 100 gramos.
* **`Entities/MealItem.cs`**: Representa la ingesta de un alimento en una comida con su gramaje real e instantanea calculada de minerales.
* **`Entities/Meal.cs`**: Raiz de agregado (Aggregate Root) que agrupa los alimentos consumidos y calcula la sumatoria consolidada de minerales con `CalculateTotalMinerals()`.
* **`Entities/Recipe.cs`**: Raiz de agregado para recetas culinarias con ingredientes dosificados, pasos numerados, porciones, imagen final y calculo nutricional por porcion.
* **`Entities/RecipeIngredient.cs`**: Ingrediente dosificado con calculo de calorias y minerales.
* **`Entities/RecipeStep.cs`**: Paso numerado con instruccion e imagen ilustrativa de la etapa.
* **`Services/DailyMineralAggregatorService.cs`**: Servicio de dominio que consolida la suma total de minerales entre multiples comidas del dia.
* **`Repositories/`**: Contratos `IFoodRepository.cs`, `IMealRepository.cs` e `IRecipeRepository.cs`.

---

### 3.2. Capa de Aplicacion (`DietApp.Application`)
Orquesta los casos de uso del sistema:

* **`DTOs/`**: `FoodItemDto.cs`, `MealDto.cs`, `MealItemDto.cs`, `MineralAmountDto.cs`, `MineralFilterCriteriaDto.cs`, `RecipeDto.cs` (con subtitulo nutricional por porcion), `RecipeIngredientDto.cs`, `RecipeStepDto.cs`.
* **`Mapping/DomainDtoMapper.cs`**: Funciones puras de extension para transformar entidades a DTOs con traduccion de nombres a espanol.
* **`Services/FoodCatalogService.cs`**: Casos de uso de consulta, busqueda y filtrado por rangos de minerales.
* **`Services/MealTrackingService.cs`**: Casos de uso de registro de comidas y balance diario de minerales.
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
    1. **Conteo Diario** (`MealTrackingPage`): Totales diarios de minerales y detalle por comida.
    2. **Recetas** (`RecipesPage`): Catalogo de recetas con buscador, tarjeta con imagen final y subtitulo nutricional por porcion.
    3. **Catalogo y Filtro** (`FoodCatalogPage`): Filtrado avanzado por umbrales minimos y maximos de minerales.
    4. **Registrar Comida** (`AddMealPage`): Registro de comidas con alimentos del catalogo y gramaje consumido.
    5. **Nuevo Alimento** (`AddFoodPage`): Formulario para ingresar alimentos adicionales al catalogo SQLite.
  * Rutas registradas:
    * `RecipeDetailPage`: Detalle de receta con imagen final, ingredientes y pasos numerados con imagenes.
    * `AddRecipePage`: Formulario para crear recetas con selector de imagenes por paso y final.
* **Inyeccion de Dependencias (`MauiProgram.cs`)**:
  * Registra `DietAppDbContext` y conecta los repositorios SQLite en el contenedor IoC.

---

## 4. Instrucciones de Compilacion y Ejecucion

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