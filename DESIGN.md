# Guia de Diseno Visual - DietApp

## 1. Identidad del Producto
DietApp es una aplicacion de seguimiento nutricional y control de ingesta orientada a la precision cuantitativa de minerales (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc) y balance calorico. Su proposito es servir como herramienta confiable tanto para dietas clinicas (renales, cardiovasculares) como para requerimientos deportivos o de bienestar general.

## 2. Personalidad
- **Sobria y Clinica**: La prioridad es la exactitud de los datos y la comprension rapida de los nutrientes.
- **Transparente**: Cada numero presentado proviene de fuentes verificables (USDA FoodData Central) o de formulas directas de agregacion.
- **Funcional**: Cero elementos decorativos superfluos que entorpezcan la lectura de porciones y gramos.
- **Accesible y Contrastada**: Elementos interactivos amplios, bordes nitidos y jerarquia visual sin ambiguedades.

---

## 3. Cuadrantes y Diales de Diseno (Antislop)
- **ENERGY 1 (Calm)**: Enfoque institucional y medico, sin contrastes estridentes ni busqueda de efectos artificiales o gradientes innecesarios.
- **RHYTHM 2 (Balanced)**: Jerarquia estructurada con tarjetas de metricas, tablas de ingredientes y listas de pasos con separadores limpios.
- **MOTION 1 (Calm)**: Movimiento restringido a retroalimentacion de acciones del usuario (toques, navegacion entre vistas y despliegue de alertas).

```text
Dial: ENERGY 1 / RHYTHM 2 / MOTION 1
```

---

## 4. Paletas de Color

El sistema visual contempla una paleta base oficial solicitada para el producto junto con dos propuestas de exploracion cromatica que resuelven contrastes, estados y niveles de jerarquia.

### 4.1. Paleta Base (Solicitada)
Es el esquema principal del sistema, caracterizado por una atmosfera fresca y calida con acentos energeticos controlados:

| Rol Semantico | Muestra / Color | Codigo HEX | Uso en Interfaz |
| :--- | :--- | :--- | :--- |
| Acento Primario / Alerta | Naranja Coral | `#ef713f` | Pildoras de nutrientes criticos en alerta (ej. Potasio excedido o foco), checks de seleccion activa e indicadores clave. |
| Acento Secundario / Accion | Verde Lima Suave | `#bed35a` | Botones de accion principal completada (`Completado`), estados exitosos y validacion de balance. |
| Contraste / Texto Principal | Negro Profundo | `#0c0c0c` | Tipografia principal, iconos interactivos, bordes definidos y contenedor de botones oscuros. |
| Superficie / Fondo | Blanco Calido | `#fffaf8` | Fondos de pantalla, tarjetas de detalle y lienzos de contenido limpio. |

### 4.2. Propuesta #1 (Tonos Tierra y Organicos)
Variacion orientada a una experiencia sobria, natural y editorial:

| Rol Semantico | Muestra / Color | Codigo HEX | Uso en Interfaz |
| :--- | :--- | :--- | :--- |
| Acento Luminoso | Mostaza / Ocre Dorado | `#f2b742` | Cabeceras destacadas, barras superiores de resumen y alertas leves. |
| Tono Medio Vegetal | Verde Oliva | `#96a757` | Contenedores secundarios, fondos de pie de pagina y estados intermedios. |
| Fondo Estructural / Base | Verde Bosque Profundo | `#4d6a4e` | Pestanas de agrupacion, encabezados modulares y contrastes oscuros. |
| Neutro Oscuro | Antracita / Carbon | `#252525` | Textos de alta jerarquia y bordes estructurales. |
| Fondo Calido | Crema Vainilla | `#ffecc2` | Lienzos generales, fondos de tarjetas y superficies de lectura prolongada. |

### 4.3. Propuesta #2 (Luminosa y Citrica Suave)
Variacion enfocada en ligereza visual y division de modulos en bloques pastel:

| Rol Semantico | Muestra / Color | Codigo HEX | Uso en Interfaz |
| :--- | :--- | :--- | :--- |
| Acento Primario | Naranja Medio | `#f5913c` | Botones principales y advertencias de umbrales. |
| Acento Secundario | Salmone / Melocoton | `#f7c6a1` | Encabezados de tarjetas, bordes de elementos agrupados y etiquetas secundarias. |
| Acento de Balance | Verde Lima Brillante | `#d5e159` | Barra superior de navegacion, pildoras activas y modales flotantes. |
| Tono de Apoyo | Salvia Pastel / Lima Suave | `#e2e5ac` | Fondos de modulos secundarios y separadores. |
| Neutro Oscuro | Oliva Terroso / Musgo | `#3c3d2d` | Texto principal, iconografia y elementos de cierre. |
| Fondo Base | Marfil Claro | `#fef8e0` | Superficie general de la aplicacion y tarjetas limpias. |

