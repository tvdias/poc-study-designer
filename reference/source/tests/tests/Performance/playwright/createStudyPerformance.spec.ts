import { test, expect, Page } from '@playwright/test';
import PerformanceUtils, { PerformanceMetrics, PerformanceThresholds } from '../../../utils/performance/performance-utils';
import { AuthService } from '../../../services/AuthService';
import performanceConfig from '../../../Test Data/performance-config.json';
import { StudyTestData } from '../../../Test Data/StudyData.json';
import { StudyService } from '../../../services/StudyService';
import { Utils } from '../../../utils/utils';
import { WebHelper } from '../../../utils/WebHelper';
import { EntityLogicalNames } from '../../../constants/CommonLogicalNames.json';
import { deleteRecord } from '../../../utils/APIHelper';

const SAVE_THRESHOLDS = {
    good: performanceConfig.performanceThresholds.good.pageLoadTime
};

test.describe('', () => {
    let authService: AuthService;
    let performanceUtils: PerformanceUtils;
    let guidStudy: string | undefined;

    // Default Project ID to use when pipeline env var isn't provided
    const DEFAULT_PROJECT_ID = 'e8f8d5f0-37ed-f011-8544-000d3abb82f9';


    test.beforeEach(async ({ page }) => {
        authService = new AuthService();
        performanceUtils = new PerformanceUtils(page);
        guidStudy = undefined;
    });

    test.afterEach(async () => {
        // Cleanup must run whether test passes or fails (if a study GUID is available)
        if (guidStudy && guidStudy.trim() !== '') {
            try {
                await deleteRecord(EntityLogicalNames.Study, guidStudy);
                console.log(`Study ${guidStudy} deleted successfully`);
            } catch (cleanupError) {
                console.error(`❌ Cleanup failed: could not delete Study ${guidStudy}`);
                console.error(cleanupError);
            } finally {
                guidStudy = undefined;
            }
        }
    });

    test("[2667536] To validate system stability during study creation when the system is under stress", { tag: ['@Regression', '@Study'] 
    }, async ({ page }) => {
      
        test.info().annotations.push({ type: 'TestCaseId', description: '2667536' });
        const studyService = new StudyService(page);
        const webHelper = new WebHelper(page);
        const baseUrl = performanceConfig.testEnviornments.baseUrl;

        // Get project ID from pipeline variables (environment)
        const projectIdEnvVar = performanceConfig.testConfiguration.projectIdEnvVar;
        const projectIdFromEnv = process.env[projectIdEnvVar];
        const projectId = (projectIdFromEnv && projectIdFromEnv.trim() !== '')
            ? projectIdFromEnv.trim()
            : DEFAULT_PROJECT_ID;

        const testUrl = `${baseUrl}&pagetype=entityrecord&etn=kt_project&id=${projectId}`;
        let saveRecordDuration = 0;
        let formFillDuration = 0;

        await test.step('Authenticate with Client Service Account', async () => {
            const credentials = await authService.getClientServiceCredentials();
            expect(credentials.username).toBeTruthy();
            expect(credentials.password).toBeTruthy();
            console.log(`Authenticating with CS user: ${credentials.username}`);
        });

        await test.step('Navigate to Project Entity Page', async () => {
            console.log(`Navigating to project: ${testUrl}`);

            await page.goto(testUrl, {
                waitUntil: 'commit',
                timeout: performanceConfig.testConfiguration.timeout
            });

            await authService.handleAuthentication(page);
            await performanceUtils.waitForFullPageLoad();
            console.log('Project page loaded successfully');
        });

        await test.step('Create Study and Measure Save Performance', async () => {
            const studyName = StudyTestData.StudyName + Utils.generateGUID();

            // Measure form fill time
            const formFillStart = Date.now();

            // Navigate to Studies tab and open new study form
            await studyService.navigateToStudiesTabAndOpenNewForm();

            // Fill in study details (without saving)
            await studyService.fillStudyForm(
                studyName,
                StudyTestData.Category,
                StudyTestData.FieldWorkMarket,
                StudyTestData.MaconomyJobNumber,
                StudyTestData.ProjectOperationsURL,
                StudyTestData.ScripterNotes
            );
            await webHelper.closeAIAlerts();
            await webHelper.closeAIForm();

            formFillDuration = Date.now() - formFillStart;
            console.log(`Form fill time: ${formFillDuration}ms`);

            // Measure save operation time - start recording immediately when save is clicked
            const saveStartTime = Date.now();
            console.log(`Save started at: ${new Date(saveStartTime).toISOString()}`);
            await webHelper.saveRecord();

            // Wait for save to complete - check for spinner to disappear and saved status
            let saveStepError: unknown;
            try {

                // Step 1: Wait for the spinner to disappear (save processing complete)
                console.log('[Save Progress] Waiting for spinner to disappear...');
                const spinnerTail = page.locator('span.fui-Spinner__spinnerTail');
                try {
                    await spinnerTail.waitFor({ state: 'hidden', timeout: 205000 });
                } catch {
                    await spinnerTail.waitFor({ state: 'detached', timeout: 205000 });
                }
                console.log('[Save Progress] Spinner disappeared');

                // Step 2: Check if error dialog is present
                const errorDialog = page.locator('[data-id="errorDialogdialog"]');
                const hasErrorDialog = await errorDialog.isVisible().catch(() => false);

                if (hasErrorDialog) {
                    // Calculate save duration at error point
                    saveRecordDuration = Date.now() - saveStartTime;
                    const saveRecordDurationSeconds = (saveRecordDuration / 1000).toFixed(2);

                    // Get error message
                    console.error(`❌ SAVE FAILED: Error dialog detected after ${saveRecordDurationSeconds} seconds`);
                    throw new Error(`Save operation failed : (Duration: ${saveRecordDurationSeconds} seconds)`);
                }

                console.log('[Save Progress] No error dialog detected, proceeding...');

                // Step 3: Check if "Saved" status appears
                const savedStatus = page.locator('span[aria-label*="Save status - Saved"], span[aria-label*="Saved"]');
                await savedStatus.first().waitFor({ state: 'visible', timeout: 15000 });
                const isSaved = await savedStatus.isVisible().catch(() => false);
                console.log(`[Save Progress] Saved status visible: ${isSaved}`);
                expect(isSaved, 'Expected Save status to be "Saved" in the header').toBeTruthy();

                // Step 4: Wait for URL to contain study entity and ID (save success indicator)
                console.log('[Save Progress] Waiting for URL to update with study ID...');
                await page.waitForFunction(() => {
                    const url = window.location.href;
                    const hasStudyEntity = url.includes('etn=kt_study');
                    const hasId = url.includes('&id=');
                    const notNewWindow = !url.includes('&newWindow=true');
                    return hasStudyEntity && hasId && notNewWindow;
                }, { timeout: 10000 });

                // Calculate save duration
                saveRecordDuration = Date.now() - saveStartTime;
                if (saveRecordDuration > SAVE_THRESHOLDS.good) {
                    console.warn(
                        `⚠️ PERFORMANCE THRESHOLD EXCEEDED: Save took ${saveRecordDuration}ms (threshold ${SAVE_THRESHOLDS.good}ms)`
                    );
                }

                // Get current URL and fetch GUID
                const currentUrl = page.url();
                console.log(`[GUID Fetch] Attempting to fetch GUID from URL: ${currentUrl}`);

                    guidStudy = await webHelper.fetchRecordGuid(currentUrl);
                    console.log(`[GUID Fetch] Extracted GUID: '${guidStudy}'`);


                // Save was successful - report performance
                if (saveRecordDuration <= SAVE_THRESHOLDS.good) {
                    console.log(`✅ GOOD PERFORMANCE: Save completed successfully in ${saveRecordDuration}ms (within ${SAVE_THRESHOLDS.good}ms threshold)`);
                } else {
                    console.warn(`⚠️ SLOWER THAN EXPECTED: Save completed successfully in ${saveRecordDuration}ms (exceeded ${SAVE_THRESHOLDS.good}ms threshold)`);
                }
                console.log(`Study GUID: ${guidStudy}`);
            } catch (error) {
                saveStepError = error;
                // Calculate duration at point of failure
                saveRecordDuration = Date.now() - saveStartTime;
                const saveRecordDurationSeconds = (saveRecordDuration / 1000).toFixed(2);

                // Get current URL to check if ID exists
                const currentUrl = page.url();
                console.log(`[Error] Current URL at failure: ${currentUrl}`);

                // Check if URL contains ID - if not, save operation failed
                const hasIdInUrl = currentUrl.includes('&id=');

                if (!hasIdInUrl) {
                    console.error(`❌ PERFORMANCE TEST FAILED: Save operation did not complete - no ID found in URL after ${saveRecordDurationSeconds} seconds`);
                    console.error(`Final URL: ${currentUrl}`);
                    throw new Error(`Performance Test Failed: Save operation did not return a study ID within the timeout period (Duration: ${saveRecordDurationSeconds} seconds)`);
                }

                // Any other error should fail the test as well.
                throw error;
            }
        });

    });

});