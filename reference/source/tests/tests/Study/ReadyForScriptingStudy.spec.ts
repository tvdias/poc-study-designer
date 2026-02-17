import { expect, test } from '@playwright/test';
import { ProjectService } from '../../services/ProjectService';
import { LoginToMDAWithTestUser } from '../../utils/Login';
import { Utils } from "../../utils/utils";
import { TestUser, AppName, AppId } from '../../constants/TestUsers.json';
import { StudyTestData } from '../../Test Data/StudyData.json';
import { DropDownList } from '../../constants/DropDownList.json';
import { StudyService } from '../../services/StudyService';
import { WebHelper } from '../../utils/WebHelper';
import { deleteRecord } from '../../utils/APIHelper';
import { StudySelectors } from '../../selectors/StudySelectors.json';
import { TestData } from '../../Test Data/ProjectData.json';
import { EntityLogicalNames } from '../../constants/CommonLogicalNames.json';
import { Questionnaire } from '../../selectors/QuestionnaireSelector.json';
import { TestData as QuestionnaireTestData } from '../../Test Data/QuestionnnaireData.json';
import { Questionnaireservice } from '../../services/QuestionnaireService';

test("[2359797,2359675] Verify Ready for Scripting study - Add scripter user to the project, validate Snapshots, status reason, version number, questionnaire lines & buttons", { tag: ['@Regression', '@Study'] }, async ({ page }) => {

    test.info().annotations.push({ type: 'TestCaseId', description: '2359797' });
    test.info().annotations.push({ type: 'TestCaseId', description: '2359675' });

    const studyService = new StudyService(page);
    const projectService = new ProjectService(page);
    const webHelper = new WebHelper(page);
    var guid: string;

    const projectName = TestData.ProjectName + Utils.generateGUID();

    await test.step('Navigating to URL', async () => {
        await LoginToMDAWithTestUser(page, TestUser.CSUser, AppId.UC1, AppName.UC1);
    });

    await test.step('Create a project', async () => {
        await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
        guid = await webHelper.fetchRecordGuid(page.url());
    });

    await test.step('Generate a master questionnaire lines', async () => {
        await projectService.addProductTemplate(TestData.Product1);
        await projectService.applyProductTemplate();
    });

    const masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();

    await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
        await projectService.addScripterUserToProject();
        const studyName = StudyTestData.StudyName + Utils.generateGUID();
        await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
            StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
        await webHelper.handleConfirmationPopup();
        await studyService.ValidateInitialDraftStateStudy();
    });

    await test.step('Update study from Draft to Ready for Scritping', async () => {
        await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
    });

    const studyQuestionsCount = await studyService.getStudyQuestionnaireLinesCount();
    expect(masterQuestionnaireLinesCount).toBe(studyQuestionsCount);


    await test.step('Validate study version & status reason', async () => {
        await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
        await studyService.ValidateStudyStatusReason(DropDownList.Status.ReadyForScripting);
    });
    await test.step('Check study record becomes read only except Status Reason field', async () => {
        await studyService.ValidateReadOnlyFieldsInStudy();
    });

    await test.step('Validate button visibility when study is in Ready for Scripting state', async () => {
        const visibleButtonsOne: string[] = StudySelectors.ExpectedResults.DraftStudyButtons;
        await studyService.ValidateStudyButtons(visibleButtonsOne, 'Visible');
        const visibleButtonsTwo: string[] = StudySelectors.ExpectedResults.DraftStudyHiddenButtons;
        await studyService.ValidateStudyButtons(visibleButtonsTwo, 'Visible');
    });

    await test.step('Validate snapshots are generated correctly & questions order', async () => {
        await studyService.ValidateStudySnapshotQuestions();
    });

    await test.step('Validate export to XML functionality', async () => {
        await studyService.ValidateXMLExportFunctionality();
    });

    await test.step('Clean up created project record', async () => {
        await deleteRecord(EntityLogicalNames.Projects, guid);
    });
});



