# 📚 QuizCraft - Plataforma de Aprendizaje Inteligente

**QuizCraft** es una plataforma web de estudio personalizada desarrollada con **ASP.NET Core 8** que combina flashcards inteligentes, quizzes adaptativos y análisis de progreso para optimizar el aprendizaje. Utiliza **repetición espaciada** e **inteligencia artificial** para crear contenido educativo automático y personalizar la experiencia de estudio según el rendimiento del usuario.

## Información de la Asignatura
* **Carrera**: Ingeniería en Informática
* **Asignatura**: Inteligencia Artificial Aplicada (3668)
* **Cuatrimestre**: Segundo Cuatrimestre
* **Año**: 2025
* **Grupo**: 2

## Trayecto - Desarrollo de Software
* **Año académico**: Quinto Año - Primer Cuatrimestre
* **Responsable / Jefe de catedra**: Montefiori, Damian
* **Carga horaria semanal**: 4 hs
* **Carga horaria total**: 64 hs
* **Modalidad**: Virtual
* **Correlativas anteriores**
  + Estadística Aplicada (3656)
  + Inteligencia Artificial (3664)

## Docentes
* Montefiori, Damian

## Integrantes
| DNI | Apellido/s | Nombre/s |
|--|--|--|
| 43.630.151 | Antonioli | Iván Oscar |
| 40.742.053 | Berti | Rodrigo Nicolás |
| 43.089.397 | Fragassi | Donatella |
| 41.548.235 | Sandoval Vasquez | Juan Leandro |

## 🎯 Características Principales

- **🎴 Gestión de Materias y Flashcards** con algoritmos de repetición espaciada
- **📝 Quizzes Personalizados** con diferentes niveles de dificultad
- **🤖 Generación Automática de Contenido** mediante integración con Google Gemini
- **📊 Análisis de Progreso** con estadísticas detalladas y visualizaciones
- **🖼️ Soporte Multimedia** para imágenes, audio y documentos
- **👥 Colaboración** para compartir contenido entre usuarios

## 🛠️ Stack Tecnológico

- **Backend:** ASP.NET Core 8 MVC, Entity Framework Core, ASP.NET Identity
- **Frontend:** Razor Views, Bootstrap 5, JavaScript, Font Awesome
- **Base de Datos:** SQL Server con migraciones Code-First
- **IA:** Integración con Google Gemini API para generación automática
- **Arquitectura:** Clean Architecture con patrón Repository y Unit of Work

## 📋 Prerrequisitos

Antes de comenzar, asegúrate de tener instalado:

- **.NET 8 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server LocalDB** (incluido con Visual Studio) o SQL Server
- **Visual Studio 2022** o **Visual Studio Code** con extensión C#
- **Git** para clonar el repositorio

## 🚀 Getting Started

### 1. Clonar el Repositorio

```bash
git clone https://dev.azure.com/IAAplicadaGrupo2/QuizCraft/_git/QuizCraft
cd QuizCraft
```

### 2. Configurar la Base de Datos

```bash
# Navegar al proyecto principal
cd src/QuizCraft.Web

# Aplicar las migraciones de Entity Framework
dotnet ef database update
```

Si no tienes Entity Framework CLI instalado:
```bash
dotnet tool install --global dotnet-ef
```

### 3. Configurar la Cadena de Conexión (Opcional)

El proyecto usa LocalDB por defecto. Si necesitas cambiar la conexión, modifica `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuizCraftDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. Ejecutar la Aplicación

**Opción A: Con .NET CLI**
```bash
# Desde el directorio src/QuizCraft.Web
dotnet run
```

**Opción B: Con Visual Studio**
1. Abrir `src/QuizCraft.sln`
2. Establecer `QuizCraft.Web` como proyecto de inicio
3. Presionar **F5** o hacer clic en "Ejecutar"

### 5. Acceder a la Aplicación

Una vez ejecutada, la aplicación estará disponible en:
- **HTTPS:** https://localhost:7249
- **HTTP:** http://localhost:5291

## 🔐 Credenciales de Demostración

Para probar la aplicación, puedes usar las siguientes credenciales:
- **Usuario:** admin@quizcraft.com
- **Contraseña:** Admin123!

## 🏗️ Build and Test

### Compilar el Proyecto

```bash
# Compilar toda la solución
dotnet build src/QuizCraft.sln

# Compilar en modo Release
dotnet build src/QuizCraft.sln --configuration Release
```

### Ejecutar Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Verificar Calidad del Código

```bash
# Analizar código con sonar (si está configurado)
dotnet sonarscanner begin /k:"QuizCraft"
dotnet build
dotnet sonarscanner end
```

## 📂 Estructura del Proyecto

```
QuizCraft/
├── src/
│   ├── QuizCraft.Web/              # Proyecto principal MVC
│   ├── QuizCraft.Core/             # Entidades y lógica de dominio
│   ├── QuizCraft.Application/      # Servicios de aplicación y ViewModels
│   └── QuizCraft.Infrastructure/   # Repositorios y acceso a datos
├── deploy/                         # Scripts y notas de despliegue
└── README.md                       # Este archivo
```

## 🛠️ Software Dependencies

### Paquetes NuGet Principales

- **Microsoft.AspNetCore.App** (8.0)
- **Microsoft.EntityFrameworkCore.SqlServer** (8.0)
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (8.0)
- **Microsoft.EntityFrameworkCore.Tools** (8.0)
- **AutoMapper** (para mapeo de entidades)
- **FluentValidation** (para validaciones)

### Dependencias del Frontend

- **Bootstrap 5.3** - Framework CSS
- **Font Awesome 6** - Iconografía
- **jQuery 3.6** - Manipulación DOM
- **Chart.js** - Gráficos y estadísticas

## 🔧 Configuración de Desarrollo

### Secretos locales

No guardes claves ni cadenas de conexión en archivos versionados. Configúralas con
[User Secrets de .NET](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```powershell
# Ejecutar una única vez para habilitar User Secrets en el proyecto web
dotnet user-secrets init --project .\src\QuizCraft.Web

