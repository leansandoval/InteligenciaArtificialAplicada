# Análisis de Uso de Tokens y Prompts de IA en QuizCraft

## 📊 Resumen Ejecutivo

**Problema Actual:** Error 429 (Too Many Requests) - La API de Gemini ha alcanzado el límite de cuota.

---

## 🎴 Generación de Flashcards

### Configuración de Tokens
- **MaxOutputTokens (Respuesta):** 1,500 tokens (configuración por defecto)
- **Límite de entrada:** Sin límite explícito en el código
- **Estimación:** ~4 caracteres = 1 token (para español)
- **Temperatura:** 0.7 (creatividad controlada)

### Prompt de Flashcards

```plaintext
ESTRUCTURA DEL PROMPT:
┌─────────────────────────────────────────────────────────┐
│ 1. System Prompt                                        │
│    "Eres un experto en educación que crea flashcards   │
│     de alta calidad."                                   │
├─────────────────────────────────────────────────────────┤
│ 2. Contenido del Usuario                                │
│    - Texto completo a procesar                          │
│    - Sin límite de caracteres explícito                 │
├─────────────────────────────────────────────────────────┤
│ 3. Instrucciones de Generación                          │
│    - MaxCardsPerDocument: Variable                      │
│    - Nivel de dificultad: Fácil/Medio/Difícil         │
│    - Idioma: Especificado por usuario                   │
│    - IncludeExplanations: Booleano                      │
│    - FocusArea: Área opcional de enfoque               │
├─────────────────────────────────────────────────────────┤
│ 4. Formato de Respuesta JSON                            │
│    {                                                    │
│      "flashcards": [                                    │
│        {                                                │
│          "pregunta": "string",                          │
│          "respuesta": "string",                         │
│          "dificultad": "string",                        │
│          "explicacion": "string (opcional)",            │
│          "etiquetas": ["tag1", "tag2"],                 │
│          "categoria": "string"                          │
│        }                                                │
│      ]                                                  │
│    }                                                    │
└─────────────────────────────────────────────────────────┘

RESTRICCIÓN FINAL: "Responde ÚNICAMENTE con el JSON válido, sin texto adicional."
```

### Ejemplo de Uso Real de Tokens (Flashcards)

**Escenario:** Usuario sube un texto de 2,000 caracteres sobre Historia de Roma

```
Tokens de Entrada (Prompt):
├─ System Prompt: ~50 tokens
├─ Contenido Usuario: ~500 tokens (2,000 chars ÷ 4)
├─ Instrucciones: ~100 tokens
└─ Formato JSON: ~80 tokens
─────────────────────────────
TOTAL ENTRADA: ~730 tokens

Tokens de Salida (Respuesta):
├─ 10 flashcards
├─ Cada flashcard: ~80-120 tokens
│  ├─ Pregunta: 15-25 tokens
│  ├─ Respuesta: 30-50 tokens
│  ├─ Explicación: 20-30 tokens
│  └─ Metadatos: 15-20 tokens
└─ Estructura JSON: ~50 tokens
─────────────────────────────
TOTAL SALIDA: ~1,000-1,200 tokens

═════════════════════════════
TOTAL GENERACIÓN: ~1,730-1,930 tokens
```

---

## 📝 Generación de Quizzes

### Configuración de Tokens
- **MaxOutputTokens (Respuesta):** 8,000 tokens (configuración aumentada para quizzes)
- **Límite de entrada:** 12,000 tokens (~48,000 caracteres)
- **Límite práctico:** El controlador trunca el contenido a 10,000 caracteres
- **Estimación:** ~4 caracteres = 1 token (para español)
- **Temperatura:** 0.7 (creatividad controlada)

### Prompt de Quizzes

