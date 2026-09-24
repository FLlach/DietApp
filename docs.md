# Documentacion Tecnica - DietApp

## 1. Descripcion General
DietApp es una aplicacion movil y de escritorio construida con **C# y .NET MAUI** orientada al registro, control y seguimiento de la ingesta de alimentos con un enfoque especializado en el contenido de **minerales** (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc). 

Permite filtrar alimentos de forma avanzada por umbrales maximos y minimos de minerales (de especial utilidad en dietas renales, cardiovasculares o deportivas) y calcular el aporte acumulado de compuestos por cada comida y a nivel diario.

---

## 2. Arquitectura del Proyecto (Domain-Driven Design)
La solucion esta estructurada en cuatro capas desacopladas con dependencias unidireccionales:

```
DietApp.slnx
├── src/
│   ├── DietApp.Domain/          # Nucleo puro de negocio (sin dependencias externas)
│   ├── DietApp.Application/     # Casos de uso, DTOs, mapeos e interfaces de servicio
│   ├── DietApp.Infrastructure/  # Persistencia de datos y catalogo semilla de minerales
│   └── DietApp.UI/              # Vistas XAML, ViewModels (MVVM) y configuracion MAUI
```

### Reglas de Diseno Aplicadas:
* **Bajo acoplamiento y alta cohesion**: La capa de dominio no depende de bases de datos ni de frameworks visuales.
* **Inversion de dependencias**: La aplicacion y la UI se comunican mediante abstracciones e interfaces (`IFoodRepository`, `IMealRepository`, `IFoodCatalogService`, `IMealTrackingService`).
* **Sin cadenas magicas ni valores arbitrarios**: Se utilizan enumeraciones fuertemente tipadas y objetos de valor inmutables.
* **Sin emojis**: Todo el codigo, comentarios y documentacion mantienen estilo profesional formal.

---

## 3. Descripcion de las Capas y Componentes

### 3.1. Capa de Dominio (`DietApp.Domain`)
Contiene las entidades, enumeraciones, objetos de valor y servicios del dominio:

* **`Enums/MineralType.cs`**:
  * *Como funciona*: Catalogo fuertemente tipado de minerales cuantificables (`Phosphorus`, `Potassium`, `Sodium`, `Calcium`, `Magnesium`, `Iron`, `Zinc`).
  * *Por que se tomo esta decision*: Previene errores por cadenas magicas, acelera las comparaciones numericas y unifica el filtrado.
* **`Enums/MealType.cs`**:
  * *Como funciona*: Define los momentos de ingesta (`Breakfast`, `Lunch`, `Dinner`, `Snack`, `Other`).
  * *Por que se tomo esta decision*: Permite clasificar y segmentar los totales de minerales a lo largo del dia.
* **`ValueObjects/MineralAmount.cs`**:
  * *Como funciona*: Objeto de valor inmutable que encapsula el tipo de mineral y su cantidad en miligramos (mg).
  * *Por que se tomo esta decision*: En DDD, cantidades con unidad de medida son objetos de valor; garantizan que no existan valores negativos y proveen operaciones de escalado proporcional.
* **`Entities/FoodItem.cs`**:
  * *Como funciona*: Entidad que representa un alimento en el catalogo con su perfil de minerales por porcion de referencia (por ejemplo 100g). Provee el metodo `CalculateMineralsForPortion(grams)`.
  * *Por que se tomo esta decision*: Aísla la definicion base del alimento para que cualquier porcion consumida mantenga una escala exacta.
* **`Entities/MealItem.cs`**:
  * *Como funciona*: Representa la ingesta de un alimento en una comida especifica con su gramaje real. Almacena una instantanea de los minerales calculados.
  * *Por que se tomo esta decision*: Evita que modificaciones futuras en el catalogo alteren registros historicos de dias pasados.
* **`Entities/Meal.cs`**:
  * *Como funciona*: Raiz de agregado (Aggregate Root) que agrupa los alimentos ingeridos en una fecha y calcula la sumatoria consolidada de minerales con `CalculateTotalMinerals()`.
  * *Por que se tomo esta decision*: Garantiza la consistencia interna y la exactitud del conteo de compuestos.
