import { expect, test } from '../Fixture/LoginAsLibrarianFixture.ts';
import { ProjectService } from '../../services/ProjectService';
import { WebHelper } from '../../utils/WebHelper';
import { Common } from '../../selectors/CommonSelector.json';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { CommissioningMarket } from '../../services/CommissioningMarket.ts';



// Using a LoginAsLibrarianFixture to handle setup and teardown for all test cases:
// 1. Login the application with Librarian user

test("[2520734] Validate all the Entities are displaying for Librarian User", { tag: ['@Smoke', '@Librarian'] }, async ({ page, loginPage }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2520734' });
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    await test.step('Validate all Entities are displaying', async () => {
        await webHelper.verifyTheEntity(Common.Entity.QuestionBank);
        await webHelper.verifyTheEntity(Common.Entity.Clients);
        await webHelper.verifyTheEntity(Common.Entity.Modules);
        await webHelper.verifyTheEntity(Common.Entity.CommissioningMarkets);
        await webHelper.verifyTheEntity(Common.Entity.FieldworkMarkets);
        await webHelper.verifyTheEntity(Common.Entity.Tags);
        await webHelper.verifyTheEntity(Common.Entity.BusinessRoleMappings);
        await webHelper.verifyTheEntity(Common.Entity.ConfigurationQuestions);
        await webHelper.verifyTheEntity(Common.Entity.Products);

    });

});

test("[2410000] Verify Librarian is able to edit commissioning market data", { tag: ['@Regression', '@Librarian'] }, async ({ page, loginPage }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2410000' });
    const commissioningMarket = new CommissioningMarket(page);
    const webHelper = new WebHelper(page);

    await test.step('Goto Commissioning market entity > Open a commissioning market', async () => {
        await webHelper.verifyTheEntity(Common.Entity.CommissioningMarkets);
        await webHelper.goToEntity(Common.Entity.CommissioningMarkets);
        await webHelper.searchAndOpenRecord(CommonTestData.AutomationTesting_Market)
    });
    await test.step('Edit the record and update > Save', async () => {
        await commissioningMarket.fillTheName(CommonTestData.AutomationTesting_Market + "1")
        const name = await commissioningMarket.getTheName();
        await expect(name).toBe(CommonTestData.AutomationTesting_Market + "1");
        await commissioningMarket.fillTheName(CommonTestData.AutomationTesting_Market)

    });

});
