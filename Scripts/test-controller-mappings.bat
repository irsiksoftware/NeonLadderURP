@echo off
REM Controller Mapping Test Runner - Bruce Banner
REM Runs only controller mapping tests to verify multi-platform support

echo 🎮 NeonLadder Controller Mapping Test Suite
echo ============================================
echo By Bruce Banner - @bruce-banner
echo.

REM Kill any existing Unity processes
echo 🔄 Stopping any running Unity processes...
powershell -Command "Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue"

REM Wait a moment
timeout /t 2 /nobreak > nul

echo 🚀 Starting Controller Mapping Tests...
echo.

REM Run Unity CLI with controller mapping tests specifically
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" ^
    -batchmode ^
    -projectPath "C:\Code\NeonLadderURP" ^
    -executeMethod CLITestRunner.RunPlayModeTests ^
    -testFilter "ControllerMappingTests" ^
    -logFile "TestOutput/controller_mapping_tests.txt"

echo.
echo 📊 Test Results:
echo ================

REM Check if Unity is still running (it sometimes doesn't auto-exit)
timeout /t 30 /nobreak > nul
powershell -Command "Stop-Process -Name Unity -Force -ErrorAction SilentlyContinue"

REM Display test results if available
if exist "TestOutput\controller_mapping_tests.txt" (
    echo ✅ Test log generated: TestOutput\controller_mapping_tests.txt
    
    REM Check for test results in the log
    findstr /i "passing\|failing\|test.*result" "TestOutput\controller_mapping_tests.txt" > nul
    if !errorlevel! == 0 (
        echo 📋 Recent test results:
        findstr /i "passing\|failing\|test.*result" "TestOutput\controller_mapping_tests.txt"
    )
) else (
    echo ❌ No test log found - tests may not have executed
)

REM Check for XML results  
set "XML_RESULTS=C:\Users\Ender\AppData\LocalLow\ShorelineGames, LLC\NeonLadder\TestResults.xml"
if exist "%XML_RESULTS%" (
    echo ✅ XML test results found: %XML_RESULTS%
) else (
    echo ℹ️ No XML results found - tests may still be running or failed to start
)

echo.
echo 🔬 Controller Mapping Test Analysis Complete
echo Run 'Scripts\analyze-controller-results.bat' for detailed analysis

pause