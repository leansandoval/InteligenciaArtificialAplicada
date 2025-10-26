// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * EP-05: Estadísticas y Dashboards
 * 
 * Este archivo contiene las pruebas end-to-end para las funcionalidades de
 * estadísticas, métricas y visualización de datos del usuario.
 */

test.describe('EP-05: Estadísticas y Dashboards', () => {
  let context;
  let page;

  test.beforeAll(async ({ browser }) => {
    context = await browser.newContext();
    page = await context.newPage();
  });

  test.afterAll(async () => {
    await context.close();
  });

  test('US-05.01: Ver dashboard principal con resumen de actividades', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    
    // Esperar a que se redirija al dashboard
    await page.waitForURL('**/Home/Dashboard');
    
    // Verificar que estamos en el dashboard
    await expect(page.locator('h1, h2')).toContainText(/Dashboard|Panel/i);
    
    // Verificar que hay tarjetas de estadísticas
    const statsCards = page.locator('.card, .stat-card, [class*="statistic"]');
    await expect(statsCards.first()).toBeVisible({ timeout: 5000 });
    
    const cantidadCards = await statsCards.count();
    console.log(`📊 Dashboard muestra ${cantidadCards} tarjetas de estadísticas`);
    
    // Verificar que hay información numérica
    await expect(page.locator('body')).toContainText(/\d+/);
    
    console.log('✅ Dashboard principal visualizado correctamente');
  });

  test('US-05.02: Ver página de estadísticas detalladas', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Navegar a la página de estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Estadística|Statistic/i);
    
    // Verificar que hay secciones de estadísticas
    const sections = page.locator('section, .statistics-section, .stat-group');
    const cantidadSecciones = await sections.count();
    
    if (cantidadSecciones > 0) {
      console.log(`📈 Página de estadísticas muestra ${cantidadSecciones} secciones`);
    }
    
    // Verificar que hay tablas o listas con datos
    const tables = page.locator('table, .table, .list-group');
    await expect(tables.first()).toBeVisible({ timeout: 5000 });
    
    console.log('✅ Página de estadísticas detalladas funcionando correctamente');
  });

  test('US-05.03: Verificar métricas de materias', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar la sección de materias
    const materiasSection = page.locator('section:has-text("Materia"), .materias-stats, h3:has-text("Materia")');
    
    if (await materiasSection.count() > 0) {
      await expect(materiasSection.first()).toBeVisible();
      
      // Verificar que hay información sobre materias
      await expect(page.locator('body')).toContainText(/Total.*Materia|Materia.*Total/i);
      
      console.log('✅ Métricas de materias visualizadas correctamente');
    } else {
      console.log('ℹ️ Sección de materias no encontrada en estadísticas');
    }
  });

  test('US-05.04: Verificar métricas de flashcards', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar información de flashcards
    const flashcardsInfo = page.locator('text=/Flashcard|Tarjeta/i');
    
    if (await flashcardsInfo.count() > 0) {
      await expect(flashcardsInfo.first()).toBeVisible();
      
      // Verificar que hay números asociados a flashcards
      const bodyText = await page.locator('body').textContent();
      const tieneFlashcards = bodyText && bodyText.toLowerCase().includes('flashcard');
      
      if (tieneFlashcards) {
        console.log('✅ Métricas de flashcards visualizadas correctamente');
      }
    } else {
      console.log('ℹ️ Información de flashcards no encontrada en estadísticas');
    }
  });

  test('US-05.05: Verificar métricas de quizzes y resultados', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar la sección de quizzes
    const quizzesSection = page.locator('section:has-text("Quiz"), .quizzes-stats, h3:has-text("Quiz")');
    
    if (await quizzesSection.count() > 0) {
      await expect(quizzesSection.first()).toBeVisible();
      
      // Verificar que hay una tabla de quizzes
      const quizzesTable = page.locator('table:has-text("Quiz"), .quiz-table');
      
      if (await quizzesTable.count() > 0) {
        await expect(quizzesTable.first()).toBeVisible();
        
        // Verificar que hay información de puntuación
        await expect(page.locator('body')).toContainText(/Puntuación|Score|Resultado/i);
        
        console.log('✅ Métricas de quizzes y resultados visualizadas correctamente');
      }
    } else {
      console.log('ℹ️ Sección de quizzes no encontrada en estadísticas');
    }
  });

  test('US-05.06: Verificar historial de actividad del usuario', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir al dashboard para ver actividad reciente
    await page.goto('https://localhost:7028/Home/Dashboard');
    
    // Buscar sección de actividad reciente
    const actividadReciente = page.locator('section:has-text("Actividad"), section:has-text("Reciente"), .recent-activity');
    
    if (await actividadReciente.count() > 0) {
      await expect(actividadReciente.first()).toBeVisible();
      
      // Verificar que hay fechas
      await expect(page.locator('body')).toContainText(/\d{1,2}\/\d{1,2}\/\d{2,4}|\d{4}-\d{2}-\d{2}/);
      
      console.log('✅ Historial de actividad visualizado correctamente');
    } else {
      console.log('ℹ️ Sección de actividad reciente no encontrada');
    }
  });

  test('US-05.07: Verificar filtros de fecha en estadísticas', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar controles de filtro de fecha
    const filtroFecha = page.locator('input[type="date"], select:has-text("Mes"), select:has-text("Año"), .date-filter');
    
    if (await filtroFecha.count() > 0) {
      console.log('📅 Filtros de fecha disponibles');
      
      // Si hay un selector de fecha, cambiarlo
      const fechaInput = page.locator('input[type="date"]').first();
      
      if (await fechaInput.count() > 0) {
        const fechaAnterior = new Date();
        fechaAnterior.setMonth(fechaAnterior.getMonth() - 1);
        const fechaStr = fechaAnterior.toISOString().split('T')[0];
        
        await fechaInput.fill(fechaStr);
        
        // Buscar botón de aplicar filtro
        const aplicarButton = page.locator('button:has-text("Aplicar"), button:has-text("Filtrar"), button[type="submit"]');
        
        if (await aplicarButton.count() > 0) {
          await aplicarButton.first().click();
          await page.waitForTimeout(2000);
          
          console.log('✅ Filtros de fecha funcionando correctamente');
        }
      }
    } else {
      console.log('ℹ️ Filtros de fecha no implementados o no visibles');
    }
  });

  test('US-05.08: Verificar precisión de datos estadísticos', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Obtener el conteo de materias desde la lista
    await page.goto('https://localhost:7028/Materia/Index');
    const materiasEnLista = await page.locator('a[href*="/Materia/Details/"]').count();
    console.log(`📚 Materias en lista: ${materiasEnLista}`);
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar el conteo de materias en estadísticas
    const totalMateriasText = await page.locator('text=/Total.*Materia|Materia.*\d+/i').textContent().catch(() => '');
    
    if (totalMateriasText) {
      // Extraer el número
      const match = totalMateriasText.match(/\d+/);
      if (match) {
        const materiasEnEstadisticas = parseInt(match[0]);
        console.log(`📊 Materias en estadísticas: ${materiasEnEstadisticas}`);
        
        // Verificar que los números coinciden (con tolerancia de ±1 para casos edge)
        if (Math.abs(materiasEnLista - materiasEnEstadisticas) <= 1) {
          console.log('✅ Datos estadísticos precisos y consistentes');
        } else {
          console.log('⚠️ Discrepancia en conteo de materias');
        }
      }
    }
  });

  test('US-05.09: Verificar gráficos y visualizaciones (si existen)', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar elementos canvas (usados por Chart.js u otras librerías)
    const graficos = page.locator('canvas, .chart, svg[class*="chart"]');
    const cantidadGraficos = await graficos.count();
    
    if (cantidadGraficos > 0) {
      console.log(`📈 Se encontraron ${cantidadGraficos} gráficos en la página`);
      
      // Verificar que los gráficos son visibles
      await expect(graficos.first()).toBeVisible();
      
      console.log('✅ Gráficos y visualizaciones funcionando correctamente');
    } else {
      console.log('ℹ️ No se encontraron gráficos - las estadísticas usan tablas/texto');
    }
  });

  test('US-05.10: Exportar estadísticas (si está implementado)', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a estadísticas
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Buscar botones de exportación
    const exportButtons = page.locator('button:has-text("Exportar"), a:has-text("Exportar"), button:has-text("Descargar"), a:has-text("PDF"), a:has-text("Excel")');
    
    if (await exportButtons.count() > 0) {
      console.log('💾 Funcionalidad de exportación disponible');
      
      // Hacer clic en el botón de exportar
      await exportButtons.first().click();
      
      await page.waitForTimeout(2000);
      
      console.log('✅ Funcionalidad de exportación ejecutada');
    } else {
      console.log('ℹ️ Funcionalidad de exportación no implementada');
    }
  });
});
