@echo off
echo Nettoyage des fichiers de compilation temporaires...

REM Suppression des dossiers bin et obj de tous les projets
FOR /D %%d IN (*) DO (
    IF EXIST "%%d\bin" (
        echo Suppression de %%d\bin
        rmdir /S /Q "%%d\bin"
    )
    IF EXIST "%%d\obj" (
        echo Suppression de %%d\obj
        rmdir /S /Q "%%d\obj"
    )
)

echo.
echo Nettoyage terminé! Les fichiers bin et obj ont été supprimés.
echo Cela réduira la taille de l'archive et éliminera les chemins absolus.
echo.
pause