---

## 5. Tipografias

El proyecto define una seleccion de tipografias de alto caracter display para encabezados y denominaciones de recetas, combinadas con tipografias de sistema para datos numericos y unidades clinicas.

### 5.1. Familias Tipograficas Especificadas
1. **Mango Grotesque Semi Bold**: Tipografia display ultra condensada de alto impacto visual. Ideal para nombres de recetas (`Receta ABCDE`), titulares de secciones (`Ingredientes`) y rotulos destacados.
2. **Saint Regus Semi Bold Condensed**: Alternativa display condensada de corte editorial y elegante, apta para titulos de fichas y encabezados de categorias.
3. **TBJ Neuetra Semi Bold**: Fuente geometrica de trazos limpios y estructura sobria, adecuada para layouts funcionales y esquemas arquitectonicos rigidos.
4. **Cotta Regular**: Fuente estilizada con remates calidos, idonea para acentos de identidad, nombres de platos de autor o subtitulos complementarios.

### 5.2. Tipografias de Sistema y Lectura de Datos
Para tablas nutricionales, balances de minerales, gramajes y datos clinicos, se utiliza la tipografia de sistema de la plataforma (`Segoe UI` en Windows, `Roboto` en Android, `.AppleSystemUIFont` en iOS/macOS) con pesos seminegrita en cifras y unidades (`mg`, `kcal`, `gr`, `ml`).

### 5.3. Jerarquia Tipografica
- **Titular Principal de Pantalla / Hero**: Mango Grotesque Semi Bold (24-32pt), mayusculas o estilo condensado ajustado.
- **Nombre de Receta / Plato**: Mango Grotesque Semi Bold (20-24pt).
- **Subtitulos de Categoria / Seccion**: Mango Grotesque Semi Bold o TBJ Neuetra Semi Bold (16-18pt).
- **Metricas y Gramajes**: Tipografia de sistema en Semi-Bold / Medium (14-16pt) para rapido escaneo visual.
- **Etiquetas de Pildora y Unidades**: Tipografia de sistema en Regular / Medium (11-13pt).

---

## 6. Ideas y Prototipos de Interfaz

Las propuestas visuales se estructuran a partir de tres exploraciones que articulan paletas de color, fuentes y composicion modular:

### 6.1. Exploracion Principal: Paleta Base + Tipografia Mango Grotesque
Implementacion de alta fidelidad que articula la paleta base (`#ef713f`, `#bed35a`, `#0c0c0c`, `#fffaf8`) con titulares en Mango Grotesque:

#### Vista A: Ficha de Detalle de Receta
1. **Hero Fotografia**:
   - Cabecera con fotografia real del plato (pechuga de pollo, arroz, brocoli, papas).
   - Boton superior circular translucidocon icono de cierre ('X') para retorno rapido.
   - Barra oscura flotante superpuesta sobre el borde inferior de la imagen: `Receta ABCDE` en tipografia Mango Grotesque junto a la indicacion energetica `500 kcal por porcion`.
2. **Pildoras de Nutrientes Criticos**:
   - Disposicion horizontal inmediata bajo el hero para escaneo clinico instantaneo.
   - Pildora neutral: `0,3 Sodio` (fondo blanco con contorno suave).
   - Pildora de acento/foco: `150 Potasio` (fondo naranja coral `#ef713f` con texto blanco para advertir o destacar el mineral preponderante).
   - Pildora neutral: `0,1 Fosforo` (fondo blanco con contorno suave).
3. **Seccion de Ingredientes**:
   - Titulo de seccion `Ingredientes` en Mango Grotesque.
   - Cuadricula regular 2x2 de tarjetas individuales de ingredientes sobre fondo blanco calido `#fffaf8` con borde fino:
     - Tarjeta 1: Imagen de papas + `Papas sin cascara` + gramaje `200 gr.`
     - Tarjeta 2: Imagen de arroz + `Arroz blanco` + gramaje `200 gr.`
     - Tarjeta 3: Imagen de brocoli + `Brocoli` + gramaje `200 gr.`
     - Tarjeta 4: Imagen de pollo + `Pollo` + gramaje `400 gr.`

