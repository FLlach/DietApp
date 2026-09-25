# Guia de Diseno Visual - DietApp

## 1. Identidad del Producto
DietApp es una aplicacion de seguimiento nutricional y control de ingesta orientada a la precision cuantitativa de minerales (fosforo, potasio, sodio, calcio, magnesio, hierro y zinc) y balance calorico. Su proposito es servir como herramienta confiable tanto para dietas clinicas (renales, cardiovasculares) como para requerimientos deportivos o de bienestar general.

## 2. Personalidad
- **Sobria y Clinica**: La prioridad es la exactitud de los datos y la comprension rapida de los nutrientes.
- **Transparente**: Cada numero presentado proviene de fuentes verificables (USDA FoodData Central) o de formulas directas de agregacion.
- **Funcional**: Cero elementos decorativos superfluos que entorpezcan la lectura de porciones y gramos.

## 3. Cuadrantes y Diales de Diseno (Antislop)
- **ENERGY 1 (Calm)**: Enfoque institucional y medico, sin contrastes estridentes ni busqueda de efectos llamativos.
- **RHYTHM 2 (Balanced)**: Jerarquia estructurada con tarjetas de metricas, tablas de ingredientes y listas de pasos con separadores limpios.
- **MOTION 1 (Calm)**: Movimiento restringido a retroalimentacion de acciones del usuario (toques, navegacion entre vistas y despliegue de alertas).

```text
Dial: ENERGY 1 / RHYTHM 2 / MOTION 1
```

## 4. Paleta de Colores
Diseñada para comunicar salud y precision, evitando gradientes artificiales o esquemas purpura genericos:

- **Color Primario (Salud / Clinico)**: `#0F766E` (Teal oscuro) / `#14B8A6` (Modo oscuro).
- **Color Secundario (Estructura)**: `#334155` (Slate neutro) para encabezados y bordes sutiles.
- **Fondos Modo Claro**: `#F8FAFC` (Superficie general) y `#FFFFFF` (Tarjetas de datos).
- **Fondos Modo Oscuro**: `#0F172A` (Superficie general) y `#1E293B` (Tarjetas de datos).
- **Texto Principal**: `#0F172A` en modo claro / `#F8FAFC` en modo oscuro (Cumplimiento estricto WCAG AA contrast ratio > 4.5:1).
- **Texto Secundario**: `#64748B` en modo claro / `#94A3B8` en modo oscuro.
- **Acento de Alerta (Exclusivo para umbrales excedidos)**: `#DC2626` (Rojo clinico) y `#D97706` (Ambar preventivo). No usar como decoracion.

## 5. Tipografia
- Tipografia de sistema de alta legibilidad: `Segoe UI` (Windows), `Roboto` (Android), `.AppleSystemUIFont` (iOS/macOS).
- Enfatizar cifras numericas y unidades (`mg`, `kcal`, `g`) con pesos seminegrita para facilitar el escaneo visual inmediato.
- Jerarquia tipografica clara: Titulo de pagina (20-24pt semi-bold), subtitulos de seccion (16pt medium), cuerpo de datos (14pt regular), etiquetas de unidades (12pt regular).

## 6. Principios de Maquetacion y Accesibilidad
- **Tamanos de toque minimos**: Todo boton y elemento interactivo debe contar con un area minima de 44x44 dp/px.
- **Sin estados ciegos**: Todo listado o vista de detalle debe soportar estado vacio explicativo, estado de carga y estado de error.
- **Sin controles muertos**: No se incluyen botones o enlaces que no ejecuten una accion real en la aplicacion.
- **Sin emojis**: Mantener la comunicacion formal y profesional en todas las cadenas de texto y vistas.
