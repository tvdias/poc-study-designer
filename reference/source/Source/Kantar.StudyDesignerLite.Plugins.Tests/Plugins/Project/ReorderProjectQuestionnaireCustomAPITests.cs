namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project
{
    using System;
    using System.Collections.Generic;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.Project;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.Plugins.Tests.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Moq;

    [TestClass]
    public class ReorderProjectQuestionnaireCustomAPITests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private readonly string _customAPIName = "ktr_reorder_project_questionnaire_unbound";
        private readonly string _paramProjectId = "projectId";

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void ExecuteCdsPlugin_ShouldSucceed_WhenNoQuestionnaireLinesFound()
        {
            // Arrange
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .Build();

            var entities = new List<Entity>
            {
                project
            };

            var pluginContext = MockPluginContext(
                project.Id,
                entities);

            // Act + Assert
            _context.ExecutePluginWith<ReorderProjectQuestionnaireCustomAPI>(pluginContext);
        }

        [TestMethod]
        public void ExecuteCdsPlugin_ShouldSucceed_WhenQuestionnaireLinesExist()
        {
            // Arrange
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .Build();

            var questionnaireLine1 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(2)
                .Build();
            var questionnaireLine2 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(4)
                .Build();
            var questionnaireLine3 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(7)
                .Build();

            var entities = new List<Entity>
            {
                project,
                questionnaireLine1, questionnaireLine2, questionnaireLine3
            };

            var pluginContext = MockPluginContext(project.Id, entities);

            // Act
            _context.ExecutePluginWith<ReorderProjectQuestionnaireCustomAPI>(pluginContext);
        }

        private XrmFakedPluginExecutionContext MockPluginContext(
            Guid projectId,
            List<Entity> entities)
        {
            _context.Initialize(entities);

            _context.AddExecutionMock<ExecuteMultipleRequest>(req =>
            {
                var response = new ExecuteMultipleResponse
                {
                    ["Responses"] = new ExecuteMultipleResponseItemCollection(),
                    ["IsFaulted"] = false
                };
                return response;
            });

            return PluginContextFactory.Create(
                _customAPIName,
                new Dictionary<string, object>
                {
                    { _paramProjectId, projectId }
                },
                new Dictionary<string, object>
                {
                }
            );
        }
    }
}
