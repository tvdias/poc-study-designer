import { test as baseTest, expect } from "@playwright/test";
import { QuestionBankPage } from "../../pages/QuestionBankPage";
import { LoginPage } from "../../pages/LoginPage";
import { WebHelper } from '../../utils/WebHelper';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { Common } from '../../selectors/CommonSelector.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';

//This is Fixture is login the application and create the project with std Question
export const test = baseTest.extend<{
  loginPage: LoginPage;
  webhelper: WebHelper;
  questionBankPage: QuestionBankPage;
  guid: string;
  projectName: string;
  managedListName: string;
  questionnaireSetup: void;
}>(
  {
    webhelper: async ({ page }, use) => {
      const webHelper = new WebHelper(page);
      await use(webHelper);
    },

    loginPage: async ({ page, context, webhelper }, use) => {
      const loginPage = new LoginPage(page, context);
      await loginPage.navigateToURL();
      await loginPage.loginToApplication(TestUser.CSUser);
      await webhelper.changeArea(Common.Text.CSUser);
      await use(loginPage);
    },

    projectName: async ({ page }, use) => {
      const projectService = new ProjectService(page);
      const name = "Auto_Project" + Utils.generateGUID();
      await projectService.CreateProject(name);
      const questionnaireService = new Questionnaireservice(page);
      await questionnaireService.addQuestion(TestData.Question1, TestData.Question_Title1);
      await use(name);
    },

    guid: async ({ page, webhelper, projectName }, use) => {
      const g = await webhelper.fetchRecordGuid(page.url());
      await use(g);
      if (g) {
        try {
          await deleteRecord(EntityLogicalNames.Projects, g);
        } catch (e) {
          console.warn('Failed to delete project during teardown', e);
        }
      }
    },

  }
);

export default test;
export { expect };
