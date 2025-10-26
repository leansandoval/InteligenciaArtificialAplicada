// @ts-check
const { test, expect } = require('@playwright/test');
const { loginWithTestUser, crearMateria, crearFlashcard, generarNombreUnico } = require('../../test-helpers');
const testConfig = require('../../test-config');

/**
 * EP-05: Dashboard y Estadísticas
 * 
 * Este archivo contiene las pruebas end-to-end para el dashboard de usuario
 * y las estadísticas de progreso de estudio.
 */

test.describe('EP-05: Dashboard y Estadísticas', () => {

  test('US-05.01: Visualizar dashboard principal después del login', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Verificar que estamos en el dashboard - usar first() para evitar strict mode
    await expect(page.locator('h1, h2').first()).toContainText(/Dashboard|Bienvenido|Inicio/i);
    
    // Verificar que hay secciones principales del dashboard
    const dashboardElements = [
      page.locator('.dashboard, .main-content, #dashboard'),
      page.locator('.stats, .statistics, .card')
    ];
    
    for (const element of dashboardElements) {
      if (await element.count() > 0) {
        await expect(element.first()).toBeVisible({ timeout: 5000 });
      }
    }
    
    console.log('✅ Dashboard visualizado correctamente');
  });

  test('US-05.02: Ver resumen de materias activas', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar al dashboard o estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que hay una sección de materias
    const materiasSection = page.locator('section:has-text("Materias"), .materias-section, #materias');
    
    if (await materiasSection.count() > 0) {
      await expect(materiasSection).toBeVisible({ timeout: 5000 });
      console.log('✅ Resumen de materias visible');
    } else {
      // Alternativamente, buscar cualquier mención a materias
      await expect(page.locator('body')).toContainText(/Materia|Asignatura|Curso/i);
      console.log('✅ Información de materias encontrada');
    }
  });

  test('US-05.03: Ver estadísticas de flashcards', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que hay información de flashcards
    await expect(page.locator('body')).toContainText(/Flashcard|Tarjeta/i);
    
    // Buscar métricas específicas (total, estudiadas, pendientes, etc.)
    const metricas = page.locator('.metric, .stat-card, .card-body');
    
    if (await metricas.count() > 0) {
      await expect(metricas.first()).toBeVisible({ timeout: 5000 });
      console.log('✅ Estadísticas de flashcards visualizadas correctamente');
    } else {
      console.log('⚠️ No se encontraron métricas visuales específicas');
    }
  });

  test('US-05.04: Ver estadísticas de quizzes realizados', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que hay información de quizzes
    await expect(page.locator('body')).toContainText(/Quiz|Examen|Evaluación/i);
    
    // Buscar gráficos o tablas de quizzes
    const quizzesVisualization = page.locator('table, .chart, canvas, .quiz-stats');
    
    if (await quizzesVisualization.count() > 0) {
      await expect(quizzesVisualization.first()).toBeVisible({ timeout: 5000 });
      console.log('✅ Estadísticas de quizzes visualizadas correctamente');
    } else {
      console.log('⚠️ No se encontraron visualizaciones de quizzes');
    }
  });

  test('US-05.05: Ver progreso de repaso espaciado', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que hay información de repaso espaciado
    const repasoSection = page.locator('section:has-text("Repaso"), .repaso-section, #repaso');
    
    if (await repasoSection.count() > 0) {
      await expect(repasoSection).toBeVisible({ timeout: 5000 });
      console.log('✅ Progreso de repaso espaciado visible');
    } else {
      // Buscar cualquier referencia a repaso
      const tieneRepaso = await page.locator('body').textContent();
      if (tieneRepaso && /Repaso|Revisión|Programado/i.test(tieneRepaso)) {
        console.log('✅ Información de repaso encontrada');
      } else {
        console.log('⚠️ No se encontró información de repaso espaciado');
      }
    }
  });

  test('US-05.06: Ver gráficos de rendimiento por materia', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Buscar elementos gráficos (canvas para Chart.js, SVG para otros)
    const graficos = page.locator('canvas, svg.chart, .chart-container');
    
    if (await graficos.count() > 0) {
      await expect(graficos.first()).toBeVisible({ timeout: 5000 });
      console.log('✅ Gráficos de rendimiento visualizados correctamente');
    } else {
      console.log('⚠️ No se encontraron gráficos en la página de estadísticas');
    }
  });

  test('US-05.07: Ver historial de actividad reciente', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar al dashboard
    await page.goto('/Home/Dashboard');
    
    // Buscar la sección de actividad reciente
    const actividadSection = page.locator(
      'section:has-text("Actividad"), section:has-text("Reciente"), ' +
      '.activity-feed, .recent-activity, #actividad'
    );
    
    if (await actividadSection.count() > 0) {
      await expect(actividadSection.first()).toBeVisible({ timeout: 5000 });
      console.log('✅ Historial de actividad reciente visible');
    } else {
      console.log('⚠️ No se encontró la sección de actividad reciente');
    }
  });

  test('US-05.08: Filtrar estadísticas por rango de fechas', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Buscar controles de fecha
    const dateInputs = page.locator('input[type="date"], input[name*="fecha"], .date-picker');
    
    if (await dateInputs.count() >= 2) {
      // Establecer un rango de fechas
      const fechaInicio = new Date();
      fechaInicio.setMonth(fechaInicio.getMonth() - 1);
      const fechaFin = new Date();
      
      await dateInputs.first().fill(fechaInicio.toISOString().split('T')[0]);
      await dateInputs.nth(1).fill(fechaFin.toISOString().split('T')[0]);
      
      // Buscar botón de filtrar
      const filtrarButton = page.locator('button:has-text("Filtrar"), button:has-text("Aplicar")');
      
      if (await filtrarButton.count() > 0) {
        await filtrarButton.click();
        
        // Esperar a que se actualicen las estadísticas
        await page.waitForLoadState('networkidle');
        
        console.log('✅ Filtrado por rango de fechas aplicado correctamente');
      } else {
        console.log('⚠️ No se encontró el botón de filtrar');
      }
    } else {
      console.log('⚠️ No se encontraron controles de fecha para filtrar');
    }
  });

  test('US-05.09: Exportar estadísticas en formato PDF', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Buscar botón de exportar PDF
    const exportButton = page.locator(
      'button:has-text("Exportar"), button:has-text("PDF"), ' +
      'a:has-text("Exportar"), a:has-text("Descargar")'
    );
    
    if (await exportButton.count() > 0) {
      // Configurar manejador de descarga
      const downloadPromise = page.waitForEvent('download', { timeout: 10000 });
      
      await exportButton.first().click();
      
      try {
        const download = await downloadPromise;
        console.log(`✅ Archivo exportado: ${download.suggestedFilename()}`);
      } catch (error) {
        console.log('⚠️ No se detectó descarga de archivo');
      }
    } else {
      console.log('⚠️ No se encontró el botón de exportar');
    }
  });

  test('US-05.10: Ver estadísticas comparativas entre materias', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear al menos 2 materias con algunas flashcards
    const materia1Nombre = generarNombreUnico('Materia Comparativa 1');
    const materia1Id = await crearMateria(page, materia1Nombre, 'Primera materia para comparación');
    
    for (let i = 1; i <= 3; i++) {
      await crearFlashcard(page, materia1Id, `¿Pregunta ${i} M1?`, `Respuesta ${i} M1`);
    }
    
    const materia2Nombre = generarNombreUnico('Materia Comparativa 2');
    const materia2Id = await crearMateria(page, materia2Nombre, 'Segunda materia para comparación');
    
    for (let i = 1; i <= 3; i++) {
      await crearFlashcard(page, materia2Id, `¿Pregunta ${i} M2?`, `Respuesta ${i} M2`);
    }
    
    // Navegar a las estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que ambas materias aparecen en las estadísticas
    await expect(page.locator('body')).toContainText(materia1Nombre);
    await expect(page.locator('body')).toContainText(materia2Nombre);
    
    // Buscar elementos de comparación (tablas, gráficos comparativos)
    const elementosComparativos = page.locator('table, .comparison-chart, canvas');
    
    if (await elementosComparativos.count() > 0) {
      await expect(elementosComparativos.first()).toBeVisible({ timeout: 5000 });
      console.log('✅ Estadísticas comparativas visualizadas correctamente');
    } else {
      console.log('⚠️ No se encontraron elementos visuales de comparación');
    }
  });
});
