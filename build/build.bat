@echo off

setlocal enabledelayedexpansion

cd /d "%~dp0"

set /p raw_version=<..\VERSION
set "version=%raw_version:v=%"
set "version=%version:-beta=%"
set "sirhurt_cleaner_cli_publish_dir=..\out\SirHurt.Cleaner.CLI"
set "sirstrap_cli_publish_dir=..\out\Sirstrap.CLI"
set "sirstrap_ui_publish_dir=..\out\Sirstrap.UI"
set "release_dir=..\out\release\Sirstrap_%raw_version%"

for %%d in ("%sirhurt_cleaner_cli_publish_dir%" "%sirstrap_cli_publish_dir%" "%sirstrap_ui_publish_dir%" "%release_dir%") do (
    if exist "%%~d" (
        rmdir /s /q "%%~d"
    )
)

for /r "..\src" %%p in (.cr .vs bin obj) do (
    if exist "%%~p" (
        rd /s /q "%%~p"
    )
)

dotnet restore ..\src\Sirstrap.slnx

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

dotnet publish ..\src\SirHurt.Cleaner.CLI\SirHurt.Cleaner.CLI.csproj -p:PublishProfile=FolderProfile -p:Version=%version% -c Release

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

del /f /q "%sirhurt_cleaner_cli_publish_dir%\*.pdb"
del /f /q "%sirhurt_cleaner_cli_publish_dir%\*.xml"

dotnet publish ..\src\Sirstrap.CLI\Sirstrap.CLI.csproj -p:PublishProfile=FolderProfile -p:Version=%version% -c Release

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_cli_publish_dir%\*.pdb"
del /f /q "%sirstrap_cli_publish_dir%\*.xml"

dotnet publish ..\src\Sirstrap.UI\Sirstrap.UI.csproj -p:PublishProfile=FolderProfile -p:Version=%version% -c Release

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

del /f /q "%sirstrap_ui_publish_dir%\*.pdb"
del /f /q "%sirstrap_ui_publish_dir%\*.xml"

mkdir "%release_dir%"

powershell Compress-Archive -Path "%sirhurt_cleaner_cli_publish_dir%\*" -DestinationPath "%release_dir%\SirHurt.Cleaner.CLI.zip" -Force

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

powershell Compress-Archive -Path "%sirstrap_cli_publish_dir%\*" -DestinationPath "%release_dir%\Sirstrap.CLI.zip" -Force

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

powershell Compress-Archive -Path "%sirstrap_ui_publish_dir%\*" -DestinationPath "%release_dir%\Sirstrap.UI.zip" -Force

if %ERRORLEVEL% neq 0 (
    exit /b %ERRORLEVEL%
)

exit /b 0

endlocal