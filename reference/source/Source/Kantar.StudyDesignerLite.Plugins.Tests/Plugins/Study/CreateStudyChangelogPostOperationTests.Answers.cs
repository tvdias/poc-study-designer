namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{

    using System;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins.Study;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    public partial class CreateStudyChangelogPostOperationTests
    {
        [TestMethod]
        public void CreateStudyChangelogPostOperation_AddedAnswer_ShouldCreateChangelog()
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
            var qlAnswerB1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();

            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshot
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshot answers
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .Build();
            var currentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerB1)
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
                    currentAnswerSnapshotA1, parentAnswerSnapshotA1, currentAnswerSnapshotB1 });

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
               expectedParentQLSnapshot: currentSnapshotA,
               expectedRelatedObject: KTR_ChangelogRelatedObject.Answer,
               expectedChangeType: KTR_ChangelogType.AnswerAdded,
               expectedFieldChanged: null,
               expectedCurrentQlAnswerSnapshot: currentAnswerSnapshotB1,
               expectedParentQlAnswerSnapshot: null,
               expectedOldValue: null,
               expectedNewValue: null,
               expectedModule: null);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_RemovedAnswer_ShouldCreateChangelog()
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
            var qlAnswerB1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshot
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            // Arrange Snapshot answers
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .Build();
            var parentAnswerSnapshotB1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerB1)
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
                qlAnswerB1,
                parentStudyQuestionnaireLineA,
                currentStudyQuestionnaireLineA,
                parentSnapshotA,
                currentSnapshotA,
                parentAnswerSnapshotA1, parentAnswerSnapshotB1,
                currentAnswerSnapshotA1
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
                expectedCurrentQLSnapshot: parentSnapshotA,
                expectedParentQLSnapshot: parentSnapshotA,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Answer,
                expectedChangeType: KTR_ChangelogType.AnswerRemoved,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: null,
                expectedParentQlAnswerSnapshot: parentAnswerSnapshotB1,
                expectedOldValue: null,
                expectedNewValue: null,
                expectedModule: null);
        }

        [DataTestMethod]
        [DataRow(KTR_ChangelogFieldChanged.AnswerAnswerId, null, "something")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerAnswerLocation, "row", "column")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerAnswerTitle, "something", "something - edited")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerCustomerProperty, "", "blabla")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerIsActive, "no", "yes")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerIsExclusive, "yes", "no")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerIsFixed, "yes", "no")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerIsOpen, "yes", "no")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerIsTranslatable, "yes", "no")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerSourceId, "", "sddsad")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerSourceName, "source name", "source name edited")]
        [DataRow(KTR_ChangelogFieldChanged.AnswerVersion, "", "v2")]
        public void CreateStudyChangelogPostOperation_AnswerFieldChanged_ShouldCreateChangelog(
            KTR_ChangelogFieldChanged fieldChanged,
            string expectedOldValue,
            string expectedNewValue)
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

            // Arrange Study QuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentAnswerSnapshotA1Builder = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1);

            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            var currentAnswerSnapshotA1Builder = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1);

            switch (fieldChanged)
            {
                case KTR_ChangelogFieldChanged.AnswerAnswerId:
                    parentAnswerSnapshotA1Builder.WithAnswerId(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithAnswerId(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerAnswerLocation:
                    parentAnswerSnapshotA1Builder.WithAnswerLocation(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithAnswerLocation(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerAnswerTitle:
                    parentAnswerSnapshotA1Builder.WithAnswerText(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithAnswerText(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerCustomerProperty:
                    parentAnswerSnapshotA1Builder.WithCustomerProperty(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithCustomerProperty(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerIsActive:
                    parentAnswerSnapshotA1Builder.WithIsActive(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithIsActive(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerIsExclusive:
                    parentAnswerSnapshotA1Builder.WithIsExclusive(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithIsExclusive(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerIsFixed:
                    parentAnswerSnapshotA1Builder.WithIsFixed(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithIsFixed(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerIsOpen:
                    parentAnswerSnapshotA1Builder.WithIsOpen(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithIsOpen(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerIsTranslatable:
                    parentAnswerSnapshotA1Builder.WithIsTranslatable(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithIsTranslatable(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerSourceId:
                    parentAnswerSnapshotA1Builder.WithSourceId(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithSourceId(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerSourceName:
                    parentAnswerSnapshotA1Builder.WithSourceName(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithSourceName(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerVersion:
                    parentAnswerSnapshotA1Builder.WithVersion(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithVersion(expectedNewValue);
                    break;
            }

            var parentAnswerSnapshotA1 = parentAnswerSnapshotA1Builder
                .Build();
            var currentAnswerSnapshotA1 = currentAnswerSnapshotA1Builder
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1,
                currentStudyQuestionnaireLineA, parentStudyQuestionnaireLineA,
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
                expectedParentQLSnapshot: currentSnapshotA,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Answer,
                expectedChangeType: KTR_ChangelogType.FieldChangeAnswer,
                expectedFieldChanged: fieldChanged,
                expectedCurrentQlAnswerSnapshot: currentAnswerSnapshotA1,
                expectedParentQlAnswerSnapshot: currentAnswerSnapshotA1,
                expectedOldValue: expectedOldValue,
                expectedNewValue: expectedNewValue,
                expectedModule: null);
        }

        [DataTestMethod]
        [DataRow(KTR_ChangelogFieldChanged.AnswerEffectiveDate)]
        [DataRow(KTR_ChangelogFieldChanged.AnswerEndDate)]
        public void CreateStudyChangelogPostOperation_AnswerFieldChangedDates_ShouldCreateChangelog(KTR_ChangelogFieldChanged fieldChanged)
        {
            var expectedOldValue = DateTime.UtcNow;
            var expectedNewValue = DateTime.UtcNow.AddDays(1);

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

            // Arrange Study QuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentAnswerSnapshotA1Builder = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1);

            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            var currentAnswerSnapshotA1Builder = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1);

            switch (fieldChanged)
            {
                case KTR_ChangelogFieldChanged.AnswerEffectiveDate:
                    parentAnswerSnapshotA1Builder.WithEffectiveDate(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithEffectiveDate(expectedNewValue);
                    break;
                case KTR_ChangelogFieldChanged.AnswerEndDate:
                    parentAnswerSnapshotA1Builder.WithEndDate(expectedOldValue);
                    currentAnswerSnapshotA1Builder.WithEndDate(expectedNewValue);
                    break;
            }

            var parentAnswerSnapshotA1 = parentAnswerSnapshotA1Builder
                .Build();
            var currentAnswerSnapshotA1 = currentAnswerSnapshotA1Builder
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1,
                currentStudyQuestionnaireLineA, parentStudyQuestionnaireLineA,
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
                expectedParentQLSnapshot: currentSnapshotA,
                expectedRelatedObject: KTR_ChangelogRelatedObject.Answer,
                expectedChangeType: KTR_ChangelogType.FieldChangeAnswer,
                expectedFieldChanged: fieldChanged,
                expectedCurrentQlAnswerSnapshot: currentAnswerSnapshotA1,
                expectedParentQlAnswerSnapshot: currentAnswerSnapshotA1,
                expectedOldValue: expectedOldValue.ToString(),
                expectedNewValue: expectedNewValue.ToString(),
                expectedModule: null);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_AnswerReordered_ShouldCreateChangelog()
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
            var qlAnswerA2 = new QuestionnaireLinesAnswerListBuilder(questionnaireLineA)
                .Build();

            // Arrange Study QuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var parentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA1)
                .WithDisplayOrder(1)
                .Build();
            var parentAnswerSnapshotA2 = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshotA, qlAnswerA2)
                .WithDisplayOrder(2)
                .Build();

            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            var currentAnswerSnapshotA1 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA1)
                .WithDisplayOrder(2)
                .Build();
            var currentAnswerSnapshotA2 = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshotA, qlAnswerA2)
                .WithDisplayOrder(1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA, qlAnswerA1, qlAnswerA2,
                currentStudyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                currentAnswerSnapshotA1, parentAnswerSnapshotA1,
                currentAnswerSnapshotA2, parentAnswerSnapshotA2

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
                expectedRelatedObject: KTR_ChangelogRelatedObject.Answer,
                expectedChangeType: KTR_ChangelogType.AnswerReordered,
                expectedFieldChanged: null,
                expectedCurrentQlAnswerSnapshot: currentAnswerSnapshotA2,
                expectedParentQlAnswerSnapshot: parentAnswerSnapshotA2,
                expectedOldValue: "2",
                expectedNewValue: "1",
                expectedModule: null);
        }
    }
}
