using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ManagedList;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.ManagedLists
{
    [TestClass]
    public class ManagedlistDeletionProtectionPreValidationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;
        private KT_Project _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
            _project = new ProjectBuilder().Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Should_ThrowException_When_AssociatedQLineExists()
        {
            // Arrange
            var qline = new QuestionnaireLineBuilder(_project)
                .WithVariableName("Q1")
                .Build();

            var managedList = new ManagedListBuilder(_project)
                .WithName("ML 1")
                .Build();

            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, managedList, qline).Build();

            _context.Initialize(new List<Entity> { _project, managedList, qline, questionnaireLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext);
        }
        [TestMethod]
        public void Should_ThrowException_When_DeactivatingAndAssociatedQLineExists()
        {
            // Arrange
            var qline = new QuestionnaireLineBuilder(_project)
                .WithVariableName("Q1")
                .Build();

            var managedList = new ManagedListBuilder(_project)
                .WithName("ML 1")
                .Build();

            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, managedList, qline).Build();

            _context.Initialize(new List<Entity> { _project, managedList, qline, questionnaireLineManagedList });

            // create plugin context for Update
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);

            // Target entity for update: sets statecode -> Inactive (1)
            var targetEntity = new Entity(KTR_ManagedList.EntityLogicalName) { Id = managedList.Id };
            targetEntity["statecode"] = new OptionSetValue(1); // new state = Inactive

            // Pre-image: previous statecode = Active (0)
            var preImage = new Entity(KTR_ManagedList.EntityLogicalName) { Id = managedList.Id };
            preImage["statecode"] = new OptionSetValue(0); // previous state = Active

            // attach InputParameters and PreEntityImages
            pluginContext.InputParameters = new ParameterCollection
            {
              { "Target", targetEntity }
            };

            pluginContext.PreEntityImages = new EntityImageCollection
            {
             { "PreImage", preImage }
            };

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext)
            );
            var expectedCount = 1;
            var expectedMessage = $"Cannot delete/deactivate this Managed List. It is associated with {expectedCount} Questionnaire Line record(s). Please remove those associations before deleting/deactivating.";

            // Exact-message assertion
            Assert.AreEqual(expectedMessage, ex.Message);
        }

        [TestMethod]
        public void Should_ThrowException_When_MLAlreadyAssociatedToQuestion()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();

            var questionnaireLine1 = new QuestionnaireLineBuilder()
                .WithStatusCode(KT_QuestionnaireLines_StatusCode.Active)
                .Build();
            var inactiveQuestionnaireLine2 = new QuestionnaireLineBuilder()
                .WithStatusCode(KT_QuestionnaireLines_StatusCode.Inactive)
                .Build();

            var questionnaireLineML1 = new QuestionnaireLineManagedListBuilder(_project, managedList, questionnaireLine1)
               .Build();
            var questionnaireLineML2 = new QuestionnaireLineManagedListBuilder(_project, managedList, inactiveQuestionnaireLine2)
                .Build();

            var expectedQuestionnaireList = new List<KT_QuestionnaireLines>
            {
                questionnaireLine1,
                inactiveQuestionnaireLine2
            };

            _context.Initialize(new List<Entity> {
                _project, managedList,
                questionnaireLine1, inactiveQuestionnaireLine2,
                questionnaireLineML1, questionnaireLineML2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList.ToEntityReference() }
            };

            // Act && Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext));

            Assert.AreEqual(
                $"Cannot delete/deactivate this Managed List. It is associated with {expectedQuestionnaireList.Count} Questionnaire Line record(s). Please remove those associations before deleting/deactivating.",
                ex.Message);
        }

        [TestMethod]
        public void Should_AllowDeletion_When_AllStudiesAreDraft()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            var study1 = new StudyBuilder(_project).WithStatusCode(KT_Study_StatusCode.Draft).Build();
            var study2 = new StudyBuilder(_project).WithStatusCode(KT_Study_StatusCode.Draft).Build();

            _context.Initialize(new List<Entity> { _project, managedList, study1, study2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext);

            // Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_ThrowException_When_MLHasBeenSnapshoted()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project)
                .WithEverInSnapshot(true)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList.ToEntityReference() }
            };

            // Act && Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext));

            Assert.AreEqual("Cannot delete this Managed List because it's already present in Snapshot.", ex.Message);
        }

        [TestMethod]
        public void Should_AllowDeletion_When_NoAssociations_And_NoBlockingStudyStatus()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            var study = new StudyBuilder(_project)
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList, study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<ManagedlistDeletionProtectionPreValidation>(pluginContext);

            // Assert
            Assert.IsTrue(true);
        }
    }
}