* **`Services/DailyMineralAggregatorService.cs`**:
  * *Como funciona*: Servicio de dominio que consolida la suma total de minerales entre multiples comidas correspondientes a un mismo dia o periodo.
  * *Por que se tomo esta decision*: Las operaciones que cruzan multiples agregados residen en servicios de dominio para evitar acoplar entidades entre si.
* **`Repositories/IFoodRepository.cs` e `IMealRepository.cs`**:
  * *Como funciona*: Contratos de persistencia para el catalogo de alimentos y el historial de comidas.

---

### 3.2. Capa de Aplicacion (`DietApp.Application`)
Orquesta los casos de uso del sistema:

* **`DTOs/`**:
  * `FoodItemDto.cs`, `MealDto.cs`, `MealItemDto.cs`, `MineralAmountDto.cs`, `MineralFilterCriteriaDto.cs`.
  * Desacoplan los modelos internos del dominio de los formatos requeridos por la interfaz visual.
* **`Mapping/DomainDtoMapper.cs`**:
  * *Como funciona*: Metodos de extension puros para traducir entidades a DTOs y formatear nombres legibles de minerales en espanol.
  * *Por que se tomo esta decision*: Centraliza la localizacion y el mapeo en un unico punto reutilizable.
* **`Services/FoodCatalogService.cs`**:
  * *Como funciona*: Implementa la consulta, busqueda y filtrado de alimentos por rango de concentracion de minerales (ej. alimentos con potasio entre 200mg y 400mg).
* **`Services/MealTrackingService.cs`**:
  * *Como funciona*: Permite registrar comidas completas, calcular minerales proporcionales segun los gramos ingeridos y consultar el balance total del dia.

---

### 3.3. Capa de Infraestructura (`DietApp.Infrastructure`)
Provee las implementaciones de acceso a datos:

* **`SeedData/InitialFoodCatalogSeed.cs`**:
  * *Como funciona*: Carga un catalogo inicial con alimentos reales y sus valores de minerales (Platano, Espinaca, Salmon, Pechuga de pollo, Lentejas, Queso parmesano, Papa).
  * *Por que se tomo esta decision*: Permite poner en marcha y probar la aplicacion de inmediato con datos de prueba realistas.
* **`Repositories/FoodRepository.cs` e `MealRepository.cs`**:
  * *Como funciona*: Implementaciones concurrentes con control de concurrencia mediante `SemaphoreSlim`.
  * *Por que se tomo esta decision*: Ofrece ejecucion rapida y consistente en todas las plataformas soportadas sin riesgo de incompatibilidades de drivers nativos durante el desarrollo inicial.

---

### 3.4. Capa de Presentacion (`DietApp.UI`)
Construida con .NET MAUI siguiendo el patron **MVVM (Model-View-ViewModel)** mediante **CommunityToolkit.Mvvm**:

* **Navegacion (`AppShell.xaml`)**:
  * Pestanas nativas (`TabBar`) que conectan las cuatro pantallas principales:
    1. **Conteo Diario** (`MealTrackingPage`): Visualiza los minerales totales del dia y el desglose por comida con opciones para eliminar registros.
    2. **Catalogo y Filtro** (`FoodCatalogPage`): Permite buscar por texto y filtrar alimentos por concentracion minima y maxima de un mineral especifico.
    3. **Registrar Comida** (`AddMealPage`): Permite armar una comida seleccionando alimentos y especificando gramos consumidos.
    4. **Nuevo Alimento** (`AddFoodPage`): Formulario para dar de alta alimentos ingresando sus valores nutricionales de fosforo, potasio, sodio, calcio, magnesio, hierro y zinc.
* **ViewModels**:
  * `FoodCatalogViewModel.cs`, `MealTrackingViewModel.cs`, `AddFoodViewModel.cs`, `AddMealViewModel.cs`.
  * Utilizan propiedades observables parciales compatibles con AOT/WinRT y comandos fuertemente tipados (`[RelayCommand]`).
* **Inyeccion de Dependencias (`MauiProgram.cs`)**:
  * Centraliza el registro de todas las dependencias en el contenedor de servicios de .NET.

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
