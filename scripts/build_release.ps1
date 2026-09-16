param (
    [string]$Version = "1.0.0"
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   EMPAQUETADOR DE INSTALADOR VELOPACK - UNA MORDIDA MÁS  " -ForegroundColor Cyan
Write-Host "   Versión objetivo: $Version" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Verificar si la herramienta vpk está instalada globalmente
$vpkInstalled = Get-Command vpk -ErrorAction SilentlyContinue
if (-not $vpkInstalled) {
    Write-Host "Instalando la herramienta global de Velopack (vpk)..." -ForegroundColor Yellow
    dotnet tool install -g vpk
}

# 2. Rutas del proyecto
$projectPath = "Presentation.WPF\Presentation.WPF.csproj"
$publishDir = "bin\publish"
$releasesDir = "Releases"

# 3. Limpiar carpeta de publicación previa
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

# 4. Publicar la aplicación en modo Release para win-x64
Write-Host "
[1/2] Publicando binarios en $publishDir..." -ForegroundColor Green
dotnet publish $projectPath -c Release -r win-x64 --self-contained -o $publishDir /p:Version=$Version

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error durante dotnet publish. Cancelando." -ForegroundColor Red
    exit 1
}

# 5. Empaquetar con Velopack (genera Setup.exe y paquetes delta en Releases\)
Write-Host "
[2/2] Empaquetando instalador con Velopack (vpk pack)..." -ForegroundColor Green
vpk pack -u "UnaMordidaMas" -v $Version -p $publishDir -e "Presentation.WPF.exe" -o $releasesDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "
==========================================================" -ForegroundColor Green
    Write-Host "  ¡INSTALADOR CREADO EXITOSAMENTE EN LA CARPETA Releases/!" -ForegroundColor Green
    Write-Host "  Archivos generados:" -ForegroundColor Cyan
    Get-ChildItem $releasesDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
    Write-Host "==========================================================" -ForegroundColor Green
} else {
    Write-Host "Error durante la creación del paquete con vpk." -ForegroundColor Red
}
