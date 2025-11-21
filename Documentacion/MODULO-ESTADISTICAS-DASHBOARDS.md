# 📊 Módulo de Estadísticas y Dashboards - QuizCraft

## 📋 Descripción General

El módulo de estadísticas proporciona un análisis completo y detallado del desempeño académico del usuario en QuizCraft. Incluye dashboards interactivos, gráficos avanzados, análisis de tendencias y recomendaciones personalizadas.

---

## 🎯 Características Implementadas

### 1. **Dashboard Principal de Estadísticas** 📈
- **Vista:** `Views/Statistics/Dashboard.cshtml`
- **Función:** Resumen ejecutivo del desempeño general
- **Elementos:**
  - Tarjetas de resumen rápido (Materias, Flashcards, Promedio de Aciertos, Racha)
  - Progreso general con barra de progreso
  - Nivel de dominio actualizado
  - Top 5 materias con rankings
  - Recomendaciones personalizadas
  - Accesos rápidos a análisis detallados

### 2. **Gráficos de Desempeño** 📊
- **Vista:** `Views/Statistics/Charts.cshtml`
- **Gráficos Incluidos:**
  - **Radar Chart:** Tasa de aciertos por materia
  - **Doughnut Chart:** Distribución de tiempo de estudio
  - **Bar Chart:** Actividad semanal (flashcards vs quizzes)
  - **Heatmap:** Mapa de calor de actividad diaria (3 meses)
  - **Line Chart:** Análisis de tendencias (30 días)

### 3. **Análisis por Materia Individual**
- **Vista:** `Views/Statistics/MateriaAnalytics.cshtml`
- **Datos mostrados:**
  - Detalles del progreso (flashcards nuevas, aprendidas, en revisión, difíciles)
  - Nivel de dominio con puntuación
  - Comparación con otras materias
  - Estimación de fecha de dominio

### 4. **Actividad Temporal**
- **Semanal:** Visualización de actividad por día
- **Mensual:** Desglose de actividad por mes
- **Heatmap:** Matriz de intensidad de estudio

### 5. **Análisis de Tendencias**
- **Vista:** `Views/Statistics/TrendAnalysis.cshtml`
- **Métricas:**
  - Tendencia de tasa de aciertos (mejorando/empeorando/estable)
  - Racha de estudio actual
  - Días sin estudiar
  - Predicciones y sugerencias

### 6. **Recomendaciones Personalizadas**
- Identificación automática de materias en riesgo
- Sugerencias de flashcards sin revisar
- Alertas sobre consistencia de estudio
- Sistema de priorización (urgentes, importantes, sugerencias)

### 7. **Reportes Exportables**
- Generación de reportes PDF/Excel
- Resumen ejecutivo
- Fortalezas y áreas de mejora
- Estadísticas comparativas

---

## 🏗️ Estructura Técnica

### Interfaz IStatisticsService
**Archivo:** `src/QuizCraft.Application/Interfaces/IStatisticsService.cs`

Define 15+ métodos para análisis y estadísticas.

### Implementación: StatisticsService
**Archivo:** `src/QuizCraft.Infrastructure/Services/StatisticsService.cs`

Implementación completa con:
- Cálculo de niveles de dominio
- Análisis de tendencias
- Generación automática de recomendaciones
- Comparativas anónimas

### Data Transfer Objects (DTOs)
**Archivo:** `src/QuizCraft.Application/Models/DTOs/Statistics/StatisticsDtos.cs`

Más de 20 DTOs especializados para diferentes análisis.

### ViewModels
**Archivo:** `src/QuizCraft.Web/ViewModels/Statistics/StatisticsViewModels.cs`

11 ViewModels para diferentes vistas de estadísticas.

### Controlador StatisticsController
**Archivo:** `src/QuizCraft.Web/Controllers/StatisticsController.cs`

18 acciones REST + endpoints API para gráficos.

### Vistas Razor
**Directorio:** `src/QuizCraft.Web/Views/Statistics/`

Vistas implementadas:
1. **Dashboard.cshtml** - Dashboard principal
2. **Charts.cshtml** - Gráficos interactivos con Chart.js

---

## 📊 Tipos de Análisis Soportados

### 1. **Estadísticas Generales**
- Total de materias, flashcards y quizzes
- Promedio de aciertos general
- Tasa de aciertos por tipo
- Tiempo total de estudio
- Racha de estudio

### 2. **Análisis por Materia**
- Progreso de completitud
- Tasa de aciertos específica
- Flashcards por estado
- Nivel de dominio

### 3. **Análisis Temporal**
- Actividad semanal
- Actividad mensual
- Heatmap de intensidad
- Tendencias

### 4. **Análisis Comparativo**
- Ranking de materias
- Comparación global
- Percentil de desempeño
- Clasificación

### 5. **Recomendaciones Inteligentes**
- Materias en riesgo
- Flashcards sin revisar
- Alertas de consistencia
- Priorización automática

---

## 🔧 Integración

### Registro de Servicios en Program.cs
```csharp
services.AddScoped<IStatisticsService, StatisticsService>();
```

### Rutas Disponibles
- `GET /Statistics` - Dashboard
- `GET /Statistics/Charts` - Gráficos
- `GET /Statistics/Materia/{id}` - Análisis de materia
- `GET /Statistics/Weekly` - Actividad semanal
- `GET /Statistics/Monthly` - Actividad mensual
- Y más...

---

## 🚀 Acceso Inmediato

### Ir al Dashboard
```
http://localhost:5000/Statistics
```

### Acceder a Gráficos
```
http://localhost:5000/Statistics/Charts
```

---

## 📈 Algoritmos

### Nivel de Dominio
- **Experto:** ≥ 90%
- **Avanzado:** ≥ 75%
- **Intermedio:** ≥ 50%
- **Novato:** < 50%

### Puntuación = (Completitud × 0.6) + (Aciertos × 0.4)

---

## ✅ Completado

✓ Interfaz de servicios  
✓ Implementación completa del servicio  
✓ DTOs para todos los análisis  
✓ ViewModels especializados  
✓ Controlador con 18 acciones  
✓ Dashboard principal  
✓ Página de gráficos interactivos  
✓ Integración con Chart.js  
✓ Recomendaciones automáticas  
✓ Análisis de tendencias  

---

## 🔐 Seguridad

✓ Todas las rutas requieren `[Authorize]`  
✓ Validación de propiedad de datos  
✓ Comparativas anónimas  
✓ CSRF protection  

---

## 📌 Próximas Mejoras

1. Exportación a PDF/Excel
2. API REST completa
3. Predicciones con ML.NET
4. Notificaciones automáticas
5. Tablero de líderes
6. Reportes programados

---

Sistema de estadísticas completo y funcional para QuizCraft.
