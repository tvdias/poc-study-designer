# Study Designer Lite Test Automation Framework

This repository contains the UI automation framework for testing the Study Designer Lite application using Playwright with Typescript

## Getting Started

### Prerequisites

- Playwright - Playwright will be installed as a dependency, but ensure your machine supports Playwright browsers.
- Node.js (version 22 or higher)
- TypeScript (version 4 or higher)
- Playwright (version 1.50 or higher)

### Folder Structure

- Pages - Contains page classes with methods representing high-level actions (e.g., login, navigation).
- PageObjects - Contains selectors for each page, which are imported into the corresponding page classes in the Pages folder.
- Tests - Contains test files for each feature or functionality, organized by scenarios (e.g., login, onboarding).
- Variables - Holds test data files, such as JSON files, that provide input values for the tests.

### Configuration

The `playwright.config.ts` file is used to configure Playwright settings. Here is an example configuration:

    import { defineConfig } from '@playwright/test';

    export default defineConfig({
    timeout: 30000,
    retries: 2,
    use: {
        headless: true,
        viewport: { width: 1280, height: 720 },
        ignoreHTTPSErrors: true,
        video: 'retain-on-failure',
    },
    projects: [
        {
        name: 'chromium',
        use: { browserName: 'chromium' },
        },
        {
        name: 'firefox',
        use: { browserName: 'firefox' },
        },
    ],
    reporter: [['list'], ['json', { outputFile: 'test-results/results.json' }]],
    });

## Installation

1. Clone the repository

   - Go to the repository in Azure DevOps.
   - Click on the Clone button to copy the HTTPS or SSH link.

   ```bash
   git clone https://kantarware.visualstudio.com/KT-Digital-Transformation/_git/DIGTX-STUDY-DESIGNER-LITE
   ```

2. Navigate to Project Directory

   ```bash
   cd tests
   ```

3. Install dependencies:

   ```bash
   npm install
   ```

4. Install Playwright browsers:

   ```bash
   npx playwright install
   ```

## Running Tests

Run All Tests - To run all tests in the repository:

```bash
npx playwright test
```

Run a Specific Test File - To run a specific test file:

```bash
npx playwright test tests/Login.spec.ts
```

Run the same test multiple times:

```bash
npx playwright test --repeat-each=<number_of_times>
```

Debugging Mode - To run tests in debug mode:

```bash
npx playwright test --debug
```

View Test Results - Playwright generates a report after each test run. To view it, use:

```bash
npx playwright show-report
```

npm install typescript ts-node @types/node --save-dev
npm install @alex_neo/playwright-azure-reporter

## Creating Tests

1. Add Selectors - Add selectors for the page elements in `pageObjects/<PageName>.ts`.
2. Create Page Classes - Define high-level methods in `pages/<PageName>.ts` using the selectors and the utility functions.
3. Write Test Cases - Add test files in the `tests` folder, importing relevant page classes and using their methods.

Sample Test Case - `example.spec.ts`

    import testUtils from "../utils/Test-utils";

    test.describe("Example Tests @Regression @Sanity @Smoke", () => {
    test.beforeEach(async ({ samplePage }) => {
    testUtils.setPage(samplePage.page);

    await samplePage.navigateToURL();
    });

    test("[ID] Example Test", async ({ samplePage, page }, testInfo) => {
        await test.step("Example Test Steps", async () => {
            await samplePage.function();
            const screenshot = await page.screenshot();
            testInfo.attach("Example Test", { body: screenshot, contentType: contentType });
            });
        });
    });

## Troubleshooting

- Check the console output for error messages
- Verify the test environment configuration
- Consult the Playwright documentation

## Contributing

- Fork the repository
- Create a new branch
- Commit changes
- Open a pull request
