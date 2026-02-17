import { expect, mergeTests } from '@playwright/test';
import { logintest } from '../Fixture/LoginFixture';
import { addproduct } from '../Fixture/CreateProjectWithProduct';
import { ProjectService } from '../../services/ProjectService';
import { WebHelper } from '../../utils/WebHelper';
import { TestData } from '../../Test Data/ProjectData.json';
import { Common } from '../../selectors/CommonSelector.json';
import { TestData as QuestTestData } from '../../Test Data/QuestionnnaireData.json';

// Using a LoginFixture and CreateProjectWithProduct to handle setup and teardown for all test cases:
// 1. Login the application with CS user
// 2. Create the project and add a Prodcut to the project before each test execution.
// 3. Delete the project after the each test case execution


// Merges the results of the login test and the create a project and add prodcut into a single combined test execution.
const test = mergeTests(logintest, addproduct);


test("[2327712] Check user is allowed to change the Questionnaire which replaces the old one when Product template is applied", { tag: ['@Regression', '@Products'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2327712' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    await test.step('Verify Questions after applying the Product', async () => {
        await webHelper.verifyTheSpantext(QuestTestData.Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question2, true);

    });

    await test.step('Edit the Product and Product template', async () => {
        await projectService.deleteProductTemplate("Delete " + TestData.Product1);
        await projectService.addProductTemplate(TestData.Product2);
    });


    await test.step('Click on Äpply Product Template button', async () => {
        await projectService.applyProductTemplate();
        await webHelper.verifyTheSpantext(QuestTestData.Question1, false);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question1, false);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question2, false);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn1, true);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn2, true);

    });

});
test("[2340989] Check a confirmation message will be displayed to the user when there are changes in the Product template or when user clicks on Apply Product Template button for the existing Project", { tag: ['@Regression', '@Products'] }, async ({ page, browser, loginPage, guid, projectName }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2340989' });

    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);

    await test.step('Verify Questions after applying the Product', async () => {
        await webHelper.verifyTheSpantext(QuestTestData.Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question2, true);

    });

    await test.step('Edit the Product and Product template', async () => {
        await projectService.deleteProductTemplate("Delete " + TestData.Product1);
        await projectService.addProductTemplate(TestData.Product2);
    });


    await test.step('Click on Apply Product Template button on general tab', async () => {
        await projectService.validateProductTemplatePopupmessage();
    });

    await test.step('Click on No', async () => {
        await webHelper.clickOnConfirmationPopup(Common.Text.No);
        await webHelper.verifyTheSpantext(QuestTestData.Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question1, true);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question2, true);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn1, false);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn2, false);

    });

    await test.step('Click on Äpply Product Template button and Click on Yes button', async () => {
        await projectService.applyProductTemplate();
        await webHelper.verifyTheSpantext(QuestTestData.Question1, false);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question1, false);
        await webHelper.verifyTheSpantext(QuestTestData.ModuleName6_Question2, false);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn1, true);
        await webHelper.verifyTheSpantext(QuestTestData.TestProduct_Qn2, true);

    });


});





