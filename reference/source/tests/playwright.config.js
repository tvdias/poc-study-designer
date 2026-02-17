import { defineConfig, devices } from "@playwright/test";

import dotenv from 'dotenv';
import path from 'path';
dotenv.config({ path: path.resolve(__dirname, '.env') });

export default defineConfig({
  testDir: "./tests",
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: [
    ['list'],
    ['dot'],
    ['./Reporter/MultiIdJUnitReporter.ts'], //Custom Report
    ['html', {outputFolder: "./test-results/html-report", open: "never" }],
   // ['json', { outputFile: "./test-results/json-report/json-test-report.json"}],
    ['junit', { outputFile: "./test-results/XMLReport/results.xml" }]
  ],
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    // baseURL: 'http://127.0.0.1:3000',

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: "on",
    video:"on",
    headless: true,
    actionTimeout: 30000
  },
  timeout: 8 * 60 * 1000, // Increase individual test timeout to 8 minute
  globalTimeout: 8*60*60*1000, // Increase global timeout to 8 hours
  expect: {
    timeout: 20000
  },
  projects: [
    {
      name: "Microsoft Edge",
      use: {
        ...devices["Desktop Edge"],
        channel: "msedge",
        viewport: { width: 1500, height: 730 },
        ignoreHTTPSErrors: true,
        acceptDownloads: true,
        screenshot: "only-on-failure",
        video: "on",
        trace: "on",
        launchOptions: {
          slowMo: 0,
          args: ["--disable-dev-shm-usage"],
        },
      },
    },
  ],
});
