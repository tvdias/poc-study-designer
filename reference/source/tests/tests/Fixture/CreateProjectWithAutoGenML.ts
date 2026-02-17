import { test as baseTest, expect } from "@playwright/test";
import { LoginPage } from "../../pages/LoginPage";
import { WebHelper } from '../../utils/WebHelper';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { Questionnaireservice } from '../../services/QuestionnaireService';
import { TestData } from '../../Test Data/QuestionnnaireData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';

//This Fixture is create the Project with Auto Generated ML Question
export const createproject = baseTest.extend<{
  loginPage: LoginPage;
  webhelper: WebHelper;
  guid: string;
  projectName: string;
}>(
  {
    webhelper: async ({ page }, use) => {
      const webHelper = new WebHelper(page);
      await use(webHelper);
    },


    projectName: async ({ page }, use) => {
      const projectService = new ProjectService(page);
      const name = "Auto_Project" + Utils.generateGUID();
      await projectService.CreateProject(name);
      const questionnaireService = new Questionnaireservice(page);
      await questionnaireService.addQuestion(TestData.AutoGenMLQuestion, TestData.AutoGenMLQuestion_Title);
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

export default createproject;
export { expect };
