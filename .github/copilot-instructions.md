Este proyecto usa Azure DevOps. Comprueba siempre si el servidor MCP de Azure DevOps tiene una herramienta relevante para la solicitud del usuario

# Arquitectura: MVC + Entity Framework
Backend: ASP.NET Core 8, C#, Entity Framework Core Code-First
Frontend: Razor Views + Bootstrap 5, Javascript (Vanilla JS + jQuery si es necesario)
Database: SQL Server LocalDB (dev) / SQL Server (prod)
IA: OpenAI GPT-4o

# Nomenclatura:
PascalCase para clases/métodos, camelCase para variables
Funciones: Comienzan con FUNC_ y verbos descriptivos

# Formato de fechas:
Usa siempre 'YYYY-MM-DD' para fechas en JS y 'yyyy-MM-dd' en C#

# Comentarios:
Agrega comentarios claros y concisos en el código

# Idioma:
Responde siempre en español

# Test de javascript:
Usa Jest para pruebas unitarias en JS. Crea archivos .test.js junto a los archivos fuente.

# Refactor:
Refactoriza código repetitivo en funciones reutilizables
Refactoriza lógica compleja en funciones separadas
 