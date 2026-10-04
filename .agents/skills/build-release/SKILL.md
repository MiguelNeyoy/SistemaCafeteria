---
name: build-release
description: Compila y empaqueta la aplicacion para produccion con Velopack ejecutando el script inteligente scripts/build_release.ps1.
---

# Empaquetado y Release (/build-release)

Usa esta skill para compilar binarios autocontenidos y empaquetar instaladores y actualizaciones con Velopack para Una Mordida.

## Descripcion
El script `scripts/build_release.ps1` automatiza todo el ciclo de entrega:
1. Verifica e instala la herramienta global `vpk` si es necesario.
2. Detecta versiones en `Releases/`, Git tags y `.csproj` para auto-incrementar el patch si no se especifica version.
3. Ejecuta la suite de 38 pruebas unitarias antes de compilar.
4. Publica la solucion en modo Release autocontenido (`win-x64`).
5. Empaqueta el instalador con Velopack incrustando el icono oficial (`Logo.ico`).

## Parametros Opcionales
- `-Version <x.y.z>`: Especifica manualmente la version (ejemplo: `1.0.1`).
- `-Force`: Permite sobrescribir si la version ya existe en `Releases/`.
- `-SkipTests`: Omite la ejecucion previa de pruebas unitarias.

## Comando de Ejecucion

Para empaquetar con version auto-incrementada:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build_release.ps1
```

Para empaquetar con una version especifica:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/build_release.ps1 -Version "1.0.1"
```

## Salida Esperada
Los instaladores y archivos de actualizacion se generan en la carpeta `Releases/`:
- `UnaMordida-Setup.exe` (Instalador completo con icono oficial).
- `UnaMordida-<version>-full.nupkg` (Paquete completo de version).
- `UnaMordida-<version>-delta.nupkg` (Paquete diferencial para actualizacion ligera).
- `releases.win.json` (Indice de versiones para actualizacion automatica).
