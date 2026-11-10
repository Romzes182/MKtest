[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($env:OS -ne 'Windows_NT') {
    throw 'Setup Project requires Windows and Visual Studio with Microsoft Visual Studio Installer Projects.'
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'Visual Studio Installer (vswhere.exe) was not found.'
}
$vsPath = & $vswhere -latest -products '*' -version '[17.12,19.0)' -property installationPath
if (-not $vsPath) { throw 'Visual Studio 2022 17.12+ or Visual Studio 2026 is required for .NET 9.' }
$devenv = Join-Path $vsPath 'Common7\IDE\devenv.com'
$msbuild = Join-Path $vsPath 'MSBuild\Current\Bin\MSBuild.exe'
$disableOutOfProc = Join-Path $vsPath 'Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe'
if (-not (Test-Path -LiteralPath $devenv) -or -not (Test-Path -LiteralPath $disableOutOfProc)) {
    throw 'Install the Microsoft Visual Studio Installer Projects extension in the full Visual Studio IDE, then restart Visual Studio.'
}

$logDirectory = Join-Path $repoRoot 'artifacts\setup-logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$buildLog = Join-Path $logDirectory 'devenv-build.log'
$publishDirectory = Join-Path $repoRoot 'artifacts\publish\win-x64'
$msi = Join-Path $PSScriptRoot 'Release\MKtest.Setup.msi'

Push-Location $repoRoot
try {
    # Only generated outputs are cleared, so stale files cannot enter the installer.
    if (Test-Path -LiteralPath $publishDirectory) {
        Remove-Item -LiteralPath $publishDirectory -Recurse -Force
    }
    if (Test-Path -LiteralPath $msi) { Remove-Item -LiteralPath $msi -Force }
    if (Test-Path -LiteralPath $buildLog) { Remove-Item -LiteralPath $buildLog -Force }

    # Use the same MSBuild/SDK resolver as devenv, and the same Setup configuration.
    & $msbuild MKtest/MKtest.csproj /restore /t:Publish /p:Configuration=Setup /p:PublishProfile=Setup-win-x64 /verbosity:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "MSBuild publish failed: $LASTEXITCODE" }

    # Official extension helper: required for command-line vdproj builds.
    # The helper locates its Visual Studio instance from the current directory.
    Push-Location (Split-Path -Parent $disableOutOfProc)
    try {
        & $disableOutOfProc
        if ($LASTEXITCODE -ne 0) { throw "DisableOutOfProcBuild failed: $LASTEXITCODE" }
    } finally {
        Pop-Location
    }

    & $devenv MKtest.sln /Build 'Setup|Any CPU' /Project MKtest.Setup /ProjectConfig Release /Out $buildLog
    $buildExitCode = $LASTEXITCODE
    if (Test-Path -LiteralPath $buildLog) { Get-Content -LiteralPath $buildLog }
    if ($buildExitCode -ne 0 -or -not (Test-Path -LiteralPath $msi)) {
        throw "Setup Project build failed (exit $buildExitCode). See $buildLog"
    }
    Write-Host "Installer: $msi"
} finally {
    Pop-Location
}
