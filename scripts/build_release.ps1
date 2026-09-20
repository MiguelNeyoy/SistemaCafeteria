param (
    [string]$Version = "",
    [switch]$Force,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

# 1. Resolver rutas absolutas del repositorio
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (Test-Path "$scriptDir\..\Presentation.WPF\Presentation.WPF.csproj") {
    $rootDir = (Resolve-Path "$scriptDir\..").Path
} else {
    $rootDir = (Get-Location).Path
}

$solutionPath = Join-Path $rootDir "Sistema-Cafeteria.slnx"
$projectPath = Join-Path $rootDir "Presentation.WPF\Presentation.WPF.csproj"
$publishDir = Join-Path $rootDir "bin\publish"
$releasesDir = Join-Path $rootDir "Releases"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   EMPAQUETADOR INTELIGENTE VELOPACK - UNA MORDIDA MÁS    " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 2. Verificar herramienta vpk de Velopack
$vpkInstalled = Get-Command vpk -ErrorAction SilentlyContinue
if (-not $vpkInstalled) {
    Write-Host "Herramienta global de Velopack (vpk) no encontrada. Instalando..." -ForegroundColor Yellow
    dotnet tool install -g vpk
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error instalando vpk. Abortando." -ForegroundColor Red
        exit 1
    }
}

# 3. Detección de versiones existentes (en Releases, Git y csproj)
$detectedVersions = @()

# A) Paquetes en carpeta Releases/
if (Test-Path $releasesDir) {
    $existingPackages = Get-ChildItem -Path $releasesDir -Filter "UnaMordidaMas-*-full.nupkg" -ErrorAction SilentlyContinue
    foreach ($pkg in $existingPackages) {
        if ($pkg.Name -match "UnaMordidaMas-(.+?)-full\.nupkg") {
            try { $detectedVersions += [System.Version]$matches[1] } catch {}
        }
    }
}

# B) Tags en Git
try {
    $gitTags = git tag -l 2>$null
    foreach ($tag in $gitTags) {
        $clean = $tag.Trim().TrimStart('v')
        try { $detectedVersions += [System.Version]$clean } catch {}
    }
} catch {}

# C) Versión en csproj
if (Test-Path $projectPath) {
    [xml]$xml = Get-Content $projectPath
    $projVer = $xml.Project.PropertyGroup.Version
    if ($projVer) {
        try { $detectedVersions += [System.Version]$projVer } catch {}
    }
}

# Obtener la versión más alta detectada
$latestVersion = $null
if ($detectedVersions.Count -gt 0) {
    $latestVersion = ($detectedVersions | Sort-Object -Descending | Select-Object -First 1)
}

# 4. Determinar la versión a compilar
if ([string]::IsNullOrWhiteSpace($Version)) {
    if ($null -eq $latestVersion) {
        $targetVersion = "1.0.0"
        Write-Host "ℹ️ No se detectaron versiones previas. Se iniciará en versión base: $targetVersion" -ForegroundColor Yellow
    } else {
        # Si la versión más alta ya existe en Releases, auto-incrementar el patch
        $fullPkgPath = Join-Path $releasesDir "UnaMordidaMas-$latestVersion-full.nupkg"
        if (Test-Path $fullPkgPath) {
            $nextPatch = [Math]::Max($latestVersion.Build, 0) + 1
            $targetVersion = "$($latestVersion.Major).$($latestVersion.Minor).$nextPatch"
            Write-Host "ℹ️ Última versión en Releases: $latestVersion" -ForegroundColor Gray
            Write-Host "✨ Auto-incremento calculado: $targetVersion" -ForegroundColor Yellow
        } else {
            $targetVersion = "$($latestVersion.Major).$($latestVersion.Minor).$([Math]::Max($latestVersion.Build, 0))"
            Write-Host "ℹ️ Versión base detectada lista para compilar: $targetVersion" -ForegroundColor Yellow
        }
    }
} else {
    $targetVersion = $Version.Trim().TrimStart('v')
}

# 5. Validaciones de Seguridad
try {
    $parsedTarget = [System.Version]$targetVersion
} catch {
    Write-Host "❌ Error: '$targetVersion' no es un formato de versión válido (ejemplo válido: 1.0.0, 1.0.1)." -ForegroundColor Red
    exit 1
}

# Comprobar si ya existe el instalador de esa versión
$targetPkgExists = $false
if (Test-Path $releasesDir) {
    $targetPkg = Join-Path $releasesDir "UnaMordidaMas-$targetVersion-full.nupkg"
    if (Test-Path $targetPkg) {
        $targetPkgExists = $true
    }
}

if ($targetPkgExists -and -not $Force) {
    Write-Host "`n⚠️  ALERTA DE VERSIÓN DUPLICADA:" -ForegroundColor Red
    Write-Host "La versión '$targetVersion' ya existe en la carpeta Releases/." -ForegroundColor Red
    Write-Host "Velopack no actualizará clientes si el número de versión no es estrictamente superior." -ForegroundColor Yellow
    $suggestedNext = "$($parsedTarget.Major).$($parsedTarget.Minor).$([Math]::Max($parsedTarget.Build, 0) + 1)"
    Write-Host "Sugerencia: Usa la versión '$suggestedNext' o agrega el parámetro -Force para sobrescribir." -ForegroundColor Cyan
    exit 1
}

if ($latestVersion -and ($parsedTarget -lt $latestVersion) -and -not $Force) {
    Write-Host "`n⚠️  ALERTA DE DEGRADACIÓN (DOWNGRADE):" -ForegroundColor Red
    Write-Host "La versión solicitada ($targetVersion) es menor a la versión más alta existente ($latestVersion)." -ForegroundColor Red
    Write-Host "Los clientes instalados con versiones superiores ignorarán este paquete." -ForegroundColor Yellow
    exit 1
}

Write-Host ">> Versión objetivo confirmada: $targetVersion" -ForegroundColor Green

# 6. Ejecutar pruebas unitarias antes de compilar el paquete
if (-not $SkipTests) {
    Write-Host "`n[1/3] Ejecutando pruebas unitarias de seguridad..." -ForegroundColor Cyan
    dotnet test $solutionPath --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Las pruebas unitarias fallaron. Empaquetado cancelado para proteger el sistema." -ForegroundColor Red
        exit 1
    }
    Write-Host "✔ Todas las pruebas unitarias pasaron con éxito." -ForegroundColor Green
} else {
    Write-Host "`n[1/3] Pruebas unitarias omitidas (-SkipTests)." -ForegroundColor Yellow
}

# 7. Limpiar carpeta de publicación previa
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

# 8. Publicar la aplicación en modo Release
Write-Host "`n[2/3] Publicando binarios autocontenidos en $publishDir..." -ForegroundColor Cyan
dotnet publish $projectPath -c Release -r win-x64 --self-contained -o $publishDir /p:Version=$targetVersion /p:AssemblyVersion=$targetVersion /p:FileVersion=$targetVersion

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error durante dotnet publish. Cancelando." -ForegroundColor Red
    exit 1
}

# 9. Empaquetar con Velopack
Write-Host "`n[3/3] Generando instalador y deltas con Velopack (vpk pack)..." -ForegroundColor Cyan
vpk pack -u "UnaMordidaMas" -v $targetVersion -p $publishDir -e "Presentation.WPF.exe" -o $releasesDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host "  ¡INSTALADOR v$targetVersion CREADO EXITOSAMENTE!" -ForegroundColor Green
    Write-Host "  Ubicación: $releasesDir" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    Get-ChildItem $releasesDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host "❌ Error durante la creación del paquete con vpk." -ForegroundColor Red
    exit 1
}
