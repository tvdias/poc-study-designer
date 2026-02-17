import { test as baseTest, expect } from "@playwright/test";
import { LoginPage } from "../../pages/LoginPage";
import { WebHelper } from '../../utils/WebHelper';
import { Utils } from "../../utils/utils";
import { TestData } from '../../Test Data/ProjectData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProjectService } from '../../services/ProjectService';
import { DropDownList } from '../../constants/DropDownList.json';

//This Fixture is create the Project and Add the Product
export const addproduct = baseTest.extend<{
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
      await projectService.CreateCustomProject(name, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, name);
      await projectService.addProductTemplate(TestData.Product1);
      await projectService.applyProductTemplate();
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

export default addproduct;
export { expect };
