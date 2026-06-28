@echo off
title Pokédex
echo.
echo  Pokédex - choix de l'interface
echo  ------------------------------
echo  1. Application Windows (WPF)
echo  2. Version web (navigateur)
echo.
set /p choice="Votre choix (1 ou 2) : "

if "%choice%"=="1" (
  cd /d "%~dp0csharp"
  call RunPokedex.bat
) else if "%choice%"=="2" (
  cd /d "%~dp0web"
  call RunPokedex.bat
) else (
  echo Choix invalide.
  pause
)
