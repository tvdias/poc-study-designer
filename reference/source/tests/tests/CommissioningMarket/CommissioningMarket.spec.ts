import { expect, test } from '@playwright/test';
import { ProjectService } from '../../services/ProjectService.ts';
import { WebHelper } from '../../utils/WebHelper.ts';
import { Common } from '../../selectors/CommonSelector.json';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { CommissioningMarket } from '../../services/CommissioningMarket.ts';
import { LoginToMDAWithTestUser, waitUntilAppIdle } from '../../utils/Login.ts';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';


test("[2410002] Verify Scripter is not able to edit the fieldwork Market and Commissioning market data", { tag: ['@Regression', '@Librarian'] }, async ({ page }) => {
  test.info().annotations.push({ type: 'TestCaseId', description: '2410002' });
  const commissioningMarket = new CommissioningMarket(page);
  const webHelper = new WebHelper(page);

  await test.step('Login into application as a Scripter', async () => {
    await LoginToMDAWithTestUser(page, TestUser.ScripterUser, AppId.UC1, AppName.UC1);
    await webHelper.changeArea(Common.Text.Librarian);
  });

  await test.step('Goto Commissioning market entity > Open a commissioning market', async () => {
    await webHelper.verifyTheEntity(Common.Entity.CommissioningMarkets);
    await webHelper.goToEntity(Common.Entity.CommissioningMarkets);
    await commissioningMarket.searchForData(CommonTestData.AutomationTesting_Market)
  });
  await test.step('verify the scripter should not be able to edit the record', async () => {
    await commissioningMarket.verifyTheNameFieldIsReadOnly()
  });
  await test.step('Goto fieldwork market  entity > Open a fieldwork marke', async () => {
    await webHelper.verifyTheEntity(Common.Entity.FieldworkMarkets);
    await webHelper.goToEntity(Common.Entity.FieldworkMarkets);
    await commissioningMarket.searchForData(CommonTestData.Mixed)
  });
  await test.step('verify the scripter should not be able to edit the record', async () => {
    await commissioningMarket.verifyTheNameFieldIsReadOnly()
  });

});
