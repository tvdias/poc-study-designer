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
        public void CreateStudyChangelogPostOperation_AddedManagedListEntity_ShouldCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder().Build();

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
            var questionnaireLineA = new QuestionnaireLineBuilder(project).Build();

            // Arrange KTR_StudyQuestionnaireLine
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA).Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA).Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA).Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA).Build();

            // Arrange Managed List Snapshots with different location values
            var parentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(parentSnapshotA)
                .Build();

            var currentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(currentSnapshotA)
                .Build();

            // Arrange Managed List Entity Snapshot (only in current study → Added)
            var managedListEntity = new KTR_QuestionnaireLinemanAgedListEntity(Guid.NewGuid());

            var currentManagedListEntitySnapshot = new StudyManagedListEntitySnapshotBuilder(currentSnapshotA)
                .WithStudyQuestionManagedListSnapshot(currentManagedListSnapshot)
                .WithManagedListEntity(new KTR_ManagedListEntity { Id = Guid.NewGuid() })
                .WithQuestionnaireLineManagedListEntity(managedListEntity)
                .WithName("Answer A")
                .WithDisplayOrder(1)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,parentManagedListSnapshot,
                currentManagedListSnapshot,
                currentManagedListEntitySnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedListEntity &&
                         x.KTR_Change == KTR_ChangelogType.ManagedListEntityAdded),
                "Expected changelog for added managed list entity.");
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_RemovedManagedListEntity_ShouldCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder().Build();

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
            var questionnaireLineA = new QuestionnaireLineBuilder(project).Build();

            // Arrange StudyQuestionnaireLines
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA).Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA).Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA).Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA).Build();

            // Arrange QML snapshot only in parent (so it’s considered removed in current study)
            var parentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(parentSnapshotA).Build();

            // Arrange Managed List Entity Snapshot associated with parent QML snapshot
            var managedListEntity = new KTR_QuestionnaireLinemanAgedListEntity(Guid.NewGuid());
            var parentManagedListEntitySnapshot = new StudyManagedListEntitySnapshotBuilder(parentSnapshotA)
                .WithStudyQuestionManagedListSnapshot(parentManagedListSnapshot)
                .WithManagedListEntity(new KTR_ManagedListEntity { Id = Guid.NewGuid() })
                .WithQuestionnaireLineManagedListEntity(managedListEntity)
                .WithName("Answer A")
                .WithDisplayOrder(1)
                .Build();

            // Plugin context
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            // Initialize context
            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                parentManagedListSnapshot,
                parentManagedListEntitySnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedListEntity &&
                         x.KTR_Change == KTR_ChangelogType.ManagedListEntityRemoved),
                "Expected changelog for removed managed list entity.");
        }

        [TestMethod]
        public void CreateStudyChangelogPostOperation_ModifiedManagedListEntity_ShouldCreateChangelog()
        {
            // Arrange Project
            var project = new ProjectBuilder().Build();

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
            var questionnaireLineA = new QuestionnaireLineBuilder(project).Build();

            // Arrange StudyQuestionnaireLines
            var parentStudyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(parentStudy, questionnaireLineA).Build();
            var studyQuestionnaireLineA = new StudyQuestionnaireLineBuilder(study, questionnaireLineA).Build();

            // Arrange Snapshots
            var parentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(parentStudy, questionnaireLineA).Build();
            var currentSnapshotA = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLineA).Build();

            // Arrange QML snapshots
            var parentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(parentSnapshotA).Build();
            var currentManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(currentSnapshotA).Build();

            // Arrange Managed List Entity Snapshots with same Id but different names (modified)
            var qLmManagedListEntity = new KTR_QuestionnaireLinemanAgedListEntity(Guid.NewGuid());
            var managedListEntity = new KTR_ManagedListEntity(Guid.NewGuid());
            var parentManagedListEntitySnapshot = new StudyManagedListEntitySnapshotBuilder(parentSnapshotA)
                .WithStudyQuestionManagedListSnapshot(parentManagedListSnapshot)
                .WithManagedListEntity(managedListEntity)
                .WithQuestionnaireLineManagedListEntity(qLmManagedListEntity)
                .WithName("Old Answer")
                .WithDisplayOrder(1)
                .Build();

            var currentManagedListEntitySnapshot = new StudyManagedListEntitySnapshotBuilder(currentSnapshotA)
                .WithStudyQuestionManagedListSnapshot(currentManagedListSnapshot)
                .WithManagedListEntity(managedListEntity)
                .WithQuestionnaireLineManagedListEntity(qLmManagedListEntity)
                .WithName("New Answer")   // <-- changed name triggers modification
                .WithDisplayOrder(1)
                .Build();

            // Plugin context
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            // Initialize context
            _context.Initialize(new Entity[]
            {
                study, parentStudy,
                questionnaireLineA,
                studyQuestionnaireLineA, parentStudyQuestionnaireLineA,
                currentSnapshotA, parentSnapshotA,
                parentManagedListSnapshot, currentManagedListSnapshot,
                parentManagedListEntitySnapshot, currentManagedListEntitySnapshot
            });

            // Act
            _context.ExecutePluginWith<CreateStudyChangelogPostOperation>(pluginContext);

            // Assert
            var changeLogRows = _context.CreateQuery<KTR_StudySnapshotLineChangelog>().ToList();
            Assert.IsTrue(
                changeLogRows.Any(
                    x => x.KTR_RelatedObject == KTR_ChangelogRelatedObject.ManagedListEntity &&
                         x.KTR_Change == KTR_ChangelogType.FieldChangeManagedListEntity),
                "Expected changelog for modified managed list entity.");
        }
    }
}
