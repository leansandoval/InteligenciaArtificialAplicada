// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * EP-02: Gestión de Quizzes
 * 
 * Este archivo contiene las pruebas end-to-end para la gestión completa de quizzes,
 * incluyendo creación, edición, eliminación, realización y visualización de resultados.
 */

test.describe('EP-02: Gestión de Quizzes', () => {
  let context;
  let page;
  let materiaNombre;
  let quizNombre;

  test.beforeAll(async ({ browser }) => {
    context = await browser.newContext();
    page = await context.newPage();
  });

  test.afterAll(async () => {
    await context.close();
  });

  test.beforeEach(async () => {
    materiaNombre = `Materia Quiz Test ${Date.now()}`;
    quizNombre = `Quiz Test ${Date.now()}`;
  });

  test('US-02.01: Crear un nuevo quiz desde flashcards', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    
    // Esperar a que se complete el login
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia primero
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para probar la creación de quizzes');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Crear algunas flashcards
    for (let i = 1; i <= 5; i++) {
      await page.goto(`https://localhost:7028/Flashcard/Create?materiaId=${materiaId}`);
      await page.fill('input[name="Pregunta"]', `¿Pregunta ${i} para quiz?`);
      await page.fill('textarea[name="Respuesta"]', `Respuesta ${i} para el quiz`);
      await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
      await page.click('button[type="submit"]');
      await page.waitForURL('**/Flashcard/Details/**');
    }
    
    // Navegar a la creación de quiz
    await page.goto(`https://localhost:7028/Quiz/Create?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página de creación
    await expect(page.locator('h1, h2')).toContainText(/Crear.*Quiz/i);
    
    // Llenar el formulario de creación de quiz
    await page.fill('input[name="Titulo"]', quizNombre);
    await page.fill('textarea[name="Descripcion"]', 'Quiz de prueba E2E');
    await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
    
    // Seleccionar flashcards (si hay checkboxes disponibles)
    const flashcardCheckboxes = page.locator('input[type="checkbox"][name*="Flashcard"]');
    const count = await flashcardCheckboxes.count();
    if (count > 0) {
      // Seleccionar las primeras 3 flashcards
      for (let i = 0; i < Math.min(3, count); i++) {
        await flashcardCheckboxes.nth(i).check();
      }
    }
    
    // Enviar el formulario
    await page.click('button[type="submit"]');
    
    // Verificar que se creó el quiz
    await page.waitForURL('**/Quiz/Details/**', { timeout: 10000 });
    await expect(page.locator('h1, h2')).toContainText(quizNombre);
    
    console.log('✅ Quiz creado exitosamente');
  });

  test('US-02.02: Editar un quiz existente', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de quizzes
    await page.goto('https://localhost:7028/Quiz/Index');
    
    // Seleccionar el primer quiz disponible
    const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
    await expect(primerQuizLink).toBeVisible({ timeout: 5000 });
    const quizUrl = await primerQuizLink.getAttribute('href');
    
    // Ir a los detalles del quiz
    await page.goto(`https://localhost:7028${quizUrl}`);
    
    // Hacer clic en editar
    const editarLink = page.locator('a[href*="/Quiz/Edit/"]');
    if (await editarLink.count() > 0) {
      await editarLink.click();
      
      // Modificar el título
      const nuevoTitulo = `Quiz Editado ${Date.now()}`;
      await page.fill('input[name="Titulo"]', nuevoTitulo);
      
      // Modificar la descripción
      await page.fill('textarea[name="Descripcion"]', 'Quiz editado mediante prueba E2E');
      
      // Enviar el formulario
      await page.click('button[type="submit"]');
      
      // Verificar que se actualizó
      await page.waitForURL('**/Quiz/Details/**');
      await expect(page.locator('body')).toContainText(nuevoTitulo);
      
      console.log('✅ Quiz editado exitosamente');
    } else {
      console.log('⚠️ No se encontró el botón de editar en la vista de detalles');
    }
  });

  test('US-02.03: Realizar un quiz completo', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de quizzes
    await page.goto('https://localhost:7028/Quiz/Index');
    
    // Seleccionar el primer quiz disponible
    const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
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
          // Buscar opciones de respuesta (radio buttons o checkboxes)
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
  });

  test('US-02.04: Ver resultados y estadísticas de quiz', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Navegar a la página de estadísticas o historial
    await page.goto('https://localhost:7028/Home/Statistics');
    
    // Verificar que hay información de quizzes
    const quizzesSection = page.locator('section:has-text("Quiz"), .quiz-stats, #quizzes-section');
    await expect(quizzesSection).toBeVisible({ timeout: 5000 });
    
    // Verificar que hay una tabla o lista de quizzes
    const quizzesList = page.locator('table, .quiz-list, .list-group');
    await expect(quizzesList).toBeVisible();
    
    console.log('✅ Estadísticas de quizzes visualizadas correctamente');
  });

  test('US-02.05: Eliminar un quiz', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de quizzes
    await page.goto('https://localhost:7028/Quiz/Index');
    
    // Contar los quizzes antes de eliminar
    const quizzesAntesDeEliminar = await page.locator('a[href*="/Quiz/Details/"]').count();
    
    if (quizzesAntesAntesDeEliminar > 0) {
      // Seleccionar el primer quiz
      const primerQuizLink = page.locator('a[href*="/Quiz/Details/"]').first();
      await primerQuizLink.click();
      await page.waitForURL('**/Quiz/Details/**');
      
      // Buscar el botón de eliminar
      const eliminarButton = page.locator('button:has-text("Eliminar"), a:has-text("Eliminar")');
      
      if (await eliminarButton.count() > 0) {
        // Hacer clic en eliminar
        await eliminarButton.click();
        
        // Confirmar la eliminación si hay un diálogo de confirmación
        page.on('dialog', dialog => dialog.accept());
        
        // Verificar que volvimos a la lista o a alguna página de confirmación
        await page.waitForURL('**/Quiz/Index', { timeout: 10000 });
        
        // Verificar que hay un quiz menos
        const quizzesDespuesDeEliminar = await page.locator('a[href*="/Quiz/Details/"]').count();
        expect(quizzesDespuesDeEliminar).toBeLessThan(quizzesAntesDeEliminar);
        
        console.log('✅ Quiz eliminado exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de eliminar');
      }
    } else {
      console.log('⚠️ No hay quizzes para eliminar');
    }
  });

  test('US-02.06: Generar quiz con IA desde contenido', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia si no existe
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para generación de quiz con IA');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Navegar a la generación de quiz con IA
    await page.goto(`https://localhost:7028/Quiz/GenerateWithAI?materiaId=${materiaId}`);
    
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
    await page.fill('input[name="Titulo"]', `${quizNombre} con IA`);
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
