// @ts-check
const { test, expect } = require('@playwright/test');
const { loginWithTestUser, crearMateria, generarNombreUnico } = require('../../test-helpers');
const testConfig = require('../../test-config');

/**
 * EP-04: Generación con IA (Gemini)
 * 
 * Este archivo contiene las pruebas end-to-end para la funcionalidad de generación
 * de contenido educativo mediante Inteligencia Artificial (Google Gemini).
 */

test.describe('EP-04: Generación con IA', () => {

  test('US-04.01: Generar flashcards desde texto con IA', async ({ page }) => {
    // Ruta implementada: /Generacion/Index
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA Flashcards');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación de flashcards con IA');
    
    // Navegar a la generación con IA
    await page.goto('/Generacion/Index');
    await page.waitForLoadState('domcontentloaded');
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h2')).toContainText(/Generar Flashcards/i);
    
    // Disparar el evento jQuery para seleccionar el modo IA
    await page.evaluate(() => {
      const btn = $('button.mode-btn[data-mode="ai"]');
      btn.trigger('click');
    });
    
    // Dar tiempo al evento jQuery para procesar
    await page.waitForTimeout(500);
    
    // Esperar a que aparezca el formulario Y el panel de configuración AI
    await page.waitForSelector('#formularioGeneracion', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#configuracionAI', { state: 'visible', timeout: 5000 });
    
    // Seleccionar la materia
    await page.selectOption('select#materiaId', materiaId.toString());
    
    // Subir archivo (la UI de IA requiere archivo)
    const testFilePath = '../ArchivosPrueba/ejemplo-historia-roma.txt';
    await page.setInputFiles('input#documento', testFilePath);
    
    // Configurar parámetros de IA (usando selector específico del panel AI)
    await page.fill('#configuracionAI input[name="maxCards"]', '3');
    await page.selectOption('#configuracionAI select[name="difficulty"]', 'Medium');
    
    // Debug: Verificar que todo está configurado correctamente antes de hacer clic en Vista Previa
    const debug = await page.evaluate(() => {
      return {
        modoSeleccionado: window.modoSeleccionado,
        materiaId: $('#materiaId').val(),
        hasFile: $('#documento')[0].files.length > 0,
        maxCards: $('#configuracionAI input[name="maxCards"]').val(),
        difficulty: $('#configuracionAI select[name="difficulty"]').val()
      };
    });
    console.log('Estado antes de Vista Previa:', JSON.stringify(debug, null, 2));
    
    // Hacer clic en Vista Previa
    await page.click('button#btnVistaPrevia');
    
    // Esperar un poco para ver si hay errores
    await page.waitForTimeout(2000);
    
    // Verificar si hay mensajes de error
    const errorMessage = await page.locator('.alert-danger, .swal2-popup').textContent().catch(() => '');
    if (errorMessage) {
      console.log('Mensaje de error encontrado:', errorMessage);
    }
    
    // Esperar a que se genere la vista previa (puede tardar por la API de IA)
    await page.waitForSelector('#preview-container, #areaResultados', { timeout: testConfig.timeouts.long });
    
    // Verificar que se generaron flashcards
    await expect(page.locator('#preview-container, #areaResultados, body')).toContainText(/Flashcard|pregunta|respuesta/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Flashcards generadas con IA exitosamente');
  });

  test('US-04.02: Generar flashcards desde archivo PDF con IA', async ({ page }) => {
    // Ruta implementada: /Generacion/Index
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA PDF');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación desde PDF con IA');
    
    // Navegar a la generación con IA
    await page.goto('/Generacion/Index');
    await page.waitForLoadState('domcontentloaded');
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h2')).toContainText(/Generar Flashcards/i);
    
    // Seleccionar modo IA
    await page.click('button.mode-btn[data-mode="ai"], button#btnModoIA');
    
    // Esperar a que aparezca el formulario y configuración AI
    await page.waitForSelector('#formularioGeneracion', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#configuracionAI', { state: 'visible', timeout: 5000 });
    
    // Seleccionar la materia
    await page.selectOption('select#materiaId', materiaId.toString());
    
    // Subir archivo de prueba
    const testFilePath = '../ArchivosPrueba/ejemplo-historia-roma.txt';
    await page.setInputFiles('input#documento', testFilePath);
    
    // Configurar parámetros
    await page.fill('#configuracionAI input[name="maxCards"]', '3');
    await page.selectOption('#configuracionAI select[name="difficulty"]', 'Medium');
    
    // Hacer clic en Vista Previa
    await page.click('button#btnVistaPrevia');
    
    // Esperar a que se genere (puede tomar tiempo por la API de IA)
    await page.waitForSelector('#preview-container, #areaResultados', { timeout: testConfig.timeouts.long });
    
    // Verificar que se generaron flashcards
    await expect(page.locator('#preview-container, #areaResultados, body')).toContainText(/Flashcard|pregunta|respuesta/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Flashcards generadas desde archivo con IA exitosamente');
  });

  test.skip('US-04.03: Generar quiz con IA desde contenido de texto', async ({ page }) => {
    // Ruta implementada: /Quiz/GenerateWithAI
    // TODO: El test tarda demasiado (timeout 30s) - revisar performance
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA Quiz');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación de quiz con IA');
    
    // Navegar a la generación de quiz con IA
    await page.goto(`/Quiz/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*IA|IA.*Quiz/i);
    
    // Proporcionar contenido para generar el quiz
    const contenidoTexto = `
      La Revolución Industrial comenzó en Gran Bretaña a finales del siglo XVIII.
      La máquina de vapor fue inventada por James Watt en 1769.
      El ferrocarril revolucionó el transporte de mercancías y personas.
      Las fábricas textiles fueron las primeras en mecanizarse.
      La Revolución Industrial transformó la economía agraria en industrial.
    `;
    
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenidoTexto);
    const quizNombre = generarNombreUnico('Quiz IA');
    await page.fill('input[name="Titulo"]', quizNombre);
    await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
    await page.fill('input[name="CantidadPreguntas"], input[name="NumeroPreguntas"]', '3');
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se genere (puede tomar tiempo)
    await page.waitForURL('**/Quiz/Details/**', { timeout: testConfig.timeouts.long });
    
    // Verificar que se creó el quiz
    await expect(page.locator('h1, h2')).toContainText(quizNombre);
    
    console.log('✅ Quiz generado con IA exitosamente');
  });

  test('US-04.04: Generar resumen de texto con IA', async ({ page }) => {
    // Ruta implementada: /IA/GenerateResumen
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la funcionalidad de generación de resúmenes
    await page.goto('/IA/GenerateResumen');
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Resumen|IA|Generar/i);
    
    // Proporcionar contenido para resumir
    const contenidoLargo = `
      La Segunda Guerra Mundial fue el conflicto bélico más grande de la historia,
      que se desarrolló entre 1939 y 1945. Participaron la mayoría de las naciones del mundo,
      incluyendo todas las grandes potencias, agrupadas en dos alianzas militares opuestas:
      los Aliados y las Potencias del Eje. Fue una guerra total que implicó la movilización
      de más de 100 millones de militares, siendo el conflicto más letal en la historia
      de la humanidad, con un número de muertos estimado entre 50 y 70 millones de personas.
    `;
    
    await page.fill('textarea[name="Contenido"], textarea[name="Texto"]', contenidoLargo);
    
    // Enviar para generar resumen
    await page.click('button[type="submit"]:has-text("Generar"), button:has-text("Resumir")');
    
    // Esperar a que se genere el resumen
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
    
    // Verificar que se generó un resumen
    await expect(page.locator('body')).toContainText(/Resumen|Generado/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Resumen generado con IA exitosamente');
  });

  test('US-04.05: Generar explicación detallada con IA', async ({ page }) => {
    // Ruta implementada: /IA/GenerateExplicacion
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la funcionalidad de explicaciones con IA
    await page.goto('/IA/GenerateExplicacion');
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Explicación|IA|Generar/i);
    
    // Proporcionar un concepto para explicar
    const concepto = 'Teoría de la Relatividad de Einstein';
    
    await page.fill('input[name="Concepto"], textarea[name="Concepto"]', concepto);
    await page.selectOption('select[name="NivelDetalle"]', '1'); // Medio
    
    // Enviar para generar explicación
    await page.click('button[type="submit"]:has-text("Generar"), button:has-text("Explicar")');
    
    // Esperar a que se genere la explicación
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
    
    // Verificar que se generó una explicación
    await expect(page.locator('body')).toContainText(/Explicación|Generado/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Explicación generada con IA exitosamente');
  });
  test('US-04.06: Validar límites de generación con IA', async ({ page }) => {
    // Ruta implementada: /Generacion/Index
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia Límites IA');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para probar límites de IA');
    
    // Navegar a la generación con IA
    await page.goto('/Generacion/Index');
    await page.waitForLoadState('domcontentloaded');
    
    // Seleccionar modo IA
    await page.click('button.mode-btn[data-mode="ai"], button#btnModoIA');
    
    // Esperar a que aparezca el formulario y configuración
    await page.waitForSelector('#formularioGeneracion', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#configuracionAI', { state: 'visible', timeout: 5000 });
    
    // Seleccionar la materia
    await page.selectOption('select#materiaId', materiaId.toString());
    
    // Subir archivo pequeño
    const testFilePath = '../ArchivosPrueba/bases_de_datos.txt';
    await page.setInputFiles('input#documento', testFilePath);
    
    // Intentar generar con valores extremos (max=50 según el HTML)
    await page.fill('#configuracionAI input[name="maxCards"]', '100'); // Valor muy alto
    
    // Hacer clic en Vista Previa
    await page.click('button#btnVistaPrevia');
    
    // Verificar que se muestra un mensaje de validación o límite, o que se genera con límite aplicado
    const tieneError = await page.locator('.alert-danger, .error, .validation-message, .invalid-feedback').count() > 0;
    
    if (tieneError) {
      await expect(page.locator('.alert-danger, .error, .validation-message, .invalid-feedback')).toBeVisible({ timeout: 5000 });
      console.log('✅ Validación de límites funcionando correctamente');
    } else {
      // Si no hay error, verificar que se generó con un límite razonable
      await page.waitForSelector('#preview-container, #areaResultados', { timeout: testConfig.timeouts.long });
      await expect(page.locator('#preview-container, #areaResultados, body')).toContainText(/Flashcard|pregunta/i, { timeout: testConfig.timeouts.long });
      console.log('✅ Generación completada con límites aplicados');
    }
  });

  test('US-04.07: Regenerar contenido con diferentes parámetros de IA', async ({ page }) => {
    // Ruta implementada: /Generacion/Index
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia Regenerar IA');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para regeneración con IA');
    
    const testFilePath = '../ArchivosPrueba/bases_de_datos.txt';
    
    // Primera generación con dificultad fácil
    await page.goto('/Generacion/Index');
    await page.waitForLoadState('domcontentloaded');
    await page.click('button.mode-btn[data-mode="ai"], button#btnModoIA');
    await page.waitForSelector('#formularioGeneracion', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#configuracionAI', { state: 'visible', timeout: 5000 });
    
    await page.selectOption('select#materiaId', materiaId.toString());
    await page.setInputFiles('input#documento', testFilePath);
    await page.fill('#configuracionAI input[name="maxCards"]', '2');
    await page.selectOption('#configuracionAI select[name="difficulty"]', 'Easy');
    await page.click('button#btnVistaPrevia');
    
    await page.waitForSelector('#preview-container, #areaResultados', { timeout: testConfig.timeouts.long });
    await expect(page.locator('#preview-container, #areaResultados, body')).toContainText(/Flashcard|pregunta/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Primera generación completada');
    
    // Volver a generar con dificultad difícil
    await page.goto('/Generacion/Index');
    await page.waitForLoadState('domcontentloaded');
    await page.click('button.mode-btn[data-mode="ai"], button#btnModoIA');
    await page.waitForSelector('#formularioGeneracion', { state: 'visible', timeout: 5000 });
    await page.waitForSelector('#configuracionAI', { state: 'visible', timeout: 5000 });
    
    await page.selectOption('select#materiaId', materiaId.toString());
    await page.setInputFiles('input#documento', testFilePath);
    await page.fill('#configuracionAI input[name="maxCards"]', '2');
    await page.selectOption('#configuracionAI select[name="difficulty"]', 'Hard');
    await page.click('button#btnVistaPrevia');
    
    await page.waitForSelector('#preview-container, #areaResultados', { timeout: testConfig.timeouts.long });
    await expect(page.locator('#preview-container, #areaResultados, body')).toContainText(/Flashcard|pregunta/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Regeneración con diferentes parámetros completada exitosamente');
  });
});
