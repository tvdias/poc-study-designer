import { test } from '@playwright/test';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser, waitUntilAppIdle } from '../../utils/Login';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';

import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { Common } from '../../selectors/CommonSelector.json';


test("[] Delete the Automation Records", { tag: [ '@DeleteRecord'] }, async ({ page }) => {

     const projectService = new ProjectService(page);
     const webHelper = new WebHelper(page);
    const recordName = "AUTO_";

     await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.LibrarianUser, AppId.UC1, AppName.UC1);

     });

     const entities = [
       { entity: Common.Entity.Products,logicname:EntityLogicalNames.Products },
       { entity: Common.Entity.QuestionBank,logicname:EntityLogicalNames.QuestionBanks },
       { entity: Common.Entity.ConfigurationQuestions,logicname:EntityLogicalNames.ConfigQuestions },
       { entity: Common.Entity.Projects,logicname:EntityLogicalNames.Projects },
       ];
       for (const item of entities) {
          await test.step('search for a record', async () => {
            if(item.entity==Common.Entity.Projects)
             await webHelper.changeArea(Common.Text.CSUser);
            else
             await webHelper.changeArea(Common.Text.Librarian);

         await webHelper.goToEntity(item.entity);
         await projectService.enterRecordNameInFilterTextbox(recordName);
       });

         await test.step('Clean up created records', async () => {
            await page.waitForTimeout(5000); //wait for loading the all the records
             const count=await webHelper.getTheCountofRecords(recordName);
             for (let i: number = 0; i < count; i++) 
             {
                await webHelper.clickOnCommandBarBtn(Common.AriaLabel.Refresh);
                await waitUntilAppIdle(page);
                await projectService.enterRecordNameInFilterTextbox(recordName);
                await waitUntilAppIdle(page);
                await webHelper.clickonStartWithText(recordName);
                var guid = await webHelper.fetchRecordGuid(page.url());
                await deleteRecord(item.logicname, guid);
                await webHelper.clickGoBackArrow();
                
             }
        });
    }
});