const userroles = [
    { user: TestUser.CSUser, TestcaseId: '2374350' },
    { user: TestUser.LibrarianUser, TestcaseId: '2621523' },
]
// Executes the same test case sequentially in the defined order using both CS user and Librarian user contexts.
test.describe.serial('', () => {
    for (const role of userroles) {

        test(`${role.TestcaseId} Verify that snapshot details for the ${role.user} user`, { tag: ['@Regression', '@Study'] }, async ({ page }) => {

            test.info().annotations.push({ type: 'TestCaseId', description: role.TestcaseId });

            const studyService = new StudyService(page);
            const projectService = new ProjectService(page);
            const questionnaireService = new Questionnaireservice(page);
            const webHelper = new WebHelper(page);
            var guid: string;

            const projectName = TestData.ProjectName + Utils.generateGUID();

            await test.step('Navigating to URL', async () => {
                await LoginToMDAWithTestUser(page, role.user, AppId.UC1, AppName.UC1);
            });

            await test.step('Create a project', async () => {
                await projectService.CreateCustomProject(projectName, DropDownList.Project.CATI, TestData.Client, TestData.CommissioningMarket, projectName);
                guid = await webHelper.fetchRecordGuid(page.url());
            });

            await test.step('Add a Question to the Project', async () => {
                await questionnaireService.addQuestion(QuestionnaireTestData.StandardQuestion, Questionnaire.Text.CategoryIntroduction);
            });
            await test.step('Verify the Added Question collapsed view', async () => {
                await questionnaireService.validateQuestionsAddedInQuestionnaire(QuestionnaireTestData.StandardQuest_VariableName1, DropDownList.QuestionBank.SmallTextInput, QuestionnaireTestData.StandardQuestionText2);
            });
            await test.step('Add a Module to the Project', async () => {
                await questionnaireService.addModule(QuestionnaireTestData.ModuleName);
            });
            await test.step('Verify the Added Module collapsed view', async () => {
                await questionnaireService.validateModulesAddedInQuestionnaire(QuestionnaireTestData.ModuleName, QuestionnaireTestData.M_QuestionName, QuestionnaireTestData.M_QuestionType, QuestionnaireTestData.M_QuestionText);
            });

            const masterQuestionnaireLinesCount = await projectService.getMasterQuestionnaireLinesCount();

            await test.step('Create a study & Compare questionnaire lines with master questionnaire lines', async () => {
                await projectService.addScripterUserToProject();
                const studyName = StudyTestData.StudyName + Utils.generateGUID();
                await studyService.CreateNewStudy(studyName, StudyTestData.Category, StudyTestData.FieldWorkMarket,
                    StudyTestData.MaconomyJobNumber, StudyTestData.ProjectOperationsURL, StudyTestData.ScripterNotes);
                await webHelper.handleConfirmationPopup();
                await studyService.ValidateInitialDraftStateStudy();
            });
            await test.step('Update study from Draft to Ready for Scritping', async () => {
                await studyService.updateStudyStatus(DropDownList.Status.ReadyForScripting);
            });

            await test.step('Verify that all modules, questions are copied into snapshot', async () => {
                const studyQuestionsCount = await studyService.getStudyQuestionnaireLinesCount();
                expect(masterQuestionnaireLinesCount).toBe(studyQuestionsCount);
            });

            await test.step('Validate study version & status reason', async () => {
                await studyService.ValidateStudyVersionNumber(StudySelectors.VersionNumber.One);
                await studyService.ValidateStudyStatusReason(DropDownList.Status.ReadyForScripting);
            });

            await test.step('Validate snapshots are generated correctly & questions order', async () => {
                await studyService.ValidateStudySnapshotQuestions();
            });

            await test.step('Clean up created project record', async () => {
                await deleteRecord(EntityLogicalNames.Projects, guid);
            });
        });

    }
});