import { Common } from '../selectors/CommonSelector.json';
import { URLS, Environment } from '../constants/TestUsers.json';
import { Page } from '@playwright/test';
import { getSecretFromKeyVault } from '../utils/KeyVault';
import { WebHelper } from './WebHelper';

export async function LoginToMDAWithTestUser(page: Page, testUser: string, appId: string, appName: string, Isadduser: boolean = false): Promise<void> {
    try {
        const webHelper = new WebHelper(page);
        let userName, password, loginDetails, loginCredentials;
        loginDetails = await getSecretFromKeyVault(testUser);
        loginCredentials = loginDetails.split(",");
        userName = loginCredentials[0];
        password = loginCredentials[1];

        const URL = await getEnvURL();
        if (URL !== undefined) {
            await page.goto(URL);
        }
        await page.getByRole('textbox', { name: Common.ByRole.UserNameTextbox }).click();
        await page.getByRole('textbox', { name: Common.ByRole.UserNameTextbox }).clear();
        await page.getByRole('textbox', { name: Common.ByRole.UserNameTextbox }).fill(userName);
        await page.getByRole('button', { name: Common.ByRole.NextButton }).click();
        await page.getByRole('textbox', { name: Common.ByRole.PasswordTextbox }).click();
        await page.getByRole('textbox', { name: Common.ByRole.PasswordTextbox }).clear();
        await page.getByRole('textbox', { name: Common.ByRole.PasswordTextbox }).fill(password);
        await page.getByRole('button', { name: Common.ByRole.SigninButton }).click();
        await waitUntilAppIdle(page);
        if (!Isadduser) {
            //await navigateToApps(page, appId, appName);
            await waitUntilAppIdle(page);
        }


    } catch (error) {
        console.log(`Error in login into MDA: ${(error as Error).message}`);
        throw error;
    }
}

export async function navigateToApps(page: Page, appId: string | number, appName: string): Promise<void> {
    try {
        console.log('Navigate to ' + appName + ' - Start');
        const url = await getEnvURL();
        await page.goto(`${url}/main.aspx?appid=${appId.toString()}`);
        await page.getByRole('button', { name: appName }).isVisible();
        console.log('Navigated to ' + appName + ' - Success');
    } catch (error) {
        console.log(`Error in navigating to application: ${(error as Error).message}`);
        throw error;
    }

}

export async function waitUntilAppIdle(page: Page): Promise<void> {
    try {
        //await page.waitForFunction(() => (window as any).UCWorkBlockTracker?.isAppIdle());
        await page.waitForLoadState("domcontentloaded");
        await page.waitForTimeout(2000);
    } catch (e) {
        console.log(`waitUntilIdle failed, ignoring.., error: ${(e as Error).message}`);
    }
}

export async function getEnvURL() {
    let url;
    if (Environment.Env.toUpperCase() == 'DEV') {
        url = URLS.UC1_Dev_URL;
    } else if (Environment.Env.toUpperCase() == 'TEST') {
        url = URLS.UC1_Test_URL;
    }
    return url;
}
