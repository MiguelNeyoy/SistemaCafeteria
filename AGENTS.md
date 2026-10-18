



>\[!important] Este archivo es para crear las instrucciones de como hacer las cosas  para un proyecto en este caso el proyecto es un  sistema de la cafetería 

\# Directivas de Automatización y Resolución (AGENTS.md)

Revisa y lee el MEMORY.md primero 

\## 1. Configuración de Entornos y Rutas

\* Repositorio de código: Este directorio de trabajo.

\* Bóveda de Obsidian asociada: `D:\\Informacion\\BobedaObisidian\\01\_Proyectos\\Sistema Una Mordida`

\* Carpeta de especificaciones: `D:\\Informacion\\BobedaObisidian\\01\_Proyectos\\Sistema Una Mordida\\Backlog`



\---



\## 2. Flujo de Trabajo al recibir la orden: "Resuelve el issue #<numero>"



El agente debe ejecutar estrictamente los siguientes 5 pasos en orden:



\### Paso 1: Inspección y Validación del Requerimiento

1\. Ejecutar en terminal:

&#x20;  `gh issue view <numero>`

2\. Extraer de la descripción:

&#x20;  \* Objetivo de la funcionalidad o descripción del fallo.

&#x20;  \* Criterios de Aceptación (condiciones obligatorias de terminado).

&#x20;  \* Tablas, modelos, controladores o vistas XAML mencionadas.

&#x20;  \* Prioridad asignada (🔴 Crítico, 🟠 Alta, 🟡 Media, 🟢 Baja).



\### Paso 2: Plan de Modificaciones (Obligatorio antes de programar)

Antes de editar cualquier archivo de código, el agente debe responder al usuario con una propuesta estructurada:

1\. \*\*Archivos a modificar o crear:\*\* Lista exacta de rutas (`.cs`, `.xaml`, migraciones).

2\. \*\*Lógica de la solución:\*\* Breve explicación de cómo se resolverá el problema.

3\. \*\*Verificación de Criterios de Aceptación:\*\* Cómo se comprobará que cada criterio del issue se cumpla.

4\. \*\*Prueba de estrés o caos según la naturaleza del issue:\*\* Diseñar un escenario extremo o caótico adaptado específicamente a lo que hace el issue para intentar romper la solución (y convertirlo en prueba unitaria o validación en el Paso 3).

5\. \*\*Pausa de confirmación:\*\* Preguntar al usuario: "¿Deseas proceder con este plan de implementación?".



\### Paso 3: Implementación y Compilación

Una vez confirmado el plan por el usuario:

1\. Aplicar los cambios necesarios en el código fuente e incluir la prueba de estrés/caos adaptada al issue.

2\. Compilar y ejecutar las pruebas para verificar que no existan errores ni fallos ante el escenario de caos:

&#x20;  `dotnet build` y `dotnet test`

3\. Si la compilación o las pruebas fallan, revisar y corregir con buenas prácticas y clean code hasta que todo pase.

4\. Actualizar `MEMORY.md` registrando el issue solucionado, el estado actual de versiones/desarrollo y cualquier trampa técnica o aprendizaje nuevo descubierto durante la implementación.



\### Paso 4: Commit Atómico

Generar un único commit que contenga exclusivamente los cambios del issue resuelto:

\* Para corrección de errores (Bugs):

&#x20; `git commit -m "fix(modulo): breve descripcion del arreglo (closes #<numero>)"`

\* Para nuevas funcionalidades (Features):

&#x20; `git commit -m "feat(modulo): breve descripcion del añadido (closes #<numero>)"`



Obtener el hash corto del commit generado mediante:

`git rev-parse --short HEAD`



\### Paso 5: Actualización Automática de vuelta en Obsidian y Memoria

1\. Buscar en la carpeta `Backlog` de Obsidian la nota que contenga `github\_issue: "https://github.com/.../issues/<numero>"` (y el archivo `Añadidos y Bugs de la APP.md`).

2\. Actualizar el contenido de esa nota:

&#x20;  \* Cambiar `estado: "Pendiente"` por `estado: "Resuelto"`.

&#x20;  \* Asignar en el frontmatter: `commit: "<hash\_commit>"`.

&#x20;  \* En la sección `## Cierre y Solución`, llenar los datos:

&#x20;    - \*\*Commit:\*\* `<hash\_commit>`

&#x20;    - \*\*Resumen de la solución:\*\* Resumen técnico de lo que se implementó.

&#x20;    - \*\*Evidencias visuales:\*\* Dejar una línea lista para pegar imágenes: `<!-- Pega capturas aquí: !\[\[nombre\_imagen.png]] -->`

3\. Verificar que `MEMORY.md` haya quedado actualizado con el issue cerrado y las lecciones aprendidas.



\---

