import { LoginToMDAWithTestUser } from '../../utils/Login';
import { test } from '../Fixture/LoginAsLibrarianFixture.ts';
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { Utils } from "../../utils/utils";
import { DropDownList } from '../../constants/DropDownList.json';
import { WebHelper } from '../../utils/WebHelper';
import { CommonTestData } from '../../Test Data/CommonTestData.json';
import { Common } from '../../selectors/CommonSelector.json';
import { QuestionBank } from '../../selectors/QuestionBankSelectors.json';
import { QuestionBankservice } from '../../services/QuestionBankService';
import { TestData } from '../../Test Data/QuestionBankData.json';
import { deleteRecord } from '../../utils/APIHelper';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';

// Using a LoginAsLibrarianFixture to handle setup and teardown for all test cases:
// 1. Login the application with Librarian user

test("[2274742] Create Standard Question Bank with Answers Lists", { tag: ['@Smoke', '@QuestionBank'] }, async ({ page,loginPage }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2274742' });

    const questionbankService = new QuestionBankservice(page);
    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle + Utils.generateGUID();
    var guid = "";

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);
    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
    });

    await test.step('Delete the Created Question Bank', async () => {
        await deleteRecord(EntityLogicalNames.QuestionBanks, guid);
    });
});
test("[2509520] To check if all additional fields are displayed in QB entity > Question view", { tag: ['@Regression', '@QuestionBank'] }, async ({ page ,loginPage}) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2509520' });

    const questionbankService = new QuestionBankservice(page);
    const webHelper = new WebHelper(page);

    await test.step('Go to QB entity', async () => {
        await webHelper.goToEntityAndClickNewButton(Common.Entity.QuestionBank);
    });

    await test.step('Now check for the fields in Question view', async () => {
        await questionbankService.verifySelectiveOptionalfieldsInQuestionBank();
    });


});
test("[2509527] To check if all additional fields are available in Admin tab in QB entity", { tag: ['@Regression', '@QuestionBank'] }, async ({ page ,loginPage}) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2509527' });

    const questionbankService = new QuestionBankservice(page);
    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle + Utils.generateGUID();
    let guid: string;

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
    });

    await test.step('Now go to Admin tab in Question view ', async () => {
        await webHelper.clickOnTab(QuestionBank.Tabs.Admin);
    });

    await test.step('Check if the UI is allowing the fields to be editable ', async () => {
        await questionbankService.FillAdminFields(CommonTestData.Answer, CommonTestData.No, TestData.MinLength, TestData.MaxLength, TestData.Property, TestData.SortOrder);
        await webHelper.saveRecord();
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Delete the Created Question Bank', async () => {
        //await deleteRecord(EntityLogicalNames.QuestionBanks, guid);
    });
});

test("[2509536] To check if Answer tab in QB - Answer tab contain all additional fields", { tag: ['@Regression', '@QuestionBank'] }, async ({ page,loginPage }) => {
    test.info().annotations.push({ type: 'TestCaseId', description: '2509536' });

    const questionbankService = new QuestionBankservice(page);
    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle + Utils.generateGUID();
    var guid = "";

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);

    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        guid = await webHelper.fetchRecordGuid(page.url());

    });

    await test.step('Navigate and Click on New Question Answer Lists button', async () => {
        await questionbankService.clickNewQuestionAnswer();
    });

    await test.step('Fill and Create Answers List', async () => {
        await questionbankService.FillandCreateQuestionAnswsersList(TestData.AnswerText, TestData.AnswerCode, TestData.Location, CommonTestData.Answer, TestData.Property, TestData.Version);
        await webHelper.clickonContinueAnyway();

    });
    await test.step('Check if the UI is allowing the fields to be editable ', async () => {
        await questionbankService.FillAnswerFields(TestData.Version, CommonTestData.No, TestData.AnswerCode, TestData.AnswerText, TestData.EffectiveDate, TestData.EndDate);
    });

    await test.step('Delete the Created Question Bank', async () => {
         await deleteRecord(EntityLogicalNames.QuestionBanks, guid);
    });
});

test("[2306282] Verify the new version of the question can be generated from the Question Bank", { tag: ['@Regression', '@QuestionBank'] }, async ({ page, loginPage }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2306282' });

    const questionbankService = new QuestionBankservice(page);
    const webHelper = new WebHelper(page);
    const questionName = CommonTestData.QuestionVariableName + Utils.generateText();
    const questionTitle = CommonTestData.QuestionTitle + Utils.generateGUID();
    var guid = "";

    await test.step('Create a Question Bank', async () => {
        await questionbankService.fillMandatoryfieldsInQuestionBank(questionName, DropDownList.QuestionBank.MultiChoice, questionTitle, TestData.QuestionText);
        await questionbankService.fillOptionalfieldsInQuestionBank(TestData.SortOrder, TestData.SortOrder, TestData.MinLength, TestData.MaxLength, TestData.QuestionFormatDetails, TestData.QuestionRationale, TestData.ScripterNotes, TestData.Methodology, TestData.CustomNotes, TestData.SingleorMulticode);
        await questionbankService.clickonSaveRecord();
        await questionbankService.searchDraftQuestionBank(questionName);
    });

    await test.step('Change the Status Reason Draft to Active', async () => {
        await questionbankService.changeStatusReason();
        await questionbankService.selectQuestionBank(questionName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Click on New Version', async () => {
        await page.reload();
        await Promise.all([
            page.waitForTimeout(5000),
            webHelper.clickOnCommandBarBtn(Common.AriaLabel.NewVersion)
        ]);
        await questionbankService.verifyNewVersion(questionName);
    });

    await test.step('Delete the Created Question Bank', async () => {
        await deleteRecord(EntityLogicalNames.QuestionBanks, guid);
    });

});