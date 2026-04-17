@echo off
title QA Automation Suite - Servidor Local
echo.
echo ======================================================
echo        QA Automation Suite - Iniciando Servidor          
echo ======================================================
echo.

:: --- Recompilar Angular (por si hubo cambios) ---
echo Compilando Angular...
cd ClientApp
call npx ng build --configuration=production
cd ..
echo.

:: --- Iniciar servidor .NET ---
echo Iniciando servidor en http://localhost:5100
echo (El navegador se abrira automaticamente)
echo.
echo Presione Ctrl+C para detener.
echo -----------------------------------------
echo.
dotnet run
pause
