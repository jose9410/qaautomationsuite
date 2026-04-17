@echo off
title KonciliaPruebas - Instalacion
echo.
echo ======================================================
echo        KonciliaPruebas - Instalacion Completa        
echo ======================================================
echo.

:: --- Paso 1: Restaurar paquetes NuGet ---
echo [1/4] Restaurando paquetes NuGet (.NET 9)...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Error restaurando paquetes NuGet.
    pause
    exit /b 1
)
echo [OK] Paquetes NuGet restaurados.
echo.

:: --- Paso 2: Instalar navegador Playwright ---
echo [2/4] Instalando navegador Playwright (Chromium)...
dotnet run -- playwright install chromium
if %ERRORLEVEL% NEQ 0 (
    echo [WARN] Intento alternativo con PowerShell...
    powershell -Command "& { $env:PLAYWRIGHT_BROWSERS_PATH='0'; dotnet build; npx playwright install chromium }" 2>nul
)
echo [OK] Navegador Playwright instalado.
echo.

:: --- Paso 3: Instalar dependencias Angular ---
echo [3/4] Instalando dependencias Angular (npm install)...
cd ClientApp
call npm install
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Error instalando dependencias Angular.
    cd ..
    pause
    exit /b 1
)
cd ..
echo [OK] Dependencias Angular instaladas.
echo.

:: --- Paso 4: Compilar Angular en wwwroot ---
echo [4/4] Compilando Angular en wwwroot...
cd ClientApp
call npx ng build --configuration=production
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Error compilando Angular.
    cd ..
    pause
    exit /b 1
)
cd ..
echo [OK] Angular compilado en wwwroot.
echo.

echo ======================================================
echo          [OK] Instalacion Completa                   
echo                                                      
echo   Ejecute run.bat para iniciar la aplicacion.        
echo                                                      
echo   IMPORTANTE: Edite appsettings.json con sus         
echo   credenciales de Windows Auth antes de usar.        
echo ======================================================
echo.
pause
