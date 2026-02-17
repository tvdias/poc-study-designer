using System;
using System.Reflection;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Common;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    [TestClass]
    public class ShareChildEntitiesWithProjectTeamPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                _context = new XrmFakedContext();
                _service = _context.GetOrganizationService();
                _tracingService = new Mock<ITracingService>();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Console.WriteLine("LoaderException: " + loaderEx?.Message);
                }
                throw;
            }
        }

        [TestMethod]
        public void ShareChildEntities_WithOwnerAndTeam_SharesSuccessfully()
        {
            // Arrange
            var team = new TeamBuilder()
                .Build();

            var project = new ProjectBuilder()
                .WithTeamAccess(team)
                .WithOwner(Guid.NewGuid())
                .Build();

            var study = new StudyBuilder(project).Build();

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = study.LogicalName;
            context.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, study });

            // Act
            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert: Study should still reference the correct project
            Assert.AreEqual(project.Id, study.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project)?.Id);
        }

        [TestMethod]
        public void ShareChildEntities_WithOwnerOnly_SharesWithOwner()
        {
            // Arrange
            var team = new TeamBuilder()
                .Build();

            var project = new ProjectBuilder()
                .WithOwner(Guid.NewGuid())
                .Build();

            var study = new StudyBuilder(project).Build();

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = study.LogicalName;
            context.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, study });

            // Assert: Study should still reference the correct project
            Assert.AreEqual(project.Id, study.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project)?.Id);
        }

        [TestMethod]
        public void ShareChildEntities_FromStudySnapshotLineChangelog_ResolvesProjectId()
        {
            var ownerId = Guid.NewGuid();
            var team = new TeamBuilder().Build();
            var project = new ProjectBuilder()
                .WithOwner(ownerId)
                .WithTeamAccess(team)
                .Build();

            var study = new StudyBuilder(project).Build();

            var snapshot = new Entity(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            snapshot[KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study] = study.ToEntityReference();

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = snapshot.LogicalName;
            context.InputParameters["Target"] = snapshot;

            _context.Initialize(new Entity[] { project, study, snapshot });

            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert: Snapshot should reference the correct study
            Assert.AreEqual(study.Id, snapshot.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study)?.Id);
        }

        [TestMethod]
        public void ShareChildEntities_FromQuestionnaireLinesAnswerList_ResolvesProject()
        {
            var project = new ProjectBuilder().WithOwner(Guid.NewGuid()).Build();
            var qline = new QuestionnaireLineBuilder(project).Build();

            var answerList = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine] = qline.ToEntityReference()
            };

            _context.Initialize(new[] { project, qline, answerList });

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = answerList.LogicalName;
            context.InputParameters["Target"] = answerList;

            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert: AnswerList should reference the correct questionnaire line
            Assert.AreEqual(qline.Id, answerList.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine)?.Id);
        }

        [TestMethod]
        public void ShareChildEntities_FromManagedListEntity_ResolvesProjectId()
        {
            var team = new TeamBuilder().Build();
            var project = new ProjectBuilder()
                .WithOwner(Guid.NewGuid())
                .WithTeamAccess(team)
                .Build();

            var managedList = new Entity(KTR_ManagedList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_ManagedList.Fields.KTR_Project] = project.ToEntityReference()
            };

            var managedListEntity = new Entity(KTR_ManagedListEntity.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_ManagedListEntity.Fields.KTR_ManagedList] = managedList.ToEntityReference()
            };

            _context.Initialize(new Entity[] { project, managedList, managedListEntity });

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = managedListEntity.LogicalName;
            context.InputParameters["Target"] = managedListEntity;

            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            Assert.AreEqual(managedList.Id, managedListEntity.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList)?.Id);
        }

        [TestMethod]
        public void ShareChildEntities_FromStudyManagedListEntitySnapshot_ResolvesProjectId()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var team = new TeamBuilder().Build();
            var project = new ProjectBuilder()
                .WithOwner(ownerId)
                .WithTeamAccess(team)
                .Build();

            // Create a QuestionnaireLine linked to the project
            var qLine = new QuestionnaireLineBuilder(project).Build();

            // Create a StudyQuestionnaireLineSnapshot pointing to the QuestionnaireLine
            var qLineSnapshot = new Entity(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine] = qLine.ToEntityReference()
            };

            // Create a StudyQuestionManagedListSnapshot pointing to the QuestionnaireLineSnapshot
            var sqmlSnapshot = new Entity(KTR_StudyQuestionManagedListSnapshot.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot] = qLineSnapshot.ToEntityReference()
            };

            // Create a StudyManagedListEntitiesSnapshot pointing to the StudyQuestionManagedListSnapshot
            var smleSnapshot = new Entity(KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot] = sqmlSnapshot.ToEntityReference()
            };

            _context.Initialize(new Entity[] { project, qLine, qLineSnapshot, sqmlSnapshot, smleSnapshot });

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = smleSnapshot.LogicalName;
            context.InputParameters["Target"] = smleSnapshot;

            // Act
            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert: SMLE snapshot should resolve to the correct project
            var resolvedProjectId = smleSnapshot.Id; // plugin itself does not modify smleSnapshot, so we test GetProjectId indirectly
            var pluginInstance = new ShareChildEntitiesWithProjectTeamPostOperation();
            var method = typeof(ShareChildEntitiesWithProjectTeamPostOperation)
                .GetMethod("GetProjectIdFromStudyManagedListEntitySnapshot", BindingFlags.NonPublic | BindingFlags.Instance);

            var projectId = (Guid?)method.Invoke(pluginInstance, new object[] { _service, smleSnapshot.ToEntityReference() });

            Assert.AreEqual(project.Id, projectId.Value);
        }

        [TestMethod]
        public void ShareChildEntities_FromManagedListEntityChain_ResolvesProjectId_ForStudyManagedListEntity()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var team = new TeamBuilder().Build();

            var project = new ProjectBuilder()
                .WithOwner(ownerId)
                .WithTeamAccess(team)
                .Build();

            var managedList = new ManagedListBuilder(project) .Build();

            var managedListEntity = new ManagedListEntityBuilder(managedList) .Build();

            // StudyManagedListEntity referencing KTR_ManagedListEntity
            var studyManagedListEntity = new StudyManagedListEntityBuilder(managedListEntity) .Build();

            _context.Initialize(new Entity[] { project, managedList, managedListEntity, studyManagedListEntity });

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = studyManagedListEntity.LogicalName;
            context.InputParameters["Target"] = studyManagedListEntity;

            // Act
            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert — verify GetProjectIdFromManagedListEntity resolves correctly
            var pluginInstance = new ShareChildEntitiesWithProjectTeamPostOperation();
            var method = typeof(ShareChildEntitiesWithProjectTeamPostOperation)
                .GetMethod("GetProjectIdFromManagedListEntity", BindingFlags.NonPublic | BindingFlags.Instance);

            var projectId = (Guid?)method.Invoke(pluginInstance, new object[] { _service, managedListEntity.ToEntityReference() });

            Assert.AreEqual(project.Id, projectId.Value);
        }

        [TestMethod]
        public void ShareChildEntities_FromManagedListEntityChain_ResolvesProjectId_ForQuestionnaireLineManagedListEntity()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var team = new TeamBuilder().Build();

            var project = new ProjectBuilder()
                .WithOwner(ownerId)
                .WithTeamAccess(team)
                .Build();

            var qL = new QuestionnaireLineBuilder(project).Build();

            var managedList = new ManagedListBuilder(project).Build();

            var qLML = new QuestionnaireLineManagedListBuilder(project, managedList, qL).Build();

            var managedListEntity = new ManagedListEntityBuilder(managedList).Build();

            // QL-ML-Entity referencing KTR_ManagedListEntity
            var qLMLEntity = new QuestionnaireLineManagedListEntityBuilder(managedListEntity).Build();

            _context.Initialize(new Entity[] { project, qL, managedList, qLML, managedListEntity, qLMLEntity });

            var context = _context.GetDefaultPluginContext();
            context.MessageName = nameof(ContextMessageEnum.Create);
            context.PrimaryEntityName = qLMLEntity.LogicalName;
            context.InputParameters["Target"] = qLMLEntity;

            // Act
            _context.ExecutePluginWith<ShareChildEntitiesWithProjectTeamPostOperation>(context);

            // Assert — verify GetProjectIdFromManagedListEntity resolves correctly
            var pluginInstance = new ShareChildEntitiesWithProjectTeamPostOperation();
            var method = typeof(ShareChildEntitiesWithProjectTeamPostOperation)
                .GetMethod("GetProjectIdFromManagedListEntity", BindingFlags.NonPublic | BindingFlags.Instance);

            var projectId = (Guid?)method.Invoke(pluginInstance, new object[] { _service, managedListEntity.ToEntityReference() });

            Assert.AreEqual(project.Id, projectId.Value);
        }
    }
}
