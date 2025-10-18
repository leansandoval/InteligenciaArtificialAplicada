# Documento de Requisitos - QuizCraft

**Proyecto:** QuizCraft  

**Organización:** IAAplicadaGrupo2  

**Tipo de Sistema:** Plataforma de Aprendizaje con Flashcards y Quizzes  

**Fecha:** 19 de Septiembre de 2025  

---

## 📋 REQUISITOS FUNCIONALES

### 1. MÓDULO DE GESTIÓN DE MATERIAS

#### RF-001: Crear Materias

- **Descripción:** El sistema debe permitir a los usuarios crear materias para organizar sus contenidos de estudio.

- **User Story:** Como usuario, quiero crear materias para organizar mis contenidos de estudio.

- **Criterios de Aceptación:**

  - El usuario puede crear una nueva materia con nombre único

  - El sistema valida que no existan materias duplicadas

  - La materia se almacena correctamente en la base de datos

#### RF-002: Editar Materias

- **Descripción:** El sistema debe permitir modificar las materias existentes.

- **User Story:** Como usuario, quiero editar y eliminar materias para mantener mi organización actualizada.

- **Criterios de Aceptación:**

  - El usuario puede modificar el nombre de una materia existente

  - Los cambios se reflejan inmediatamente en el sistema

  - Se mantiene la asociación con las flashcards existentes

#### RF-003: Eliminar Materias

- **Descripción:** El sistema debe permitir eliminar materias existentes.

- **Criterios de Aceptación:**

  - El usuario puede eliminar una materia

  - El sistema solicita confirmación antes de eliminar

  - Se eliminan también las flashcards asociadas o se reasignan

---

### 2. MÓDULO DE GESTIÓN DE FLASHCARDS

#### RF-004: Crear Flashcards

- **Descripción:** El sistema debe permitir crear flashcards asociadas a una materia específica.

- **User Story:** Como usuario, quiero crear flashcards asociadas a una materia para repasar conceptos clave.

- **Criterios de Aceptación:**

  - El usuario puede crear flashcards con pregunta y respuesta

  - Las flashcards se asocian correctamente a una materia

  - Se admite formato de texto enriquecido

#### RF-005: Editar Flashcards

- **Descripción:** El sistema debe permitir modificar flashcards existentes.

- **User Story:** Como usuario, quiero editar y eliminar flashcards para mantener mi material actualizado.

- **Criterios de Aceptación:**

  - El usuario puede modificar tanto la pregunta como la respuesta

  - Los cambios se guardan inmediatamente

  - Se mantiene el historial de cambios

#### RF-006: Eliminar Flashcards

- **Descripción:** El sistema debe permitir eliminar flashcards existentes.

- **Criterios de Aceptación:**

  - El usuario puede eliminar flashcards individuales

  - Se solicita confirmación antes de eliminar

  - Se actualiza el contador de flashcards de la materia

#### RF-007: Repasar Flashcards

- **Descripción:** El sistema debe proporcionar una interfaz para repasar flashcards por materia.

- **User Story:** Como usuario, quiero repasar flashcards de una materia específica para estudiar de forma focalizada.

- **Criterios de Aceptación:**

  - El usuario puede seleccionar una materia para repasar

  - Las flashcards se presentan de forma aleatoria

  - El usuario puede marcar respuestas como correctas o incorrectas

---

### 3. MÓDULO DE GENERACIÓN AUTOMÁTICA DE CONTENIDO

#### RF-008: Generar Flashcards desde Texto

- **Descripción:** El sistema debe generar flashcards automáticamente a partir de documentos de texto.

- **User Story:** Como usuario, quiero generar flashcards automáticamente a partir de documentos de texto.

- **Criterios de Aceptación:**

  - El sistema acepta archivos de texto (.txt, .docx)

  - Utiliza IA para extraer preguntas y respuestas relevantes

  - Permite al usuario revisar y editar antes de guardar

#### RF-009: Generar Flashcards desde PDFs

- **Descripción:** El sistema debe generar flashcards a partir de archivos PDF y presentaciones.

- **User Story:** Como usuario, quiero generar flashcards a partir de PDFs y presentaciones.

- **Criterios de Aceptación:**

  - Soporte para archivos PDF y PPTX

  - Extracción de texto con OCR si es necesario

  - Mantenimiento del contexto del documento original

---

### 4. MÓDULO DE SOPORTE MULTIMEDIA

#### RF-010: Agregar Imágenes a Flashcards

- **Descripción:** El sistema debe permitir agregar imágenes a las flashcards.

