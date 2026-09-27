# Documentacion de Funcionalidades y Vistas - Plataforma de Escritorio
Proyecto: DietApp (.NET 10 MAUI para Windows y macOS)

---

## 1. Vision General de la Plataforma de Escritorio

DietApp en su edicion de escritorio esta optimizada para ejecutarse en entornos de trabajo en PC y estaciones de diseno nutricional (especialmente Windows 10/11 mediante Windows App SDK / WinUI 3 y macOS mediante Mac Catalyst). Proporciona un entorno robusto, de alta productividad y totalmente desconectado (offline-first), enfocado en el diseno culinario, la auditoria de ingesta clinica y el analisis de micronutrientes y proteinas.

### Caracteristicas Tecnicas en Escritorio
* Framework base: .NET 10 MAUI con destino net10.0-windows10.0.19041.0 (WinUI 3 / Windows App SDK) y compatibilidad con Mac Catalyst.
* Almacenamiento y Rendimiento: Base de datos SQLite relacional alojada en `%LOCALAPPDATA%\DietApp` (`FileSystem.AppDataDirectory`). Utiliza indices B-Tree optimizados en columnas de minerales y proteinas para consultas instantaneas sin latencia ni cuellos de botella en discos SSD/NVMe.
* Arquitectura de Pantalla y Ventana: Ventana redimensionable libremente con controles nativos del sistema operativo (minimizar, maximizar, restaurar, pantalla completa) y persistencia de proporciones visuales.
* Interaccion Orientada a Raton y Teclado:
  - Navegacion con puntero, estados hover visuales en botones y tarjetas.
  - Desplazamiento fluido mediante la rueda del raton (`mouse wheel`) y barras de scroll visibles.
  - Captura rapida con teclado numerico fisico (Numpad) en campos de gramajes, miligramos y calorias, con navegacion por tabulacion accesible (`Tab` y `Shift + Tab`).
* Integracion con el Sistema de Archivos: Dialogo nativo de seleccion de archivos de Windows (`Windows File Open Picker`) para vincular fotografias de alta resolucion a recetas culinarias y pasos numerados directamente desde discos locales o unidades de red.
* Modo Claro y Modo Oscuro: Integracion con el tema global del sistema operativo o seleccion forzada manual mediante [AppThemeService](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Services/AppThemeService.cs).

---

## 2. Estructura de Navegacion en Escritorio

