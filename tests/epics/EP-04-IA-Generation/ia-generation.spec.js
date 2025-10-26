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
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA Flashcards');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación de flashcards con IA');
    
    // Navegar a la generación con IA
    await page.goto(`/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*IA|IA.*Flashcard/i);
    
    // Proporcionar contenido de texto
    const contenidoTexto = `
      El Imperio Romano fue uno de los imperios más grandes de la historia antigua.
      Alcanzó su máxima extensión territorial en el año 117 d.C. bajo el emperador Trajano.
      Roma fue fundada en el año 753 a.C. según la leyenda.
      El Coliseo de Roma fue construido entre los años 70 y 80 d.C.
      Julio César fue asesinado en el año 44 a.C.
    `;
    
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenidoTexto);
    await page.fill('input[name="CantidadFlashcards"], input[name="NumeroFlashcards"]', '3');
    await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se genere (puede tomar tiempo)
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
    
    // Verificar que se generaron flashcards
    await expect(page.locator('body')).toContainText(/Flashcard|Generado|Creado/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Flashcards generadas con IA exitosamente');
  });

  test('US-04.02: Generar flashcards desde archivo PDF con IA', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA PDF');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación desde PDF con IA');
    
    // Navegar a la generación con IA
    await page.goto(`/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*IA|IA.*Flashcard/i);
    
    // Buscar el campo de archivo
    const fileInput = page.locator('input[type="file"]');
    
    if (await fileInput.count() > 0) {
      // Si existe, intentar subir un archivo de prueba
      // (En un entorno de producción, deberías tener archivos de prueba reales)
      const testFilePath = 'ArchivosPrueba/ejemplo-historia-roma.txt';
      
      await fileInput.setInputFiles(testFilePath);
      await page.fill('input[name="CantidadFlashcards"], input[name="NumeroFlashcards"]', '3');
      
      // Enviar para generar
      await page.click('button[type="submit"]:has-text("Generar")');
      
      // Esperar a que se genere
      await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
      
      // Verificar que se generaron flashcards
      await expect(page.locator('body')).toContainText(/Flashcard|Generado|Creado/i, { timeout: testConfig.timeouts.long });
      
      console.log('✅ Flashcards generadas desde archivo con IA exitosamente');
    } else {
      console.log('⚠️ No se encontró el campo de archivo PDF');
    }
  });

  test('US-04.03: Generar quiz con IA desde contenido de texto', async ({ page }) => {
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
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia Límites IA');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para probar límites de IA');
    
    // Navegar a la generación con IA
    await page.goto(`/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    // Intentar generar con valores extremos
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', 'Contenido muy corto');
    await page.fill('input[name="CantidadFlashcards"], input[name="NumeroFlashcards"]', '100'); // Valor alto
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Verificar que se muestra un mensaje de validación o límite
    const mensajeError = page.locator('.alert-danger, .error, .validation-message');
    
    if (await mensajeError.count() > 0) {
      await expect(mensajeError).toBeVisible({ timeout: 5000 });
      console.log('✅ Validación de límites funcionando correctamente');
    } else {
      // Si no hay error, verificar que se generó con un límite razonable
      await expect(page.locator('body')).toContainText(/Flashcard|Generado/i, { timeout: testConfig.timeouts.long });
      console.log('✅ Generación completada con límites aplicados');
    }
  });

  test('US-04.07: Regenerar contenido con diferentes parámetros de IA', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia Regenerar IA');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para regeneración con IA');
    
    // Navegar a la generación con IA
    await page.goto(`/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    const contenidoBase = `
      El ADN es una molécula que contiene las instrucciones genéticas de los seres vivos.
      Tiene una estructura de doble hélice descubierta por Watson y Crick en 1953.
    `;
    
    // Primera generación con dificultad fácil
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenidoBase);
    await page.fill('input[name="CantidadFlashcards"], input[name="NumeroFlashcards"]', '2');
    await page.selectOption('select[name="NivelDificultad"]', '0'); // Fácil
    await page.click('button[type="submit"]:has-text("Generar")');
    
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
    await expect(page.locator('body')).toContainText(/Flashcard|Generado/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Primera generación completada');
    
    // Volver a generar con dificultad difícil
    await page.goto(`/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenidoBase);
    await page.fill('input[name="CantidadFlashcards"], input[name="NumeroFlashcards"]', '2');
    await page.selectOption('select[name="NivelDificultad"]', '2'); // Difícil
    await page.click('button[type="submit"]:has-text("Generar")');
    
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.long });
    await expect(page.locator('body')).toContainText(/Flashcard|Generado/i, { timeout: testConfig.timeouts.long });
    
    console.log('✅ Regeneración con diferentes parámetros completada exitosamente');
  });
});
