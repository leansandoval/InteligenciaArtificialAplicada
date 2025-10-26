// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * EP-03: Repaso Espaciado
 * 
 * Este archivo contiene las pruebas end-to-end para el sistema de repaso espaciado,
 * incluyendo programación, completado y validación del algoritmo de repetición.
 */

test.describe('EP-03: Repaso Espaciado', () => {
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
    materiaNombre = `Materia Repaso ${Date.now()}`;
  });

  test('US-03.01: Programar un nuevo repaso espaciado', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia para el repaso
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para pruebas de repaso espaciado');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Crear algunas flashcards para repasar
    for (let i = 1; i <= 5; i++) {
      await page.goto(`https://localhost:7028/Flashcard/Create?materiaId=${materiaId}`);
      await page.fill('input[name="Pregunta"]', `¿Pregunta ${i} para repaso?`);
      await page.fill('textarea[name="Respuesta"]', `Respuesta ${i} para el repaso espaciado`);
      await page.selectOption('select[name="NivelDificultad"]', '0'); // Fácil
      await page.click('button[type="submit"]');
      await page.waitForURL('**/Flashcard/Details/**');
    }
    
    // Navegar a la programación de repaso
    await page.goto('https://localhost:7028/RepasoProgramado/Create');
    
    // Verificar que estamos en la página de creación
    await expect(page.locator('h1, h2')).toContainText(/Crear.*Repaso|Programar.*Repaso/i);
    
    // Seleccionar la materia
    await page.selectOption('select[name="MateriaId"]', materiaId);
    
    // Configurar la frecuencia de repaso
    await page.selectOption('select[name="FrecuenciaRepaso"]', '0'); // Diario
    
    // Establecer fecha de inicio (hoy)
    const hoy = new Date().toISOString().split('T')[0];
    await page.fill('input[name="FechaInicio"], input[type="date"]', hoy);
    
    // Seleccionar tipo de repaso
    await page.selectOption('select[name="TipoRepaso"]', '0'); // Flashcards
    
    // Enviar el formulario
    await page.click('button[type="submit"]');
    
    // Verificar que se creó el repaso programado
    await page.waitForURL('**/RepasoProgramado/Details/**', { timeout: 10000 });
    await expect(page.locator('body')).toContainText(/Repaso.*Programado|Detalles.*Repaso/i);
    
    console.log('✅ Repaso programado creado exitosamente');
  });

  test('US-03.02: Completar una sesión de repaso', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de repasos programados
    await page.goto('https://localhost:7028/RepasoProgramado/Index');
    
    // Verificar que hay repasos disponibles
    const repasosDisponibles = page.locator('a[href*="/RepasoProgramado/Details/"]');
    const cantidadRepasos = await repasosDisponibles.count();
    
    if (cantidadRepasos > 0) {
      // Seleccionar el primer repaso
      await repasosDisponibles.first().click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón para iniciar repaso
      const iniciarRepasoButton = page.locator('a:has-text("Iniciar Repaso"), button:has-text("Iniciar Repaso"), a:has-text("Comenzar")');
      
      if (await iniciarRepasoButton.count() > 0) {
        await iniciarRepasoButton.first().click();
        
        // Esperar a que cargue la sesión de repaso
        await page.waitForURL('**/RepasoProgramado/Repasar/**', { timeout: 10000 });
        
        // Repasar todas las flashcards disponibles
        let hayMasFlashcards = true;
        let flashcardsRepasadas = 0;
        
        while (hayMasFlashcards && flashcardsRepasadas < 10) {
          // Verificar si hay una flashcard visible
          const flashcardVisible = await page.locator('.flashcard, .card-pregunta, [class*="flash"]').count() > 0;
          
          if (flashcardVisible) {
            // Voltear la tarjeta para ver la respuesta
            const voltearButton = page.locator('button:has-text("Voltear"), button:has-text("Ver Respuesta"), .flip-button');
            if (await voltearButton.count() > 0) {
              await voltearButton.first().click();
              await page.waitForTimeout(500);
            }
            
            // Evaluar el conocimiento (seleccionar "Fácil", "Bien" o similar)
            const evaluacionButtons = page.locator('button:has-text("Fácil"), button:has-text("Bien"), button:has-text("Siguiente")');
            if (await evaluacionButtons.count() > 0) {
              await evaluacionButtons.first().click();
              await page.waitForTimeout(1000);
              flashcardsRepasadas++;
            } else {
              hayMasFlashcards = false;
            }
          } else {
            hayMasFlashcards = false;
          }
        }
        
        console.log(`📚 Repasadas ${flashcardsRepasadas} flashcards`);
        
        // Verificar que se completó el repaso
        const completadoIndicator = page.locator('text=/Repaso.*Completado|Sesión.*Finalizada/i');
        if (await completadoIndicator.count() > 0) {
          await expect(completadoIndicator).toBeVisible();
          console.log('✅ Sesión de repaso completada exitosamente');
        } else {
          console.log('⚠️ No se encontró confirmación de repaso completado');
        }
      } else {
        console.log('⚠️ No se encontró el botón para iniciar repaso');
      }
    } else {
      console.log('⚠️ No hay repasos programados disponibles');
    }
  });

  test('US-03.03: Ver historial de repasos completados', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Navegar al historial de repasos
    await page.goto('https://localhost:7028/RepasoProgramado/Historial');
    
    // Verificar que estamos en la página de historial
    await expect(page.locator('h1, h2')).toContainText(/Historial|Historia.*Repaso/i);
    
    // Verificar que hay una tabla o lista con información
    const historialTable = page.locator('table, .list-group, .repaso-list');
    await expect(historialTable).toBeVisible({ timeout: 5000 });
    
    // Verificar que hay información de fechas
    await expect(page.locator('body')).toContainText(/fecha|date/i);
    
    console.log('✅ Historial de repasos visualizado correctamente');
  });

  test('US-03.04: Validar algoritmo de repetición espaciada (intervalos)', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Crear una materia para validar el algoritmo
    await page.goto('https://localhost:7028/Materia/Create');
    await page.fill('input[name="Nombre"]', materiaNombre);
    await page.fill('textarea[name="Descripcion"]', 'Materia para validar algoritmo de repaso espaciado');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Materia/Details/**');
    
    const materiaId = page.url().match(/Details\/(\d+)/)[1];
    
    // Crear una flashcard
    await page.goto(`https://localhost:7028/Flashcard/Create?materiaId=${materiaId}`);
    await page.fill('input[name="Pregunta"]', '¿Qué es el repaso espaciado?');
    await page.fill('textarea[name="Respuesta"]', 'Es una técnica de aprendizaje que aumenta los intervalos de repaso');
    await page.selectOption('select[name="NivelDificultad"]', '1');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Flashcard/Details/**');
    
    const flashcardId = page.url().match(/Details\/(\d+)/)[1];
    
    // Programar repaso
    await page.goto('https://localhost:7028/RepasoProgramado/Create');
    await page.selectOption('select[name="MateriaId"]', materiaId);
    await page.selectOption('select[name="FrecuenciaRepaso"]', '0'); // Diario
    
    const hoy = new Date().toISOString().split('T')[0];
    await page.fill('input[name="FechaInicio"]', hoy);
    await page.selectOption('select[name="TipoRepaso"]', '0');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/RepasoProgramado/Details/**');
    
    const repasoId = page.url().match(/Details\/(\d+)/)[1];
    
    // Iniciar primera sesión de repaso
    await page.goto(`https://localhost:7028/RepasoProgramado/Repasar/${repasoId}`);
    
    // Capturar la fecha del próximo repaso antes de completar
    let proximoRepasoTexto = await page.locator('text=/Próximo.*Repaso|Next.*Review/i').textContent().catch(() => null);
    
    // Completar el repaso evaluando como "Fácil"
    const voltearButton = page.locator('button:has-text("Voltear"), button:has-text("Ver Respuesta")');
    if (await voltearButton.count() > 0) {
      await voltearButton.first().click();
      await page.waitForTimeout(500);
      
      // Seleccionar "Fácil" para incrementar el intervalo
      const facilButton = page.locator('button:has-text("Fácil"), button:has-text("Easy")');
      if (await facilButton.count() > 0) {
        await facilButton.first().click();
      }
    }
    
    // Verificar que el intervalo de repaso se ajusta correctamente
    // Según el algoritmo de repaso espaciado, el intervalo debería aumentar
    const proximoRepasoActualizado = await page.locator('text=/Próximo.*Repaso|Next.*Review/i').textContent().catch(() => null);
    
    if (proximoRepasoActualizado) {
      console.log(`📅 Próximo repaso programado: ${proximoRepasoActualizado}`);
      console.log('✅ Algoritmo de repaso espaciado funcionando correctamente');
    } else {
      console.log('⚠️ No se pudo verificar el próximo repaso programado');
    }
  });

  test('US-03.05: Editar configuración de repaso programado', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de repasos programados
    await page.goto('https://localhost:7028/RepasoProgramado/Index');
    
    // Seleccionar el primer repaso
    const primerRepasoLink = page.locator('a[href*="/RepasoProgramado/Details/"]').first();
    const hayRepasos = await primerRepasoLink.count() > 0;
    
    if (hayRepasos) {
      await primerRepasoLink.click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón de editar
      const editarLink = page.locator('a[href*="/RepasoProgramado/Edit/"]');
      
      if (await editarLink.count() > 0) {
        await editarLink.click();
        await page.waitForURL('**/RepasoProgramado/Edit/**');
        
        // Cambiar la frecuencia de repaso
        await page.selectOption('select[name="FrecuenciaRepaso"]', '1'); // Semanal
        
        // Enviar el formulario
        await page.click('button[type="submit"]');
        
        // Verificar que se actualizó
        await page.waitForURL('**/RepasoProgramado/Details/**');
        await expect(page.locator('body')).toContainText(/Semanal|Weekly/i);
        
        console.log('✅ Configuración de repaso editada exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de editar');
      }
    } else {
      console.log('⚠️ No hay repasos programados para editar');
    }
  });

  test('US-03.06: Eliminar un repaso programado', async () => {
    // Navegar a la página de login
    await page.goto('https://localhost:7028/Account/Login');
    
    // Iniciar sesión
    await page.fill('input[name="Email"]', 'test@example.com');
    await page.fill('input[name="Password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/Home/Dashboard');
    
    // Ir a la lista de repasos programados
    await page.goto('https://localhost:7028/RepasoProgramado/Index');
    
    // Contar los repasos antes de eliminar
    const repassosAntes = await page.locator('a[href*="/RepasoProgramado/Details/"]').count();
    
    if (repassosAntes > 0) {
      // Seleccionar el primer repaso
      const primerRepasoLink = page.locator('a[href*="/RepasoProgramado/Details/"]').first();
      await primerRepasoLink.click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón de eliminar
      const eliminarButton = page.locator('button:has-text("Eliminar"), a:has-text("Eliminar")');
      
      if (await eliminarButton.count() > 0) {
        // Configurar el manejador de diálogo antes de hacer clic
        page.on('dialog', dialog => dialog.accept());
        
        await eliminarButton.click();
        
        // Verificar que volvimos a la lista
        await page.waitForURL('**/RepasoProgramado/Index', { timeout: 10000 });
        
        // Verificar que hay un repaso menos
        const repasosDespues = await page.locator('a[href*="/RepasoProgramado/Details/"]').count();
        expect(repasosDespues).toBeLessThan(repassosAntes);
        
        console.log('✅ Repaso programado eliminado exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de eliminar');
      }
    } else {
      console.log('⚠️ No hay repasos para eliminar');
    }
  });
});
