# PhantomOS Release Script
# Automates the building and packaging of PhantomOS as a single-file executable.

$projectName = "PhantomOS"
$projectFile = "$projectName.csproj"
$outputDir = ".\artifacts"
$runtime = "win-x64"

Write-Host "🚀 Iniciando proceso de Release para $projectName ($runtime)..." -ForegroundColor Cyan

# 1. Clean up old artifacts
if (Test-Path $outputDir) {
    Write-Host "🧹 Limpiando directorio de artifacts..."
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

# 2. Publish Single-File Executable
Write-Host "📦 Publicando ejecutable de archivo único..." -ForegroundColor Yellow
dotnet publish $projectFile `
    -c Release `
    -r $runtime `
    --output "$outputDir\$runtime" `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Publicación exitosa." -ForegroundColor Green
    
    # 3. Zip the artifact
    $zipPath = "$outputDir\$projectName-$runtime-v0.1.zip"
    Write-Host "📚 Comprimiendo artifact en $zipPath..."
    Compress-Archive -Path "$outputDir\$runtime\*" -DestinationPath $zipPath
    
    Write-Host "🎉 Proceso de Release completado con éxito." -ForegroundColor Green
    Write-Host "📂 Los archivos se encuentran en: $((Get-Location).Path)\artifacts"
} else {
    Write-Error "❌ Error durante la publicación."
}
