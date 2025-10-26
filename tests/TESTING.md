# 🧪 Guía de Testing - QuizCraft

## 📋 Resumen de Cambios Realizados

Se ha realizado una refactorización completa de la suite de tests E2E de QuizCraft para corregir los problemas de configuración que causaban que 30 de 34 tests fallaran.

### ✅ Cambios Implementados

#### 1. **Configuración Centralizada** (`tests/test-config.js`)
- **baseURL**: `https://localhost:7249` (puerto correcto de la aplicación)
- **Credenciales**: `admin@quizcraft.com` / `Admin123!` (credenciales reales)
- **Timeouts**: Configuración de timeouts para diferentes escenarios
- **Selectores**: Selectores CSS reutilizables para elementos comunes
- **Mensajes**: Regex flexibles para validaciones de texto

#### 2. **Funciones Helper** (`tests/test-helpers.js`)
Creación de funciones reutilizables para reducir duplicación:
- `loginWithTestUser(page)`: Inicia sesión automáticamente
- `crearMateria(page, nombre, descripcion)`: Crea una materia de prueba
- `crearFlashcard(page, materiaId, pregunta, respuesta)`: Crea una flashcard
- `generarNombreUnico(prefix)`: Genera nombres únicos con timestamp
- `esperarElementoVisible(page, selector, timeout)`: Espera elementos dinámicos

#### 3. **Actualización de playwright.config.js**
- Importa `test-config.js` para usar baseURL centralizado
- `ignoreHTTPSErrors: true` - Ignora errores de certificados SSL en localhost
- `retries: 1` - Reintenta tests fallidos una vez
- Configuración de reportes: JUnit XML, JSON y HTML

#### 4. **Refactorización de Archivos de Test**

Todos los archivos de test han sido refactorizados para:
- Usar `loginWithTestUser()` en lugar de código duplicado de login
- Usar rutas relativas (`/Account/Login`) en lugar de URLs absolutas
- Usar credenciales correctas desde `test-config.js`
- Cambiar de pattern `beforeAll/afterAll` con contexto compartido a `async ({ page })` individual
- Usar `generarNombreUnico()` para evitar conflictos de nombres

**Archivos refactorizados:**
- ✅ `epics/EP-01-Gestion-Materias-Flashcards/flashcard.spec.js` (4 tests)
- ✅ `epics/EP-01-Gestion-Materias-Flashcards/materias/materias.spec.js` (ya funcionaba)
- ✅ `epics/EP-02-Gestion-Quiz/quiz.spec.js` (6 tests)
- ✅ `epics/EP-03-Repaso-Espaciado/repaso.spec.js` (6 tests)
- ✅ `epics/EP-04-IA-Generation/ia-generation.spec.js` (7 tests)
- ✅ `epics/EP-05-Statistics/statistics.spec.js` (10 tests)

---

## 🚀 Cómo Ejecutar los Tests

### Prerequisitos

1. **Iniciar la aplicación QuizCraft**
   ```powershell
   cd C:\QuizCraft\src\QuizCraft.Web
   dotnet run
   ```
   
   La aplicación debe estar corriendo en **https://localhost:7249**

2. **Verificar que la base de datos está configurada**
   - Asegúrate de que el usuario `admin@quizcraft.com` con contraseña `Admin123!` existe
   - La base de datos debe estar inicializada con las migraciones aplicadas

### Ejecutar Todos los Tests

```powershell
cd C:\QuizCraft\tests
npx playwright test
```

### Ejecutar Tests con Reporte Detallado

```powershell
npx playwright test --reporter=list
```

### Ejecutar Tests de un Epic Específico

```powershell
# EP-01: Gestión de Materias y Flashcards
npx playwright test epics/EP-01-Gestion-Materias-Flashcards

# EP-02: Gestión de Quizzes
npx playwright test epics/EP-02-Gestion-Quiz

# EP-03: Repaso Espaciado
npx playwright test epics/EP-03-Repaso-Espaciado

# EP-04: Generación con IA
npx playwright test epics/EP-04-IA-Generation

# EP-05: Dashboard y Estadísticas
npx playwright test epics/EP-05-Statistics
```

### Ejecutar un Test Específico

```powershell
npx playwright test epics/EP-01-Gestion-Materias-Flashcards/flashcard.spec.js:11
```

### Modo Debug (Ver el navegador)

```powershell
npx playwright test --headed --debug
```

### Ver el Reporte HTML

```powershell
npx playwright show-report
```

---

## 📊 Estructura de Tests

```
tests/
├── test-config.js              # Configuración centralizada
├── test-helpers.js             # Funciones helper reutilizables
├── playwright.config.js        # Configuración de Playwright
├── epics/
│   ├── EP-01-Gestion-Materias-Flashcards/
│   │   ├── flashcard.spec.js   # 4 tests de flashcards
│   │   └── materias/
│   │       └── materias.spec.js # 1 test de materias
│   ├── EP-02-Gestion-Quiz/
│   │   └── quiz.spec.js         # 6 tests de quizzes
│   ├── EP-03-Repaso-Espaciado/
│   │   └── repaso.spec.js       # 6 tests de repaso
│   ├── EP-04-IA-Generation/
│   │   └── ia-generation.spec.js # 7 tests de IA
│   └── EP-05-Statistics/
│       └── statistics.spec.js   # 10 tests de estadísticas
└── reports/
    ├── junit-results.xml        # Reporte JUnit
    ├── test-results.json        # Reporte JSON
    └── html/                    # Reporte HTML visual
```

---

## 🔧 Solución de Problemas

### Error: `ERR_CONNECTION_REFUSED`

**Problema**: La aplicación no está corriendo.

**Solución**:
```powershell
cd C:\QuizCraft\src\QuizCraft.Web
dotnet run
```

