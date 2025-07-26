@echo off
REM Unity 6 CLI Test Runner - Workaround for broken -runTests flag
REM Created by Donatello (CLITestRunner solution) - 2025-07-26

echo ============================================
echo Unity 6 CLI Test Runner (TestRunnerApi)
echo ============================================

set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe"
set PROJECT_PATH="C:\Code\NeonLadderURP"
set LOG_PATH="C:\Code\NeonLadderURP\TestOutput\cli_test_execution.txt"
set RESULTS_PATH="C:/Users/Ender/AppData/LocalLow/ShorelineGames, LLC/NeonLadder/TestResults.xml"

echo Starting Unity CLI test execution...
echo Unity Path: %UNITY_PATH%
echo Project Path: %PROJECT_PATH%
echo Log Path: %LOG_PATH%
echo Results Path: %RESULTS_PATH%
echo.

REM Kill any existing Unity processes
echo Killing existing Unity processes...
powershell -Command "Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue"

REM Run PlayMode tests via our TestRunnerApi workaround
echo Executing PlayMode tests via TestRunnerApi...
%UNITY_PATH% -batchmode -projectPath %PROJECT_PATH% -executeMethod CLITestRunner.RunPlayModeTests -logFile %LOG_PATH%

REM Check if Unity process is still running (it shouldn't exit automatically due to callback issues)
timeout /t 10 /nobreak >nul

REM Force kill Unity after test execution
echo Terminating Unity process...
powershell -Command "Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue"

REM Check if results file exists
if exist %RESULTS_PATH% (
    echo.
    echo ============================================
    echo TEST RESULTS SUMMARY
    echo ============================================
    
    REM Parse XML results for summary
    findstr /C:"testcasecount.*result.*total.*passed.*failed" %RESULTS_PATH%
    
    REM Check for failures
    findstr /C:"failed=\"0\"" %RESULTS_PATH% >nul
    if %ERRORLEVEL%==0 (
        echo.
        echo ✅ ALL TESTS PASSED!
        exit /b 0
    ) else (
        echo.
        echo ❌ SOME TESTS FAILED!
        echo.
        echo Failed test details:
        findstr /C:"result=\"Failed\"" %RESULTS_PATH%
        exit /b 1
    )
) else (
    echo.
    echo ❌ ERROR: Test results file not found at %RESULTS_PATH%
    echo Check the log file at %LOG_PATH% for details.
    exit /b 1
)