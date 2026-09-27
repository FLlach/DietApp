# Documentacion de Funcionalidades y Vistas - Plataforma Android
Proyecto: DietApp (.NET 10 MAUI)

---

## 1. Vision General de la Plataforma Android

DietApp para Android esta concebida como una aplicacion movil ergonomica, reactiva y totalmente offline-first, orientada al registro, control y seguimiento clinico-nutricional de la ingesta de alimentos con foco especializado en 7 minerales criticos (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc), proteinas (g) y valor calorico (kcal).

### Caracteristicas Tecnicas en Android
* Framework base: .NET 10 MAUI con destino net10.0-android (API 21 a API 35).
* Persistencia local: SQLite relacional indexado mediante [SqliteFoodRepository](file:///e:/GitHub_desktop/DietApp/src/DietApp.Infrastructure/Repositories/SqliteFoodRepository.cs) y [DietAppDbContext](file:///e:/GitHub_desktop/DietApp/src/DietApp.Infrastructure/Data/DietAppDbContext.cs) ubicado en el almacenamiento protegido de la aplicacion (`FileSystem.AppDataDirectory`).
* Pre-carga de datos: 363 alimentos fundacionales de USDA FoodData Central con 383 porciones caseras y codigos nutricionales oficiales.
* Ergonomia tactil: Objetivos de pulsacion con altura minima de 44dp / 48dp, espaciados tactiles, carruseles deslizables por gestos y teclados virtuales numericos especializados.
* Navegacion: Shell inferior nativo ([AppShell](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/AppShell.xaml)) de 4 pestanas nucleares accesibles con el pulgar mas rutas modulares apiladas en la pila de navegacion.
* Temas: Soporte reactivo de Modo Claro, Modo Oscuro y sincronizacion con el sistema operativo de Android ([AppThemeService](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Services/AppThemeService.cs)).

---

## 2. Mapa de Navegacion y Rutas en Android

La aplicacion organiza su flujo movil mediante un patron jerarquico basado en [AppShell.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/AppShell.xaml):

```
AppShell (Barra Inferior de 4 Pestanas)
├── Pestana 1: Conteo Diario (MealTrackingPage)
│   └── Ruta Modal: Registrar Comida (AddMealPage)
├── Pestana 2: Recetas (RecipesPage)
│   ├── Ruta Detalle: Ficha de Receta (RecipeDetailPage)
│   ├── Ruta Creacion: Crear Receta (AddRecipePage)
│   └── Ruta Modulo: Alinos y Componentes (SeasoningsPage)
├── Pestana 3: Catalogo (FoodCatalogPage)
│   └── Ruta Creacion: Alta de Alimento (AddFoodPage)
└── Pestana 4: Ajustes (SettingsPage)
```

---

## 3. Catalogo Detallado de Vistas en Android

A continuacion se detallan las 9 vistas que componen la interfaz movil de DietApp, especificando sus componentes XAML, ViewModels asociados, adaptaciones tactiles y funcionalidades operativas.

---

### Vista 1: Conteo Diario de Ingesta
* Archivo XAML: [MealTrackingPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/MealTrackingPage.xaml)
* Code-Behind: [MealTrackingPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/MealTrackingPage.xaml.cs)
* ViewModel: [MealTrackingViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/MealTrackingViewModel.cs)
* Tipo de Navegacion: Pestana primaria del TabBar inferior (Ruta: `MealTrackingPage`).
* Proposito en Movil: Es la pantalla de inicio habitual del usuario. Permite inspeccionar en un solo vistazo el acumulado diario de minerales y proteinas, recibir alertas clinicas inmediatas y desglosar cada comida del dia.

#### Componentes Visuales y Adaptacion Tactil
1. Barra superior de navegacion por fechas:
   - Botones tactiles para retroceder o avanzar de dia con altura de 44dp (`GoToPreviousDayCommand`, `GoToNextDayCommand`).
   - Indicador central de la fecha seleccionada en formato extendido.
2. Banner de Alertas y Avisos Preventivos:
   - Contenedor con borde redondeado y fondo contrastante que se activa dinamicamente si el consumo alcanza el umbral preventivo (ambar) o rebasa el 100% del maximo fijado (rojo coral).
   - Lista vertical con chips de severidad ("AVISO PREVENTIVO" / "LIMITE SUPERADO"), identificador del mineral, exceso en miligramos y porcentaje alcanzado.
3. Tarjeta de Resumen Total del Dia:
   - Bloque superior con badge verde destacado que exhibe los gramos totales de proteina consumidos (`DailyProteinDisplay`).
   - Coleccion fluida envuelta (`FlexLayout Wrap="Wrap"`) con pastillas nutricionales para los 7 minerales cuantificables en miligramos (Fosforo, Potasio, Sodio, Calcio, Magnesio, Hierro, Zinc).
4. Listado Vertical de Comidas Ingeridas:
   - Tarjetas independientes para cada comida (Desayuno, Almuerzo, Cena, Snack, Otro).
   - Cabecera con nombre del momento y resumen nutricional (kcal y proteina).
   - Boton rojo tactil de eliminacion individual con altura de 44dp y confirmacion inmediata.
   - Lista indentada con viñetas de cada alimento consumido y gramaje ingerido.
   - Desglose de minerales aportados por cada comida especifica en pastillas compactas.
5. Boton de Accion Principal (+ Registrar Comida):
   - Ubicado junto al encabezado de comidas para registrar nuevas ingestas sin necesidad de scroll prolongado.
6. Estado Vacio:
   - Ilustracion textual y boton directo para registrar la primera ingesta cuando no existen registros en la fecha consultada.

---

### Vista 2: Catalogo de Recetas Culinarias
* Archivo XAML: [RecipesPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipesPage.xaml)
* Code-Behind: [RecipesPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipesPage.xaml.cs)
* ViewModel: [RecipesViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/RecipesViewModel.cs)
* Tipo de Navegacion: Pestana primaria del TabBar inferior (Ruta: `RecipesPage`).
* Proposito en Movil: Explorar preparaciones culinarias planificadas, realizar busquedas textuales, ordenar segun la concentracion de cualquier nutriente por porcion y acceder al modulo de alinos o al alta de recetas.

#### Componentes Visuales y Adaptacion Tactil
1. Cabecera fija superior:
   - Titulo de seccion y boton "+ Nueva Receta" que navega a [AddRecipePage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddRecipePage.xaml).
   - Barra de busqueda nativa (`SearchBar`) con filtrado reactivo por texto.
2. Controles de Ordenamiento Nutricional:
   - Dos desplegables tactiles (`Picker`): uno para seleccionar el mineral objetivo (Fosforo, Potasio, Sodio, Calcio, Magnesio, Hierro, Zinc o Proteina) y otro para la direccion (Mayor a menor o Menor a mayor).
   - Permite a pacientes con restricciones clinicas encontrar recetas bajas en sodio/potasio o a deportistas buscar platos hiperproteicos.
3. Seccion Integrada de Alinos y Condimentos:
   - Tarjeta destacada con borde de marca y boton secundario "Ver Alinos" que conduce a [SeasoningsPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml).
4. Coleccion Vertical de Recetas (`CollectionView`):
   - Tarjetas con bordes redondeados (14dp), fotografia culinaria de alta resolucion, titulo en negrita y subtitulo nutricional automatico (kcal y proteina por porcion).
   - Fila de pastillas nutricionales en la base de la tarjeta con el desglose de minerales aportados por porcion.
   - Tap interactivo sobre la tarjeta para abrir la ficha tecnica detallada.

---

### Vista 3: Ficha de Detalle de Receta (Vista A)
* Archivo XAML: [RecipeDetailPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipeDetailPage.xaml)
* Code-Behind: [RecipeDetailPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipeDetailPage.xaml.cs)
* ViewModel: [RecipeDetailViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/RecipeDetailViewModel.cs)
* Tipo de Navegacion: Ruta secundaria apilada (Ruta: `RecipeDetailPage`).
* Proposito en Movil: Visualizar todos los detalles de una receta siguiendo la identidad visual oficial (Vista A de DESIGN.md), consultar ingredientes en cuadricula 2x2, auditar advertencias clinicas y registrar raciones directamente en el diario.

#### Componentes Visuales y Adaptacion Tactil
1. Hero Fotografico Superior con Contenedores Flotantes:
   - Imagen en alta resolucion del plato terminado con altura fija de 250dp y esquinas redondeadas de 20dp.
   - Boton circular flotante translucido 'X' (44dp x 44dp, esquinas 22dp) en la esquina superior izquierda para cerrar la vista con un toque del pulgar (`Shell.Current.GoToAsync("..")`).
   - Barra inferior flotante oscura superpuesta sobre el borde de la foto con el titulo del plato y badge de energia en color verde lima (`kcal por porcion`).
2. Fila de Pildoras de Nutrientes Criticos:
   - Tres pastillas distribuidas uniformemente: Sodio (neutra), Potasio (destacada en color Coral Orange `#ef713f`) y Fosforo (neutra).
3. Cuadricula 2x2 de Ingredientes Dosificados:
   - Tarjetas compactas en dos columnas (`FlexLayout` con `Basis="48%"`) con bordes calidos, fotografia del alimento base, nombre y gramaje exacto.
   - Desplegable tactil inferior para ordenar los ingredientes de la receta segun la cantidad de cualquier mineral aportado.
4. Banner de Auditoria Clinica Proactiva:
   - Evalua en tiempo real si consumir una porcion excede el limite diario del usuario o si sumada al consumo acumulado del dia seleccionado alcanza el umbral de advertencia preventivo.
5. Resumen Completo de Minerales por Porcion:
   - Panel desplegado con las 7 concentraciones de minerales en miligramos.
6. Modulo de Registro en Ingesta Diaria:
   - Selector de fecha nativo de Android (`DatePicker`).
   - Selector de momento de comida (`Picker` para Desayuno, Almuerzo, Cena, Snack).
   - Campo numerico para cantidad de raciones consumidas.
   - Boton verde lima "Registrar Consumo" con calculo proporcional automatico.
7. Pasos Numerados de Elaboracion:
   - Tarjetas individuales por paso con insignia numerada, texto de instruccion e imagen ilustrativa de la tecnica culinaria.

---

### Vista 4: Modulo de Alinos, Componentes y Registro (Vista B)
* Archivo XAML: [SeasoningsPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml)
* Code-Behind: [SeasoningsPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml.cs)
* ViewModel: [SeasoningsViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/SeasoningsViewModel.cs)
* Tipo de Navegacion: Ruta secundaria apilada (Ruta: `SeasoningsPage`).
* Proposito en Movil: Administrar condimentos, vinagretas y marinadas preconfiguradas segun el prototipo visual oficial (Vista B de DESIGN.md), con seleccion interactiva mediante toques y barra de accion fija al alcance del pulgar.

#### Componentes Visuales y Adaptacion Tactil
1. Carrusel Superior Horizontal de Modulos del Menu:
   - Desplazamiento horizontal fluido (`ScrollView Orientation="Horizontal"`) con tarjetas compactas (145dp de ancho) que muestran el plato principal (con badges de alerta), postre y liquido (con badges de check).
2. Segmentacion por Categorias:
   - Pildoras horizontales desplazables para filtrar entre `Base`, `Alinos`, `Postre` y `Liquido`.
3. Cuadricula 2x2 de Seleccion Interactiva:
   - Tarjetas cuadradas con bordes calidos, fotografia del aliño y badge circular de check coral en la esquina superior derecha que se ilumina al tocar el elemento para marcarlo como seleccionado.
4. Panel Desplegable de Creacion de Alino Personalizado:
   - Formulario para nombrar el aliño, ingresar descripcion y anadir alimentos dosificados en gramos mediante un selector y campo numerico.
   - Lista borrador de ingredientes con botones tactiles de eliminacion antes de guardar en SQLite.
5. Listado de Alinos Guardados en Base de Datos:
   - Tarjetas con nombre, descripcion, calorias totales y boton de borrado persistente.
6. Barra Inferior Fija de Acciones (Bottom Action Bar):
   - Contenedor oscuro flotante anclado en la parte inferior de la pantalla con dos botones pildora de 44dp:
     - "Agregar" (fondo blanco con borde oscuro) para abrir o cerrar el panel constructor.
     - "Completado" (fondo verde lima suave `#bed35a`) para confirmar la seleccion activa y cerrar.

---

### Vista 5: Catalogo y Filtrado Avanzado de Alimentos
* Archivo XAML: [FoodCatalogPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/FoodCatalogPage.xaml)
* Code-Behind: [FoodCatalogPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/FoodCatalogPage.xaml.cs)
* ViewModel: [FoodCatalogViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/FoodCatalogViewModel.cs)
* Tipo de Navegacion: Pestana primaria del TabBar inferior (Ruta: `FoodCatalogPage`).
* Proposito en Movil: Consultar la base de datos fundacional de alimentos, aplicar filtros avanzados por rangos de nutrientes (especial para planes medicos) y acceder al registro de nuevos alimentos.

#### Componentes Visuales y Adaptacion Tactil
1. Panel de Filtrado Clinico y Ordenamiento:
   - Campo de busqueda por nombre con soporte de teclado virtual.
   - Desplegable de seleccion de nutriente: Fosforo, Potasio, Sodio, Calcio, Magnesio, Hierro, Zinc o Proteina.
   - Entradas de texto numericas compactas para valores minimos y maximos en miligramos o gramos.
   - Desplegable de ordenamiento: Por Nombre (A-Z), Por Proteina (Mayor a menor / Menor a mayor).
   - Botones tactiles para limpiar filtros o aplicar la consulta SQL indexada.
2. Boton de Accion Rapida (+ Nuevo Alimento):
   - Conduce directamente a [AddFoodPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml).
3. Lista Optimizada de Alimentos (`CollectionView`):
   - Tarjetas individuales con nombre del alimento, badge verde de aporte proteico (`{X} g proteina`), porcion base de referencia (ej. 100g) y valor calorico.
   - Contenedor fluido con chips de cada mineral cuantificado en miligramos.

---

### Vista 6: Alta de Alimentos
* Archivo XAML: [AddFoodPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml)
* Code-Behind: [AddFoodPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml.cs)
* ViewModel: [AddFoodViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddFoodViewModel.cs)
* Tipo de Navegacion: Ruta secundaria modal/apilada (Ruta: `AddFoodPage`).
* Proposito en Movil: Incorporar alimentos personalizados al catalogo local SQLite con su desglose mineral completo.

#### Componentes Visuales y Adaptacion Tactil
1. Formulario de Datos Generales:
   - Entradas para nombre del alimento, categoria, porcion de referencia (100g por defecto), calorias (kcal) y proteinas (g).
2. Cuadricula 2 Columnas de Minerales:
   - Entradas numericas con teclado virtual especializado (`Keyboard="Numeric"`) para los 7 minerales en miligramos.
3. Boton Inferior de Guardado:
   - Boton de altura completa (48dp) de facil acceso para el pulgar con validacion de entradas.

---

### Vista 7: Compositor de Comidas
* Archivo XAML: [AddMealPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddMealPage.xaml)
* Code-Behind: [AddMealPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddMealPage.xaml.cs)
* ViewModel: [AddMealViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddMealViewModel.cs)
* Tipo de Navegacion: Ruta secundaria modal/apilada (Ruta: `AddMealPage`).
* Proposito en Movil: Armar una ingesta completa combinando alimentos sueltos dosificados en gramos y recetas culinarias dosificadas en raciones.

#### Componentes Visuales y Adaptacion Tactil
1. Datos de la Comida:
   - Selector de fecha nativo de Android (`DatePicker`), selector de momento de ingesta (`MealType`) y nota opcional.
2. Incorporacion de Alimentos:
   - Desplegable de alimentos y campo numerico en gramos con boton "+".
3. Incorporacion de Recetas:
   - Desplegable de recetas disponibles y campo numerico de raciones consumidas con boton "+".
4. Lista de Elementos en el Borrador:
   - Muestra cada alimento o receta agregada con su gramaje y un boton rojo tactil de eliminacion.
5. Boton de Guardado:
   - Boton de 48dp de altura que ejecuta `SaveMealCommand`, consolidando la instantanea de nutrientes en SQLite y retornando al diario.

---

### Vista 8: Compositor de Recetas Culinarias
* Archivo XAML: [AddRecipePage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddRecipePage.xaml)
* Code-Behind: [AddRecipePage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddRecipePage.xaml.cs)
* ViewModel: [AddRecipeViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddRecipeViewModel.cs)
* Tipo de Navegacion: Ruta secundaria modal/apilada (Ruta: `AddRecipePage`).
* Proposito en Movil: Crear preparaciones culinarias completas con ingredientes dosificados, pasos ilustrados y fotografia final.

#### Componentes Visuales y Adaptacion Tactil
1. Informacion Basica:
   - Titulo, descripcion y numero de raciones resultantes.
2. Selector de Imagen Final:
   - Campo de ruta con boton "Examinar" que invoca el selector de archivos multimedia de Android (`FilePicker`) para elegir fotos desde la galeria o camara del telefono.
3. Seccion de Ingredientes e Integracion de Alinos:
   - Dosificacion de ingredientes individuales en gramos.
   - Opcion rapida para incorporar un aliño guardado: al seleccionar el aliño, se descomponen y anaden automaticamente todos sus ingredientes a la receta en un solo paso.
   - Lista borrador de ingredientes con botones de eliminacion.
4. Pasos Numerados de Preparacion:
   - Editor de texto para la explicacion del paso y selector multimedia para vincular una fotografia a la etapa.
   - Lista de pasos agregados.
5. Boton de Persistencia:
   - Boton principal "Guardar Receta" que almacena la estructura relacional completa en SQLite.

---

### Vista 9: Configuracion y Ajustes
* Archivo XAML: [SettingsPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SettingsPage.xaml)
* Code-Behind: [SettingsPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SettingsPage.xaml.cs)
* ViewModel: [SettingsViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/SettingsViewModel.cs)
* Tipo de Navegacion: Pestana primaria del TabBar inferior (Ruta: `SettingsPage`).
* Proposito en Movil: Ajustar el idioma de la app, conmutar el tema visual entre claro/oscuro y configurar limites diarios de minerales con alertas preventivas.

#### Componentes Visuales y Adaptacion Tactil
1. Selector de Idioma (Espanol / Ingles):
   - Dos tarjetas tactiles con banderas estilizadas ("ES" y "EN") y botones de seleccion que aplican cambios instantaneos en toda la interfaz sin recargar la app.
2. Selector de Tema Visual:
   - Tres botones de seleccion: Modo Claro, Modo Oscuro y Modo Sistema (sincronizado con el modo oscuro de Android).
3. Configuracion de Limites y Alertas Clinicas de Minerales:
   - Control deslizante (`Slider`) para calibrar el umbral preventivo (50% a 95%, por defecto 80%) con indicador visual en tiempo real.
   - Lista vertical de los 7 minerales cuantificables con entrada de texto numerica para el limite maximo diario en miligramos y un interruptor tactil (`Switch`) de Android para activar o desactivar la alarma de cada mineral de forma independiente.
   - Boton "Guardar Limites" y boton "Restablecer Valores".
4. Informacion del Sistema:
   - Tarjeta informativa con version de la aplicacion, motor SQLite y dataset USDA.

---

## 4. Matriz Resumen de Funcionalidades en Android

| Funcionalidad | Vistas Involucradas | Componentes / Servicios Clave |
|---|---|---|
| Conteo y Balance Diario de Minerales | MealTrackingPage | DailyMineralAggregatorService, MealTrackingService |
| Cuantificacion de Proteinas | MealTrackingPage, FoodCatalogPage, RecipesPage, RecipeDetailPage | FoodItem.CalculateProteinForPortion, DailyMinerals |
| Sistema de Alertas Preventivas y Excesos | MealTrackingPage, RecipeDetailPage, SettingsPage | MineralAlertService, MauiPreferencesMineralAlertStorage |
| Modulo de Recetas con Auditoria Clinica | RecipesPage, RecipeDetailPage, AddRecipePage | RecipeService, SqliteRecipeRepository |
| Modulo de Alinos y Componentes | SeasoningsPage, AddRecipePage | SeasoningService, SqliteSeasoningRepository |
| Filtrado y Busqueda Avanzada de Catalogo | FoodCatalogPage | FoodCatalogService, SqliteFoodRepository |
| Registro Mixto de Comidas (Alimentos + Recetas) | AddMealPage, MealTrackingPage | MealTrackingService, SqliteMealRepository |
| Internacionalizacion Dinamica (ES / EN) | Todas las vistas | LocalizationService, LocalizationResourceManager |
| Modo Oscuro / Claro / Sistema | Todas las vistas | AppThemeService, AppThemeBinding |
| Seleccion de Fotos en Galeria Android | AddRecipePage, RecipeDetailPage | Microsoft.Maui.Storage.FilePicker |
