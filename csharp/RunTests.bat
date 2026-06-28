@echo off
echo Lancement des tests unitaires...
echo.

REM Vérifier si .NET est installé
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK n'est pas installé.
    echo Veuillez installer .NET 8.0 ou supérieur depuis https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Exécuter les tests
cd Pokedex.Tests
echo Exécution des tests unitaires...
dotnet test --configuration Release

cd ..
pause
