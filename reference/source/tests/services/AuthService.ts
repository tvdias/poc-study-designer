import { Page } from '@playwright/test';
import { getSecretFromKeyVault } from '../utils/KeyVault';

export interface Credentials {
  username: string;
  password: string;
}

export class AuthService {
  async getClientServiceCredentials(): Promise<Credentials> {
    try {
      const loginDetails = await getSecretFromKeyVault('CSUser');
      const loginCredentials = loginDetails.split(',');
      
      return {
        username: loginCredentials[0],
        password: loginCredentials[1]
      };
    } catch (error) {
      console.error(`Error getting CS credentials: ${(error as Error).message}`);
      throw error;
    }
  }

  async getLibrarianCredentials(): Promise<Credentials> {
    try {
      const loginDetails = await getSecretFromKeyVault('LibrarianUser');
      const loginCredentials = loginDetails.split(',');
      
      return {
        username: loginCredentials[0],
        password: loginCredentials[1]
      };
    } catch (error) {
      console.error(`Error getting Librarian credentials: ${(error as Error).message}`);
      throw error;
    }
  }

  async getScripterCredentials(): Promise<Credentials> {
    try {
      const loginDetails = await getSecretFromKeyVault('ScripterUser');
      const loginCredentials = loginDetails.split(',');
      
      return {
        username: loginCredentials[0],
        password: loginCredentials[1]
      };
    } catch (error) {
      console.error(`Error getting Scripter credentials: ${(error as Error).message}`);
      throw error;
    }
  }

  async handleAuthentication(page: Page, credentials?: Credentials): Promise<void> {
    try {
      // Wait a bit to see if we're redirected to login
      await page.waitForTimeout(2000);
      
      const currentUrl = page.url();
      
      if (currentUrl.includes('login.microsoftonline.com') || 
          await page.locator('input[type="email"], input[name="loginfmt"]').isVisible().catch(() => false)) {
        
        console.log('Authentication required, performing login...');
        
        // If no credentials provided, get CS user credentials
        if (!credentials) {
          credentials = await this.getClientServiceCredentials();
        }
        
        await this.performLogin(page, credentials);
      } else {
        console.log('Already authenticated or no authentication required');
      }
    } catch (error) {
      console.error(`Error in authentication handling: ${(error as Error).message}`);
      throw error;
    }
  }

  private async performLogin(page: Page, credentials: Credentials): Promise<void> {
    try {
      const usernameSelector = 'input[type="email"], input[name="loginfmt"], input[name="username"]';
      await page.waitForSelector(usernameSelector, { timeout: 10000 });
      await page.fill(usernameSelector, credentials.username);
      
      const nextButtonSelector = 'input[type="submit"], button[type="submit"], input[value="Next"]';
      await page.click(nextButtonSelector);
      
      const passwordSelector = 'input[type="password"], input[name="passwd"], input[name="password"]';
      await page.waitForSelector(passwordSelector, { timeout: 10000 });
      await page.fill(passwordSelector, credentials.password);
      
      const signInButtonSelector = 'input[type="submit"], button[type="submit"], input[value="Sign in"]';
      await page.click(signInButtonSelector);
      
      try {
        const staySignedInButton = page.locator('input[value="Yes"], button:has-text("Yes")');
        if (await staySignedInButton.isVisible({ timeout: 5000 })) {
          await staySignedInButton.click();
        }
      } catch {
      }
      
      await page.waitForLoadState('domcontentloaded');
      
      console.log('Login completed successfully');
      
    } catch (error) {
      console.error(`Error during login process: ${(error as Error).message}`);
      throw error;
    }
  }

  async waitUntilAppIdle(page: Page): Promise<void> {
    try {
      await page.waitForLoadState('domcontentloaded');
      await page.waitForLoadState('networkidle', { timeout: 30000 });
      
      await page.waitForTimeout(2000);
      
    } catch (error) {
      console.warn(`waitUntilAppIdle warning: ${(error as Error).message}`);
    }
  }

  async navigateToAppWithAuth(page: Page, url: string, credentials?: Credentials): Promise<void> {
    try {
      await page.goto(url, { waitUntil: 'domcontentloaded' });
      await this.handleAuthentication(page, credentials);
      await this.waitUntilAppIdle(page);
    } catch (error) {
      console.error(`Error navigating to app: ${(error as Error).message}`);
      throw error;
    }
  }
}