```plaintext
ESTRUCTURA DEL PROMPT:
┌─────────────────────────────────────────────────────────┐
│ 1. Instrucción Principal                                │
│    "Crea N preguntas de quiz [con/sin explicaciones]   │
│     basadas en el siguiente contenido:"                 │
├─────────────────────────────────────────────────────────┤
│ 2. Contenido del Usuario                                │
│    - Texto extraído del documento                       │
│    - Máximo: 10,000 caracteres (~2,500 tokens)         │
├─────────────────────────────────────────────────────────┤
│ 3. Configuración del Quiz                               │
│    - Nivel de dificultad: Fácil/Intermedio/Difícil    │
│    - Tipos de pregunta: MultipleChoice, TrueFalse, etc.│
│    - Idioma: Español (por defecto)                      │
│    - Materia: Opcional                                  │
│    - Instrucciones personalizadas: Opcional             │
├─────────────────────────────────────────────────────────┤
│ 4. Formato JSON Detallado                               │
│    {                                                    │
│      "questions": [                                     │
│        {                                                │
│          "questionText": "string",                      │
│          "questionType": "MultipleChoice|TrueFalse",    │
│          "difficultyLevel": "string",                   │
│          "answerOptions": [                             │
│            {                                            │
│              "text": "string",                          │
│              "isCorrect": boolean,                      │
│              "explanation": "string"                    │
│            }                                            │
│          ],                                             │
│          "explanation": "string",                       │
│          "points": number,                              │
│          "tags": ["string"],                            │
│          "sourceReference": "string",                   │
│          "confidenceScore": number                      │
│        }                                                │
│      ]                                                  │
│    }                                                    │
├─────────────────────────────────────────────────────────┤
│ 5. REGLAS CRÍTICAS PARA DISTRACTORES                    │
│    ❌ NUNCA usar frases genéricas:                      │
│       - "Opción incorrecta A/B/C"                       │
│       - "Respuesta falsa"                               │
│                                                         │
│    ✅ Los distractores DEBEN ser:                       │
│       - Plausibles (parecen correctos)                  │
│       - Relacionados (del mismo tema)                   │
│       - Específicos (términos reales del contenido)     │
│       - Educativos (refuerzan el aprendizaje)           │
│                                                         │
│    📚 Ejemplos de BUENOS distractores:                  │
│       - Fechas cercanas: 1492 vs 1498                   │
│       - Conceptos relacionados: mitosis vs meiosis      │
│       - Definiciones parciales pero incompletas         │
│       - Términos similares: Java vs JavaScript          │
│                                                         │
│    📏 Similitud requerida:                              │
│       - Longitud similar entre opciones                 │
│       - Complejidad de lenguaje comparable              │
│       - Nivel de detalle equivalente                    │
├─────────────────────────────────────────────────────────┤
│ 6. REGLAS GENERALES                                     │
│    - MultipleChoice: Exactamente 4 opciones            │
│      (1 correcta + 3 distractores)                      │
│    - TrueFalse: Exactamente 2 opciones                 │
│    - SOLO UNA opción con isCorrect: true               │
│    - Cada opción debe tener explicación                │
│    - NO usar numeración (A), B), etc.)                 │
│    - Preguntas sobre conceptos importantes              │
│    - JSON válido sin markdown                           │
└─────────────────────────────────────────────────────────┘

RESTRICCIÓN FINAL: "Devuelve SOLO JSON válido, sin texto adicional ni formato markdown"
```

### Ejemplo de Uso Real de Tokens (Quizzes)

**Escenario:** Usuario sube un PDF de 8,000 caracteres sobre Bases de Datos

```
Tokens de Entrada (Prompt):
├─ Instrucción Principal: ~30 tokens
├─ Contenido Usuario: ~2,000 tokens (8,000 chars ÷ 4)
├─ Configuración Quiz: ~50 tokens
├─ Formato JSON: ~150 tokens
├─ Reglas de Distractores: ~500 tokens
└─ Reglas Generales: ~100 tokens
─────────────────────────────
TOTAL ENTRADA: ~2,830 tokens

Tokens de Salida (Respuesta):
├─ 10 preguntas de opción múltiple
├─ Cada pregunta: ~350-450 tokens
│  ├─ Pregunta: 20-30 tokens
│  ├─ 4 opciones: 80-120 tokens
│  ├─ Explicaciones (5): 150-200 tokens
│  ├─ Metadatos: 50-70 tokens
│  └─ Tags y referencias: 30-50 tokens
└─ Estructura JSON: ~100 tokens
─────────────────────────────
TOTAL SALIDA: ~3,500-4,500 tokens

═════════════════════════════
TOTAL GENERACIÓN: ~6,330-7,330 tokens
```

---

## 📈 Comparación de Consumo de Tokens

| Característica | Flashcards | Quizzes |
|----------------|-----------|---------|
| **MaxOutputTokens** | 1,500 | 8,000 |
| **Límite de Entrada** | Sin límite explícito | 12,000 tokens (~48K chars) |
| **Truncamiento** | No implementado | 10,000 caracteres |
| **Prompt Base** | ~230 tokens | ~830 tokens |
| **Tokens por Item** | 80-120 | 350-450 |
| **Generación Típica** | 1,500-2,500 tokens | 6,000-8,000 tokens |
| **Complejidad** | Baja-Media | Alta |

---

## 🔍 Análisis de Prompts

### Flashcards: Prompt Conciso y Directo

**Ventajas:**
- ✅ Prompt corto (~230 tokens fijos)
- ✅ Instrucciones claras y simples
- ✅ Formato JSON sencillo
- ✅ Rápida generación

**Desventajas:**
- ❌ Sin límite de entrada (puede generar prompts muy largos)
- ❌ Menos control sobre calidad de distractores
- ❌ Menos contexto para la IA

### Quizzes: Prompt Detallado y Educativo

**Ventajas:**
- ✅ Instrucciones exhaustivas sobre distractores
- ✅ Control de calidad mediante reglas explícitas
- ✅ Límite de entrada controlado (12,000 tokens)
- ✅ Formato JSON muy estructurado
- ✅ Previene respuestas genéricas