- **User Story:** Como usuario, quiero agregar imágenes a mis flashcards para enriquecer el contenido.

- **Criterios de Aceptación:**

  - Soporte para formatos JPG, PNG, GIF

  - Redimensionamiento automático de imágenes

  - Almacenamiento eficiente de archivos multimedia

#### RF-011: Gestionar Archivos Adjuntos

- **Descripción:** El sistema debe permitir adjuntar archivos a las flashcards.

- **Criterios de Aceptación:**

  - Soporte para múltiples tipos de archivo

  - Límite de tamaño por archivo

  - Visualización previa cuando sea posible

---

### 5. MÓDULO DE QUIZZES PERSONALIZADOS

#### RF-012: Crear Quizzes

- **Descripción:** El sistema debe generar quizzes personalizados basados en las flashcards de una materia.

- **User Story:** Como usuario, quiero generar quizzes personalizados en base a las flashcards de una materia.

- **Criterios de Aceptación:**

  - Selección automática de flashcards para el quiz

  - Configuración del número de preguntas

  - Mezcla aleatoria de preguntas

#### RF-013: Configurar Dificultad

- **Descripción:** El sistema debe permitir seleccionar el nivel de dificultad de los quizzes.

- **User Story:** Como usuario, quiero seleccionar el nivel de dificultad de los quizzes.

- **Criterios de Aceptación:**

  - Niveles: Fácil, Intermedio, Difícil

  - Ajuste automático según el historial de respuestas

  - Algoritmo de selección inteligente

#### RF-014: Resolver Quizzes

- **Descripción:** El sistema debe proporcionar una interfaz para responder quizzes con feedback inmediato.

- **User Story:** Como usuario, quiero responder quizzes y recibir feedback inmediato sobre mis respuestas.

- **Criterios de Aceptación:**

  - Interfaz intuitiva para responder preguntas

  - Feedback inmediato tras cada respuesta

  - Cronómetro opcional para medir tiempo

#### RF-015: Revisar Resultados

- **Descripción:** El sistema debe permitir revisar respuestas al finalizar un quiz.

- **User Story:** Como usuario, quiero revisar mis respuestas correctas e incorrectas al finalizar un quiz.

- **Criterios de Aceptación:**

  - Resumen completo del quiz realizado

  - Explicaciones para respuestas incorrectas

  - Puntuación y porcentaje de acierto

---

### 6. MÓDULO DE ESTADÍSTICAS Y ANÁLISIS

#### RF-016: Estadísticas por Materia

- **Descripción:** El sistema debe mostrar estadísticas de desempeño por materia y tipo de pregunta.

- **User Story:** Como usuario, quiero ver estadísticas de mis respuestas por materia y por tipo de pregunta.

- **Criterios de Aceptación:**

  - Gráficos de desempeño por materia

  - Análisis por tipo de pregunta

  - Comparación temporal

#### RF-017: Visualizar Progreso

- **Descripción:** El sistema debe mostrar el progreso del usuario a lo largo del tiempo.

- **User Story:** Como usuario, quiero visualizar mi progreso a lo largo del tiempo.

- **Criterios de Aceptación:**

  - Gráficos de progreso temporal

  - Métricas de mejora

  - Objetivos y metas alcanzadas

---

### 7. MÓDULO DE GESTIÓN INTELIGENTE DE REPASOS

#### RF-018: Sugerir Preguntas para Repaso

- **Descripción:** El sistema debe sugerir preguntas para repasar según errores previos.

- **User Story:** Como usuario, quiero recibir sugerencias de preguntas para repasar según mis errores previos.

- **Criterios de Aceptación:**

  - Algoritmo de repetición espaciada

  - Priorización de contenido con mayor dificultad

  - Sugerencias personalizadas

#### RF-019: Programar Repasos Automáticos

- **Descripción:** El sistema debe permitir programar repasos automáticos.

- **User Story:** Como usuario, quiero programar repasos automáticos para reforzar el aprendizaje.

- **Criterios de Aceptación:**

  - Configuración de horarios de estudio

  - Notificaciones automáticas

  - Ajuste dinámico según disponibilidad

---

### 8. MÓDULO DE PERSONALIZACIÓN

#### RF-020: Personalizar Interfaz

- **Descripción:** El sistema debe permitir personalizar la interfaz y notificaciones.

- **User Story:** Como usuario, quiero personalizar la interfaz y las notificaciones según mis preferencias.

- **Criterios de Aceptación:**

  - Selección de temas visuales

  - Configuración de notificaciones

  - Personalización del dashboard

---

### 9. MÓDULO DE INTEGRACIÓN CON IA

