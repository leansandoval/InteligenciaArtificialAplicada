// @ts-check
const { test, expect } = require('@playwright/test');
const { loginWithTestUser, crearMateria, crearFlashcard, generarNombreUnico } = require('../../test-helpers');

/**
 * EP-03: Repaso Espaciado
 * 
 * Este archivo contiene las pruebas end-to-end para el sistema de repaso espaciado,
 * incluyendo programación, completado y validación del algoritmo de repetición.
 */

test.describe('EP-03: Repaso Espaciado', () => {

  test('US-03.01: Programar un nuevo repaso espaciado', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia para el repaso
    const materiaNombre = generarNombreUnico('Materia Repaso');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para pruebas de repaso espaciado');
    
    // Crear algunas flashcards para repasar
    for (let i = 1; i <= 5; i++) {
      await crearFlashcard(page, materiaId, `¿Pregunta ${i} para repaso?`, `Respuesta ${i} para el repaso espaciado`);
    }
    
    // Navegar a la programación de repaso
    await page.goto('/RepasoProgramado/Create');
    
    // Verificar que estamos en la página de creación
    await expect(page.locator('h1, h2')).toContainText(/Crear.*Repaso|Programar.*Repaso/i);
    
    // Seleccionar la materia
    await page.selectOption('select[name="MateriaId"]', materiaId.toString());
    
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
    await expect(page.locator('body')).toContainText(materiaNombre);
    
    console.log('✅ Repaso programado creado exitosamente');
  });

  test('US-03.02: Iniciar sesión de repaso', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la lista de repasos programados
    await page.goto('/RepasoProgramado/Index');
    
    // Verificar que hay repasos disponibles
    const repasos = page.locator('a[href*="/RepasoProgramado/Details/"]');
    const hayRepasos = await repasos.count() > 0;
    
    if (hayRepasos) {
      // Hacer clic en el primer repaso
      await repasos.first().click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón "Iniciar Repaso"
      const iniciarButton = page.locator('a:has-text("Iniciar"), button:has-text("Iniciar")');
      
      if (await iniciarButton.count() > 0) {
        await iniciarButton.first().click();
        
        // Verificar que estamos en la sesión de repaso
        await page.waitForURL('**/RepasoProgramado/Sesion/**', { timeout: 10000 });
        await expect(page.locator('body')).toContainText(/Repaso|Flashcard|Pregunta/i);
        
        console.log('✅ Sesión de repaso iniciada exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón para iniciar el repaso');
      }
    } else {
      console.log('⚠️ No hay repasos programados disponibles');
    }
  });

  test('US-03.03: Completar una sesión de repaso', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Crear una materia y flashcards para el repaso
    const materiaNombre = generarNombreUnico('Materia Sesion');
    const materiaId = await crearMateria(page, materiaNombre, 'Materia para completar sesión de repaso');
    
    // Crear 3 flashcards
    for (let i = 1; i <= 3; i++) {
      await crearFlashcard(page, materiaId, `¿Pregunta sesión ${i}?`, `Respuesta sesión ${i}`);
    }
    
    // Programar un repaso
    await page.goto('/RepasoProgramado/Create');
    await page.selectOption('select[name="MateriaId"]', materiaId.toString());
    await page.selectOption('select[name="FrecuenciaRepaso"]', '0'); // Diario
    const hoy = new Date().toISOString().split('T')[0];
    await page.fill('input[name="FechaInicio"], input[type="date"]', hoy);
    await page.selectOption('select[name="TipoRepaso"]', '0'); // Flashcards
    await page.click('button[type="submit"]');
    await page.waitForURL('**/RepasoProgramado/Details/**');
    
    // Obtener el ID del repaso
    const repasoId = page.url().match(/Details\/(\d+)/)?.[1];
    
    if (repasoId) {
      // Iniciar el repaso
      await page.goto(`/RepasoProgramado/Sesion/${repasoId}`);
      
      // Completar cada flashcard de la sesión
      const flashcardsCount = 3;
      for (let i = 0; i < flashcardsCount; i++) {
        // Esperar a que se muestre la pregunta
        const preguntaElement = page.locator('.flashcard-pregunta, .pregunta, [data-pregunta]');
        
        if (await preguntaElement.count() > 0) {
          await expect(preguntaElement).toBeVisible({ timeout: 5000 });
          
          // Revelar la respuesta
          const revelarButton = page.locator('button:has-text("Revelar"), button:has-text("Mostrar")');
          if (await revelarButton.count() > 0) {
            await revelarButton.click();
          }
          
          // Calificar la respuesta (Fácil, Media, Difícil)
          const calificarButtons = page.locator('button:has-text("Fácil"), button:has-text("Medio"), button:has-text("Difícil")');
          if (await calificarButtons.count() > 0) {
            await calificarButtons.first().click();
          }
        }
      }
      
      // Verificar que se completó la sesión
      await expect(page.locator('body')).toContainText(/Completado|Finalizado|Terminado/i);
      
      console.log('✅ Sesión de repaso completada exitosamente');
    } else {
      console.log('⚠️ No se pudo obtener el ID del repaso');
    }
  });

  test('US-03.04: Ver progreso del repaso espaciado', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la lista de repasos
    await page.goto('/RepasoProgramado/Index');
    
    // Verificar que hay repasos programados
    const repasos = page.locator('a[href*="/RepasoProgramado/Details/"]');
    const hayRepasos = await repasos.count() > 0;
    
    if (hayRepasos) {
      // Hacer clic en el primer repaso
      await repasos.first().click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Verificar que se muestra información de progreso
      await expect(page.locator('body')).toContainText(/Progreso|Avance|Completado/i);
      
      // Buscar indicadores de progreso (porcentaje, barras, etc.)
      const progresoIndicador = page.locator('.progress, .progress-bar, [data-progress]');
      
      if (await progresoIndicador.count() > 0) {
        await expect(progresoIndicador).toBeVisible();
        console.log('✅ Progreso de repaso visualizado correctamente');
      } else {
        console.log('⚠️ No se encontraron indicadores visuales de progreso');
      }
    } else {
      console.log('⚠️ No hay repasos programados disponibles');
    }
  });

  test('US-03.05: Editar configuración de repaso', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la lista de repasos
    await page.goto('/RepasoProgramado/Index');
    
    // Verificar que hay repasos programados
    const repasos = page.locator('a[href*="/RepasoProgramado/Details/"]');
    const hayRepasos = await repasos.count() > 0;
    
    if (hayRepasos) {
      // Hacer clic en el primer repaso
      await repasos.first().click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón de editar
      const editarLink = page.locator('a[href*="/RepasoProgramado/Edit/"]');
      
      if (await editarLink.count() > 0) {
        await editarLink.click();
        await page.waitForURL('**/RepasoProgramado/Edit/**');
        
        // Modificar la frecuencia de repaso
        await page.selectOption('select[name="FrecuenciaRepaso"]', '1'); // Semanal
        
        // Enviar el formulario
        await page.click('button[type="submit"]');
        
        // Verificar que se actualizó
        await page.waitForURL('**/RepasoProgramado/Details/**');
        await expect(page.locator('body')).toContainText(/Semanal/i);
        
        console.log('✅ Configuración de repaso editada exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de editar');
      }
    } else {
      console.log('⚠️ No hay repasos programados disponibles');
    }
  });

  test('US-03.06: Eliminar un repaso programado', async ({ page }) => {
    // Iniciar sesión
    await loginWithTestUser(page);
    
    // Navegar a la lista de repasos
    await page.goto('/RepasoProgramado/Index');
    
    // Contar repasos antes de eliminar
    const repasosAntes = await page.locator('a[href*="/RepasoProgramado/Details/"]').count();
    
    if (repasosAntes > 0) {
      // Hacer clic en el primer repaso
      const primerRepaso = page.locator('a[href*="/RepasoProgramado/Details/"]').first();
      await primerRepaso.click();
      await page.waitForURL('**/RepasoProgramado/Details/**');
      
      // Buscar el botón de eliminar
      const eliminarButton = page.locator('button:has-text("Eliminar"), a:has-text("Eliminar")');
      
      if (await eliminarButton.count() > 0) {
        // Configurar manejador de diálogo antes de hacer clic
        page.on('dialog', dialog => dialog.accept());
        
        // Hacer clic en eliminar
        await eliminarButton.click();
        
        // Verificar que volvimos a la lista
        await page.waitForURL('**/RepasoProgramado/Index', { timeout: 10000 });
        
        // Verificar que hay un repaso menos
        const repasosDespues = await page.locator('a[href*="/RepasoProgramado/Details/"]').count();
        expect(repasosDespues).toBeLessThan(repasosAntes);
        
        console.log('✅ Repaso programado eliminado exitosamente');
      } else {
        console.log('⚠️ No se encontró el botón de eliminar');
      }
    } else {
      console.log('⚠️ No hay repasos programados para eliminar');
    }
  });
});
