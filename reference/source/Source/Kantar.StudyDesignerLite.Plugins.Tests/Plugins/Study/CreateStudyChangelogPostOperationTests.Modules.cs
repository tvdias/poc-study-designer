using System.Linq;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    public partial class CreateStudyChangelogPostOperationTests
    {
        [TestMethod]
        public void CreateStudyChangelogPostOperation_ModuleAdded_ShouldCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Parent Study 
            var parentStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithIsSnapshotCreated(true)
                .WithVersion(1)
                .Build();

            parentStudy.KTR_MasterStudy = parentStudy.ToEntityReference();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study v2")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithIsSnapshotCreated(true)
                .WithParentStudy(parentStudy)
                .WithVersion(2)
                .WithMasterStudy(parentStudy)
                .Build();

            // Arrange KTR_Module
            var module = new ModuleBuilder()
                .Build();

            // Arrange KTR_QuestionnaireLine
            var questionnaireLine = new QuestionnaireLineBuilder(project)
                .WithModule(module)
                .Build();

            var qlAnswer = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            // Arrange Snapshot (only current, none in parent — added)
            var currentSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine)
                .WithModule(module)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                module,
                questionnaireLine, qlAnswer,
                studyQuestionnaireLine,
                currentSnapshot,
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            AssertChangeLogs(
                changeLogRows,
                expectedCount: 1,
                expectedCurrentStudy: study,
                expectedParentStudy: parentStudy,
                expectedCurrentQLSnapshot: currentSnapshot,
                expectedParentQLSnapshot: null,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Module,
                expectedChangeType: KTR_ChangelogType.ModuleAdded,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: null,
                expectedOldValue: null,
                expectedNewValue: null,
                expectedModule: module);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_ModuleRemoved_ShouldCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Parent Study 
            var parentStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithIsSnapshotCreated(true)
                .WithVersion(1)
                .Build();

            parentStudy.KTR_MasterStudy = parentStudy.ToEntityReference();

            // Arrange Study (current study) - simulate removal by not including module/questionnaire line snapshot
            var study = new StudyBuilder(project)
                .WithName("Study v2")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithIsSnapshotCreated(true)
                .WithParentStudy(parentStudy)
                .WithVersion(2)
                .WithMasterStudy(parentStudy)
                .Build();

            // Arrange KTR_Module
            var module = new ModuleBuilder()
                .Build();

            // Arrange KTR_QuestionnaireLine
            var questionnaireLine = new QuestionnaireLineBuilder(project)
                .WithModule(module)
                .Build();

            // No current study questionnaire line snapshot to simulate removal
            // Arrange Parent Snapshot with module (exists only in parent)
            var parentSnapshot = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLine)
                .WithModule(module)
                .Build();

            // Setup the plugin context and initialize the entities
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
            study, parentStudy,
            module,
            questionnaireLine,
            parentSnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            AssertChangeLogs(
                changeLogRows,
                expectedCount: 1,
                expectedCurrentStudy: study,
                expectedParentStudy: parentStudy,
                expectedCurrentQLSnapshot: null,           // current snapshot missing (removed)
                expectedParentQLSnapshot: parentSnapshot,  // present only in parent
                expectedRelatedObject: KTR_ChangelogRelatedObject.Module,
                expectedChangeType: KTR_ChangelogType.ModuleRemoved,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: null,
                expectedOldValue: null,
                expectedNewValue: null,
                expectedModule: module);
        }
    }
}