#### Vista B: Seleccion de Componentes, Alinos y Registro
1. **Carrusel Superior de Resumen de Menu**:
   - Tarjetas modulares compactas en fila superior para visualizar los bloques de la ingesta:
     - Modulo Receta: `Receta ABCD`, `1 porcion`, desglose `23 Sodio`, `120 Potasio`, `0,1 Fosforo` con indicador de alerta superior.
     - Modulo Postre: `Postre`, `1 porcion`, `00 Fibra`, `110 Azucar` con indicador de confirmacion/check.
     - Modulo Liquido: `Liquido`, `Min. 200 ml`, `Agua Mineral`, `Jugo Natural`.
2. **Segmentacion por Categorias (Tabs)**:
   - Fila de pestanas horizontales limpias: `Base` | `Alinos` (activa) | `Postre` | `Postre`.
3. **Cuadricula de Seleccion de Alinos / Ingredientes**:
   - Cuadricula de tarjetas 2x2 con imagen de producto:
     - `#Personalizado` (con icono de check naranja `#ef713f` en la esquina superior derecha indicando seleccion activa).
     - `Aceite de Oliva`.
     - `Oregano`.
     - `Aceite de Canola`.
4. **Barra Fija de Acciones Inferiores**:
   - Boton secundario en forma de pildora: `Agregar` (fondo blanco, borde y texto negro `#0c0c0c`).
   - Boton primario de finalizacion: `Completado` (fondo verde lima suave `#bed35a`, texto negro `#0c0c0c` para maximo contraste y visibilidad).

---

### 6.2. Exploracion Estructural: Paleta 1 + Tipografia TBJ Neuetra
Enfoque modular y jerarquico basado en la Paleta 1 (`#f2b742`, `#96a757`, `#4d6a4e`, `#252525`, `#ffecc2`):
- **Barra Superior**: Franja continua en tono mostaza `#f2b742` para establecer anclaje visual.
- **Espacio Hero / Imagen**: Marco principal con cruce de encuadre para fotografias de alta resolucion.
- **Modulo Central de Accion**: Contenedor con pestana en verde bosque `#4d6a4e` sobre fondo crema `#ffecc2`.
- **Base / Pie de Contenedor**: Bloque en verde oliva `#96a757` que agrupa controles de navegacion o resumen diario.
- **Sensacion Visual**: Calida, terrosa y equilibrada, reforzada por la geometria sobria de TBJ Neuetra Semi Bold.

---

### 6.3. Exploracion Modular: Paleta 2 + Tipografia Mango Grotesque
Enfoque de tarjetas modulares y hojas de dialogo sobre la Paleta 2 (`#f5913c`, `#f7c6a1`, `#d5e159`, `#e2e5ac`, `#3c3d2d`, `#fef8e0`):
- **Encabezado Superior**: Barra en verde lima luminoso `#d5e159`.
- **Fila de Modulos de Seleccion**: Tres contenedores superiores con remates en color melon/melocoton `#f7c6a1`.
- **Listado de Registros**: Tarjetas alargadas con contornos en melocoton suave y pastillas de estado en verde lima `#d5e159`.
- **Ventana Modal / Sheet Flotante**: Panel emergente superpuesto en verde lima claro `#d5e159` sobre fondo marfil `#fef8e0` para confirmacion de porciones y ajustes rapidos.

---

## 7. Principios de Maquetacion, Accesibilidad y Usabilidad

1. **Areas de Toque Minimas**: Todos los botones (como `Agregar`, `Completado` y tarjetas de seleccion) deben contar con un area minima interactiva de 44x44 dp/px.
2. **Legibilidad de Minerales y Gramajes**: Las cifras numericas de sodio, potasio y fosforo deben destacarse siempre con suficiente contraste (WCAG AA ratio > 4.5:1) y sin ambiguedades de lectura.
3. **Ausencia de Estados Ciegos**: Toda lista o pantalla de detalle debe contemplar estado vacio explicativo, estado de carga y estado de error.
4. **Sin Controles Muertos ni Adornos Innecesarios**: Todo elemento visual cumple una funcion de orientacion, advertencia clinica o seleccion explicita.
5. **Politica Estricta Sin Emojis**: La aplicacion mantiene un tono profesional, clinico y sobrio. Se prohibe el uso de emojis en interfaces, cadenas de texto, notificaciones y documentacion.
