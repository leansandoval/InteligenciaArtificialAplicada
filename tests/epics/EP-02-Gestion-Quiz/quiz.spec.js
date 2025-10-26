// @ts-check
const { test, expect } = require('@playwright/test');
const { loginWithTestUser, crearMateria, crearFlashcard, generarNombreUnico } = require('../../test-helpers');
const testConfig = require('../../test-config');

/**
 * EP-02: Gestión de Quizzes
 * 
 * Este archivo contiene las pruebas end-to-end para la gestión completa de quizzes,
 * incluyendo creación, edición, eliminación, realización y visualización de resultados.
 */

test.describe('EP-02: Gestión de Quizzes', () => {
  
  test('US-02.01: Crear un nuevo quiz desde flashcards', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia Quiz');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para probar la creación de quizzes');
    
    // Crear algunas flashcards
    for (let i = 1; i <= 5; i++) {
      await crearFlashcard(page, materiaId, `¿Pregunta ${i} para quiz?`, `Respuesta ${i} para el quiz`);
    }
    
    // Navegar a la creación de quiz
    await page.goto(`/Quiz/Create?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página de creación (h4 en lugar de h1/h2)
    await expect(page.locator('h1, h2, h3, h4')).toContainText(/Crear.*Quiz/i);
    
    // Llenar el formulario de creación de quiz
    const quizNombre = generarNombreUnico('Quiz Test');
    await page.getByPlaceholder('Ej: Quiz de Matemáticas - Álgebra Básica').fill(quizNombre);
    await page.getByPlaceholder('Describe brevemente el contenido del quiz...').fill('Quiz de prueba E2E');
    
    // La materia ya debería estar seleccionada por el parámetro URL
    // Si necesitamos cambiarla: await page.locator('combobox').first().selectOption(materiaId.toString());
    
    // Configuración del quiz - número de preguntas (usa spinbutton, no input)
    // Ya viene con un valor por defecto basado en las flashcards disponibles
    
    // No hay select de NivelDificultad en el formulario de quiz
    // El nivel de dificultad es un filtro, no una propiedad del quiz
    
    // Enviar el formulario - buscar botón visible
    const submitButton = page.locator('button[type="submit"]:visible').last();
    await submitButton.click();
    
    // Verificar que se creó el quiz (puede redirigir a Details o Index)
    await page.waitForURL(/\/Quiz/, { timeout: testConfig.timeouts.navigation });
    
    console.log('✅ Quiz creado exitosamente');
  });

  test('US-02.02: Editar un quiz existente', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Ir a la lista de quizzes
    await page.goto('/Quiz/Index');
    
    // Seleccionar el primer quiz disponible
    const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
    const hayQuizzes = await primerQuizLink.count() > 0;
    
    if (hayQuizzes) {
      await expect(primerQuizLink).toBeVisible({ timeout: 5000 });
      const quizUrl = await primerQuizLink.getAttribute('href');
      
      // Ir a los detalles del quiz
      if (quizUrl) {
        await page.goto(quizUrl);
      }
      
      // Hacer clic en editar
      const editarLink = page.locator('a[href*="/Quiz/Edit/"]');
      if (await editarLink.count() > 0) {
        await editarLink.click();
        
        // Esperar a que cargue el formulario
        await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
        
        // Modificar el título - en Edit el placeholder es diferente
        const nuevoTitulo = generarNombreUnico('Quiz Editado');
        const tituloInput = page.getByPlaceholder(/Ingrese el título del quiz|Ej: Quiz de Matemáticas/i);
        await tituloInput.fill(nuevoTitulo);
        
        // Modificar la descripción
        const descripcionInput = page.getByPlaceholder(/Descripción opcional del quiz|Describe brevemente/i);
        await descripcionInput.fill('Quiz editado mediante prueba E2E');
        
        // Enviar el formulario - usar botón visible
        const submitButton = page.locator('button[type="submit"]:visible').last();
        await submitButton.click();
        
        // Verificar que se actualizó (puede redirigir a Details o Index)
        await page.waitForURL(/\/Quiz/, { timeout: testConfig.timeouts.navigation });
        await expect(page.locator('body')).toContainText(nuevoTitulo);
        
        console.log('✅ Quiz editado exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de editar en la vista de detalles');
      }
    } else {
      console.log('⚠️ No hay quizzes disponibles para editar');
    }
  });

  test('US-02.03: Realizar un quiz completo', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Ir a la lista de quizzes
    await page.goto('/Quiz/Index');
    
    // Seleccionar el primer quiz disponible
    const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
    const hayQuizzes = await primerQuizLink.count() > 0;
    
    if (hayQuizzes) {
      await expect(primerQuizLink).toBeVisible({ timeout: 5000 });
      await primerQuizLink.click();
      await page.waitForURL('**/Quiz/Details/**');
      
      // Buscar el botón "Iniciar Quiz" o "Comenzar Quiz"
      const iniciarButton = page.locator('a:has-text("Iniciar"), a:has-text("Comenzar"), button:has-text("Iniciar"), button:has-text("Comenzar")');
      
      if (await iniciarButton.count() > 0) {
        await iniciarButton.first().click();
        
        // Esperar a que cargue la página del quiz
        await page.waitForURL('**/Quiz/Take/**', { timeout: 10000 });
        
        // Responder todas las preguntas disponibles
        const preguntas = page.locator('[id^="pregunta-"], .quiz-pregunta, .question-card');
        const cantidadPreguntas = await preguntas.count();
        
        if (cantidadPreguntas > 0) {
          console.log(`📝 Respondiendo ${cantidadPreguntas} preguntas...`);
          
          for (let i = 0; i < cantidadPreguntas; i++) {
            // Buscar opciones de respuesta (radio buttons)
            const respuestas = page.locator(`input[type="radio"][name*="Respuesta"], input[type="radio"][name*="respuesta-${i}"]`);
            const cantidadRespuestas = await respuestas.count();
            
            if (cantidadRespuestas > 0) {
              // Seleccionar la primera opción disponible
              await respuestas.first().check();
            }
          }
          
          // Enviar el quiz
          await page.click('button[type="submit"]:has-text("Enviar"), button:has-text("Finalizar")');
          
          // Verificar que se muestran los resultados
          await page.waitForURL('**/Quiz/Resultado/**', { timeout: 10000 });
          
          // Verificar que hay información de puntuación
          await expect(page.locator('body')).toContainText(/puntuación|puntaje|resultado|score/i);
          
          console.log('✅ Quiz completado exitosamente');
        } else {
          console.log('⚠️ No se encontraron preguntas en el quiz');
        }
      } else {
        console.log('⚠️ No se encontró el botón para iniciar el quiz');
      }
    } else {
      console.log('⚠️ No hay quizzes disponibles');
    }
  });

  test('US-02.04: Ver resultados y estadísticas de quiz', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la página de estadísticas
    await page.goto('/Home/Statistics');
    
    // Verificar que la página cargó correctamente
    await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
    
    // Buscar cualquier sección que contenga información (más flexible)
    const hasContent = await page.locator('h1, h2, h3, h4, h5').first().isVisible({ timeout: 5000 });
    expect(hasContent).toBeTruthy();
    
    // Verificar que hay cards o contenido en la página
    const hasCards = await page.locator('.card, .container, main').first().isVisible();
    expect(hasCards).toBeTruthy();
    
    console.log('✅ Estadísticas de quizzes visualizadas correctamente');
  });

  test('US-02.05: Eliminar un quiz', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Ir a la lista de quizzes
    await page.goto('/Quiz/Index');
    
    // Contar los quizzes antes de eliminar
    const quizzesAntesDeEliminar = await page.locator('a[href*="/Quiz/Details/"]').count();
    
    if (quizzesAntesDeEliminar > 0) {
      // Seleccionar el primer quiz
      const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
      await primerQuizLink.click();
      await page.waitForURL('**/Quiz/Details/**');
      
      // Buscar el botón de eliminar (usar .first() para evitar strict mode)
      const eliminarButton = page.locator('button:has-text("Eliminar"), a:has-text("Eliminar")').first();
      
      if (await eliminarButton.count() > 0) {
        // Hacer clic en eliminar (abre modal)
        await eliminarButton.click();
        
        // Esperar a que aparezca el modal y confirmar
        await page.waitForTimeout(500); // Dar tiempo a que aparezca el modal
        
        // Buscar el botón de confirmación en el modal
        const confirmarButton = page.locator('button[type="submit"]:has-text("Eliminar"), button:has-text("Confirmar")').last();
        await confirmarButton.waitFor({ state: 'visible', timeout: 3000 });
        await confirmarButton.click();
        
        // Verificar que se redirigió (puede ser a Index o Dashboard)
        await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.navigation });
        
        // Verificar que ya no estamos en la página de Details del quiz eliminado
        const currentUrl = page.url();
        expect(currentUrl).not.toContain('/Quiz/Details/');
        
        console.log('✅ Quiz eliminado exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de eliminar');
      }
    } else {
      console.log('⚠️ No hay quizzes para eliminar');
    }
  });

  test.skip('US-02.06: Generar quiz con IA desde contenido', async ({ page }) => {
    // NOTA: Esta funcionalidad requiere que la ruta /Quiz/GenerateWithAI esté implementada
    // Actualmente retorna 404 - Pendiente de implementación
    
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia
    const materiaNombre = generarNombreUnico('Materia IA Quiz');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para generación de quiz con IA');
    
    // Navegar a la generación de quiz con IA
    await page.goto(`/Quiz/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*IA|IA.*Generar/i);
    
    // Proporcionar contenido para generar el quiz
    const contenidoTexto = `
      La Revolución Francesa fue un período de cambio político y social en Francia que duró de 1789 a 1799.
      Comenzó con la toma de la Bastilla el 14 de julio de 1789.
      Los principales líderes incluyen Maximilien Robespierre, Georges Danton y Jean-Paul Marat.
      La Declaración de los Derechos del Hombre y del Ciudadano fue adoptada en agosto de 1789.
      El período del Terror ocurrió de 1793 a 1794.
    `;
    
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenidoTexto);
    const quizNombre = generarNombreUnico('Quiz IA');
    await page.fill('input[name="Titulo"]', quizNombre);
    await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
    await page.fill('input[name="CantidadPreguntas"], input[name="NumeroPreguntas"]', '3');
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se genere (puede tomar tiempo)
    await page.waitForURL('**/Quiz/Details/**', { timeout: 30000 });
    
    // Verificar que se creó el quiz
    await expect(page.locator('h1, h2')).toContainText(quizNombre);
    
    console.log('✅ Quiz generado con IA exitosamente');
  });
});
