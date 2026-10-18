---
name: ui-design
description: Explora, disena y propone interfaces visuales y variantes UX/UI para la app usando StitchMCP, capturas de pantalla, vistas XAML actuales y Generative UI.
---

# Diseno y Creatividad Visual (/ui-design)

Esta skill guia el proceso de ideacion, propuesta creativa y redineno de interfaces graficas para el sistema POS de la cafeteria "Una Mordida", combinando el contexto tecnico del repositorio, las capacidades generativas de StitchMCP y herramientas de visualizacion interactiva (canvas y artefactos).

---

## 1. Contexto Preconfigurado del Proyecto

El agente debe utilizar las siguientes fuentes de verdad predeterminadas:

* **StitchMCP:**
  * **Project ID:** `5570853905336234929` (Proyecto: `UnaMordida`)
  * **Sistema de Diseno Activo:** `Roasted Earth POS`
  * **Paleta de Colores Oficial:**
    * Primario (Espresso): `#442a22` / `#5d4037`
    * Secundario / Accion (Honey Amber): `#ffb300` / `#feb300`
    * Fondo / Superficie base (Cream / Parchment): `#fbfbe2` / `#f5f5dc`
    * Superficie blanca (Tarjetas y campos): `#ffffff`
    * Acento secundario (Roasted Nut / Gris calido): `#3f2d26` / `#827470`
    * Error / Alerta: `#ba1a1a`
  * **Tipografia:**
    * Titulos y Categorias: `Manrope` (Bold / SemiBold)
    * Cuerpo y Contenido: `Work Sans`
    * Numeros, Precios y Comandas: `JetBrains Mono`
  * **Dimensiones, Ergonomia y Perfil de Usuario:**
    * Orientado a **monitor normal de escritorio** (1366x768, 1920x1080) operado con **mouse y teclado** (ya no se usa pantalla touch).
    * **Usuarios principales: Personas mayores.** La interfaz debe ser **simple, despejada y sin exceso de informacion en pantalla**. Evitar iconos abstractos sin texto, subtitulos innecesarios o controles saturados; usar etiquetas explicitas en espanol, tipografia legible de buen tamano y acciones directas para agregar o editar elementos.
    * Esquinas redondeadas estandar: `0.5rem` (8px).
* **Vistas XAML del Repositorio:**
  * `Presentation.WPF/Views/` (`Comanda.xaml`, `ConfiguracionMenuView.xaml`, `FinalizarCompra.xaml`, `CorteCajaView.xaml`, etc.)
  * Estilos globales: `Presentation.WPF/Assets/` (`Colores.xaml`, `Tipografia.xaml`, `EstilosBotones.xaml`).
* **Documentacion y Backlog en Obsidian:**
  * Boveda: `D:\Informacion\BobedaObisidian\01_Proyectos\Sistema Una Mordida\Backlog`
  * Tablero: `D:\Informacion\BobedaObisidian\01_Proyectos\Sistema Una Mordida\Añadidos y Bugs de la APP.md`
* **Memoria del Agente:**
  * Archivo `MEMORY.md` en la raiz del repositorio.

---

## 2. Flujo de Trabajo en 5 Fases

El agente debe ejecutar estrictamente las siguientes fases:

### Fase 1: Identificacion del Requerimiento y Preguntas Clave

1. Revisar `MEMORY.md` para entender las decisiones y directivas vigentes.
2. Si el usuario proporciono un issue, una vista especifica o una imagen:
   * Extraer el objetivo funcional y los elementos que componen dicha pantalla.
3. Si la solicitud es abierta o ambigua (por ejemplo: *"quiero proponer un nuevo diseno"*):
   * Formular preguntas claras al usuario (usando `ask_question` si hay opciones cerradas, o preguntas directas) abordando:
     a) **Pantalla objetivo:** ¿Que modulo o pantalla se va a crear o redisenar? (Ej. Comanda, Configuracion de Menu, Modal de Descuentos, Corte de Caja).
     b) **Fuente de contexto:** ¿Se toma como base el XAML actual del repositorio, una imagen de referencia provista por el usuario, o se disena desde cero?
     c) **Objetivo de UX:** ¿Que limitacion o problema de usabilidad actual busca resolver la nueva propuesta?
     d) **Rango de exploracion creativa:**
        * `REFINE`: Ajustes sutiles de proporciones y espaciado manteniendo la distribucion actual.
        * `EXPLORE`: Exploracion balanceada con cambios de layout, componentes y navegacion.
        * `REIMAGINE`: Rediseno radical y estructural del flujo de trabajo.

