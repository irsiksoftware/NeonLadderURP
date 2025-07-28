# Unity 6 CLI Test Runner - PowerShell Version
# Created by Donatello (CLITestRunner solution) - 2025-07-26
# Workaround for broken Unity 6 -runTests flag

param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe",
    [string]$ProjectPath = "C:\Code\NeonLadderURP",
    [string]$LogPath = "C:\Code\NeonLadderURP\TestOutput\cli_test_execution.txt",
    [string]$ResultsPath = "C:/Users/Ender/AppData/LocalLow/ShorelineGames, LLC/NeonLadder/TestResults.xml",
    [int]$TimeoutSeconds = 300
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Unity 6 CLI Test Runner (TestRunnerApi)" -ForegroundColor Cyan  
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Unity Path: $UnityPath"
Write-Host "  Project Path: $ProjectPath"
Write-Host "  Log Path: $LogPath"
Write-Host "  Results Path: $ResultsPath"
Write-Host "  Timeout: $TimeoutSeconds seconds"
Write-Host ""

# Kill any existing Unity processes
Write-Host "Killing existing Unity processes..." -ForegroundColor Yellow
try {
    Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
} catch {
    Write-Host "No Unity processes to kill." -ForegroundColor Gray
}

# Ensure TestOutput directory exists
$logDir = Split-Path $LogPath -Parent
if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# Run PlayMode tests via TestRunnerApi workaround
Write-Host "Executing PlayMode tests via TestRunnerApi..." -ForegroundColor Green

$process = Start-Process -FilePath $UnityPath -ArgumentList @(
    "-batchmode",
    "-projectPath", "`"$ProjectPath`"",
    "-executeMethod", "CLITestRunner.RunPlayModeTests",
    "-logFile", "`"$LogPath`""
) -PassThru -NoNewWindow

# Wait for process completion or timeout
Write-Host "Waiting for test execution to complete (timeout: $TimeoutSeconds seconds)..." -ForegroundColor Yellow

$completed = $process.WaitForExit($TimeoutSeconds * 1000)

if (!$completed) {
    Write-Host "Test execution timed out after $TimeoutSeconds seconds. Terminating Unity..." -ForegroundColor Red
    try {
        Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue
    } catch {
        Write-Host "Failed to terminate Unity process." -ForegroundColor Red
    }
} else {
    Write-Host "Unity process completed." -ForegroundColor Green
}

# Additional safety - ensure Unity is terminated
Start-Sleep -Seconds 3
Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "TEST RESULTS ANALYSIS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Check if results file exists
if (Test-Path $ResultsPath) {
    Write-Host "✅ Test results file found!" -ForegroundColor Green
    
    # Parse XML results
    try {
        [xml]$xmlResults = Get-Content $ResultsPath
        $testRun = $xmlResults."test-run"
        
        $totalTests = $testRun.total
        $passedTests = $testRun.passed  
        $failedTests = $testRun.failed
        $skippedTests = $testRun.skipped
        
        Write-Host ""
        Write-Host "📊 TEST SUMMARY:" -ForegroundColor Cyan
        Write-Host "  Total Tests: $totalTests" -ForegroundColor White  
        Write-Host "  Passed: $passedTests" -ForegroundColor Green
        Write-Host "  Failed: $failedTests" -ForegroundColor $(if($failedTests -eq "0") {"Green"} else {"Red"})
        Write-Host "  Skipped: $skippedTests" -ForegroundColor Yellow
        
        if ($failedTests -eq "0") {
            Write-Host ""
            Write-Host "🎉 ALL TESTS PASSED!" -ForegroundColor Green -BackgroundColor DarkGreen
            exit 0
        } else {
            Write-Host ""
            Write-Host "❌ $failedTests TESTS FAILED!" -ForegroundColor Red -BackgroundColor DarkRed
            
            # Show failed test details
            $failedTestCases = $xmlResults.SelectNodes("//test-case[@result='Failed']")
            if ($failedTestCases.Count -gt 0) {
                Write-Host ""
                Write-Host "Failed Tests:" -ForegroundColor Red
                foreach ($failedTest in $failedTestCases) {
                    Write-Host "  - $($failedTest.fullname)" -ForegroundColor Red
                    if ($failedTest.failure) {
                        Write-Host "    Error: $($failedTest.failure.message)" -ForegroundColor Gray
                    }
                }
            }
            exit 1
        }
        
    } catch {
        Write-Host "❌ ERROR: Failed to parse test results XML: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
    
} else {
    Write-Host "❌ ERROR: Test results file not found at: $ResultsPath" -ForegroundColor Red
    Write-Host "Check the log file for details: $LogPath" -ForegroundColor Yellow
    
    if (Test-Path $LogPath) {
        Write-Host ""
        Write-Host 'Last 20 lines of log file:' -ForegroundColor Yellow
        Get-Content $LogPath | Select-Object -Last 20 | Out-Host
    }
    
    exit 1
}