#### RF-023: Integración con Google Gemini para Generación de Contenido

- **Descripción:** El sistema debe integrar la API de Google Gemini para la generación automática de flashcards y quizzes.

- **User Story:** Como usuario, quiero generar contenido automáticamente usando IA para ahorrar tiempo en la creación de materiales de estudio.

- **Criterios de Aceptación:**
  - Configuración segura de API keys
  - Manejo de rate limits y fallbacks
  - Procesamiento asíncrono de solicitudes
  - Validación y filtrado de contenido generado

### 10. MÓDULO DE COLABORACIÓN

#### RF-021: Compartir Contenido

- **Descripción:** El sistema debe permitir compartir flashcards y quizzes con otros usuarios.

- **User Story:** Como usuario, quiero compartir mis flashcards y quizzes con otros usuarios.

- **Criterios de Aceptación:**

  - Generación de enlaces de compartición

  - Control de permisos de acceso

  - Versionado de contenido compartido

#### RF-022: Importar Contenido

- **Descripción:** El sistema debe permitir importar flashcards y quizzes compartidos.

- **User Story:** Como usuario, quiero importar flashcards y quizzes compartidos por otros.

- **Criterios de Aceptación:**

  - Importación desde enlaces compartidos

  - Verificación de integridad del contenido

  - Asignación a materias propias

---

## 🔧 REQUISITOS NO FUNCIONALES

### 1. RENDIMIENTO

#### RNF-001: Tiempo de Respuesta

- **Descripción:** El sistema debe responder a las solicitudes del usuario en tiempo óptimo.

- **Criterio:**

  - Carga de páginas: < 2 segundos

  - Generación de flashcards con IA: < 10 segundos

  - Consultas de base de datos: < 500ms

#### RNF-002: Capacidad

- **Descripción:** El sistema debe soportar múltiples usuarios simultáneos.

- **Criterio:**

  - Mínimo 100 usuarios concurrentes

  - Escalabilidad horizontal

  - Balanceador de carga

#### RNF-003: Disponibilidad

- **Descripción:** El sistema debe estar disponible la mayor parte del tiempo.

- **Criterio:**

  - Uptime del 99.5%

  - Ventanas de mantenimiento programadas

  - Recuperación ante fallos < 1 hora

---

### 2. SEGURIDAD

#### RNF-004: Autenticación

- **Descripción:** El sistema debe garantizar la identidad de los usuarios.

- **Criterio:**

  - Autenticación multifactor opcional

  - Políticas de contraseñas seguras

  - Sesiones con timeout automático

#### RNF-005: Autorización

- **Descripción:** El sistema debe controlar el acceso a los recursos según permisos.

- **Criterio:**

  - Control de acceso basado en roles

  - Validación de permisos en cada operación

  - Auditoría de accesos

#### RNF-006: Protección de Datos

- **Descripción:** El sistema debe proteger la información de los usuarios.

- **Criterio:**

  - Encriptación de datos sensibles

  - Cumplimiento GDPR/LOPD

  - Backup seguro y cifrado

---

### 3. USABILIDAD

#### RNF-007: Interfaz Intuitiva

- **Descripción:** El sistema debe ser fácil de usar para usuarios no técnicos.

- **Criterio:**

  - Navegación clara y consistente

  - Mensajes de error comprensibles

  - Ayuda contextual disponible

#### RNF-008: Accesibilidad

- **Descripción:** El sistema debe ser accesible para usuarios con discapacidades.

- **Criterio:**

  - Cumplimiento WCAG 2.1 nivel AA

  - Soporte para lectores de pantalla

  - Navegación por teclado

#### RNF-009: Diseño Responsivo

- **Descripción:** El sistema debe funcionar correctamente en diferentes dispositivos.

- **Criterio:**

  - Compatible con móviles, tablets y desktop

  - Adaptación automática de la interfaz

  - Touch-friendly en dispositivos móviles

---

### 4. COMPATIBILIDAD

#### RNF-010: Navegadores

- **Descripción:** El sistema debe funcionar en los principales navegadores web.

- **Criterio:**

  - Chrome, Firefox, Safari, Edge (últimas 2 versiones)

  - Degradación elegante en navegadores antiguos

  - JavaScript ES6+ compatible

#### RNF-011: Dispositivos

- **Descripción:** El sistema debe ser compatible con diferentes dispositivos.

- **Criterio:**

  - Resoluciones desde 320px hasta 4K

  - iOS y Android (últimas 3 versiones)

  - Windows, macOS, Linux

---

### 5. MANTENIBILIDAD

