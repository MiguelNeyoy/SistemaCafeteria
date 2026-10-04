---
name: plan-issue
description: Inspecciona un issue de GitHub y presenta la propuesta tecnica estructurada (pasos 1 y 2 de AGENTS.md) sin modificar codigo.
---

# Plan de Issue (/plan-issue <numero>)

Ejecuta unicamente los pasos 1 y 2 especificados en `AGENTS.md` (inspecciona el issue en GitHub y presenta el plan tecnico sin modificar ningun archivo).

## Pasos de Ejecucion

1. **Paso 1: Inspeccion y Validacion del Requerimiento**
   - Ejecutar en terminal: `gh issue view <numero>`
   - Extraer de la descripcion:
     - Objetivo de la funcionalidad o descripcion del fallo.
     - Criterios de Aceptacion (condiciones obligatorias de terminado).
     - Tablas, modelos, controladores o vistas XAML mencionadas.
     - Prioridad asignada (Critico, Alta, Media, Baja).

2. **Paso 2: Plan de Modificaciones (Propuesta Tecnica)**
   - Responder al usuario con la propuesta estructurada antes de modificar archivos:
     - **Archivos a modificar o crear:** Lista exacta de rutas (`.cs`, `.xaml`, migraciones).
     - **Logica de la solucion:** Breve explicacion tecnica de como se resolvera el problema.
     - **Verificacion de Criterios de Aceptacion:** Como se comprobara que cada criterio del issue se cumpla.
     - **Simulacion de posibles errores:** Ejemplo de comportamiento como si tuviera 1000 productos y se requiera listar todos esos.
     - **Pausa de confirmacion:** Preguntar al usuario: "¿Deseas proceder con este plan de implementacion?".
