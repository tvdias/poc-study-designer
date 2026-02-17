import {test } from '../Fixture/LoginAsLibrarianFixture.ts';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { ConfigQuestionsService } from '../../services/ConfigQuestionsService';
import { DropDownList } from '../../constants/DropDownList.json';
import { Utils } from "../../utils/utils";
import { TestData } from '../../Test Data/ProjectData.json';
import { ConfigQuestions } from '../../selectors/ConfigQuestionsSelector.json';
import { WebHelper } from '../../utils/WebHelper';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { ProductsService } from '../../services/ProductsService';

// Using a LoginAsLibrarianFixture to handle setup and teardown for all test cases:
// 1. Login the application with Librarian user

test("[2338548,2323108,2351117] Configuration Questions - Create, add answers, update draft to active status & add dependency rule", { tag: ['@Regression', '@ConfigQuestions'] }, async ({ page, browser,loginPage }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2338548' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2323108' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2351117' });

    const configQuestionsService = new ConfigQuestionsService(page);
    const webHelper = new WebHelper(page);
    const question = TestData.Question + Utils.generateGUID();
    var guid: string;

    await test.step('Create configuration question', async () => {
        await configQuestionsService.createConfigQuestions(question, DropDownList.ConfigQuestions.SingleCoded);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add & Validate config answers', async () => {
        let answers: string[] = [CommonTestData.Answer];
        await configQuestionsService.addConfigAnswers(answers);
        await webHelper.validateSubGridFirstRecordIsDisplayed(ConfigQuestions.Views.ActiveConfigurationAnswersSubgrid)
    });

    await test.step('Add dependency rule - module content type', async () => {
        await configQuestionsService.addDependencyRule(CommonTestData.DependencyRuleName + guid, CommonTestData.Answer, DropDownList.ConfigQuestions.Include,
            DropDownList.ConfigQuestions.PRIMARY, DropDownList.ConfigQuestions.Module, CommonTestData.ModuleName);
    });

    await test.step('Add dependency rule - question content type', async () => {
        await configQuestionsService.addDependencyRule(CommonTestData.DependencyRuleName + guid, CommonTestData.Answer, DropDownList.ConfigQuestions.Include,
            DropDownList.ConfigQuestions.PRIMARY, DropDownList.ConfigQuestions.Question, CommonTestData.QuestionBankName);
        await deleteRecord(EntityLogicalNames.ConfigQuestions, guid);
    });


});

test("[2557096] End to End flow for master questionnaire generation by answering config. questions", { tag: ['@Regression', '@Projects'] }, async ({ page, browser,loginPage }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2557096' });

    const productsService = new ProductsService(page);
    const configQuestionsService = new ConfigQuestionsService(page);
    const webHelper = new WebHelper(page);

    const productName = CommonTestData.ProductName + Utils.generateGUID();
    const productTemplateName = CommonTestData.ProductTemplateName + Utils.generateGUID();
    const productTemplateVersion = CommonTestData.ProductTemplateVersion;
    let answers: string[] = [CommonTestData.Answer, CommonTestData.No];
    let answerSelection: string[] = [CommonTestData.Answer];
    const configQuestion = TestData.Question + Utils.generateGUID();

    let configQuestionGuid: string, productGuid: string, projectGuid: string;

    await test.step('Create configuration question', async () => {
        await configQuestionsService.createConfigQuestions(configQuestion, DropDownList.ConfigQuestions.SingleCoded);
        configQuestionGuid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add & Validate config answers', async () => {
        await configQuestionsService.addConfigAnswers(answers);
        await webHelper.validateSubGridFirstRecordIsDisplayed(ConfigQuestions.Views.ActiveConfigurationAnswersSubgrid)
    });

    await test.step('Add dependency rule - module content type', async () => {
        const guid = Utils.generateGUID();
        await configQuestionsService.addDependencyRule(CommonTestData.DependencyRuleName + guid, answers[0], DropDownList.ConfigQuestions.Include,
            DropDownList.ConfigQuestions.PRIMARY, DropDownList.ConfigQuestions.Module, CommonTestData.ModuleName);
        await configQuestionsService.activateDependencyRules();
    });

    await test.step('Create a product', async () => {
        await productsService.createProduct(productName);
        productGuid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Add configurations questions', async () => {
        await productsService.addConfigQuestions(configQuestion);
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

    const context = await browser.newContext();
    const csUserPage = await context.newPage();
    const projectService = new ProjectService(csUserPage);

    await test.step('Login with CS user into application', async () => {
        await LoginToMDAWithTestUser(csUserPage, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        const projectName = TestData.ProjectName + Utils.generateGUID();
        await projectService.CreateProject(projectName);
        projectGuid = await webHelper.fetchRecordGuid(csUserPage.url());
    });

    await test.step('Add product template & answer config questions', async () => {
        await projectService.addProductTemplate(productName);
        await projectService.waitUntilConfigQuestionsVisible();
        await projectService.selectConfigQuestionAnswers(answerSelection);
    });

    await test.step('Apply product template & validate master questionnaire', async () => {
        await projectService.applyProductTemplate();
        await projectService.validateNoOfMasterQuestionnaire(2);
    });

    await test.step('Clean up created record', async () => {
        await deleteRecord(EntityLogicalNames.ConfigQuestions, configQuestionGuid);
        await deleteRecord(EntityLogicalNames.Products, productGuid);
        await deleteRecord(EntityLogicalNames.Projects, projectGuid);
    });
});