# SQL Server LocalDB (ajusta el servidor si usas otra instancia)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=QuizCraftDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True" --project .\src\QuizCraft.Web

# Gemini (opcional)
dotnet user-secrets set "Gemini:ApiKey" "TU_API_KEY_DE_GEMINI" --project .\src\QuizCraft.Web
```

En Azure, usa variables de entorno o referencias a Azure Key Vault, por ejemplo
`Gemini__ApiKey`; nunca incluyas su valor en `appsettings*.json`.

### Configuración de Google Gemini (Opcional)

Para habilitar la generación automática de contenido:

1. Obtén una API Key GRATUITA de [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Agrégala mediante User Secrets con `Gemini:ApiKey`
3. El sistema detectará automáticamente la disponibilidad

**💡 Ventajas de Gemini:**
- ✅ **Completamente gratuito** (no requiere tarjeta de crédito)
- ✅ **Límites generosos** para uso académico y desarrollo
- ✅ **Modelo avanzado** gemini-2.0-flash-exp
- ✅ **Solo necesitas** una cuenta de Google

## 🚨 Solución de Problemas Comunes

### Error: `The ConnectionString property has not been initialized`

Falta configurar `ConnectionStrings:DefaultConnection`. Configúrala con User Secrets
siguiendo la sección anterior y vuelve a iniciar el proyecto.

### Error de Migraciones

```bash
# Eliminar base de datos y recrear
dotnet ef database drop --force
dotnet ef database update
```

### Problemas con LocalDB

1. Verificar que SQL Server LocalDB esté instalado
2. Comprobar que el servicio esté ejecutándose:
   ```cmd
   sqllocaldb info mssqllocaldb
   ```

### Puertos Ocupados

Los puertos se configuran en `Properties/launchSettings.json`. Puedes cambiarlos si están ocupados.

## 🤝 Contribute

### Cómo Contribuir

1. **Fork** el proyecto desde Azure DevOps
2. Crea una **rama feature** para tu funcionalidad:
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```
3. **Commit** tus cambios con mensajes descriptivos:
   ```bash
   git commit -m "feat: agregar nueva funcionalidad X"
   ```
4. **Push** a tu rama:
   ```bash
   git push origin feature/nueva-funcionalidad
   ```
5. Crea un **Pull Request** en Azure DevOps

### Estándares de Código

- Seguir las convenciones de C# y .NET
- Usar nombres descriptivos para variables y métodos
- Incluir comentarios XML para métodos públicos
- Mantener cobertura de pruebas > 80%
- Validar que todas las pruebas pasen antes del PR

### Reportar Issues

Para reportar bugs o solicitar features:
1. Ve a la sección **Work Items** en Azure DevOps
2. Crea un nuevo **Bug** o **Feature Request**
3. Incluye información detallada y pasos para reproducir

## 📚 Latest Releases

### v1.0.0 (Actual)
- ✅ Sistema completo de materias y flashcards
- ✅ Autenticación con ASP.NET Identity
- ✅ Interfaz responsive con Bootstrap 5
- ✅ Base de datos con Entity Framework Core
- ✅ Arquitectura limpia y escalable

### Funcionalidades incluidas
- ✅ Integración con Google Gemini para generación automática
- ✅ Sistema de quizzes
- ✅ Estadísticas y dashboards

## 📖 API References

### Controladores Principales

- **`HomeController`** - Dashboard y páginas principales
- **`MateriaController`** - CRUD de materias
- **`AccountController`** - Autenticación y perfil de usuario
- **`FlashcardController`** - Gestión de flashcards

### Servicios de Aplicación

- **`IMateriaRepository`** - Repositorio de materias
- **`IUnitOfWork`** - Patrón Unit of Work
- **`IAIService`** - Integración con servicios de IA, incluido Google Gemini

## 📞 Información del Proyecto

- **Organización:** IAAplicadaGrupo2
- **Proyecto:** QuizCraft
- **Repositorio:** https://dev.azure.com/IAAplicadaGrupo2/QuizCraft
- **Documentación:** [Wiki del Proyecto](https://dev.azure.com/IAAplicadaGrupo2/QuizCraft/_wiki)

## 📄 Licencia

Este proyecto está desarrollado como parte de un proyecto académico para el curso de Inteligencia Artificial Aplicada.

---

## 🌟 Recursos Adicionales

- [Documentación de ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)
- [Scripts de despliegue](deploy/README.md)

---

> 💡 **¿Necesitas ayuda?** Abre un Work Item en Azure DevOps con los pasos para reproducir el problema.
