// playwright.config.js
const testConfig = require('./test-config');

module.exports = {
  testDir: './epics',
  timeout: 30000,
  retries: 1, // 1 reintento en caso de fallos temporales
  reporter: [
    ['html', { outputFolder: './reports/html' }],
    ['json', { outputFile: './reports/test-results.json' }],
    ['junit', { outputFile: './reports/junit-results.xml' }]
  ],
  use: {
    baseURL: testConfig.baseURL,
    headless: false,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry',
    ignoreHTTPSErrors: true, // Ignorar errores de certificado SSL en localhost
  },
  projects: [
    {
      name: 'chromium',
      use: { ...require('@playwright/test').devices['Desktop Chrome'] },
    },
    // Comentado para desarrollo más rápido - solo usar Chromium
    // {
    //   name: 'firefox',
    //   use: { ...require('@playwright/test').devices['Desktop Firefox'] },
    // },
    // {
    //   name: 'webkit',
    //   use: { ...require('@playwright/test').devices['Desktop Safari'] },
    // },
  ],
  // webServer deshabilitado - usar aplicación manual
  // webServer: {
  //   command: 'dotnet run --project ../src/QuizCraft.Web',
  //   port: 5291,
  //   reuseExistingServer: !process.env.CI,
  // },
};