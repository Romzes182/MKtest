[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($env:OS -ne 'Windows_NT') { throw 'Installer verification requires Windows.' }

$repoRoot = Split-Path -Parent $PSScriptRoot
$msi = Join-Path $PSScriptRoot 'Release\MKtest.Setup.msi'
$publishDirectory = Join-Path $repoRoot 'artifacts\publish\win-x64'
$logDirectory = Join-Path $repoRoot 'artifacts\setup-logs'
$installDirectory = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Programs\MKtest'
$exe = Join-Path $installDirectory 'MKtest.exe'
$shortcuts = @(
    (Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'MKtest.lnk'),
    (Join-Path ([Environment]::GetFolderPath('Programs')) 'MKtest.lnk')
)

if (-not (Test-Path -LiteralPath $msi)) { throw "MSI not found: $msi" }
# Never run this smoke test over a user's existing installation or shortcuts.
foreach ($path in @($installDirectory) + $shortcuts) {
    if (Test-Path -LiteralPath $path) { throw "Use a clean Windows VM; path already exists: $path" }
}
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

function Invoke-Msi([string] $Action, [string] $LogName) {
    $log = Join-Path $logDirectory $LogName
    $process = Start-Process msiexec.exe -ArgumentList "$Action `"$msi`" /qn /norestart /l*v `"$log`"" -PassThru -Wait
    if ($process.ExitCode -notin @(0, 3010)) {
        throw "msiexec $Action failed with code $($process.ExitCode); see $log"
    }
}

function Get-ComProperty([object] $Object, [string] $Name, [object[]] $Arguments) {
    $Object.GetType().InvokeMember($Name, [Reflection.BindingFlags]::GetProperty,
        $null, $Object, $Arguments)
}

$installed = $false
$application = $null
try {
    Invoke-Msi '/i' 'install.log'
    $installed = $true
    if (-not (Test-Path -LiteralPath $exe)) { throw "Application not installed at the default per-user path: $exe" }

    # Verify every published dependency and data file, including extensionless routes.
    $publishedFiles = @(Get-ChildItem -LiteralPath $publishDirectory -Recurse -File)
    if ($publishedFiles.Count -eq 0) { throw 'Publish output is empty.' }
    foreach ($file in $publishedFiles) {
        $relative = [IO.Path]::GetRelativePath($publishDirectory, $file.FullName)
        $installedFile = Join-Path $installDirectory $relative
        if (-not (Test-Path -LiteralPath $installedFile)) { throw "Missing installed file: $relative" }
        if ((Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $installedFile).Hash) {
            throw "Installed file differs from publish output: $relative"
        }
    }
    $runtime = Get-Content -LiteralPath (Join-Path $installDirectory 'MKtest.runtimeconfig.json') -Raw | ConvertFrom-Json
    if (-not $runtime.runtimeOptions.includedFrameworks) { throw 'The installed application is not self-contained.' }

    $shell = New-Object -ComObject WScript.Shell
    $installer = New-Object -ComObject WindowsInstaller.Installer
    foreach ($path in $shortcuts) {
        if (-not (Test-Path -LiteralPath $path)) { throw "Shortcut not found: $path" }
        $shortcut = $shell.CreateShortcut($path)
        $target = $shortcut.TargetPath
        if ($target -ne $exe) {
            # Setup Project creates advertised MSI shortcuts. Resolve their actual
            # installed component through Windows Installer, not the icon/cache path.
            $record = Get-ComProperty $installer 'ShortcutTarget' @($path)
            $productCode = Get-ComProperty $record 'StringData' @(1)
            $componentCode = Get-ComProperty $record 'StringData' @(3)
            $target = Get-ComProperty $installer 'ComponentPath' @($productCode, $componentCode)
        }
        if ($target -ne $exe) { throw "Incorrect shortcut target: $target" }
        if ($shortcut.WorkingDirectory.TrimEnd('\') -ne $installDirectory.TrimEnd('\')) {
            throw "Incorrect shortcut working directory: $($shortcut.WorkingDirectory)"
        }
    }

    $application = Start-Process -FilePath $exe -WorkingDirectory $installDirectory -PassThru
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    do {
        Start-Sleep -Milliseconds 250
        $application.Refresh()
        if ($application.HasExited) { throw "Installed application exited unexpectedly: $($application.ExitCode)" }
    } until ($application.MainWindowTitle -eq 'MKtest' -or [DateTime]::UtcNow -ge $deadline)
    if ($application.MainWindowTitle -ne 'MKtest') { throw "Main form did not open: $($application.MainWindowTitle)" }
    $coreClr = @($application.Modules | Where-Object ModuleName -eq 'coreclr.dll')
    if ($coreClr.Count -ne 1 -or $coreClr[0].FileName -ne (Join-Path $installDirectory 'coreclr.dll')) {
        throw 'Application did not load its bundled .NET runtime.'
    }
    $appLogs = @(Get-ChildItem -LiteralPath (Join-Path $installDirectory 'logs') -Filter 'log_*.txt')
    if (-not ($appLogs | Select-String -SimpleMatch 'Приложение инициализирован')) {
        throw 'Application initialization was not confirmed in its log.'
    }
    if (-not $application.CloseMainWindow() -or -not $application.WaitForExit(15000)) {
        throw 'Application did not shut down cleanly.'
    }
    if ($application.ExitCode -ne 0) { throw "Application shutdown exit code: $($application.ExitCode)" }

    # User-generated settings must not be removed along with installed binaries.
    $userConfig = Join-Path $installDirectory 'config.json'
    $configContent = '{"setupSmokeTest":"preserve-user-data"}'
    Set-Content -LiteralPath $userConfig -Value $configContent -Encoding utf8
    Invoke-Msi '/x' 'uninstall.log'
    $installed = $false
    foreach ($path in @($exe) + $shortcuts) {
        if (Test-Path -LiteralPath $path) { throw "Uninstall left a program file or shortcut: $path" }
    }
    if ((Get-Content -LiteralPath $userConfig -Raw).Trim() -ne $configContent) {
        throw 'Uninstall changed user-generated settings.'
    }
    Write-Host "Verified $($publishedFiles.Count) files, both shortcuts, bundled runtime, application startup/shutdown and uninstall."
} finally {
    if ($null -ne $application -and -not $application.HasExited) {
        Stop-Process -Id $application.Id -Force
    }
    if ($installed) { Invoke-Msi '/x' 'cleanup-uninstall.log' }
}
