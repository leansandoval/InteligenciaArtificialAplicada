# Pruebas E2E de QuizCraft

Este directorio contiene todas las pruebas end-to-end (E2E) para la aplicación QuizCraft.

## Estructura del Proyecto

```
tests/
├── epics/                            # Organizados por épicas de Azure DevOps
│   ├── EP-01-Gestion-Materias-Flashcards/
│   │   ├── flashcard.spec.js         # Pruebas automatizadas
│   │   └── flashcard-creation-test.md # Documentación manual
│   ├── EP-02-Generacion-Flashcards-Contenidos/
│   ├── EP-03-Quizzes-Personalizados/
│   ├── EP-04-Estadisticas-Repasos/
│   ├── EP-05-Personalizacion-Colaboracion/
│   ├── EP-06-Infraestructura-Performance/
│   ├── EP-07-Seguridad-Compliance/
│   ├── EP-08-Testing-Quality-Assurance/
│   └── EP-MVP-Minimum-Viable-Product/
├── reports/                          # Reportes de ejecución
├── package.json                      # Dependencias de Node.js
├── playwright.config.js             # Configuración de Playwright
└── README.md                        # Este archivo
```

## Herramientas Utilizadas

- **Playwright**: Framework de testing E2E
- **MCP Playwright**: Extensión de Model Context Protocol
- **Node.js**: Runtime para ejecutar las pruebas

## Instalación

1. Navegar al directorio de pruebas:
```bash
cd tests
```

2. Instalar dependencias:
```bash
npm install
```

3. Instalar navegadores de Playwright:
```bash
npx playwright install
```

## Ejecución de Pruebas

### Ejecución básica
```bash
npm test
```

### Ejecución con interfaz gráfica
```bash
npm run test:headed
```

### Modo debug
```bash
npm run test:debug
```

### Solo pruebas de una épica específica
```bash
npm run test:ep01  # EP-01: Gestión de Materias y Flashcards
npm run test:ep02  # EP-02: Generación de Flashcards y Contenidos
# etc...
```

### Ver reportes
```bash
npm run test:report
```

## Casos de Prueba Implementados

### EP-01: Gestión de Materias y Flashcards ✅
- ✅ **TC001**: Autenticación con credenciales demo
- ✅ **TC002**: Creación exitosa de flashcard
- ✅ **TC003**: Validaciones de campos obligatorios
- ✅ **TC004**: Filtrado por materia

### EP-02: Generación de Flashcards y Contenidos 🔄
- 🚧 Tests pendientes de implementar

### EP-03: Quizzes Personalizados 📋
- 📋 Tests por planificar

### EP-04: Estadísticas y Repasos 📋
- 📋 Tests por planificar

### EP-05: Personalización y Colaboración 📋
- 📋 Tests por planificar

### EP-06: Infraestructura y Performance 📋
- 📋 Tests por planificar

### EP-07: Seguridad y Compliance 📋
- 📋 Tests por planificar

### EP-08: Testing y Quality Assurance 📋
- 📋 Tests por planificar

### EP-MVP: Minimum Viable Product 📋
- 📋 Tests por planificar

## Pruebas Manuales Documentadas

### Creación de Flashcards
Ver: `e2e/playwright/flashcard-creation-test.md`

Prueba manual completa realizada el 10/10/2025 que incluye:
- Autenticación
- Navegación
- Completado de formulario
- Vista previa
- Persistencia de datos
- Verificación de estadísticas

## Configuración

### Navegadores Soportados
- Chrome/Chromium
- Firefox
- Safari/WebKit

### Configuración de Reportes
- HTML: `./reports/html/`
- JSON: `./reports/test-results.json`
- JUnit: `./reports/junit-results.xml`

### Variables de Entorno
- `CI`: Configuración para integración continua
- Base URL: `http://localhost:5291`

## Integración con Azure DevOps

### Test Plans
Las pruebas pueden integrarse con Azure DevOps Test Plans:

1. Importar casos de prueba desde los archivos .md
2. Asociar pruebas automatizadas con test cases
3. Configurar pipelines para ejecución automática

### Reportes en Azure DevOps
Los archivos JUnit pueden ser publicados en Azure DevOps:

```yaml
# En el pipeline de Azure DevOps
- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'JUnit'
    testResultsFiles: 'tests/reports/junit-results.xml'
    testRunTitle: 'QuizCraft E2E Tests'
```

## Mejores Prácticas

1. **Ejecutar pruebas localmente** antes de hacer commit
2. **Mantener datos de prueba** independientes
3. **Documentar nuevos casos** de prueba
4. **Actualizar README** al agregar nuevas pruebas
5. **Usar page objects** para pruebas complejas

## Contribución

Para agregar nuevas pruebas:

1. Crear archivo `.spec.js` en la carpeta correspondiente
2. Seguir el patrón de naming: `[feature].spec.js`
3. Documentar casos de prueba en archivos `.md`
4. Actualizar este README

## Contacto

Para preguntas sobre las pruebas, contactar al equipo de QA de QuizCraft.

---

**Última actualización:** 10 de octubre de 2025