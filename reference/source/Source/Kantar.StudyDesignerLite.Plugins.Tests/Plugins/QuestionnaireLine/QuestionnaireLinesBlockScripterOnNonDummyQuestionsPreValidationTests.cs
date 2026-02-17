using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Setup()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }
        [TestMethod]
        public void ScripterOnly_Dummy_ShouldAllow()
        {
            var userId = Guid.NewGuid();

            var project = new ProjectBuilder().Build();

            var user = new SystemUserBuilder().Build();
            var userRole = new SystemUserBuilder().WithKantarScripterRoleProfile().Build();
            _context.Initialize(new List<Entity> { user, userRole });

            var plugin = new QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation();
            var target = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(true).Build();
            var preImage = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(true).Build();
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 10;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InitiatingUserId = userId;

            _context.ExecutePluginWith(pluginContext, plugin);
        }

        [TestMethod]
        public void UserWithMultipleRoles_ShouldAllowNonDummy()
        {
            var userId = Guid.NewGuid();

            var project = new ProjectBuilder().Build();
            var user = new SystemUserBuilder().Build();
            var userRole1 = new SystemUserBuilder().WithKantarScripterRoleProfile().Build();
            var userRole2 = new SystemUserBuilder().WithKantarCSUserRoleProfile().Build(); ; // extra role

            _context.Initialize(new List<Entity> { user, userRole1, userRole2 });

            var plugin = new QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation();
            var target = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(true).Build();
            var preImage = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(true).Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 10;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InitiatingUserId = userId;

            _context.ExecutePluginWith(pluginContext, plugin);
        }

        [TestMethod]
        public void NonScripter_ShouldAllowNonDummy()
        {
            var userId = Guid.NewGuid();

            var project = new ProjectBuilder().Build();

            var user = new SystemUserBuilder().Build();
            var otherRole = new SystemUserBuilder().WithKantarCSUserRoleProfile().Build(); ; // other role

            // Initialize once with user + role + userRole
            _context.Initialize(new List<Entity> { user, otherRole });

            var plugin = new QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation();
            var target = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(false).Build();
            var preImage = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(false).Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 10;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InitiatingUserId = userId;

            _context.ExecutePluginWith(pluginContext, plugin);
        }
        [TestMethod]
        public void Execute_ShouldAllowUpdate_WhenSortOrderIsChangedByScripter()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // User with Scripter role only
            var scripterUser = new SystemUserBuilder()
                .WithKantarScripterRoleProfile()
                .Build();

            // Non-dummy questionnaire line
            var preImage = new QuestionnaireLineBuilder(project)
                .WithIsDummyQuestion(false)
                .WithSortOrder(1)
                .Build();

            // Target has updated sort order
            var target = new QuestionnaireLineBuilder(project)
                .WithId(preImage.Id)
                .WithSortOrder(2) // sort order changed
                .Build();

            _context.Initialize(new List<Entity> { scripterUser });

            var plugin = new QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 10;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.InitiatingUserId = scripterUser.Id;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act & Assert → Should NOT throw exception
            _context.ExecutePluginWith(pluginContext, plugin);
        }

        [TestMethod]
        public void PreImageMissing_ShouldAllow()
        {
            var userId = Guid.NewGuid();

            var project = new ProjectBuilder().Build();
            var user = new SystemUserBuilder().Build();
            var userRole = new SystemUserBuilder().WithKantarScripterRoleProfile().Build();

            _context.Initialize(new List<Entity> { user, userRole });

            var plugin = new QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation();
            var target = new QuestionnaireLineBuilder(project).WithIsDummyQuestion(false).Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.Stage = 10;
            pluginContext.InputParameters["Target"] = target;
            pluginContext.InitiatingUserId = userId;

            _context.ExecutePluginWith(pluginContext, plugin);
        }
    }
}
