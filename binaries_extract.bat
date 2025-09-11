@echo off
setlocal EnableDelayedExpansion

:: Paths
set "componentsPath=%~dp0ext_components"
set "zipFile=%~dp0components.zip"
set "downloadUrl=https://github.com/ramjke/Translumo/releases/download/v.0.8.5/_components_v.0.8.0_case_sensitive_fix.zip"

set "targetPaths[0]=%~1python\"
set "targetPaths[1]=%~1models\easyocr\"
set "targetPaths[2]=%~1models\tessdata\"
set "targetPaths[3]=%~1models\prediction\"

set "inputBinariesPaths[0]=%componentsPath%\python"
set "inputBinariesPaths[1]=%componentsPath%\models\easyocr"
set "inputBinariesPaths[2]=%componentsPath%\models\tessdata"
set "inputBinariesPaths[3]=%componentsPath%\models\prediction"

echo Prebuild: Starting binaries downloading and extraction (binaries_extract.bat)...

:: Check if target folders already exist
set "allFoldersExist=1"
for %%i in (0,1,2,3) do if NOT exist "!targetPaths[%%i]!" set "allFoldersExist=0"

if "!allFoldersExist!"=="1" (
    echo Folders exist in the target location. Extraction and copy skipped.
    exit /b 0
)

:: Ensure components folder exists
if NOT exist "%componentsPath%" (
    mkdir "%componentsPath%"
)

:TryExtract
echo Attempting to extract components.zip...
powershell -Command "try { Expand-Archive -Path '%zipFile%' -DestinationPath '%componentsPath%' -Force; Write-Output 'Extraction successful' } catch { exit 1 }"

if errorlevel 1 (
    echo Zip missing or corrupted. Downloading components.zip...
    if exist "%zipFile%" del "%zipFile%"
    powershell -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; try { Invoke-WebRequest '%downloadUrl%' -OutFile '%zipFile%'; Write-Output 'Download successful' } catch { Write-Output $_.Exception.Message; exit 1 }"
    goto TryExtract
)

echo Copy binary/model folders to target locations...

for %%i in (0,1,2,3) do (
    if NOT exist "!targetPaths[%%i]!" (
        echo Path not found: !targetPaths[%%i]!
        mkdir "!targetPaths[%%i]!" 2>nul
        xcopy /s /y /i "!inputBinariesPaths[%%i]!" "!targetPaths[%%i]!"
    )
)

echo Binaries downloading and extraction finished.
exit /b 0