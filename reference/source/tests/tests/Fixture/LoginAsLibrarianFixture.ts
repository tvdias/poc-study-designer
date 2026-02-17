import { test as baseTest, expect } from "@playwright/test";
import { LoginPage } from "../../pages/LoginPage";
import { WebHelper } from '../../utils/WebHelper';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Common } from '../../selectors/CommonSelector.json';

//This Fixture is login the application with Librarian user
export const test = baseTest.extend<{
  loginPage: LoginPage;
  webhelper: WebHelper;
  guid: string;
}>(
  {
    webhelper: async ({ page }, use) => {
      const webHelper = new WebHelper(page);
      await use(webHelper);
    },

    loginPage: async ({ page, context, webhelper }, use) => {
      const loginPage = new LoginPage(page, context);
      await loginPage.navigateToURL();
      await loginPage.loginToApplication(TestUser.LibrarianUser);
      await webhelper.changeArea(Common.Text.Librarian);
      await use(loginPage);
    },

  }
);

export default test;
export { expect };
