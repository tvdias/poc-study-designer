namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Services.Project
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.Plugins.Tests.TestHelpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Project;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Moq;

    [TestClass]
    public class ProjectServiceTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingMock;
        private ProjectService _projectService;

        [TestInitialize]
        public void Setup()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingMock = new Mock<ITracingService>();

            _projectService = new ProjectService(
                _service,
                _tracingMock.Object);
        }

        [TestMethod]
        public void ReorderProjectQuestionnaire_ShouldReturnEmpty_WhenNoQuestionnaireLinesFound()
        {
            // Arrange
            var project = new ProjectBuilder()
                .WithName("Project 123")
                .Build();

            // Act + Assert
            var ids = _projectService.ReorderProjectQuestionnaire(project.Id);

            Assert.IsTrue(ids.Count == 0);
        }

        [TestMethod]
        public void ReorderProjectQuestionnaire_ShouldReturnTrue_WhenReorderSucceeds()
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

            var pluginContext = _context.Mock(entities);

            // Act
            var ids = _projectService.ReorderProjectQuestionnaire(project.Id);

            // Assert
            Assert.AreEqual(3, ids.Count());
        }

        [TestMethod]
        public void ReorderProjectQuestionnaire_InBetweenRows_ShouldReturnTrue_WhenReorderSucceeds()
        {
            // Arrange
            var project = new ProjectBuilder()
                 .WithName("Project 123")
                 .Build();

            var dateTimeYesterday = DateTime.UtcNow.AddDays(-1);
            var dateTimeNow = DateTime.UtcNow;
            var questionnaireLine1 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithCreatedOn(dateTimeYesterday)
                .Build();
            var questionnaireLineNew = new QuestionnaireLineBuilder(project)
                .WithSortOrder(1)
                .WithCreatedOn(dateTimeNow)
                .Build();
            var questionnaireLine2 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(1)
                .WithCreatedOn(dateTimeYesterday)
                .Build();
            var questionnaireLine3 = new QuestionnaireLineBuilder(project)
                .WithSortOrder(2)
                .WithCreatedOn(dateTimeYesterday)
                .Build();

            var expectedResultIds = new List<Guid>
            {
                questionnaireLine1.Id,
                questionnaireLine2.Id,
                questionnaireLineNew.Id,
                questionnaireLine3.Id
            };

            var entities = new List<Entity>
            {
                project,
                questionnaireLine1, questionnaireLine2, questionnaireLineNew, questionnaireLine3
            };

            var pluginContext = _context.Mock(entities);

            // Act
            var orderedIds = _projectService.ReorderProjectQuestionnaire(project.Id);

            // Assert
            Assert.AreEqual(4, orderedIds.Count());

            var areEqual = expectedResultIds.SequenceEqual(orderedIds);
            Assert.IsTrue(areEqual);
        }
    }
}

