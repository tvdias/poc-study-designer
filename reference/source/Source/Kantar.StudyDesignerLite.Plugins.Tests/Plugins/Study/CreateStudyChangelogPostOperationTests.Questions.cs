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
        public void CreateStudyChangelogPostOperation_AddedQuestion_ShouldCreateChangelog()
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

            // Arrange KTR_QuestionnaireLine
            var questionnaireLineA = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerA1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            var questionnaireLineB = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerB1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineB)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();
            var studyQuestionnaireLineB = new StudyQuestionnaireLineBuilder(study, questionnaireLineB)
                .Build();
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();

            // Arrange Snapshot
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            var currentSnapshotB = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineB)
                .Build();
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();

            // Arrange Snapshot answers
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .Build();
            var currentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotB, qlAnswerB1)
                .Build();
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
                {
                    study, parentStudy,
                    questionnaireLineA, qlAnswerA1, questionnaireLineB, qlAnswerB1,
                    studyQuestionnaireLineA, studyQuestionnaireLineB, parentStudyQuestionnaireLineA,
                    currentSnapshotA, parentSnapshotA, currentSnapshotB,
                    currentAnswerSnapshotA1, currentAnswerSnapshotB1, parentAnswerSnapshotA1 });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            AssertChangeLogs(
               changeLogRows,
               expectedCount: 1,
               expectedCurrentStudy: study,
               expectedParentStudy: parentStudy,
               expectedCurrentQLSnapshot: currentSnapshotB,
               expectedParentQLSnapshot: null,
               expectedRelatedObject: KTR_ChangelogRelatedObject.Question,
               expectedChangeType: KTR_ChangelogType.QuestionAdded,
               expectedFieldChanged: null,
               expectedCurrentQlAnswerSnapshot: null,
               expectedParentQlAnswerSnapshot: null,
               expectedOldValue: null,
               expectedNewValue: null,
               expectedModule: null);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_RemovedQuestion_ShouldCreateChangelog()
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

            // Arrange KTR_QuestionnaireLine
            var questionnaireLineA = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerA1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            var questionnaireLineB = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerB1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineB)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentStudyQuestionnaireLineB = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineB)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var currentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshot
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentSnapshotB = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineB)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            // Arrange Snapshot answers
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();
            var parentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotB, qlAnswerB1)
                .Build();
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1,
                questionnaireLineB, qlAnswerB1,
                parentStudyQuestionnaireLineA, parentStudyQuestionnaireLineB,
                currentStudyQuestionnaireLineA,
                parentSnapshotA,parentSnapshotB,
                currentSnapshotA,
                parentAnswerSnapshotA1, parentAnswerSnapshotB1,
                currentAnswerSnapshotA1
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            var questionRemovedChangeLogRow = changeLogRows
                .Where(x => x.KTR_Change == KTR_ChangelogType.QuestionRemoved)
                .ToList();
            Assert.IsTrue(changeLogRows.Count == 1, "Expected exactly one QuestionRemoved changelog row.");

            AssertChangeLogs(
                questionRemovedChangeLogRow,
                expectedCount: 1,
                expectedCurrentStudy: study,
                expectedParentStudy: parentStudy,
                expectedCurrentQLSnapshot: null,
                expectedParentQLSnapshot: parentSnapshotB,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Question,
                expectedChangeType: KTR_ChangelogType.QuestionRemoved,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: null,
                expectedOldValue: null,
                expectedNewValue: null,
                expectedModule: null);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_QuestionFieldChanged_ShouldCreateChangelog()
        {
            string expectedOldValue = null;
            string expectedNewValue = "scripter notes example - edited";

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

            // Arrange KTR_QuestionnaireLine
            var questionnaireLineA = new QuestionnaireLineBuilder(project)
                .WithScripterNotes(expectedOldValue)
                .Build();
            var qlAnswerA1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            // Arrange Parent Study:
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();

            // Arrange Study:
            questionnaireLineA.KTR_ScripterNotes = expectedNewValue;

            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .WithScripterNotes(expectedNewValue)
                .Build();
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                currentAnswerSnapshotA1, parentAnswerSnapshotA1
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
                expectedCurrentQLSnapshot: currentSnapshotA,
                expectedParentQLSnapshot: parentSnapshotA,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Question,
                expectedChangeType: KTR_ChangelogType.FieldChangeQuestion,
                expectedFieldChanged: KTR_ChangelogFieldChanged.QuestionScripterNotes,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: null,
                expectedOldValue: expectedOldValue,
                expectedNewValue: expectedNewValue,
                expectedModule: null);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_QuestionReordered_ShouldCreateChangelog()
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

            // Arrange KTR_QuestionnaireLine
            var questionnaireLineA = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerA1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();
            var questionnaireLineB = new QuestionnaireLineBuilder(project)
                .Build();
            var qlAnswerB1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineB)
                .Build();

            // Arrange Parent Study:
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .WithSortOrder(1)
                .Build();
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();
            var parentStudyQuestionnaireLineB = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineB)
                .Build();
            var parentSnapshotB = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineB)
                .WithSortOrder(2)
                .Build();
            var parentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotB, qlAnswerB1)
                .Build();

            // Arrange Study:
            //?

            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .WithSortOrder(2) // Reordered to 2
                .Build();
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .Build();
            var studyQuestionnaireLineB = new StudyQuestionnaireLineBuilder(study, questionnaireLineB)
                .Build();
            var currentSnapshotB = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineB)
                .WithSortOrder(1) // Reordered to 1
                .Build();
            var currentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotB, qlAnswerB1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                currentAnswerSnapshotA1, parentAnswerSnapshotA1,
                questionnaireLineB, qlAnswerB1,
                studyQuestionnaireLineB, parentStudyQuestionnaireLineB,
                currentSnapshotB, parentSnapshotB,
                currentAnswerSnapshotB1, parentAnswerSnapshotB1
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
                expectedCurrentQLSnapshot: currentSnapshotB,
                expectedParentQLSnapshot: parentSnapshotB,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Question,
                expectedChangeType: KTR_ChangelogType.QuestionReordered,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: null,
                expectedOldValue: "2",
                expectedNewValue: "1",
                expectedModule: null);
        }
    }
}
