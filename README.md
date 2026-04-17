# QA Automation Suite

![Estado](https://img.shields.io/badge/Estado-Activo-success) ![Plataforma](https://img.shields.io/badge/Plataforma-.NET_9_+_Angular_17-blue) ![Herramienta](https://img.shields.io/badge/Automatizaci%C3%B3n-Playwright-green)

Una potente suite local de **Robot Process Automation (RPA)** diseñada específicamente para interactuar, navegar y ejecutar reprocesos automatizados en masa sobre sistemas web de infraestructura heredada y plataformas modernas.

## ✨ Características Principales

- **Ejecución en Lotes (Batch):** Capacidad de ejecutar "N" sub-procesos consecutivos configurados bajo una misma aplicación principal, automatizando por completo las entradas de datos (Años, Meses, Días) extraídos del Backend.
- **Auto-Navegación e Intercepción Dinámica:** Configurado para sortear obstáculos como alertas del sistema nativo del navegador (`Window.confirm()`) sin detener el flujo.
- **Arquitectura Basada en Configuración:** Añade o modifica flujos (URLs, GUIDs, timeouts, fechas quemadas) sin tener que compilar el código. Todo vive en un archivo dinámico `appsettings.json`.
- **UI Reactiva Minimalista:** Un Frontend limpio de un-solo-clic que oculta la complejidad del RPA al usuario, proporcionando una barra de progreso que indica los pasos ejecutados y genera un recibo al finalizar cada lote.

## 🛠 Tecnologías Utilizadas

- **Backend:** C# con **ASP.NET Core (.NET 9)** estructurador de API REST.
- **Frontend:** **Angular 17+** servido de forma nativa desde la misma instancia del proyecto usando SPA fallback (monolítico).
- **Motor de Automatización:** Microsoft **Playwright** (Arquitectura Chromium) para Web Scraping y simulación de flujo. 
- **Manejo en Memoria:** Procesos `Thread-Safe` con `ConcurrentDictionary` para control de estado continuo sin saturar bases de datos en disco.

## 🚀 Instalación y Uso

**Requisitos previos:**
- Tener .NET SDK 9.0 o superior instalado.
- Node.js (con Angular CLI instalado globalmente `npm i -g @angular/cli`).

**Pasos para ejecutar:**
1. Clonar el repositorio.
2. (Si tienes variables y credenciales que proteger) Configura tu archivo `appsettings.json` o crea uno nuevo siguiendo las reglas originales ignoradas por Git.
3. Ejecutar el script `run.bat` que automatiza la transpilación del Frontend y levanta el servidor Kestrel subyacente de C#.
4. Acceder al sistema abriendo el navegador (en caso de que no auto-lance) a `http://localhost:5100`.

## 📂 Archivos Importantes

- `appsettings.json`: *(Ignorado en control de versiones)*. El motor y los enrutamientos de la automatización se alimentan de acá, así como el login.
- `PlaywrightService.cs`: Archivo núcleo del controlador RPA C#.
- `app.component.ts`: Front controller de Angular para el sistema de estados.

---
*Desarrollado para automatización de ciclo y cruces en Koncilia automatizado.*
