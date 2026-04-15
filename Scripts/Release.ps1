# PhantomOS Release Script
# Automates the building and packaging of PhantomOS as a single-file executable.

$projectName = "PhantomOS"
$projectFile = "$projectName.csproj"
$outputDir = ".\artifacts"
$runtime = "win-x64"

Write-Host "Starting Release process for $projectName ($runtime)..."

# 1. Clean up old artifacts
if (Test-Path $outputDir) {
    Write-Host "Cleaning artifacts directory..."
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

# 2. Publish Single-File Executable
Write-Host "Publishing single-file executable..."
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
    Write-Host "Publish successful."
    
    # 3. Zip the artifact
    $zipPath = "$outputDir\$projectName-$runtime-v0.2.zip"
    Write-Host "Compressing artifact into $zipPath..."
    Compress-Archive -Path "$outputDir\$runtime\*" -DestinationPath $zipPath
    
    Write-Host "Release process completed successfully."
    $path = (Get-Location).Path
    Write-Host "Artifacts are located in: $path\artifacts"
} else {
    Write-Error "Error during publish."
}
