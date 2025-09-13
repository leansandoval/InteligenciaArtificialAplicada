
# HolaMundoWeb

Este proyecto es una aplicación ASP.NET Core 8 MVC con Razor Views y Bootstrap 5.

## Características
- Muestra "Hola mundo" en la página principal.
- Estructura estándar: Controllers, Views, wwwroot.
- Listo para desarrollo en Visual Studio Code en Windows.

## Requisitos
- .NET 8 SDK
- Visual Studio Code

## Ejecución
1. Abre la terminal en la raíz del proyecto.
2. Ejecuta:
	```pwsh
	dotnet restore
	dotnet build
	dotnet run
	```
3. Accede a `http://localhost:5000` o el puerto indicado en la terminal.

## Estructura
- `Controllers/` - Controladores MVC
- `Views/` - Vistas Razor
- `wwwroot/` - Archivos estáticos (Bootstrap, CSS, JS)

## Personalización
Puedes modificar la vista principal en `Views/Home/Index.cshtml` para cambiar el mensaje o el diseño.

---
Generado por GitHub Copilot