### Fase 2: Extraccion de Contexto y Preparacion Tecnica

1. **Si se parte del diseno actual:** Leer el archivo XAML en `Presentation.WPF/Views/` correspondiente y su ViewModel para registrar:
   * Controles visuales indispensables (botones de accion, listados, inputs, totales).
   * Bindings existentes para no omitir capacidades del sistema en la nueva propuesta.
2. **Si el usuario proporciona imagenes:**
   * Analizar la distribucion, colores, jerarquia visual y elementos destacables de la imagen.
3. **Consultar pantallas existentes en Stitch:**
   * Ejecutar la herramienta MCP `list_screens` con `projectId: "5570853905336234929"`.
   * Verificar si la pantalla ya existe registrada en Stitch para usarla como punto de partida o generar variantes sobre ella.

### Fase 3: Generacion de Disenos con StitchMCP

1. **Para variantes sobre una pantalla ya existente en Stitch:**
   * Invocar `generate_variants` en StitchMCP especificando:
     * `projectId`: `"5570853905336234929"`
     * `selectedScreenIds`: ID de la pantalla en Stitch (ej. `cfcf8feeeafc4b9780b8dff7550de13c`).
     * `prompt`: Descripcion detallada de las modificaciones solicitadas y reglas ergonomicas tactiles.
     * `variantOptions`:
       * `variantCount`: Entre 2 y 4 variantes.
       * `creativeRange`: `REFINE`, `EXPLORE` o `REIMAGINE`.
       * `aspects`: `LAYOUT`, `COLOR_SCHEME`, `TEXT_FONT` segun aplique.
2. **Para una pantalla completamente nueva:**
   * Invocar `generate_screen_from_text` en StitchMCP especificando:
     * `projectId`: `"5570853905336234929"`
     * `prompt`: Descripcion detallada de la pantalla incorporando las entidades, modales y botones requeridos.
     * `deviceType`: `"DESKTOP"` (o `"TABLET"`).
3. **Obtencion de recursos generados:**
   * Para cada pantalla o variante generada, llamar a `get_screen` para obtener:
     * La URL de la captura visual (`screenshot.downloadUrl`).
     * El enlace al archivo de prototipo HTML (`htmlCode.downloadUrl`).

### Fase 4: Presentacion Visual Interactiva (Canva / Artefacto)

1. Crear un artefacto Markdown en el directorio de artefactos con la siguiente estructura:
   * **Resumen de la propuesta:** Explicacion de los cambios introducidos y la justificacion ergonomica.
   * **Carrusel comparativo (`carousel`):** Mostrar secuencialmente:
     * Slide 1: Pantalla actual / Referencia original.
     * Slide 2+: Cada una de las variantes generadas con sus enlaces e imagenes renderizadas:
       `![Variante 1](URL_IMAGEN)`
   * **Tabla de comparacion:** Cuadro comparativo evaluando ergonomia tactil, facilidad de lectura bajo iluminacion de cafeteria y velocidad de uso para el cajero/barista.
2. **Lienzo interactivo (Generative UI):**
   * Si el usuario desea probar la interaccion antes de programar XAML, generar un componente HTML interactivo local que permita hacer clic en botones, cambiar estados y visualizar modales en tiempo real.

### Fase 5: Registro de Decisiones en Memoria y Backlog

1. **Actualizar `MEMORY.md`:**
   * Si no existe, agregar la seccion `## Decisiones de Diseno UI`.
   * Registrar:
     * Pantalla o flujo intervenido.
     * IDs de pantallas aprobadas en Stitch (`projects/5570853905336234929/screens/<id>`).
     * Criterios y directivas visuales acordadas (ej. "En la comanda el teclado numerico se mantiene anclado al tercio derecho").
2. **Actualizar nota en Obsidian:**
   * Si la sesion corresponde a un issue de la boveda (ej. `#8 Rediseño de Configuración de Menú`):
     * Abrir la nota correspondiente en `D:\Informacion\BobedaObisidian\01_Proyectos\Sistema Una Mordida\Backlog`.
     * Anadir la seccion de propuestas de diseno con los enlaces visuales de Stitch.
