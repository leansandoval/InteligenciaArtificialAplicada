# Funcionalidad de Compartir Quizzes - Documentación

## Resumen General
Se ha implementado una funcionalidad completa para compartir quizzes entre usuarios de QuizCraft. Los usuarios pueden generar códigos únicos de compartición para sus quizzes y otros usuarios pueden importar estos quizzes a su propia colección.

## Características Implementadas

### 1. Compartir Quiz
- **Ruta**: `/QuizCompartido/Compartir/{quizId}`
- **Funcionalidad**: 
  - Genera un código alfanumérico único de 8 caracteres
  - Permite configurar fecha de expiración (opcional)
  - Permite limitar el número máximo de usos (opcional)
  - Opción para permitir/restringir modificaciones del quiz importado

### 2. Importar Quiz
- **Ruta**: `/QuizCompartido/Importar`
- **Funcionalidad**:
  - Importa un quiz usando un código de compartición
  - Muestra información previa del quiz antes de importar
  - Permite seleccionar la materia de destino
  - Crea una copia completa del quiz con todas sus preguntas
  - Valida expiración y límites de uso

### 3. Gestión de Quizzes Compartidos
- **Ruta**: `/QuizCompartido/Index`
- **Funcionalidad**:
  - Lista de quizzes compartidos por el usuario (con códigos y estadísticas)
  - Lista de quizzes importados de otros usuarios
  - Opción para revocar comparticiones
  - Botón de copiar código al portapapeles
  - Indicadores de estado (activo, expirado, agotado)

## Componentes Técnicos Creados

### Entidades (Core Layer)
1. **QuizCompartido** - `Core/Entities/QuizCompartido.cs`
   - Almacena información de compartición
   - Propiedades: código, expiración, límites de uso, permisos

2. **QuizImportado** - `Core/Entities/QuizImportado.cs`
   - Registra importaciones realizadas por usuarios
   - Relación entre quiz compartido, quiz importado y usuario

### Repositorios (Infrastructure Layer)
1. **IQuizCompartidoRepository** - `Core/Interfaces/IQuizCompartidoRepository.cs`
   - Interfaz del repositorio

2. **QuizCompartidoRepository** - `Infrastructure/Repositories/QuizCompartidoRepository.cs`
   - Implementación con consultas especializadas
   - Incluye eager loading de relaciones

### Servicios (Infrastructure Layer)
1. **IQuizCompartidoService** - `Application/Interfaces/IQuizCompartidoService.cs`
   - Interfaz del servicio con lógica de negocio

2. **QuizCompartidoService** - `Infrastructure/Services/QuizCompartidoService.cs`
   - Generación de códigos únicos
   - Validaciones de permisos y límites
   - Clonación completa de quizzes

### ViewModels (Application Layer)
1. **QuizCompartidoViewModels.cs** - `Application/ViewModels/QuizCompartidoViewModels.cs`
   - CompartirQuizViewModel
   - ImportarQuizViewModel
   - QuizzesCompartidosViewModel
   - QuizCompartidoListItem
   - QuizImportadoListItem

### Controlador (Web Layer)
**QuizCompartidoController** - `Web/Controllers/QuizCompartidoController.cs`
- Index: Lista de compartidos e importados
- Compartir (GET/POST): Formulario y procesamiento de compartición
- Importar (GET/POST): Formulario y procesamiento de importación
- Revocar: Desactivar una compartición

### Vistas (Web Layer)
1. **Index.cshtml** - `Web/Views/QuizCompartido/Index.cshtml`
   - Tabs para quizzes compartidos e importados
   - Cards con información detallada
   - Botones de acción (ver, jugar, revocar)

2. **Compartir.cshtml** - `Web/Views/QuizCompartido/Compartir.cshtml`
   - Formulario con opciones de compartición
   - Validaciones del lado del cliente

3. **Importar.cshtml** - `Web/Views/QuizCompartido/Importar.cshtml`
   - Formulario de entrada de código
   - Vista previa del quiz a importar
   - Selección de materia destino

### Migración de Base de Datos
**AgregarQuizzesCompartidos** - `Infrastructure/Migrations/...`
- Tabla QuizzesCompartidos con índices
- Tabla QuizzesImportados con relaciones
- Índice único en CodigoCompartido

## Integraciones en la UI

### Menú de Navegación
Se agregó el enlace "Quizzes Compartidos" en el menú desplegable "Estudio":
```
Estudio > Quizzes Compartidos
```

### Vista de Detalles del Quiz
Se agregó el botón "Compartir" junto a los botones "Editar" y "Eliminar" en la vista de detalles del quiz (solo visible para el propietario).

