import {test } from '../Fixture/LoginAsLibrarianFixture.ts';
import { ProductsService } from '../../services/ProductsService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { Products } from '../../selectors/ProductsSelectors.json'
import { WebHelper } from '../../utils/WebHelper'
import { CommonTestData } from '../../Test Data/CommonTestData.json'
import { Common } from '../../selectors/CommonSelector.json'

// Using a LoginAsLibrarianFixture to handle setup and teardown for all test cases:
// 1. Login the application with Librarian user

test("[2320409,2321811,2322296,2322639,2322653,2322654,2320883] Products Entity - Create product, add product template & add template lines- questions & modules", { tag: ['@Regression', '@Products'] }, async ({ page,loginPage }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2320409' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2321811' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2322296' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2322639' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2322653' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2322654' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2320883' });

    const productsService = new ProductsService(page);
    const webHelper = new WebHelper(page);
    const productName = CommonTestData.ProductName + Utils.generateGUID();
    const productTemplateName = CommonTestData.ProductTemplateName + Utils.generateGUID();
    const productTemplateVersion = CommonTestData.ProductTemplateVersion;

    await test.step('Create a product', async () => {
        await productsService.createProduct(productName);
    });

    await test.step('Add configurations questions', async () => {
        await productsService.addConfigQuestions(CommonTestData.ConfigQuestion);
    });

    await test.step('Create a product template', async () => {
        await productsService.createProductTemplate(productTemplateName, productTemplateVersion);
    });

    await test.step('Add a product template lines with questions & include is false', async () => {
        await productsService.addProductTemplateLines(productTemplateName, DropDownList.ConfigQuestions.Question, DropDownList.ConfigQuestions.False, CommonTestData.QuestionBankName);
    });

    await test.step('Add a product template lines with module & include is true', async () => {
        await productsService.addProductTemplateLines(productTemplateName, DropDownList.ConfigQuestions.Module, DropDownList.ConfigQuestions.True, CommonTestData.ModuleGenderSeries);
    });

    await test.step('Deactivate product template lines - module & question', async () => {
        await webHelper.openRelatedEntity(Common.Entity.ProductTemplateLines);
        await webHelper.deactivateActivateAllSubgridRecords(Common.Text.Deactivate);
        await productsService.verifyPopupMessageForDeactivateRecords(Common.Text.Deactivate);
    });

    await test.step('Validate Deactivated product template lines ', async () => {
        await webHelper.selectSubgridView(Products.Views.InactiveProductTemplateLines);
        await webHelper.assertRowsCount(2);
    });
});


test("[2350986, 2351049] Edit Product Config Question Display Rule for Configuration Question", { tag: ['@Regression', '@Products'] }, async ({ page, browser,loginPage }) => {
   test.info().annotations.push({ type: 'TestCaseId', description: '2350986' });
   test.info().annotations.push({ type: 'TestCaseId', description: '2351049' });
   
    const productsService = new ProductsService(page);
    const webHelper = new WebHelper(page);
    const productName = CommonTestData.ProductName + Utils.generateGUID();
    const configQuestion = CommonTestData.ConfigQuestion;
    const answers = [CommonTestData.BrandHealthAnswer1, CommonTestData.BrandHealthAnswer2, CommonTestData.BrandHealthAnswer3];

    await test.step('Create a product', async () => {
        await productsService.createProduct(productName);
    });

    await test.step('Add configurations questions', async () => {
        await productsService.addConfigQuestions(configQuestion);
    });

    await test.step('Validate - Product Config Question Display Rule fields', async () => {
        await productsService.openRecordFromGrid(configQuestion);
        await webHelper.clickOnCommandBarBtn(Products.Buttons.NewProductConfigQuestionDisplayRule);
        await productsService.validateProductConfigDisplayRuleFormFields();
        await webHelper.clickQCCancel();
        //await webHelper.discardChanges();
        await page.close();
    });

    const context = await browser.newContext();
    const csUserPage = await context.newPage();
    const webHelperSecond = new WebHelper(csUserPage);
    const productsServiceSecond = new ProductsService(csUserPage);

    await test.step('Login into application with CS user', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1);
        await webHelperSecond.changeArea(Common.Text.Librarian)
    });

    await test.step('Open config question & validate', async () => {
        await webHelperSecond.goToEntity(Common.Entity.Products);
        await webHelperSecond.searchAndOpenRecord(productName);
        await webHelperSecond.clickOnTab(Products.Tabs.ConfigurationQuestions);
        await productsServiceSecond.openRecordFromGrid(configQuestion);
        await webHelperSecond.clickByAriaLabel(configQuestion);
        await productsServiceSecond.compareConfigAnswers(answers);
    });
});