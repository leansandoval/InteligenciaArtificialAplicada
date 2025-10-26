// test-helpers.js
// Funciones auxiliares compartidas para todos los tests

const testConfig = require('./test-config');

/**
 * Realiza el login con el usuario de prueba
 * @param {import('@playwright/test').Page} page 
 */
async function loginWithTestUser(page) {
  await page.goto('/Account/Login');
  await page.fill(testConfig.selectors.login.emailInput, testConfig.testUser.email);
  await page.fill(testConfig.selectors.login.passwordInput, testConfig.testUser.password);
  await page.click(testConfig.selectors.login.submitButton);
  
  // Esperar a que se complete el login (aceptar cualquier redirección que no sea /Account/)
  await page.waitForURL(url => !url.pathname.includes('/Account/'), { timeout: testConfig.timeouts.navigation });
  
  // Esperar a que la página termine de cargar completamente
  await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
}

/**
 * Crea una materia de prueba
 * @param {import('@playwright/test').Page} page 
 * @param {string} nombre 
 * @param {string} descripcion 
 * @returns {Promise<string>} ID de la materia creada
 */
async function crearMateria(page, nombre, descripcion) {
  await page.goto('/Materia/Create');
  await page.fill('input[name="Nombre"]', nombre);
  await page.fill('textarea[name="Descripcion"]', descripcion);
  
  // Buscar el botón de submit visible (no el del dropdown)
  const submitButton = page.locator('button[type="submit"]:visible').last();
  await submitButton.click();
  
  // El formulario redirige a /Materia (Index) después de crear
  await page.waitForURL('**/Materia', { timeout: testConfig.timeouts.default });
  await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
  
  // Buscar la materia recién creada en la lista por su título
  const materiaCard = page.locator(`h5:has-text("${nombre}")`).first();
  await materiaCard.waitFor({ state: 'visible', timeout: testConfig.timeouts.default });
  
  // Obtener el link de Details que está en el card de la materia
  const detailsLink = materiaCard.locator('..').locator('..').locator('..').locator(`a[href*="/Materia/Details/"]`).first();
  const href = await detailsLink.getAttribute('href');
  const urlMatch = href.match(/Details\/(\d+)/);
  
  return urlMatch ? urlMatch[1] : null;
}

/**
 * Crea una flashcard de prueba
 * @param {import('@playwright/test').Page} page 
 * @param {string} materiaId 
 * @param {string} pregunta 
 * @param {string} respuesta 
 * @returns {Promise<string>} ID de la flashcard creada
 */
async function crearFlashcard(page, materiaId, pregunta, respuesta) {
  await page.goto(`/Flashcard/Create?materiaId=${materiaId}`);
  await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
  
  // Seleccionar la materia en el combobox explícitamente por su valor (ID)
  // El parámetro URL a veces no pre-selecciona correctamente
  const materiaSelect = page.locator('select').first();
  await materiaSelect.selectOption(materiaId.toString());
  
  // La interfaz usa textbox en lugar de input/textarea
  await page.getByPlaceholder('Escribe tu pregunta aquí...').fill(pregunta);
  await page.getByPlaceholder('Escribe la respuesta correcta aquí...').fill(respuesta);
  
  // Seleccionar dificultad Intermedio (radio button ya está seleccionado por defecto)
  // Si necesitamos cambiarlo: await page.getByText(' Intermedio').click();
  
  // Buscar el botón de submit visible (no el del dropdown)
  const submitButton = page.locator('button[type="submit"]:visible').last();
  await submitButton.click();
  
  // Verificar si redirige a Details o a Index
  await page.waitForURL(/\/Flashcard/, { timeout: testConfig.timeouts.default });
  
  // Si estamos en Details, obtener el ID de la URL
  if (page.url().includes('/Details/')) {
    const urlMatch = page.url().match(/Details\/(\d+)/);
    return urlMatch ? urlMatch[1] : null;
  }
  
  // Si redirige a Index, buscar la flashcard recién creada
  await page.waitForLoadState('networkidle', { timeout: testConfig.timeouts.default });
  const flashcardCard = page.locator('.card').filter({ hasText: pregunta }).first();
  await flashcardCard.waitFor({ state: 'visible', timeout: testConfig.timeouts.default });
  
  const detailsLink = flashcardCard.locator(`a[href*="/Flashcard/Details/"]`).first();
  const href = await detailsLink.getAttribute('href');
  const urlMatch = href.match(/Details\/(\d+)/);
  
  return urlMatch ? urlMatch[1] : null;
}

/**
 * Genera un nombre único con timestamp
 * @param {string} prefix 
 * @returns {string}
 */
function generarNombreUnico(prefix) {
  return `${prefix} ${Date.now()}`;
}

/**
 * Espera y verifica que un elemento sea visible
 * @param {import('@playwright/test').Page} page 
 * @param {string} selector 
 * @param {number} timeout 
 */
async function esperarElementoVisible(page, selector, timeout = testConfig.timeouts.default) {
  await page.waitForSelector(selector, { state: 'visible', timeout });
}

module.exports = {
  loginWithTestUser,
  crearMateria,
  crearFlashcard,
  generarNombreUnico,
  esperarElementoVisible,
  testConfig
};
