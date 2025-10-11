import { test, expect } from '@playwright/test';

test.describe('US-59: Gestión de Materias de Estudio', () => {

  test.beforeEach(async ({ page }) => {
    // Navegar a la aplicación
    await page.goto('http://localhost:5291');
    
    // Hacer clic en Iniciar Sesión
    await page.getByRole('button', { name: 'Iniciar Sesión' }).click();
    
    // Usar credenciales demo
    await page.getByRole('button', { name: 'Usar Credenciales Demo' }).click();
    
    // Iniciar sesión
    await page.getByRole('button', { name: 'Iniciar Sesión' }).click();
    
    // Navegar a materias
    await page.goto('http://localhost:5291/Materia');
  });

  test('Debería mostrar la lista de materias', async ({ page }) => {
    // Verificar que estamos en la página de materias
    await expect(page).toHaveTitle(/Materias/);
    
    // Verificar que existe el botón para crear nueva materia (usar el botón principal)
    await expect(page.getByRole('link', { name: '+ Nueva Materia' })).toBeVisible();
    
    // Verificar que estamos en la página correcta por la URL
    await expect(page).toHaveURL(/.*\/Materia/);
    
    console.log('✅ Página de materias cargada correctamente');
  });

});