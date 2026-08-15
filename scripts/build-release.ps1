[CmdletBinding()]
param(
    [string]$DotnetPath = "dotnet",
    [string]$InnoCompilerPath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $projectRoot "ConnectionWatcher.sln"
$appProject = Join-Path $projectRoot "src\ConnectionWatcher.App\ConnectionWatcher.App.csproj"
$testProject = Join-Path $projectRoot "tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj"
$uiTestProject = Join-Path $projectRoot "tests\ConnectionWatcher.UiSmoke\ConnectionWatcher.UiSmoke.csproj"
$publishDirectory = Join-Path $projectRoot "artifacts\publish\win-x64"
$distDirectory = Join-Path $projectRoot "dist"
$finalShareDirectory = Join-Path $projectRoot "Final-Share"
$docsDirectory = Join-Path $projectRoot "docs"
$installerScript = Join-Path $projectRoot "packaging\ConnectionWatcher.iss"

function Confirm-ProjectPath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $rootWithSeparator = [System.IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
    if (-not $fullPath.StartsWith($rootWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify a path outside the project: $fullPath"
    }

    return $fullPath
}

function Reset-GeneratedDirectory {
    param([string]$Path)

    $verifiedPath = Confirm-ProjectPath $Path
    if (Test-Path -LiteralPath $verifiedPath) {
        Remove-Item -LiteralPath $verifiedPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $verifiedPath | Out-Null
}

function Resolve-InnoCompiler {
    if (-not [string]::IsNullOrWhiteSpace($InnoCompilerPath)) {
        return (Resolve-Path -LiteralPath $InnoCompilerPath).Path
    }

    $command = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 7\ISCC.exe"),
        (Join-Path (Split-Path -Parent (Split-Path -Parent $projectRoot)) "work\inno7\ISCC.exe")
    )
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Inno Setup Compiler was not found. Pass -InnoCompilerPath explicitly."
}

function Copy-ReleaseDocument {
    param(
        [string]$SourceName,
        [string]$LanguageFolder
    )

    $destination = Join-Path $finalShareDirectory "Docs\$LanguageFolder"
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $docsDirectory $SourceName) -Destination $destination
}

Write-Host "[1/7] Building the solution"
& $DotnetPath build $solutionPath --configuration Release
if ($LASTEXITCODE -ne 0) { throw "Solution build failed." }

Write-Host "[2/7] Running functional tests"
& $DotnetPath run --project $testProject --configuration Release --no-build
if ($LASTEXITCODE -ne 0) { throw "Functional tests failed." }

Write-Host "[3/7] Running multilingual UI tests"
$uiOutput = Join-Path ([System.IO.Path]::GetTempPath()) ("ConnectionWatcherUi-" + [guid]::NewGuid().ToString("N"))
try {
    & $DotnetPath run --project $uiTestProject --configuration Release --no-build -- $uiOutput
    if ($LASTEXITCODE -ne 0) { throw "UI tests failed." }
}
finally {
    if (Test-Path -LiteralPath $uiOutput) {
        Remove-Item -LiteralPath $uiOutput -Recurse -Force
    }
}

Write-Host "[4/7] Publishing the Windows executable"
Reset-GeneratedDirectory $publishDirectory
& $DotnetPath publish $appProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDirectory `
    -p:DebugType=None `
    -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { throw "Application publish failed." }

Write-Host "[5/7] Building the installer"
Reset-GeneratedDirectory $distDirectory
$innoCompiler = Resolve-InnoCompiler
& $innoCompiler $installerScript
if ($LASTEXITCODE -ne 0) { throw "Installer build failed." }

Write-Host "[6/7] Preparing Final-Share"
Reset-GeneratedDirectory $finalShareDirectory
$installer = Join-Path $distDirectory "SocketSight-Setup-win-x64.exe"
Copy-Item -LiteralPath $installer -Destination $finalShareDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot "RELEASE_NOTES.md") -Destination $finalShareDirectory

Copy-ReleaseDocument "Project-Overview.md" "English"
Copy-ReleaseDocument "User-Guide.md" "English"
$simplifiedProject = (-join @([char]0x9879, [char]0x76EE, [char]0x8BF4, [char]0x660E)) + ".md"
$simplifiedGuide = (-join @([char]0x4F7F, [char]0x7528, [char]0x8BF4, [char]0x660E)) + ".md"
$traditionalProject = (-join @([char]0x5C08, [char]0x6848, [char]0x8AAA, [char]0x660E)) + ".md"
$traditionalGuide = (-join @([char]0x4F7F, [char]0x7528, [char]0x8AAA, [char]0x660E)) + "-" +
    (-join @([char]0x7E41, [char]0x9AD4, [char]0x4E2D, [char]0x6587)) + ".md"
Copy-ReleaseDocument $simplifiedProject "Chinese (Simplified)"
Copy-ReleaseDocument $simplifiedGuide "Chinese (Simplified)"
Copy-ReleaseDocument $traditionalProject "Chinese (Traditional)"
Copy-ReleaseDocument $traditionalGuide "Chinese (Traditional)"
Copy-ReleaseDocument "Descripcion-del-proyecto.md" "Spanish"
Copy-ReleaseDocument "Guia-del-usuario.md" "Spanish"
Copy-ReleaseDocument "Presentation-du-projet.md" "French"
Copy-ReleaseDocument "Guide-utilisateur.md" "French"
Copy-ReleaseDocument "Projektuebersicht.md" "Deutsch"
Copy-ReleaseDocument "Benutzerhandbuch.md" "Deutsch"
Copy-ReleaseDocument "Visao-geral-do-projeto.md" "Portuguese (Brazil)"
Copy-ReleaseDocument "Guia-do-usuario.md" "Portuguese (Brazil)"

Write-Host "[7/7] Generating SHA-256"
$finalInstaller = Join-Path $finalShareDirectory (Split-Path -Leaf $installer)
$hash = (Get-FileHash -LiteralPath $finalInstaller -Algorithm SHA256).Hash.ToLowerInvariant()
$checksumPath = Join-Path $finalShareDirectory "SHA256SUMS.txt"
Set-Content -LiteralPath $checksumPath -Value "$hash *$(Split-Path -Leaf $finalInstaller)" -Encoding ascii

Write-Host "Release files are ready: $finalShareDirectory"