#### RNF-012: Código Limpio

- **Descripción:** El código debe seguir estándares de calidad para facilitar el mantenimiento.

- **Criterio:**

  - Cobertura de pruebas > 80%

  - Documentación técnica actualizada

  - Convenciones de nomenclatura consistentes

#### RNF-013: Monitoreo

- **Descripción:** El sistema debe permitir monitoreo y diagnóstico de problemas.

- **Criterio:**

  - Logs estructurados y centralizados

  - Métricas de rendimiento en tiempo real

  - Alertas automáticas ante errores

---

### 6. ESCALABILIDAD

#### RNF-014: Crecimiento de Datos

- **Descripción:** El sistema debe manejar el crecimiento de datos a lo largo del tiempo.

- **Criterio:**

  - Particionado de base de datos

  - Archivado automático de datos antiguos

  - Compresión de archivos multimedia

#### RNF-015: Carga de Usuarios

- **Descripción:** El sistema debe escalar según el número de usuarios.

- **Criterio:**

  - Arquitectura de microservicios

  - Auto-scaling en la nube

  - CDN para contenido estático

---

### 7. INTEGRACIÓN CON IA

#### RNF-018: Rendimiento de IA

- **Descripción:** El sistema debe optimizar el rendimiento de las operaciones con IA.

- **Criterio:**
  - Tiempo de respuesta para generación de contenido < 5 segundos
  - Cache de respuestas frecuentes
  - Procesamiento por lotes de solicitudes
  - Monitoreo de costos y uso de tokens

#### RNF-019: Protección de Datos de IA

- **Descripción:** El sistema debe garantizar la seguridad y privacidad de los datos procesados por IA.

- **Criterio:**
  - No almacenar datos sensibles en prompts
  - Filtrado de información personal
  - Logs de auditoría de uso de IA
  - Políticas de retención de datos generados

#### RNF-016: API de Google Gemini

- **Descripción:** El sistema debe integrar de forma eficiente con Google Gemini.

- **Criterio:**

  - Manejo de rate limits

  - Fallback ante indisponibilidad

  - Control de costos por tokens

#### RNF-017: Procesamiento de Documentos

- **Descripción:** El sistema debe procesar eficientemente documentos multimedia.

- **Criterio:**

  - OCR para imágenes y PDFs

  - Procesamiento en background

  - Validación de formatos soportados

---

## 📊 MATRIZ DE TRAZABILIDAD

| Epic | Feature | User Stories | Requisitos Funcionales |

|------|---------|--------------|------------------------|

| Gestión de Materias y Flashcards | Gestión de Materias | US-22, US-23 | RF-001, RF-002, RF-003 |

| Gestión de Materias y Flashcards | Gestión de Flashcards | US-24, US-25, US-26 | RF-004, RF-005, RF-006, RF-007 |

| Generación de Flashcards y Soporte | Generación automática | US-27, US-28 | RF-008, RF-009 |

| Generación de Flashcards y Soporte | Soporte multimedia | US-29 | RF-010, RF-011 |

| Quizzes Personalizados | Creación de quizzes | US-31, US-32 | RF-012, RF-013 |

| Quizzes Personalizados | Resolución y feedback | US-33, US-34 | RF-014, RF-015 |

| Estadísticas y Gestión de Repasos | Estadísticas | US-35, US-36 | RF-016, RF-017 |

| Estadísticas y Gestión de Repasos | Gestión inteligente | US-37, US-38 | RF-018, RF-019 |

| Personalización y Colaboración | Personalización | US-39 | RF-020 |

| Personalización y Colaboración | Compartir y colaborar | US-40, US-41 | RF-021, RF-022 |

---

## 🎯 PRIORIZACIÓN

### Prioridad Alta (MVP)

- RF-001 a RF-007: Gestión básica de materias y flashcards

- RF-014, RF-015: Funcionalidad básica de quizzes

- RNF-001, RNF-004, RNF-007: Rendimiento, seguridad y usabilidad básicos

### Prioridad Media

- RF-008, RF-009: Generación automática con IA

- RF-016, RF-017: Estadísticas básicas

- RF-018: Sugerencias de repaso

### Prioridad Baja

- RF-010, RF-011: Soporte multimedia avanzado

- RF-019 a RF-022: Funcionalidades de colaboración

- RNF-008, RNF-016, RNF-017: Características avanzadas

---

**Documento generado automáticamente desde Azure DevOps**  

**Fecha de generación:** 19 de Septiembre de 2025  

**Total de Requisitos Funcionales:** 22  

**Total de Requisitos No Funcionales:** 17