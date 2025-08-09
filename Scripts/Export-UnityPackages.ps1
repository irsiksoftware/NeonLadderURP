#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Export Unity packages and upload to Google Drive
.DESCRIPTION
    Cross-platform PowerShell script for automating Unity package exports
    Works on Windows, macOS, and Linux with PowerShell Core
.PARAMETER ProjectPath
    Path to Unity project (default: parent directory)
.PARAMETER UnityPath
    Path to Unity executable (auto-detected if not specified)
.PARAMETER Packages
    Specific packages to export (default: all with DownloadInstructions.txt)
.PARAMETER SkipUpload
    Skip Google Drive upload step
.PARAMETER DryRun
    Show what would be exported without actually doing it
.EXAMPLE
    .\Export-UnityPackages.ps1 -DryRun
.EXAMPLE
    .\Export-UnityPackages.ps1 -Packages "HeroEditor" "LeartesStudios/CyberpunkCity"
#>

[CmdletBinding()]
param(
    [string]$ProjectPath = (Split-Path -Parent $PSScriptRoot),
    [string]$UnityPath = "",
    [string[]]$Packages = @(),
    [switch]$SkipUpload,
    [switch]$DryRun
)

# Set up colors for output
$InformationPreference = 'Continue'

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = 'White'
    )
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "Unity Package Export Tool" -Color Green
Write-ColorOutput ("=" * 40) -Color Green

# Check Python installation
function Get-PythonCommand {
    $pythonCommands = @('python3', 'python', 'py')
    
    foreach ($cmd in $pythonCommands) {
        if (Get-Command $cmd -ErrorAction SilentlyContinue) {
            # Verify it's Python 3
            $version = & $cmd --version 2>&1
            if ($version -match "Python 3") {
                return $cmd
            }
        }
    }
    
    throw "Python 3 is not installed or not in PATH"
}

try {
    $pythonCmd = Get-PythonCommand
    $pythonVersion = & $pythonCmd --version
    Write-ColorOutput "Using $pythonVersion" -Color Cyan
} catch {
    Write-ColorOutput "Error: $_" -Color Red
    exit 1
}

# Check if Unity is running
function Test-UnityRunning {
    if ($IsWindows -or $PSVersionTable.Platform -eq 'Win32NT') {
        $unityProcess = Get-Process -Name Unity -ErrorAction SilentlyContinue
    } elseif ($IsMacOS -or $PSVersionTable.Platform -eq 'Unix') {
        $unityProcess = Get-Process | Where-Object { $_.ProcessName -like "*Unity*" }
    } else {
        $unityProcess = $null
    }
    
    return $null -ne $unityProcess
}

if (Test-UnityRunning) {
    Write-ColorOutput "Warning: Unity appears to be running" -Color Yellow
    Write-ColorOutput "Please close Unity before exporting packages" -Color Yellow
    
    if (-not $DryRun) {
        $response = Read-Host "Press Enter to continue anyway, or Ctrl+C to cancel"
    }
}

# Build Python script arguments
$scriptPath = Join-Path $PSScriptRoot "export_unity_packages.py"
$arguments = @(
    $scriptPath,
    "--project-path", $ProjectPath
)

if ($UnityPath) {
    $arguments += @("--unity-path", $UnityPath)
}

if ($Packages.Count -gt 0) {
    $arguments += "--packages"
    $arguments += $Packages
}

if ($SkipUpload) {
    $arguments += "--skip-upload"
}

if ($DryRun) {
    $arguments += "--dry-run"
    Write-ColorOutput "Running in dry-run mode (no actual exports)" -Color Yellow
}

# Change to project directory
Push-Location $ProjectPath

try {
    Write-ColorOutput "Starting package export..." -Color Cyan
    
    # Run the Python script
    $process = Start-Process -FilePath $pythonCmd -ArgumentList $arguments -NoNewWindow -PassThru -Wait
    $exitCode = $process.ExitCode
    
    if ($exitCode -eq 0) {
        Write-ColorOutput "Export completed successfully!" -Color Green
    } else {
        Write-ColorOutput "Export failed with code $exitCode" -Color Red
    }
    
    exit $exitCode
} finally {
    Pop-Location
}