Espera a ver el mensaje:
```
Now listening on: https://localhost:7249
```

### Error: `ERR_SSL_PROTOCOL_ERROR`

**Problema**: Certificado SSL de localhost no confiable.

**Solución**: Ya está configurado `ignoreHTTPSErrors: true` en `playwright.config.js`.

Si persiste, confía en el certificado de desarrollo:
```powershell
dotnet dev-certs https --trust
```

### Error: Credenciales incorrectas

**Problema**: El usuario de prueba no existe en la base de datos.

**Solución**: 
1. Verifica que `appsettings.Development.json` tiene el usuario admin configurado
2. O crea el usuario manualmente:
   - Email: `admin@quizcraft.com`
   - Password: `Admin123!`

### Tests Lentos o Timeout

**Problema**: Tests tardan demasiado.

**Solución**:
- Aumenta los timeouts en `test-config.js`
- Reduce el número de tests ejecutándolos por epic
- Verifica que la aplicación responde rápido (sin debugging)

### Base de Datos con Datos de Tests Anteriores

**Problema**: Tests fallan porque hay datos de ejecuciones anteriores.

**Solución**:
```powershell
# Opción 1: Resetear la base de datos
cd C:\QuizCraft\src\QuizCraft.Web
dotnet ef database drop --force
dotnet ef database update

# Opción 2: Limpiar solo los datos de prueba
# (implementar script SQL para eliminar entidades con nombres que contengan "Test", "IA", "Quiz Test", etc.)
```

---

## 📈 Mejoras Futuras

### 1. **Test Data Management**
- Implementar setup/teardown de datos por test
- Crear base de datos separada para testing
- Implementar fixtures de datos de prueba

### 2. **CI/CD Integration**
```yaml
# .github/workflows/tests.yml
name: E2E Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Run App
        run: dotnet run --project src/QuizCraft.Web &
      - name: Install Playwright
        run: npx playwright install
      - name: Run Tests
        run: npx playwright test
      - name: Upload Report
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: tests/playwright-report/
```

### 3. **Visual Regression Testing**
```javascript
// Agregar screenshots de referencia
await expect(page).toHaveScreenshot('dashboard.png');
```

### 4. **API Testing**
- Separar tests de UI y API
- Crear tests de integración para endpoints
- Mockear llamadas a Gemini API para tests más rápidos

### 5. **Parallel Execution**
```javascript
// playwright.config.js
workers: process.env.CI ? 1 : 4, // 4 workers en local, 1 en CI
```

---

## 📝 Convenciones de Testing

### Nombres de Tests
- **Formato**: `US-XX.YY: [Descripción clara de lo que hace]`
- **Ejemplo**: `US-02.01: Crear un nuevo quiz desde flashcards`

### Estructura de un Test
```javascript
test('US-XX.YY: Descripción', async ({ page }) => {
  // 1. Arrange - Preparar datos
  await loginWithTestUser(page);
  const materiaNombre = generarNombreUnico('Materia Test');
  
  // 2. Act - Ejecutar acción
  const materiaId = await crearMateria(page, materiaNombre, 'Descripción');
  
  // 3. Assert - Verificar resultado
  await expect(page.locator('h1')).toContainText(materiaNombre);
  
  // 4. Log - Confirmar éxito
  console.log('✅ Test completado exitosamente');
});
```

### Assertions
- Usar regex flexibles en lugar de texto exacto: `/Dashboard|Bienvenido/i`
- Proporcionar mensajes claros en assertions
- Verificar múltiples elementos cuando sea posible

### Manejo de Datos
- Generar nombres únicos: `generarNombreUnico('Prefijo')`
- Limpiar datos después de tests (pendiente implementar)
- No depender de datos de tests anteriores

---

## ✅ Checklist de Pre-Deployment

Antes de desplegar a producción, verificar:

- [ ] **Aplicación corriendo** en https://localhost:7249
- [ ] **Base de datos** inicializada con datos de prueba
- [ ] **Usuario admin** existe con credenciales correctas
- [ ] **Ejecutar tests**: `npx playwright test`
- [ ] **Revisar reporte HTML**: `npx playwright show-report`
- [ ] **Todos los tests críticos pasan** (al menos EP-01, EP-02, EP-03)
- [ ] **Tests de IA funcionan** (EP-04 - requiere Gemini API configurada)
- [ ] **Revisar screenshots** de tests fallidos en `test-results/`
- [ ] **Verificar logs** de la aplicación durante tests
- [ ] **Performance aceptable** (tiempo total < 5 minutos)

---

## 🎯 Resultados Esperados

Después de iniciar la aplicación correctamente, deberías ver:

```
Running 35 tests using 1 worker

✓ [chromium] › epics\EP-01-Gestion-Materias-Flashcards\flashcard.spec.js (4 tests)
✓ [chromium] › epics\EP-01-Gestion-Materias-Flashcards\materias\materias.spec.js (1 test)
✓ [chromium] › epics\EP-02-Gestion-Quiz\quiz.spec.js (6 tests)
✓ [chromium] › epics\EP-03-Repaso-Espaciado\repaso.spec.js (6 tests)
✓ [chromium] › epics\EP-04-IA-Generation\ia-generation.spec.js (7 tests)
✓ [chromium] › epics\EP-05-Statistics\statistics.spec.js (10 tests)

35 passed (3.2m)
```

---

## 📞 Contacto y Soporte

Si encuentras problemas:
1. Revisa los logs en `test-results/`
2. Verifica los screenshots de fallos
3. Consulta la traza: `npx playwright show-trace test-results/.../trace.zip`
4. Revisa esta documentación

---

**Fecha de última actualización**: Enero 2025  
**Versión**: 1.0  
**Estado**: ✅ Tests refactorizados y listos para ejecución
