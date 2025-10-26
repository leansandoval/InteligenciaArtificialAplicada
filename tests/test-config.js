// test-config.js
// Configuración centralizada para todos los tests E2E

module.exports = {
  // URL base de la aplicación
  baseURL: 'http://localhost:5291',
  
  // Credenciales de usuario de prueba
  testUser: {
    email: 'admin@quizcraft.com',
    password: 'Admin123!',
    nombreCompleto: 'Admin QuizCraft'
  },
  
  // Timeouts
  timeouts: {
    navigation: 10000,
    default: 5000,
    long: 30000, // Para operaciones con IA
  },
  
  // Selectores comunes
  selectors: {
    login: {
      emailInput: 'input[name="Email"]',
      passwordInput: 'input[name="Password"]',
      submitButton: 'button[type="submit"]'
    },
    navigation: {
      dashboardLink: 'a[href*="/Home/Dashboard"]',
      materiasLink: 'a[href*="/Materia"]',
      flashcardsLink: 'a[href*="/Flashcard"]',
      quizzesLink: 'a[href*="/Quiz"]',
      repasoLink: 'a[href*="/RepasoProgramado"]',
      statisticsLink: 'a[href*="/Statistics"]'
    }
  },
  
  // Mensajes esperados
  messages: {
    loginSuccess: /Dashboard|Bienvenido/i,
    loginError: /error|inválido|incorrecto/i
  }
};
