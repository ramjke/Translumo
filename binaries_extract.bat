@echo off
setlocal EnableDelayedExpansion

set "componentsPath=%~dp0ext_components"
set "logFile=%~dp0download_log.txt"

:: Step 1: Create or overwrite the log file at the start
echo Log started at %date% %time% > "%logFile%"

:: Step 2: Check if the components folder exists
if NOT exist %componentsPath% (
    :: Log the creation of the folder
    echo "Folder '%componentsPath%' does not exist. Creating folder." >> "%logFile%"
    mkdir %componentsPath%
) else (
    :: Step 3: If folder exists, ask user if they want to re-download the files
    set /p downloadChoice="The folder '%componentsPath%' already exists. Do you want to download the files again? (yes/no): "
    
    :: Trim and normalize user input to handle different responses (y, yes, Y, YES)
    set "downloadChoice=!downloadChoice: =!"
    set "downloadChoice=!downloadChoice:~0,1!"

    echo "User selected: !downloadChoice!" >> "%logFile%"

    if /i "!downloadChoice!" == "y" (
        goto DownloadFiles
    ) else (
        echo "User chose not to re-download files." >> "%logFile%"
        goto ContinueScript
    )
)

:DownloadFiles
:: Step 4: Download the files and log the process
powershell -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; try { Invoke-WebRequest 'https://github.com/Danily07/Translumo/releases/download/v.0.8.5/_Components-v.0.8.0.zip' -OutFile '%~dp0components.zip'; Write-Output 'Download successful' } catch { Write-Error $_.Exception.Message }" >> "%logFile%" 2>&1

:: Check if the zip file was downloaded successfully
if exist "%~dp0components.zip" (
    echo "Download successful." >> "%logFile%"
    powershell Expand-Archive "%~dp0components.zip" -DestinationPath !componentsPath!
    del "%~dp0components.zip"
) else (
    echo "Download failed or no output was generated. Check previous log entries for errors." >> "%logFile%"
    exit /b 1
)

:ContinueScript
:: Step 5: Continue with the rest of the script and log missing paths

set "targetPaths[0]=%1python\"
set "targetPaths[1]=%1models\easyocr\"
set "targetPaths[2]=%1models\tessdata\"
set "targetPaths[3]=%1models\prediction\"

set "inputBinariesPaths[0]=%componentsPath%\python"
set "inputBinariesPaths[1]=%componentsPath%\models\easyocr"
set "inputBinariesPaths[2]=%componentsPath%\models\tessdata"
set "inputBinariesPaths[3]=%componentsPath%\models\prediction"

for %%i in (0,1,2,3) do (
    if NOT exist !targetPaths[%%i]! (
        echo "Path not found: !targetPaths[%%i]!" >> "%logFile%"
        xcopy /s !inputBinariesPaths[%%i]! !targetPaths[%%i]!
    )
)

:: Final log entry
echo "Script completed at %date% %time%" >> "%logFile%"
