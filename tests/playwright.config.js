// playwright.config.js
module.exports = {
  testDir: './epics',
  timeout: 30000,
  retries: 0, // Sin reintentos para desarrollo más rápido
  reporter: [
    ['html', { outputFolder: './reports/html' }],
    ['json', { outputFile: './reports/test-results.json' }],
    ['junit', { outputFile: './reports/junit-results.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:5291',
    headless: false,
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry',
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