En entornos de escritorio, [AppShell.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/AppShell.xaml) adapta la barra de navegacion ([TabBar](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/AppShell.xaml#L19-L47)) a la parte superior de la ventana o en formato de barra lateral segun la resolucion y configuracion de la ventana, permitiendo alternar rapidamente entre modulos de trabajo:

```
AppShell (Barra Superior / Lateral de Navegacion en Escritorio)
├── Modulo 1: Conteo Diario (MealTrackingPage)
│   └── Ventana/Ruta de Registro: Registrar Comida (AddMealPage)
├── Modulo 2: Recetario Culinario (RecipesPage)
│   ├── Ficha Tecnica de Receta (RecipeDetailPage)
│   ├── Estudio de Creacion de Recetas (AddRecipePage)
│   └── Modulo de Condimentos y Alinos (SeasoningsPage)
├── Modulo 3: Catalogo Nutricional (FoodCatalogPage)
│   └── Formulario de Alta de Alimento (AddFoodPage)
└── Modulo 4: Configuracion y Alertas (SettingsPage)
```

---

## 3. Catalogo Detallado de Vistas en Escritorio

A continuacion se describen las 9 vistas de DietApp, detallando su disposicion espacial en pantallas de gran formato (1080p, 1440p, 4K), su aprovechamiento del ancho de pantalla y sus casos de uso en productividad de escritorio.

---

### Vista 1: Panel de Control y Conteo Diario
* Archivo XAML: [MealTrackingPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/MealTrackingPage.xaml)
* Code-Behind: [MealTrackingPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/MealTrackingPage.xaml.cs)
* ViewModel: [MealTrackingViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/MealTrackingViewModel.cs)
* Proposito en Escritorio: Funcionar como un tablero de control diario (Dashboard). Permite a medicos, nutricionistas o usuarios avanzados auditar el balance diario de nutrientes en una pantalla amplia mientras planifican la alimentacion semanal.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Selector de Fechas Centralizado:
   - Controles de navegacion de fecha con boton de dia anterior y dia siguiente accesibles mediante clic o atajo.
2. Banner de Alertas Clinicas Expandido:
   - Se despliega horizontalmente a lo ancho del contenedor, presentando las alertas preventivas (ambar) y de limites superados (rojo coral) con sus correspondientes badges ("AVISO PREVENTIVO", "LIMITE SUPERADO"), valor consumido, maximo permitido y exceso en mg.
3. Tablero de Resumen de Nutrientes del Dia:
   - La disposicion fluida (`FlexLayout Wrap="Wrap"`) se ensancha para exhibir todos los minerales (Fosforo, Potasio, Sodio, Calcio, Magnesio, Hierro, Zinc) en una sola fila armoniosa de pastillas informativas, junto al badge destacado de proteina acumulada (`DailyProteinDisplay`).
4. Matriz de Comidas del Dia:
   - Las tarjetas de comida se expanden horizontalmente, permitiendo leer simultaneamente los alimentos ingeridos con sus gramajes, el resumen nutricional (kcal y proteina) y el bloque consolidado de minerales de esa ingesta particular.
   - Boton de eliminacion de facil acceso mediante raton, con recalculacion instantanea de metricas en SQLite.
5. Accion de Alta Rapida:
   - Boton "+ Registrar Comida" situado en la barra de seccion, abriendo el compositor de comidas [AddMealPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddMealPage.xaml).

---

### Vista 2: Catalogo y Estudio de Recetas
* Archivo XAML: [RecipesPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipesPage.xaml)
* Code-Behind: [RecipesPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipesPage.xaml.cs)
* ViewModel: [RecipesViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/RecipesViewModel.cs)
* Proposito en Escritorio: Exploracion visual y filtrado de recetas planificadas con capacidad de visualizacion en alta densidad.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Barra Superior de Control y Filtros:
   - La barra de busqueda textual (`SearchBar`) y los selectores de ordenamiento por mineral y sentido se alinean de forma holgada sin comprecion.
   - Selector de ordenamiento avanzado para clasificar recetas segun su contenido por porcion de cualquier mineral o de proteinas (util para planificacion de dietas especificas).
2. Modulo Integrado de Alinos y Condimentos:
   - Panel banner con boton de accion "Ver Alinos" que permite transicionar fluidamente hacia el gestor [SeasoningsPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml).
3. Visualizacion de Tarjetas de Recetas:
   - En escritorio, la coleccion (`CollectionView`) aprovecha el ancho disponible mostrando fotografias panoramicas del plato terminado en alta definicion, subtitulos de calorias y proteinas, y filas completas de badges nutricionales.
   - Al hacer clic con el raton en cualquier tarjeta, se navega hacia la ficha tecnica [RecipeDetailPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipeDetailPage.xaml).

---

### Vista 3: Ficha Tecnica de Detalle de Receta (Vista A)
* Archivo XAML: [RecipeDetailPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipeDetailPage.xaml)
* Code-Behind: [RecipeDetailPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/RecipeDetailPage.xaml.cs)
* ViewModel: [RecipeDetailViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/RecipeDetailViewModel.cs)
* Proposito en Escritorio: Inspeccion exhaustiva de una preparacion culinaria segun los lineamientos de alta fidelidad de DESIGN.md (Vista A), permitiendo visualizar en un solo monitor amplio la imagen del plato, los ingredientes dosificados, la auditoria clinica y los pasos de elaboracion.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Hero Visual con Contenedores Superpuestos:
   - Imagen en alta resolucion del plato terminado con boton circular flotante 'X' de retroceso rapido en la esquina superior izquierda.
   - Barra flotante inferior oscura de alto impacto con el titulo de la receta y badge de energia por porcion (`kcal por porcion`).
2. Fila de Pildoras de Minerales Criticos:
   - Pastillas horizontales para Sodio, Potasio (destacada en Coral Orange `#ef713f`) y Fosforo.
3. Cuadricula Expandida de Ingredientes:
   - El contenedor flexible (`FlexLayout Wrap="Wrap"`) acomoda las tarjetas de ingredientes con fotografia, nombre y gramaje exacto aprovechando el ancho de la ventana de escritorio.
   - Desplegables para ordenar interactivamente los ingredientes segun el mineral aportado, permitiendo analizar de que alimento proviene la mayor carga de sodio, potasio o fosforo.
4. Banner de Auditoria Clinica Preventiva:
   - Verifica si la porcion consumida individual o acumulada con el consumo del dia supera los limites fijados en los ajustes de la aplicacion.
5. Modulo de Registro en la Ingesta Diaria:
   - Permite seleccionar fecha, momento del dia y cantidad de porciones a consumir con entrada directa de teclado y confirmacion inmediata mediante el boton "Registrar Consumo".
6. Pasos Numerados de Elaboracion Culinaria:
   - Tarjetas legibles con formato editorial, instruccion detallada y fotografia de cada etapa.

---

### Vista 4: Modulo de Alinos, Componentes y Registro (Vista B)
* Archivo XAML: [SeasoningsPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml)
* Code-Behind: [SeasoningsPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SeasoningsPage.xaml.cs)
* ViewModel: [SeasoningsViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/SeasoningsViewModel.cs)
* Proposito en Escritorio: Gestion centralizada de alinos, vinagretas y condimentos (Vista B de DESIGN.md), con capacidad de seleccion rapida con raton y armado interactivo de mezclas personalizadas.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Carrusel Superior de Modulos Nutricionales:
   - Fila de tarjetas de resumen (plato base, postre, liquido) con badges de alerta y checks visuales de seleccion.
2. Filtro Horizontal por Categorias:
   - Segmentacion por pestanas (`Base`, `Alinos`, `Postre`, `Liquido`) activables mediante clic de raton.
3. Cuadricula de Seleccion de Alinos:
   - Tarjetas de componentes con fotografia del aliño y checkmark coral en la esquina superior derecha para el elemento activo.
4. Constructor Desplegable de Mezclas Personalizadas:
   - Formulario para nombrar el aliño, redactar su uso culinario y dosificar ingredientes en gramos con calculo instantaneo de minerales totales.
   - Lista borrador interactiva con eliminacion de items mediante clic en el boton 'X'.
5. Barra Inferior de Accion:
   - Barra fija inferior con boton "Agregar" (abre el panel formulador) y boton "Completado" (confirma la seleccion y retorna al flujo principal).

---

### Vista 5: Catalogo Nutricional y Filtrado de Alimentos
* Archivo XAML: [FoodCatalogPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/FoodCatalogPage.xaml)
* Code-Behind: [FoodCatalogPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/FoodCatalogPage.xaml.cs)
* ViewModel: [FoodCatalogViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/FoodCatalogViewModel.cs)
* Proposito en Escritorio: Herramienta de consulta y filtrado rapido sobre los mas de 360 alimentos de la base oficial USDA FoodData Central.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Panel de Filtros Multidimensional:
   - En pantallas anchas, los campos de busqueda por nombre, selector de mineral (Fosforo, Potasio, Sodio, etc.), umbrales minimo y maximo en miligramos y selector de ordenamiento se presentan alineados con gran claridad.
   - Entrada rapida de rangos numericos con teclado fisico.
2. Boton "+ Nuevo Alimento":
   - Permite dar de alta alimentos adicionales abriendo [AddFoodPage](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml).
3. Lista de Resultados:
   - Tarjetas amplias con nombre del alimento, badge de proteina, porcion de referencia y pastillas con los valores exactos de los 7 minerales cuantificados en miligramos.

---

### Vista 6: Formulario de Alta de Alimentos
* Archivo XAML: [AddFoodPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml)
* Code-Behind: [AddFoodPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddFoodPage.xaml.cs)
* ViewModel: [AddFoodViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddFoodViewModel.cs)
* Proposito en Escritorio: Carga eficiente y categorizacion de nuevos alimentos en la base de datos local SQLite.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Formulario de Datos Generales:
   - Nombre, categoria, porcion base (100g), calorias (kcal) y proteinas (g).
2. Matriz Estructurada de Minerales:
   - Formulario organizado en 2 columnas balanceadas para Fósforo, Potasio, Sodio, Calcio, Magnesio, Hierro y Zinc.
   - Facilidad de tabulacion continua (`Tab`) entre campos numericos para agilizar la transcripcion de tablas nutricionales o etiquetas de productos comerciales.
3. Boton de Guardado:
   - Valida e inserta el nuevo alimento en SQLite actualizando de inmediato los indices B-Tree.

---

### Vista 7: Compositor de Comidas
* Archivo XAML: [AddMealPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddMealPage.xaml)
* Code-Behind: [AddMealPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddMealPage.xaml.cs)
* ViewModel: [AddMealViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddMealViewModel.cs)
* Proposito en Escritorio: Registro combinado y rapido de comidas completas con soporte mixto para alimentos e ingestas basadas en recetas.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Parametros de la Ingesta:
   - Seleccion de fecha mediante calendario nativo, selector de momento (`MealType`) y campo de notas.
2. Dos Secciones de Agregacion Simultanea:
   - Seccion Alimentos: selector de alimento del catalogo mas gramaje consumido.
   - Seccion Recetas: selector de recetas preexistentes mas numero de raciones consumidas.
3. Tabla de Elementos Seleccionados:
   - Lista clara con nombre, cantidad y boton de remocion por cada item anadido.
4. Boton Guardar Comida:
   - Consolida en una sola transaccion SQLite la comida y todos sus items asociados.

---

### Vista 8: Estudio de Creacion de Recetas Culinarias
* Archivo XAML: [AddRecipePage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddRecipePage.xaml)
* Code-Behind: [AddRecipePage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/AddRecipePage.xaml.cs)
* ViewModel: [AddRecipeViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/AddRecipeViewModel.cs)
* Proposito en Escritorio: Entorno de autor de recetas culinarias con dosificacion de ingredientes, incorporacion de aliños preconfigurados y redaccion de procedimientos paso a paso.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Datos Principales y Selector de Fotografia:
   - Titulo, descripcion y raciones.
   - Boton "Examinar" que invoca el dialogo de explorador de archivos nativo de Windows (`File Open Picker`) para buscar fotografias del plato terminado en carpetas del disco duro.
2. Dosificacion de Ingredientes:
   - Selector de alimentos con campo de gramos.
   - Selector de aliños guardados: permite descomponer e incorporar en lote todos los componentes de una marinada o vinagreta a la receta con un solo clic.
   - Lista visual de ingredientes con boton de eliminacion individual.
3. Redaccion y Fotografia de Pasos:
   - Editor de texto amplio para las instrucciones y explorador de archivos para asociar imagenes a cada paso.
4. Guardado Relacional:
   - Persistencia completa en las tablas relacionales `Recipes`, `RecipeIngredients` y `RecipeSteps`.

---

### Vista 9: Configuracion de Sistema, Temas y Alertas
* Archivo XAML: [SettingsPage.xaml](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SettingsPage.xaml)
* Code-Behind: [SettingsPage.xaml.cs](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/Views/SettingsPage.xaml.cs)
* ViewModel: [SettingsViewModel](file:///e:/GitHub_desktop/DietApp/src/DietApp.UI/ViewModels/SettingsViewModel.cs)
* Proposito en Escritorio: Centro de calibracion clinica y adaptacion visual del entorno de trabajo.

#### Comportamiento y Disposicion en Pantallas Anchas
1. Selector de Idioma (i18n):
   - Tarjetas interactivas para alternar entre Espanol e Ingles con impacto inmediato en todas las ventanas y pestañas de la aplicacion.
2. Selector de Tema Visual:
   - Opciones para Modo Claro, Modo Oscuro y Sincronizacion con el Tema de Windows.
3. Matriz de Alertas y Limites Maximos de Minerales:
   - Control deslizante (`Slider`) para definir el porcentaje de advertencia temprana (del 50% al 95%).
   - Lista completa de los 7 minerales con campos numericos para ingresar el limite maximo en miligramos e interruptores (`Switch`) para habilitar o deshabilitar cada alarma de forma individual.
   - Botones "Guardar Limites" y "Restablecer Valores".
4. Informacion del Entorno:
   - Detalle de la version (.NET 10 MAUI para Windows), motor SQLite y procedencia del dataset USDA FoodData Central.

---

## 4. Matriz Resumen de Funcionalidades en Escritorio

| Funcionalidad | Vistas Involucradas | Beneficio / Implementacion en Escritorio |
|---|---|---|
| Tablero de Control Nutricional Diario | MealTrackingPage | Vision panoramica del acumulado de 7 minerales y proteinas en pantalla completa |
| Auditoria Clinica de Recetas (Vista A) | RecipeDetailPage | Analisis de impacto antes del consumo con alertas visuales de limites |
| Diseno de Recetas e Ingredientes | AddRecipePage | Seleccion de fotos con Windows File Picker y dosificacion rapida con teclado |
| Modulo de Alinos y Componentes (Vista B) | SeasoningsPage | Seleccion interactiva con raton y barra de accion fija |
| Busqueda y Filtrado Clinico de Alimentos | FoodCatalogPage | Consulta SQL indexada de alto rendimiento sobre disco local SSD |
| Composicion Mixta de Ingestas | AddMealPage | Combinacion de alimentos sueltos y raciones de recetas en un solo registro |
| Cambio Dinamico de Idioma (ES / EN) | Todas las vistas | Reevaluacion instantanea de bindings XAML sin reiniciar la aplicacion |
| Soporte de Modo Oscuro / Modo Claro | Todas las vistas | Integracion con el esquema de colores de Windows (WinUI 3) |
| Captura de Datos por Teclado Fisico | AddFoodPage, AddMealPage, AddRecipePage, SettingsPage | Navegacion accesible mediante tecla Tab y soporte de teclado numerico |