## Validaciones Implementadas

### Compartir Quiz
- ✅ El quiz debe existir
- ✅ Solo el propietario puede compartir
- ✅ El quiz debe tener al menos una pregunta
- ✅ Fecha de expiración debe ser futura
- ✅ Máximo de usos debe estar entre 1 y 1000

### Importar Quiz
- ✅ El código debe existir y ser válido
- ✅ El quiz no debe estar expirado
- ✅ No debe haber alcanzado el límite de usos
- ✅ El usuario no puede importar su propio quiz
- ✅ El usuario no puede importar el mismo quiz dos veces
- ✅ La materia destino debe pertenecer al usuario

## Seguridad

1. **Códigos Únicos**: Generados con `RandomNumberGenerator` (criptográficamente seguros)
2. **Validación de Propiedad**: Solo el propietario puede compartir o revocar
3. **Prevención de Auto-Importación**: No se puede importar el propio quiz
4. **Prevención de Duplicados**: No se puede importar el mismo quiz más de una vez
5. **Control de Acceso**: Todas las rutas requieren autenticación (`[Authorize]`)

## Flujo de Usuario

### Compartir un Quiz
1. Usuario navega a "Mis Quizzes"
2. Selecciona un quiz y abre sus detalles
3. Click en botón "Compartir"
4. Configura opciones (expiración, límites, permisos)
5. Click en "Generar Código de Compartición"
6. Sistema genera código único y muestra confirmación
7. Usuario puede copiar el código desde "Quizzes Compartidos"

### Importar un Quiz
1. Usuario navega a "Quizzes Compartidos"
2. Click en "Importar Quiz" o usa el enlace del menú
3. Ingresa el código de 8 caracteres
4. Click en "Buscar Quiz"
5. Sistema muestra información del quiz
6. Usuario selecciona la materia destino
7. Click en "Importar Quiz"
8. Sistema crea la copia y redirige a la vista de detalles

## Registro de Cambios

### Archivos Creados
- Core/Entities/QuizCompartido.cs
- Core/Entities/QuizImportado.cs
- Core/Interfaces/IQuizCompartidoRepository.cs
- Infrastructure/Repositories/QuizCompartidoRepository.cs
- Application/Interfaces/IQuizCompartidoService.cs
- Infrastructure/Services/QuizCompartidoService.cs
- Application/ViewModels/QuizCompartidoViewModels.cs
- Web/Controllers/QuizCompartidoController.cs
- Web/Views/QuizCompartido/Index.cshtml
- Web/Views/QuizCompartido/Compartir.cshtml
- Web/Views/QuizCompartido/Importar.cshtml
- Application/Models/ServiceResult.cs

### Archivos Modificados
- Infrastructure/Data/ApplicationDbContext.cs (DbSets y configuración)
- Core/Interfaces/IUnitOfWork.cs (propiedad QuizCompartidoRepository)
- Infrastructure/Repositories/UnitOfWork.cs (implementación)
- Web/Program.cs (registro de servicios)
- Web/Views/Quiz/Details.cshtml (botón Compartir)
- Web/Views/Shared/_Layout.cshtml (enlace en menú)

### Migración Aplicada
- AgregarQuizzesCompartidos

## Testing Manual Recomendado

1. **Compartir Quiz**:
   - Crear un quiz con preguntas
   - Compartir el quiz sin opciones
   - Compartir con fecha de expiración
   - Compartir con límite de usos

2. **Importar Quiz**:
   - Importar un quiz válido
   - Intentar importar con código inválido
   - Intentar importar un quiz ya importado
   - Intentar importar el propio quiz

3. **Gestión**:
   - Ver lista de compartidos
   - Ver lista de importados
   - Copiar código al portapapeles
   - Revocar una compartición

4. **Validaciones**:
   - Probar con quiz sin preguntas
   - Probar con fecha de expiración pasada
   - Probar con límite de usos alcanzado
   - Probar con materia de otro usuario

## Estado Final

✅ **Compilación**: Exitosa sin errores ni warnings
✅ **Migración**: Aplicada correctamente a la base de datos
✅ **Aplicación**: Ejecutándose correctamente en http://localhost:5291
✅ **Funcionalidad**: Completa y lista para pruebas

## Próximos Pasos Sugeridos

1. Implementar tests unitarios para el servicio
2. Agregar notificaciones cuando se importe un quiz propio
3. Estadísticas de uso de comparticiones
4. Posibilidad de re-compartir un quiz ya compartido
5. Exportar/importar quizzes en formato JSON/CSV
