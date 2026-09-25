# Reglas e Instrucciones para Agentes - DietApp

## Filosofia Principal
> "Readability and Order > Speed and Complex Interconnections"

1. Asegurar siempre bajo acoplamiento y alta cohesion entre modulos.
2. Seguir estrictamente Domain-Driven Design (DDD) con separacion en cuatro capas (`DietApp.Domain`, `DietApp.Application`, `DietApp.Infrastructure`, `DietApp.UI`).
3. Mantener actualizado `.gitignore` ante cualquier directorio de datos sensibles o artefactos locales.
4. Realizar commits detallados entre funcionalidades para preservar un historial coherente.
5. Prohibido el uso de emojis en comentarios, codigo, commits y documentacion.
6. Emplear nombres de variables autoexplicativos y evitar lineas ambiguas.
7. Evitar duplicacion de codigo aprovechando funciones existentes sin crear dependencias cruzadas.
8. Documentar cada componente con la explicacion del como y el porque de cada decision tecnica.
9. Mantener la documentacion viva de funcionalidades principales en `docs.md`.

## Direccion Visual y Filtro de Diseno
Para tareas de interfaz grafica, redaccion, experiencia de usuario y maquetacion:
1. Consultar `DESIGN.md` para la identidad, paleta y lineamientos visuales de DietApp.
2. Aplicar el filtro de antislop para evitar patrones genericos de inteligencia artificial.
3. Modo de operacion de antislop seleccionado para este proyecto: **Auditoria (Modo 2)**. Los hallazgos se reportan en `anti-slop/` con prioridad numerada antes de ejecutar cambios.

<!-- antislop:start -->
## antislop
For UI, copy, people, mobile layout, or code comments work, read `antislop.md` (core) and then the skill for the task:
- UI / visual: `skills/antislop-ui/SKILL.md`
- Copy & text: `skills/antislop-copywriting/SKILL.md`
- People: `skills/antislop-human/SKILL.md`
- Mobile / responsive: `skills/antislop-layoutmobile/SKILL.md`
- Code comments: `skills/antislop-code/SKILL.md`
Before starting, ask the user when antislop applies: during the work, or after it is done.
To update antislop later: download `antislop.md` again, or run `npx antislop-ai --update` if it was installed as skill folders.
<!-- antislop:end -->
