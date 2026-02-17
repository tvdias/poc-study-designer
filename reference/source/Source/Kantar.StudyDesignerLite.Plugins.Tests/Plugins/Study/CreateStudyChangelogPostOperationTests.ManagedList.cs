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
        public void CreateStudyChangelogPostOperation_AddedManagedList_ShouldCreateChangelog()
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

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Managed List Snapshots
            var currentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(currentSnapshotA)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                currentManagedListSnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedList &&
                         x.KTR_Change == KTR_ChangelogType.ManagedListAdded),
                "Expected changelog for added managed list.");
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_RemovedManagedList_ShouldCreateChangelog()
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

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Managed List Snapshots
            var parentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(parentSnapshotA)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                parentManagedListSnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedList &&
                         x.KTR_Change == KTR_ChangelogType.ManagedListRemoved),
                "Expected changelog for removed managed list.");
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_ModifiedManagedList_ShouldCreateChangelog()
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

            // Arrange KTR_ManagedList
            var managedListId = Guid.NewGuid();
            var managedList = new KTR_ManagedList(managedListId);

            // Arrange KTR_QuestionnaireLineManagedList using builder
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, questionnaireLineA)
                .WithId(Guid.NewGuid())
                .Build();

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA)
                .Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA)
                .Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA)
                .Build();

            // Arrange Managed List Snapshots with different location values
            var parentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(parentSnapshotA)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .WithLocation(KTR_Location.Row)
                .Build();

            var currentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(currentSnapshotA)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .WithLocation(KTR_Location.Column)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                questionnaireLineManagedList,
                currentManagedListSnapshot, parentManagedListSnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedList &&
                         x.KTR_Change == KTR_ChangelogType.FieldChangeManagedList),
                "Expected changelog for modified managed list.");
        }
    }
}
