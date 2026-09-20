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
Write-Host "       EMPAQUETADOR INTELIGENTE VELOPACK - UNA MORDIDA    " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 2. Verificar herramienta vpk de Velopack
$vpkInstalled = Get-Command vpk -ErrorAction SilentlyContinue
if (-not $vpkInstalled) {
    Write-Host "[AVISO] Herramienta global de Velopack (vpk) no encontrada. Instalando..." -ForegroundColor Yellow
    dotnet tool install -g vpk
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] No se pudo instalar vpk. Abortando." -ForegroundColor Red
        exit 1
    }
}

# 3. Deteccion de versiones existentes (en Releases, Git y csproj)
$detectedVersions = @()

# A) Paquetes en carpeta Releases/
if (Test-Path $releasesDir) {
    $existingPackages = Get-ChildItem -Path $releasesDir -Filter "UnaMordida-*-full.nupkg" -ErrorAction SilentlyContinue
    foreach ($pkg in $existingPackages) {
        if ($pkg.Name -match "UnaMordida-(.+?)-full\.nupkg") {
            try { $detectedVersions += [System.Version]$matches[1] } catch {}
        }
    }
}

# B) Tags en Git
try {
    $gitTags = git tag -l
    foreach ($tag in $gitTags) {
        $cleanTag = $tag.Trim().TrimStart('v')
        try { $detectedVersions += [System.Version]$cleanTag } catch {}
    }
} catch {}

# C) Version en csproj
if (Test-Path $projectPath) {
    [xml]$xml = Get-Content $projectPath
    $projVer = $xml.Project.PropertyGroup.Version
    if ($projVer) {
        try { $detectedVersions += [System.Version]$projVer } catch {}
    }
}

# Obtener la version mas alta detectada
$latestVersion = $null
if ($detectedVersions.Count -gt 0) {
    $latestVersion = ($detectedVersions | Sort-Object -Descending | Select-Object -First 1)
}

# 4. Determinar la version a compilar
if ([string]::IsNullOrWhiteSpace($Version)) {
    if ($null -eq $latestVersion) {
        $targetVersion = "1.0.0"
        Write-Host "[INFO] No se detectaron versiones previas. Se iniciara en version base: $targetVersion" -ForegroundColor Yellow
    } else {
        # Si la version mas alta ya existe en Releases, auto-incrementar el patch
        $fullPkgPath = Join-Path $releasesDir "UnaMordida-$latestVersion-full.nupkg"
        if (Test-Path $fullPkgPath) {
            $prevPatch = [Math]::Max($latestVersion.Build, 0)
            $nextPatch = $prevPatch + 1
            $targetVersion = "$($latestVersion.Major).$($latestVersion.Minor).$nextPatch"
            Write-Host "[INFO] Ultima version en Releases: $latestVersion" -ForegroundColor Gray
            Write-Host "[AUTO] Siguiente version calculada: $targetVersion" -ForegroundColor Yellow
        } else {
            $basePatch = [Math]::Max($latestVersion.Build, 0)
            $targetVersion = "$($latestVersion.Major).$($latestVersion.Minor).$basePatch"
            Write-Host "[INFO] Version base detectada lista para compilar: $targetVersion" -ForegroundColor Yellow
        }
    }
} else {
    $targetVersion = $Version.Trim().TrimStart('v')
}

# 5. Validaciones de Seguridad
try {
    $parsedTarget = [System.Version]$targetVersion
} catch {
    Write-Host "[ERROR] '$targetVersion' no es un formato de version valido (ejemplo valido: 1.0.0, 1.0.1)." -ForegroundColor Red
    exit 1
}

# Comprobar si ya existe el instalador de esa version
$targetPkgExists = $false
if (Test-Path $releasesDir) {
    $targetPkg = Join-Path $releasesDir "UnaMordida-$targetVersion-full.nupkg"
    if (Test-Path $targetPkg) {
        $targetPkgExists = $true
    }
}

if ($targetPkgExists -and -not $Force) {
    Write-Host ""
    Write-Host "[ALERTA] VERSION DUPLICADA DETECTADA:" -ForegroundColor Red
    Write-Host "La version '$targetVersion' ya existe en la carpeta Releases/." -ForegroundColor Red
    Write-Host "Velopack no actualizara clientes si el numero de version no es estrictamente superior." -ForegroundColor Yellow
    $suggestedBuild = [Math]::Max($parsedTarget.Build, 0) + 1
    $suggestedNext = "$($parsedTarget.Major).$($parsedTarget.Minor).$suggestedBuild"
    Write-Host "Sugerencia: Usa la version '$suggestedNext' o agrega el parametro -Force para sobrescribir." -ForegroundColor Cyan
    exit 1
}

if ($latestVersion -and ($parsedTarget -lt $latestVersion) -and -not $Force) {
    Write-Host ""
    Write-Host "[ALERTA] DEGRADACION DE VERSION (DOWNGRADE):" -ForegroundColor Red
    Write-Host "La version solicitada ($targetVersion) es menor a la version mas alta existente ($latestVersion)." -ForegroundColor Red
    Write-Host "Los clientes instalados con versiones superiores ignoraran este paquete." -ForegroundColor Yellow
    exit 1
}

Write-Host ">> Version objetivo confirmada: $targetVersion" -ForegroundColor Green

# 6. Ejecutar pruebas unitarias antes de compilar el paquete
if (-not $SkipTests) {
    Write-Host "`n[1/3] Ejecutando pruebas unitarias de seguridad..." -ForegroundColor Cyan
    dotnet test $solutionPath --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Las pruebas unitarias fallaron. Empaquetado cancelado para proteger el sistema." -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Todas las pruebas unitarias pasaron con exito." -ForegroundColor Green
} else {
    Write-Host "`n[1/3] Pruebas unitarias omitidas (-SkipTests)." -ForegroundColor Yellow
}

# 7. Limpiar carpeta de publicacion previa
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

# 8. Publicar la aplicacion en modo Release
Write-Host "`n[2/3] Publicando binarios autocontenidos en $publishDir..." -ForegroundColor Cyan
dotnet publish $projectPath -c Release -r win-x64 --self-contained -o $publishDir /p:Version=$targetVersion /p:AssemblyVersion=$targetVersion /p:FileVersion=$targetVersion

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Fallo durante dotnet publish. Cancelando." -ForegroundColor Red
    exit 1
}

# 9. Empaquetar con Velopack
Write-Host "`n[3/3] Generando instalador y deltas con Velopack (vpk pack)..." -ForegroundColor Cyan
vpk pack -u "UnaMordida" -v $targetVersion -p $publishDir -e "Presentation.WPF.exe" -o $releasesDir

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "  INSTALADOR v$targetVersion CREADO EXITOSAMENTE!" -ForegroundColor Green
    Write-Host "  Ubicacion: $releasesDir" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    Get-ChildItem $releasesDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
} else {
    Write-Host "[ERROR] Fallo durante la creacion del paquete con vpk." -ForegroundColor Red
    exit 1
}
