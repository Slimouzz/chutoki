# Compile Chutoki Free dans .\dist (aucun SDK requis : utilise le compilateur C# de Windows)
param([string]$PreviewOut = "")
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$dist = Join-Path $root 'dist'
$lib = Join-Path $root 'lib'
$src = Join-Path $root 'src'
$version = '1.1.0'
New-Item -ItemType Directory -Force $dist | Out-Null
if (-not (Test-Path "$src\app.ico")) { & "$root\tools\make-icon.ps1" -Out "$src\app.ico" -Png "$root\logo.png" }

$refs = @('System.Windows.Forms.dll', 'System.Drawing.dll', 'System.Core.dll', 'System.Management.dll', 'System.ServiceProcess.dll', 'System.Web.Extensions.dll', "$lib\LibreHardwareMonitorLib.dll") | % { "/r:$_" }
$common = @('/nologo', '/target:winexe', '/platform:x64', '/optimize+', '/codepage:65001', "/win32icon:$src\app.ico") + $refs
$files = @("$src\Chutoki.cs", "$src\Lang.cs")

if ($PreviewOut) {
    # Version d'aperçu sans manifeste admin, pour générer les captures d'écran des fenêtres
    New-Item -ItemType Directory -Force $PreviewOut | Out-Null
    & $csc @common "/out:$PreviewOut\Chutoki.exe" @files
    if ($LASTEXITCODE -ne 0) { throw "Compilation aperçu échouée" }
    Copy-Item "$lib\*.dll" $PreviewOut -Force
    Copy-Item "$src\app.config" "$PreviewOut\Chutoki.exe.config" -Force
    return
}

& $csc @common "/win32manifest:$src\app.manifest" "/out:$dist\Chutoki.exe" @files
if ($LASTEXITCODE -ne 0) { throw "Compilation échouée" }
Copy-Item "$lib\*.dll" $dist -Force
Copy-Item "$src\app.config" "$dist\Chutoki.exe.config" -Force
Copy-Item "$root\LICENSE.txt", "$root\THIRD-PARTY-NOTICES.txt", "$root\LISEZMOI.txt", "$root\README.txt" $dist -Force
# Archive prête à publier
$zip = Join-Path $root "Chutoki-Free-$version-win-x64.zip"
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path "$dist\*" -DestinationPath $zip
# nom fixe utilisé par le bouton du site (releases/latest/download/...)
Copy-Item $zip (Join-Path $root "Chutoki-Free-win-x64.zip") -Force
"ZIP -> $zip"
"OK -> $dist"
