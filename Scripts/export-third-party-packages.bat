@echo off
REM Third-Party Asset Unity Package Export Script
REM Exports all third-party assets as .unitypackage files for easy backup and restore

echo Starting Unity package export for third-party assets...

REM Set paths
set PROJECT_ROOT=C:\Users\Ender\NeonLladder
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe"
set GDRIVE_BACKUP="C:\tools\gdrive"
set BACKUP_FOLDER_ID=your-gdrive-folder-id-here

REM Create timestamp for backup
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "datestamp=%YYYY%-%MM%-%DD%_%HH%-%Min%"

echo Export timestamp: %datestamp%

REM Create backup directory
set PACKAGE_BACKUP=%TEMP%\NeonLadder_UnityPackages_%datestamp%
mkdir "%PACKAGE_BACKUP%"

echo Exporting Unity packages...

REM Export Pixel Crushers Dialogue System
echo Exporting Pixel Crushers Dialogue System...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Plugins/Pixel Crushers" "%PACKAGE_BACKUP%/PixelCrushers_DialogueSystem_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: Pixel Crushers export may have issues

REM Export DOTween
echo Exporting DOTween...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Plugins/Demigiant" "%PACKAGE_BACKUP%/DOTween_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: DOTween export may have issues

REM Export DamageNumbersPro
echo Exporting DamageNumbersPro...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Packages/DamageNumbersPro" "%PACKAGE_BACKUP%/DamageNumbersPro_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: DamageNumbersPro export may have issues

REM Export Modern UI Pack
echo Exporting Modern UI Pack...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Packages/Modern UI Pack" "%PACKAGE_BACKUP%/ModernUIPack_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: Modern UI Pack export may have issues

REM Export Hero Editor
echo Exporting Hero Editor...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Packages/HeroEditor" "%PACKAGE_BACKUP%/HeroEditor_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: Hero Editor export may have issues

REM Export SkyMaster URP
echo Exporting SkyMaster URP...
%UNITY_PATH% -batchmode -projectPath "%PROJECT_ROOT%" -exportPackage "Assets/Packages/SkyMasterURP" "%PACKAGE_BACKUP%/SkyMasterURP_%datestamp%.unitypackage" -quit
if errorlevel 1 echo WARNING: SkyMaster URP export may have issues

REM Create package manifest
echo Creating package manifest...
echo NeonLadder Third-Party Unity Packages > "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo Export Date: %datestamp% >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo Unity Version: 6000.0.26f1 >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo. >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo Exported Packages: >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - PixelCrushers_DialogueSystem_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - DOTween_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - DamageNumbersPro_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - ModernUIPack_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - HeroEditor_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo - SkyMasterURP_%datestamp%.unitypackage >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo. >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo Restore Instructions: >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo 1. Open Unity project >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo 2. Assets -^> Import Package -^> Custom Package >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo 3. Select each .unitypackage file and import >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"
echo 4. Resolve any dependency conflicts as needed >> "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt"

REM Upload to Google Drive
echo Uploading packages to Google Drive...
for %%f in ("%PACKAGE_BACKUP%\*.unitypackage") do (
    echo Uploading %%~nxf...
    %GDRIVE_BACKUP% files upload "%%f" --parent %BACKUP_FOLDER_ID%
)

REM Upload manifest
%GDRIVE_BACKUP% files upload "%PACKAGE_BACKUP%\PACKAGE_MANIFEST.txt" --parent %BACKUP_FOLDER_ID%

echo ✅ Unity package export completed!
echo Packages saved and uploaded:
dir "%PACKAGE_BACKUP%\*.unitypackage" /B

REM Optional: Keep local copies
set /p KEEP_LOCAL="Keep local copies? (y/N): "
if /i not "%KEEP_LOCAL%"=="y" (
    echo Cleaning up local files...
    rmdir /S /Q "%PACKAGE_BACKUP%"
) else (
    echo Local packages kept at: %PACKAGE_BACKUP%
)

pause