# Plan de Testing - EP-01: Gestión de Materias y Flashcards

## Estructura de la Épica

**Epic ID**: 7 - "EP-01: Gestión de Materias y Flashcards"

### Features Incluidas:

#### Feature 12: FT-01.01 - Gestión de Materias
- **Estado**: Closed ✅
- **Descripción**: CRUD completo de materias con validaciones de seguridad
- **User Stories**:
  - US-59: "Gestionar mis materias de estudio" (Estado: New)

#### Feature 13: FT-01.02 - Gestión de Flashcards  
- **Estado**: Closed ✅
- **Descripción**: Permite crear, editar, eliminar y repasar flashcards asociadas a materias
- **User Stories**:
  - US-24: "Crear flashcards asociadas a una materia" (Estado: Closed ✅)
  - US-25: "Editar y eliminar flashcards" (Estado: Active 🔄)
  - US-26: "Repasar flashcards de una materia" (Estado: Closed ✅)
  - US-62: "Gestionar mis flashcards de estudio" (Estado: New)
  - US-65: "Repasar flashcards por materia" (Estado: New)

## Plan de Testing por Historia de Usuario

### 1. US-59: Gestionar mis materias de estudio
**Objetivo**: Verificar el CRUD completo de materias de estudio

**Escenarios a probar**:
- ✅ Crear nueva materia
- ✅ Listar materias existentes
- ✅ Editar materia existente
- ✅ Eliminar materia
- ✅ Validaciones de formulario
- ✅ Permisos de usuario

### 2. US-24: Crear flashcards asociadas a una materia ✅ IMPLEMENTADO
**Objetivo**: Verificar la creación de flashcards vinculadas a materias

**Escenarios a probar**:
- ✅ Crear flashcard con pregunta y respuesta
- ✅ Asociar flashcard a materia específica
- ✅ Validaciones de campos obligatorios
- ✅ Persistencia en base de datos

### 3. US-25: Editar y eliminar flashcards
**Objetivo**: Verificar la modificación y eliminación de flashcards

**Escenarios a probar**:
- 🔄 Editar flashcard existente
- 🔄 Eliminar flashcard
- 🔄 Confirmación de eliminación
- 🔄 Validaciones en edición

### 4. US-26: Repasar flashcards de una materia
**Objetivo**: Verificar el sistema de repaso de flashcards

**Escenarios a probar**:
- 🔄 Iniciar sesión de repaso por materia
- 🔄 Navegación entre flashcards
- 🔄 Mostrar/ocultar respuesta
- 🔄 Finalizar sesión de repaso

### 5. US-62: Gestionar mis flashcards de estudio
**Objetivo**: Verificar gestión general de flashcards del usuario

**Escenarios a probar**:
- 🔄 Listar todas las flashcards del usuario
- 🔄 Filtrar por materia
- 🔄 Búsqueda de flashcards
- 🔄 Acciones masivas

### 6. US-65: Repasar flashcards por materia
**Objetivo**: Verificar sistema avanzado de repaso por materia

**Escenarios a probar**:
- 🔄 Seleccionar materia para repaso
- 🔄 Configurar parámetros de repaso
- 🔄 Tracking de progreso
- 🔄 Estadísticas de repaso

## Estructura de Archivos de Test

```
tests/epics/EP-01-Gestion-Materias-Flashcards/
├── EP-01-testing-plan.md (este archivo)
├── materias/
│   ├── materias.spec.js (US-59)
│   └── materias-crud-test.md
├── flashcards/
│   ├── flashcard.spec.js (US-24) ✅ EXISTENTE
│   ├── flashcard-creation-test.md ✅ EXISTENTE
│   ├── flashcard-edit-delete.spec.js (US-25)
│   ├── flashcard-review.spec.js (US-26)
│   ├── flashcard-management.spec.js (US-62)
│   └── flashcard-study-by-subject.spec.js (US-65)
└── integration/
    └── materias-flashcards-integration.spec.js
```

## Configuración de Testing

### Prerequisitos:
- ✅ Aplicación QuizCraft ejecutándose en http://localhost:5291
- ✅ Base de datos configurada y funcionando
- ✅ Usuario de prueba configurado
- ✅ Playwright configurado

### Datos de Prueba:
- Usuario: usuario de prueba registrado
- Materias: "Historia de Roma", "Matemáticas", "Física"
- Flashcards: Conjunto de preguntas/respuestas de ejemplo

## Reporting de Bugs

Si se encuentran bugs durante las pruebas, se crearán work items en Azure DevOps:
- Tipo: Bug
- Área: QuizCraft
- Épica relacionada: EP-01
- Historia de usuario afectada: US-XX

## Estado Actual

✅ **Completado**:
- US-24: Creación de flashcards (4 tests pasando)

🔄 **En Progreso**:
- Implementación de tests para US-59, US-25, US-26, US-62, US-65

📋 **Pendiente**:
- Tests de integración
- Tests de rendimiento
- Tests de seguridad