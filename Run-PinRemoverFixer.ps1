<#
.SYNOPSIS
    Launcher for "PIN Remover Fixer.exe". Ensures .NET Framework 4.8 is present
    (installing it silently from Microsoft's offline installer if needed), then
    starts the tool. Distribute this file alongside the exe.

.PARAMETER CheckOnly
    Only report whether .NET Framework 4.8 is detected; do not install or launch anything.
#>
param(
    [switch]$CheckOnly
)

$ErrorActionPreference = 'Stop'

# .NET Framework 4.8 registry "Release" values (per Microsoft's version table).
# 528040 = Win10 May 2019 Update and earlier OS builds; 528049/528372/528449/533320
# cover later Windows/Server releases. >= 528040 reliably means 4.8 or later.
$Net48ReleaseMin = 528040
$NdpKey = 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
$InstallerUrl = 'https://go.microsoft.com/fwlink/?linkid=2088631'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ToolExe = Join-Path $ScriptDir 'PIN Remover Fixer.exe'

function Test-Net48Installed {
    if (-not (Test-Path $NdpKey)) { return $false }
    $release = (Get-ItemProperty -Path $NdpKey -Name Release -ErrorAction SilentlyContinue).Release
    return ($null -ne $release) -and ($release -ge $Net48ReleaseMin)
}

$installed = Test-Net48Installed

if ($CheckOnly) {
    if ($installed) {
        Write-Host ".NET Framework 4.8 (or later) is already installed."
    } else {
        Write-Host ".NET Framework 4.8 is NOT installed."
    }
    exit 0
}

if (-not $installed) {
    Write-Host ".NET Framework 4.8 not detected. Downloading offline installer..."

    $installerPath = Join-Path $env:TEMP 'ndp48-x86-x64-allos-enu.exe'

    try {
        Invoke-WebRequest -Uri $InstallerUrl -OutFile $installerPath -UseBasicParsing
    } catch {
        Write-Error "Failed to download .NET Framework 4.8 installer: $_"
        exit 1
    }

    Write-Host "Installing .NET Framework 4.8 silently (this can take several minutes)..."
    $proc = Start-Process -FilePath $installerPath -ArgumentList '/q', '/norestart' -Wait -PassThru
    Remove-Item $installerPath -ErrorAction SilentlyContinue

    switch ($proc.ExitCode) {
        0    { Write-Host ".NET Framework 4.8 installed successfully." }
        3010 {
            Write-Warning ".NET Framework 4.8 installed but a reboot is required before the tool can run. Please restart the computer and run this launcher again."
            exit 3010
        }
        1641 {
            Write-Warning ".NET Framework 4.8 installed; a reboot has been initiated."
            exit 1641
        }
        default {
            Write-Error ".NET Framework 4.8 installation failed (exit code $($proc.ExitCode))."
            exit $proc.ExitCode
        }
    }

    if (-not (Test-Net48Installed)) {
        Write-Error "Installer reported success but .NET Framework 4.8 still not detected. Aborting."
        exit 1
    }
}

if (-not (Test-Path $ToolExe)) {
    Write-Error "Could not find '$ToolExe'. Make sure this script sits next to 'PIN Remover Fixer.exe'."
    exit 1
}

Start-Process -FilePath $ToolExe
