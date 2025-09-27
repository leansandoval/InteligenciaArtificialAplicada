# 🚀 Azure DevOps CLI - Comandos para QuizCraft

Esta guía contiene todos los comandos útiles de Azure CLI para trabajar con el proyecto **QuizCraft** en Azure DevOps.

## 📋 Configuración Inicial

```powershell
# Verificar autenticación
az account show

# Configurar organización y proyecto por defecto
az devops configure --defaults organization=https://dev.azure.com/IAAplicadaGrupo2 project=QuizCraft

# Ver configuración actual
az devops configure --list
```

## 📁 Repositorios

### Ver información de repositorios
```powershell
# Listar todos los repositorios
az repos list --output table

# Ver detalles de un repositorio específico
az repos show --repository QuizCraft

# Ver ramas del repositorio
az repos ref list --output table

# Ver commits recientes
az repos commit list --output table --top 10
```

### Gestión de ramas
```powershell
# Crear una nueva rama
az repos ref create --name "refs/heads/feature/nueva-funcionalidad" --object-id $(az repos ref list --filter "heads/main" --query "[0].objectId" -o tsv)

# Eliminar una rama
az repos ref delete --name "refs/heads/feature/rama-a-eliminar"
```

## 📋 Work Items (Tareas/Bugs/Features)

### Consultar work items
```powershell
# Ver todas las tareas del proyecto
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.TeamProject] = 'QuizCraft'" --output table

# Ver solo tareas asignadas a mí
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.AssignedTo] = @Me" --output table

# Ver tareas por estado
az boards query --wiql "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.State] = 'In Progress'" --output table

# Ver detalles de una tarea específica
az boards work-item show --id 1
```

### Crear y actualizar work items
```powershell
# Crear una nueva tarea
az boards work-item create --title "Mi nueva tarea" --type "Task" --description "Descripción de la tarea"

# Crear un bug
az boards work-item create --title "Bug encontrado" --type "Bug" --description "Descripción del bug" --priority 1

# Crear una user story
az boards work-item create --title "Como usuario quiero..." --type "User Story" --description "Descripción de la funcionalidad"

# Actualizar el estado de una tarea
az boards work-item update --id 1 --state "In Progress"

# Asignar una tarea a mí
az boards work-item update --id 1 --assigned-to @Me

# Agregar comentario a una tarea
az boards work-item update --id 1 --discussion "Este es mi comentario"
```

### Consultas útiles con WIQL
```powershell
# Tareas creadas en los últimos 7 días
az boards query --wiql "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.CreatedDate] >= @Today - 7"

# Tareas de alta prioridad
az boards query --wiql "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [Microsoft.VSTS.Common.Priority] = 1"

# Bugs activos
az boards query --wiql "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.WorkItemType] = 'Bug' AND [System.State] <> 'Closed'"
```

## 🔀 Pull Requests

### Ver pull requests
```powershell
# Ver todos los pull requests activos
az repos pr list --status "active" --output table

# Ver pull requests completados
az repos pr list --status "completed" --output table

# Ver mis pull requests
az repos pr list --creator @Me --output table

# Ver detalles de un pull request específico
az repos pr show --id [PR_ID]
```

### Crear y gestionar pull requests
```powershell
# Crear un pull request
az repos pr create --title "Mi nueva feature" --description "Descripción del cambio" --source-branch "feature/nueva-funcionalidad" --target-branch "main"

# Crear PR con revisores
az repos pr create --title "Mi feature" --description "Descripción" --source-branch "feature/mi-feature" --target-branch "main" --reviewers "usuario@email.com"

# Aprobar un pull request
az repos pr update --id [PR_ID] --status "approved"

# Completar un pull request
az repos pr update --id [PR_ID] --status "completed"

# Abandonar un pull request
az repos pr update --id [PR_ID] --status "abandoned"
```

## 🔧 Pipelines y Builds

### Ver pipelines
```powershell
# Listar todos los pipelines
az pipelines list --output table

# Ver detalles de un pipeline específico
az pipelines show --name "NombrePipeline"

# Ver builds recientes
az pipelines build list --output table --top 10
```

### Ejecutar builds
```powershell
# Ejecutar un build
az pipelines run --name "QuizCraft-CI"

# Ejecutar build con parámetros
az pipelines run --name "QuizCraft-CI" --variables "BuildConfiguration=Release"

# Ver detalles de un build específico
az pipelines build show --id [BUILD_ID]

# Ver logs de un build
az pipelines build log --id [BUILD_ID]
```

### Crear pipelines
```powershell
# Crear un pipeline desde YAML
az pipelines create --name "QuizCraft-CI" --repository QuizCraft --branch main --yml-path "azure-pipelines.yml"

# Crear pipeline desde template
az pipelines create --name "QuizCraft-Release" --repository QuizCraft --branch main --yml-path "release-pipeline.yml"
```

## 📊 Información del Proyecto

```powershell
# Ver información general del proyecto
az devops project show --project QuizCraft

# Ver equipos del proyecto
az devops team list --project QuizCraft --output table

# Ver usuarios del proyecto
az devops user list --output table
```

## 🔍 Búsqueda

```powershell
# Buscar en código
az devops search code --search-text "function" --output table

# Buscar work items
az devops search workitem --search-text "bug" --output table
```

## 🛠️ Comandos de Utilidad

### Configuración
```powershell
# Ver todas las extensiones instaladas
az extension list --output table

# Actualizar extensión de Azure DevOps
az extension update --name azure-devops

# Ver ayuda de un comando específico
az repos pr create --help
```

### Formato de salida
```powershell
# Salida en tabla (más legible)
az repos list --output table

# Salida en JSON (para scripts)
az repos list --output json

# Salida en YAML
az repos list --output yaml

# Consulta específica con JMESPath
az repos list --query "[].{Name:name, DefaultBranch:defaultBranch}"
```

## 📝 Ejemplos de Flujo de Trabajo

### Workflow típico para una nueva feature:

```powershell
# 1. Crear work item para la feature
az boards work-item create --title "Implementar login de usuario" --type "User Story" --description "Como usuario quiero poder iniciar sesión"

# 2. Crear rama para la feature
az repos ref create --name "refs/heads/feature/user-login" --object-id $(az repos ref list --filter "heads/main" --query "[0].objectId" -o tsv)

# 3. Después de desarrollar, crear pull request
az repos pr create --title "Implementar login de usuario" --description "Implementación completa del sistema de login" --source-branch "feature/user-login" --target-branch "main"

# 4. Una vez aprobado, completar el PR
az repos pr update --id [PR_ID] --status "completed"

# 5. Actualizar el work item
az boards work-item update --id [WORK_ITEM_ID] --state "Done"
```

## 🚨 Comandos de Emergencia

```powershell
# Ver últimos commits en main
az repos commit list --output table --top 5

# Ver quien hizo el último cambio
az repos commit list --output table --top 1

# Ver pull requests que necesitan revisión
az repos pr list --status "active" --query "[?reviewers[0].vote==0]" --output table
```

## 📚 Recursos Adicionales

- [Documentación oficial de Azure CLI](https://docs.microsoft.com/en-us/cli/azure/)
- [Extensión Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/cli/)
- [WIQL Reference](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax)

---

## 📞 Contacto del Proyecto

- **Organización**: IAAplicadaGrupo2
- **Proyecto**: QuizCraft
- **Repositorio**: QuizCraft
- **URL**: https://dev.azure.com/IAAplicadaGrupo2/QuizCraft

---

> 💡 **Tip**: Guarda este archivo en tu repositorio para que todo el equipo tenga acceso a estos comandos útiles.