**Desventajas:**
- ❌ Prompt muy largo (~830 tokens fijos)
- ❌ Mayor consumo de tokens
- ❌ Más lento de generar
- ❌ Las reglas detalladas aumentan el costo

---

## 💡 Recomendaciones para Optimización

### 1. Implementar Límites de Entrada para Flashcards
```csharp
// Actualmente NO existe este control
// SUGERENCIA: Agregar en FlashcardController
if (contenido.Length > 10000)
{
    contenido = contenido.Substring(0, 10000);
    _logger.LogWarning("Contenido truncado a 10,000 caracteres");
}
```

### 2. Reducir Tamaño del Prompt de Quiz
- Las reglas de distractores son muy extensas (~500 tokens)
- Podrían resumirse a ~200 tokens manteniendo calidad
- Ahorro estimado: ~300 tokens por generación

### 3. Implementar Caché de Prompts
- Los prompts base se repiten en cada llamada
- Usar system prompts de Gemini (si está disponible)
- Reducir duplicación de instrucciones

### 4. Ajustar MaxOutputTokens según Necesidad
```csharp
// Actual: 8,000 tokens para quizzes
// Optimización: Calcular dinámicamente
int maxTokens = settings.NumberOfQuestions * 450 + 500;
// 5 preguntas = 2,750 tokens
// 10 preguntas = 5,000 tokens
// 20 preguntas = 9,500 tokens
```

### 5. Implementar Reintentos con Backoff Exponencial
```csharp
// Para manejar error 429 (Too Many Requests)
private async Task<AIResponse> GenerateWithRetry(string prompt, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var response = await GenerateTextAsync(prompt);
        if (response.Success || response.ErrorCode != 429)
            return response;
        
        // Esperar 2^i segundos: 1s, 2s, 4s
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
    }
}
```

---

## 🚨 Problema Actual: Error 429

### Causa Raíz
La API de Google Gemini ha alcanzado su límite de cuota debido a:

1. **Rate Limiting:** Demasiadas solicitudes por minuto
   - Plan gratuito: ~15 requests/minuto
   - Plan gratuito: ~1,500 requests/día

2. **Alto Consumo de Tokens por Quiz:**
   - Cada quiz: 6,000-8,000 tokens
   - Prompts largos y detallados

3. **Sin Manejo de Rate Limits:**
   - No hay retry logic
   - No hay detección de error 429
   - No hay mensajes informativos al usuario

### Soluciones Inmediatas

#### Opción A: Nueva API Key (Solución Temporal)
1. Ir a [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Crear nueva API key
3. Actualizar `appsettings.json`
4. Reiniciar aplicación

#### Opción B: Esperar Reset de Cuota
- Las cuotas se restablecen cada minuto/día
- Esperar 1-2 horas y reintentar

#### Opción C: Implementar Manejo de Rate Limits (Solución Permanente)
1. Detectar error 429 en `GenerateTextAsync`
2. Agregar retry logic con exponential backoff
3. Mostrar mensaje informativo al usuario
4. Implementar cola de solicitudes

---

## 📊 Estimación de Costos y Uso

### API Gratuita de Gemini (Límites Actuales)

```
Límites del Plan Gratuito:
├─ Requests por minuto (RPM): 15
├─ Requests por día (RPD): 1,500
├─ Tokens por minuto (TPM): 32,000
└─ Tokens por día: ~1,000,000

Consumo por Operación:
├─ 1 Flashcard (10 cards): ~2,000 tokens
│  └─ Con límite actual: ~750 generaciones/día
│
└─ 1 Quiz (10 preguntas): ~7,000 tokens
   └─ Con límite actual: ~142 generaciones/día

Uso Mixto Estimado:
├─ 50 flashcards/día: 100,000 tokens
├─ 50 quizzes/día: 350,000 tokens
└─ Total: 450,000 tokens/día (45% del límite)
```

### Recomendación Final

**Para uso en producción:**
- Implementar sistema de colas
- Agregar caché para contenido similar
- Considerar upgrade a plan de pago si el uso aumenta
- Monitorear uso de tokens en tiempo real
- Implementar alertas de cuota

---

## 🔧 Próximos Pasos

1. **Inmediato:** Resolver error 429
   - Obtener nueva API key o esperar reset
   
2. **Corto Plazo:** Implementar manejo de errores
   - Detectar y comunicar error 429 al usuario
   - Agregar retry logic básico
   
3. **Mediano Plazo:** Optimizar consumo
   - Reducir tamaño de prompts
   - Implementar caché
   - Ajustar MaxOutputTokens dinámicamente
   
4. **Largo Plazo:** Sistema robusto
   - Cola de solicitudes
   - Monitoreo de cuota
   - Múltiples API keys con balanceo

---

**Fecha de Análisis:** 21 de noviembre de 2025
**Versión:** QuizCraft v1.0
**Modelo IA:** Google Gemini 2.0 Flash Experimental
