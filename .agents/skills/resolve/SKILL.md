---
name: resolve
description: Resuelve un issue de GitHub de principio a fin ejecutando estrictamente el flujo de 5 pasos definido en AGENTS.md.
---

# Resuelve Issue (/resolve <numero>)

Ejecuta el flujo completo de 5 pasos descrito en `AGENTS.md` para el issue indicado:

## Pasos de Ejecucion

1. **Paso 1: Inspeccion y Validacion del Requerimiento**
   - Revisar y leer `MEMORY.md` primero para conocer el contexto, decisiones y trampas conocidas del proyecto.
   - Ejecutar en terminal: `gh issue view <numero>`
   - Extraer:
     - Objetivo de la funcionalidad o descripcion del fallo.
     - Criterios de Aceptacion (condiciones obligatorias de terminado).
     - Tablas, modelos, controladores o vistas XAML mencionadas.
     - Prioridad asignada (Critico, Alta, Media, Baja).

2. **Paso 2: Plan de Modificaciones (Obligatorio antes de programar)**
   - Responder al usuario con la propuesta estructurada antes de modificar cualquier archivo:
     - Archivos exactos a modificar o crear (`.cs`, `.xaml`, migraciones).
     - Logica tecnica de la solucion.
     - Verificacion de Criterios de Aceptacion.
     - Prueba de estres o caos segun la naturaleza del issue: Disenar un escenario extremo o caotico adaptado especificamente a lo que hace el issue para intentar romper la solucion (y convertirlo en prueba unitaria en el Paso 3).
     - Pausa de confirmacion: Preguntar al usuario "¿Deseas proceder con este plan de implementacion?".

3. **Paso 3: Implementacion y Compilacion**
   - Una vez confirmado el plan por el usuario:
     - Aplicar los cambios necesarios en el codigo fuente siguiendo Clean Code e incluir la prueba de estres/caos adaptada al issue.
     - Compilar y ejecutar pruebas con `dotnet build Sistema-Cafeteria.slnx` y `dotnet test Sistema-Cafeteria.slnx`.
     - Si la compilacion o las pruebas fallan, corregir hasta que todo pase con 0 errores y 0 advertencias.
     - Actualizar `MEMORY.md` registrando el issue solucionado, el estado actual y cualquier trampa tecnica o aprendizaje nuevo descubierto.

4. **Paso 4: Commit Atomico**
   - Generar un unico commit exclusivo:
     - Para Bugs: `git commit -m "fix(modulo): breve descripcion del arreglo (closes #<numero>)"`
     - Para Features: `git commit -m "feat(modulo): breve descripcion del anadido (closes #<numero>)"`
   - Obtener hash corto: `git rev-parse --short HEAD`

5. **Paso 5: Actualizacion Automatica en Obsidian y Memoria**
   - Buscar en la carpeta `D:\Informacion\BobedaObisidian\01_Proyectos\Sistema Una Mordida\Backlog` la nota que contenga `github_issue: "https://github.com/.../issues/<numero>"` (y el archivo `Añadidos y Bugs de la APP.md`).
   - Actualizar frontmatter:
     - Cambiar `estado: "Pendiente"` por `estado: "Resuelto"`.
     - Asignar `commit: "<hash_commit>"`.
   - En la seccion `## Cierre y Solucion`, registrar:
     - **Commit:** `<hash_commit>`
     - **Resumen de la solucion:** Resumen tecnico implementado.
     - **Evidencias visuales:** `<!-- Pega capturas aqui: ![[nombre_imagen.png]] -->`
   - Verificar que `MEMORY.md` haya quedado actualizado con el issue cerrado y las lecciones aprendidas.
