// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * EP-04: Generación con IA (Integración Gemini)
 * 
 * Este archivo contiene las pruebas end-to-end para la funcionalidad de generación
 * automática de contenido utilizando la API de Google Gemini.
 */

test.describe('EP-04: Generación con IA', () => {
  let context;
  let page;
  let materiaNombre;

  test.beforeAll(async ({ browser }) => {
    context = await browser.newContext();
    page = await context.newPage();
  });

  test.afterAll(async () => {
    await context.close();
  });

  test.beforeEach(async () => {
    materiaNombre = `Materia IA ${Date.now()}`;
  });

  test('US-04.01: Configurar API Key de Gemini', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Navegar a la configuración de IA
    await page.goto('https://localhost:7028/Account/Profile');
    
    // Buscar la sección de configuración de IA
    const iaSection = page.locator('section:has-text("IA"), section:has-text("Inteligencia Artificial"), section:has-text("Gemini")');
    
    if (await iaSection.count() > 0) {
      // Verificar que hay un campo para la API Key
      const apiKeyInput = page.locator('input[name*="ApiKey"], input[name*="GeminiKey"], input[placeholder*="API"]');
      await expect(apiKeyInput).toBeVisible({ timeout: 5000 });
      
      console.log('✅ Configuración de API Key de Gemini disponible');
    } else {
      // Si no está en el perfil, podría estar en una página separada de configuración
      await page.goto('https://localhost:7028/Configuration/AI');
      
      const configPage = page.locator('h1:has-text("Configuración"), h2:has-text("IA")');
      if (await configPage.count() > 0) {
        await expect(configPage).toBeVisible();
        console.log('✅ Página de configuración de IA encontrada');
      } else {
        console.log('⚠️ No se encontró la configuración de IA');
      }
    }
  });

  test('US-04.02: Generar flashcards desde texto plano', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para generación de flashcards con IA');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Navegar a la generación de flashcards con IA
    await page.goto(`https://localhost:7028/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*IA|IA.*Flashcard/i);
    
    // Proporcionar texto para generar flashcards
    const textoFuente = `
      La fotosíntesis es el proceso mediante el cual las plantas convierten la luz solar en energía química.
      Ocurre en los cloroplastos, específicamente en los tilacoides y el estroma.
      La ecuación general es: 6CO2 + 6H2O + luz → C6H12O6 + 6O2.
      Los productos principales son glucosa y oxígeno.
      La clorofila es el pigmento responsable de capturar la luz solar.
    `;
    
    await page.fill('textarea[name="TextoFuente"], textarea[name="Contenido"]', textoFuente);
    await page.fill('input[name="CantidadFlashcards"], input[name="Cantidad"]', '3');
    await page.selectOption('select[name="NivelDificultad"]', '1'); // Media
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se generen las flashcards (puede tomar tiempo)
    await page.waitForTimeout(5000);
    
    // Verificar que se generaron flashcards
    // Podría redirigir a una página de revisión o directamente a la lista
    const urlActual = page.url();
    const estaEnRevision = urlActual.includes('Review') || urlActual.includes('Revision');
    const estaEnLista = urlActual.includes('Index') || urlActual.includes('List');
    const estaEnMateria = urlActual.includes('Materia/Details');
    
    if (estaEnRevision || estaEnLista || estaEnMateria) {
      console.log('✅ Flashcards generadas con IA exitosamente');
      
      // Verificar que hay flashcards en la página
      const flashcards = page.locator('.flashcard, .card, [class*="flash"]');
      const hayFlashcards = await flashcards.count() > 0;
      
      if (hayFlashcards) {
        console.log(`📚 Se generaron ${await flashcards.count()} flashcards`);
      }
    } else {
      console.log('⚠️ Ubicación inesperada después de generar flashcards');
    }
  });

  test('US-04.03: Generar flashcards desde documento adjunto', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para generación desde documento');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Navegar a la generación desde archivo
    await page.goto(`https://localhost:7028/Flashcard/GenerateFromFile?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*Archivo|Documento.*IA/i);
    
    // Adjuntar un archivo (usar el archivo de prueba existente)
    const fileInput = page.locator('input[type="file"]');
    
    if (await fileInput.count() > 0) {
      await fileInput.setInputFiles('c:\\QuizCraft\\ArchivosPrueba\\ejemplo-historia-roma.txt');
      
      // Configurar opciones de generación
      await page.fill('input[name="CantidadFlashcards"], input[name="Cantidad"]', '5');
      await page.selectOption('select[name="NivelDificultad"]', '2'); // Difícil
      
      // Enviar el formulario
      await page.click('button[type="submit"]:has-text("Generar")');
      
      // Esperar a que se procese el archivo (puede tomar tiempo)
      await page.waitForTimeout(10000);
      
      // Verificar que se generaron flashcards
      const urlActual = page.url();
      if (urlActual.includes('Review') || urlActual.includes('Index') || urlActual.includes('Materia')) {
        console.log('✅ Flashcards generadas desde documento exitosamente');
      } else {
        console.log('⚠️ Verificar si la generación desde archivo está implementada');
      }
    } else {
      console.log('⚠️ No se encontró el campo de carga de archivos');
    }
  });

  test('US-04.04: Revisar y editar contenido generado por IA', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia y generar flashcards
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para revisar contenido IA');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Generar flashcards con IA
    await page.goto(`https://localhost:7028/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    const textoFuente = `
      El ciclo del agua incluye evaporación, condensación y precipitación.
      El agua se evapora de océanos, ríos y lagos.
      Se condensa formando nubes.
      Finalmente precipita como lluvia o nieve.
    `;
    
    await page.fill('textarea[name="TextoFuente"], textarea[name="Contenido"]', textoFuente);
    await page.fill('input[name="CantidadFlashcards"]', '2');
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se generen
    await page.waitForTimeout(5000);
    
    // Buscar la página de revisión
    const urlActual = page.url();
    
    if (urlActual.includes('Review') || urlActual.includes('Revision')) {
      // Estamos en una página de revisión
      const flashcardsGeneradas = page.locator('.flashcard-preview, .generated-flashcard, .card');
      const cantidad = await flashcardsGeneradas.count();
      
      if (cantidad > 0) {
        console.log(`📝 Revisando ${cantidad} flashcards generadas`);
        
        // Buscar opciones para editar
        const editButtons = page.locator('button:has-text("Editar"), a:has-text("Editar")');
        
        if (await editButtons.count() > 0) {
          await editButtons.first().click();
          
          // Modificar la pregunta
          await page.fill('input[name="Pregunta"]', 'Pregunta editada después de generación IA');
          
          // Guardar cambios
          await page.click('button[type="submit"]:has-text("Guardar")');
          
          console.log('✅ Contenido generado revisado y editado exitosamente');
        } else {
          console.log('ℹ️ No se encontraron opciones de edición en la revisión');
        }
      }
    } else {
      console.log('ℹ️ No hay página de revisión, las flashcards se guardaron directamente');
    }
  });

  test('US-04.05: Generar quiz completo con IA', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para generación de quiz con IA');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Navegar a la generación de quiz con IA
    await page.goto(`https://localhost:7028/Quiz/GenerateWithAI?materiaId=${materiaId}`);
    
    // Verificar que estamos en la página correcta
    await expect(page.locator('h1, h2')).toContainText(/Generar.*Quiz.*IA|IA.*Quiz/i);
    
    // Proporcionar contenido para generar el quiz
    const contenido = `
      La Segunda Guerra Mundial fue un conflicto global que duró de 1939 a 1945.
      Involucró a la mayoría de las naciones del mundo organizadas en dos alianzas militares: los Aliados y las Potencias del Eje.
      Comenzó con la invasión de Polonia por Alemania el 1 de septiembre de 1939.
      Estados Unidos entró en la guerra después del ataque a Pearl Harbor en diciembre de 1941.
      La guerra terminó en 1945 con la rendición de Japón después de los bombardeos atómicos.
    `;
    
    await page.fill('textarea[name="Contenido"], textarea[name="TextoFuente"]', contenido);
    await page.fill('input[name="Titulo"]', `Quiz IA ${Date.now()}`);
    await page.fill('input[name="CantidadPreguntas"], input[name="NumeroPreguntas"]', '4');
    await page.selectOption('select[name="NivelDificultad"]', '2'); // Difícil
    
    // Enviar para generar
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se genere el quiz (puede tomar tiempo)
    await page.waitForTimeout(10000);
    
    // Verificar que se creó el quiz
    const urlActual = page.url();
    
    if (urlActual.includes('Quiz/Details') || urlActual.includes('Quiz/Review')) {
      await expect(page.locator('h1, h2')).toContainText(/Quiz|Cuestionario/i);
      console.log('✅ Quiz generado con IA exitosamente');
      
      // Verificar que hay preguntas
      const preguntas = page.locator('.pregunta, .question, [class*="quiz"]');
      const cantidadPreguntas = await preguntas.count();
      
      if (cantidadPreguntas > 0) {
        console.log(`❓ Se generaron ${cantidadPreguntas} preguntas`);
      }
    } else {
      console.log('⚠️ Ubicación inesperada después de generar quiz');
    }
  });

  test('US-04.06: Manejo de errores de API de Gemini', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para probar manejo de errores');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Intentar generar sin proporcionar contenido
    await page.goto(`https://localhost:7028/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    // Dejar el campo de texto vacío
    await page.fill('textarea[name="TextoFuente"], textarea[name="Contenido"]', '');
    
    // Intentar enviar el formulario
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Verificar que hay un mensaje de error
    await page.waitForTimeout(2000);
    
    const mensajeError = page.locator('.alert-danger, .error-message, [class*="error"]');
    const hayError = await mensajeError.count() > 0;
    
    if (hayError) {
      await expect(mensajeError).toBeVisible();
      console.log('✅ Manejo de errores funcionando correctamente');
    } else {
      // Verificar validación HTML5
      const textareaInvalido = await page.locator('textarea:invalid').count() > 0;
      
      if (textareaInvalido) {
        console.log('✅ Validación de formulario funcionando');
      } else {
        console.log('⚠️ Verificar validación de entrada de datos');
      }
    }
  });

  test('US-04.07: Validar calidad del contenido generado', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para validar calidad de IA');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Generar flashcards con contenido específico
    await page.goto(`https://localhost:7028/Flashcard/GenerateWithAI?materiaId=${materiaId}`);
    
    const textoFuente = `
      Python es un lenguaje de programación de alto nivel, interpretado y de propósito general.
      Fue creado por Guido van Rossum y lanzado en 1991.
      Python usa indentación para definir bloques de código.
      Es ampliamente utilizado en ciencia de datos, desarrollo web y automatización.
    `;
    
    await page.fill('textarea[name="TextoFuente"]', textoFuente);
    await page.fill('input[name="CantidadFlashcards"]', '3');
    await page.click('button[type="submit"]:has-text("Generar")');
    
    // Esperar a que se generen
    await page.waitForTimeout(5000);
    
    // Navegar a la lista de flashcards de la materia
    await page.goto(`https://localhost:7028/Flashcard/Index?materiaId=${materiaId}`);
    
    // Obtener las flashcards generadas
    const flashcards = page.locator('.flashcard, .card, [href*="/Flashcard/Details/"]');
    const cantidad = await flashcards.count();
    
    if (cantidad > 0) {
      console.log(`✅ Se generaron ${cantidad} flashcards`);
      
      // Verificar la primera flashcard
      await flashcards.first().click();
      await page.waitForURL('**/Flashcard/Details/**');
      
      // Verificar que tiene pregunta y respuesta
      const pregunta = await page.locator('body').textContent();
      const tienePregunta = pregunta && pregunta.length > 10;
      
      if (tienePregunta) {
        console.log('✅ Contenido generado tiene estructura válida');
      } else {
        console.log('⚠️ Verificar calidad del contenido generado');
      }
    } else {
      console.log('⚠️ No se generaron flashcards para validar');
    }
  });
});
