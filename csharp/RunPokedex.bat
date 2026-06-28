@echo off
cd /d "%~dp0"
echo Lancement du Pokédex (WPF)...
dotnet run --project Pokedex.Wpf --configuration Release
