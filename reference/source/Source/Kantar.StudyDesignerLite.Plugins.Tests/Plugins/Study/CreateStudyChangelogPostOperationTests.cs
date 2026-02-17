namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    using System.Collections.Generic;
    using System.Linq;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.Study;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Moq;

    [TestClass]
    public partial class CreateStudyChangelogPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        private void AssertChangeLogs(
            List<KTR_StudySnapshotLineChangelog> changeLogRows,
            int expectedCount,
            KT_Study expectedCurrentStudy,
            KT_Study expectedParentStudy,
            KTR_StudyQuestionnaireLineSnapshot expectedCurrentQLSnapshot,
            KTR_StudyQuestionnaireLineSnapshot expectedParentQLSnapshot,
            KTR_ChangelogRelatedObject expectedRelatedObject,
            KTR_ChangelogType expectedChangeType,
            KTR_ChangelogFieldChanged? expectedFieldChanged,
            KTR_StudyQuestionAnswerListSnapshot expectedCurrentQlAnswerSnapshot,
            KTR_StudyQuestionAnswerListSnapshot expectedParentQlAnswerSnapshot,
            string expectedOldValue,
            string expectedNewValue,
            KT_Module expectedModule)
        {
            Assert.IsTrue(changeLogRows.Count == expectedCount);
            Assert.IsTrue(expectedCurrentStudy != null
                ? changeLogRows.All(s => s.KTR_CurrentStudy.Id == expectedCurrentStudy.Id)
                : changeLogRows.All(s => s.KTR_CurrentStudy == null));
            Assert.IsTrue(expectedParentStudy != null
                ? changeLogRows.All(s => s.KTR_FormerStudy.Id == expectedParentStudy.Id)
                : changeLogRows.All(s => s.KTR_FormerStudy == null));
            Assert.IsTrue(expectedCurrentQLSnapshot != null
                ? changeLogRows.All(s => s.KTR_CurrentStudyQuestionnaireSnapshotLine.Id == expectedCurrentQLSnapshot.Id)
                : changeLogRows.All(s => s.KTR_CurrentStudyQuestionnaireSnapshotLine == null));
            Assert.IsTrue(expectedParentQLSnapshot != null
                ? changeLogRows.All(s => s.KTR_FormerStudyQuestionnaireSnapshotLine.Id == expectedParentQLSnapshot.Id)
                : changeLogRows.All(s => s.KTR_FormerStudyQuestionnaireSnapshotLine == null));
            Assert.IsTrue(changeLogRows.All(s => s.KTR_RelatedObject == expectedRelatedObject));
            Assert.IsTrue(changeLogRows.All(s => s.KTR_Change == expectedChangeType));
            Assert.IsTrue(changeLogRows.All(s => s.KTR_FieldChanged == expectedFieldChanged));
            Assert.IsTrue(expectedModule != null
                ? changeLogRows.All(s => s.KTR_Module2 != null && s.KTR_Module2.Id == expectedModule.Id)
                : changeLogRows.All(s => s.KTR_Module2 == null));
            Assert.IsTrue(expectedCurrentQlAnswerSnapshot != null
                ? changeLogRows.All(s => s.KTR_CurrentStudyQuestionAnswerListSnapshot.Id == expectedCurrentQlAnswerSnapshot.Id)
                : changeLogRows.All(s => s.KTR_CurrentStudyQuestionAnswerListSnapshot == null));
            Assert.IsTrue(expectedParentQlAnswerSnapshot != null
                ? changeLogRows.All(s => s.KTR_FormerStudyQuestionAnswerListSnapshot.Id == expectedParentQlAnswerSnapshot.Id)
                : changeLogRows.All(s => s.KTR_FormerStudyQuestionAnswerListSnapshot == null));
            if (expectedChangeType == KTR_ChangelogType.FieldChangeAnswer
                && (expectedFieldChanged == KTR_ChangelogFieldChanged.AnswerEndDate
                || expectedFieldChanged == KTR_ChangelogFieldChanged.AnswerEffectiveDate))
            {
                Assert.IsTrue(changeLogRows.All(s => s.KTR_OldValue2 == expectedOldValue));
                Assert.IsTrue(changeLogRows.All(s => s.KTR_NewValue2 == expectedNewValue));
            }
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_NoChanges_ShouldNotCreateChangelog()
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

            // Arrange KTR_StudyQuestionnaireLine
            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();
            var parentStudyQuestionnaireLine = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();

            // Arrange Snapshot
            var currentSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();
            var parentSnapshot = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();

            // Arrange Snapshot answers
            var currentAnswerSnapshot = new StudyQuestionnaireLineAnswerSnapshotBuilder(currentSnapshot, qlAnswerA1)
                .Build();
            var parentAnswerSnapshot = new StudyQuestionnaireLineAnswerSnapshotBuilder(parentSnapshot, qlAnswerA1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
                {
                    study, parentStudy,
                    questionnaireLineA, qlAnswerA1,
                    studyQuestionnaireLine, parentStudyQuestionnaireLine,
                    currentSnapshot, parentSnapshot,
                    currentAnswerSnapshot, parentAnswerSnapshot });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(changeLogRows.Count == 0);
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_NoSnapshotCreated_ShouldNotCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            study.KTR_IsSnapshotCreated = false;

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { study });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var snapshotEntities = _context.CreateQuery<KTR_StudyQuestionnaireLineSnapshot>().ToList();
            Assert.IsTrue(snapshotEntities.Count == 0, "Expected no snapshots to be created.");
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_NoParentStudy_ShouldNotCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { study });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var snapshotEntities = _context.CreateQuery<KTR_StudyQuestionnaireLineSnapshot>().ToList();
            Assert.IsTrue(snapshotEntities.Count == 0, "Expected no snapshots to be created.");
        }
    }
}
