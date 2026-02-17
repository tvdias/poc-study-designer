using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tags;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class UpdateStudyStatusCodePreOperationTest
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

        [TestMethod]
        public void UpdateStudyPreOperation_TeamNotAssigned_ThrowsException()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Team
            var team = new TeamBuilder()
                .WithName(project.Id.ToString())
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            // Arrange SystemUser
            var systemUser = new SystemUserBuilder()
                .WithKantarScripterRoleProfile()
                .Build();

            // Arrange KTR_Project Team
            var projectTeamMem = new TeamMembershipBuilder()
                .WithTeamMember(team,systemUser)
                .Build();
            //projectTeamMem.KTR_TeamAccess = new EntityReference(Team.EntityLogicalName, team.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, team, study, projectTeamMem });

            // Act
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() => _context.ExecutePluginWith<UpdateStudyStatusCodePreOperation>(pluginContext));

            // Assert
            Assert.AreEqual("No Scripter exists in Project’s Access Team, the Study cannot be updated as Ready for Scripting.", exception.Message.ToString());
        }

        [TestMethod]
        public void UpdateStudyPreOperation_TeamAssigned_RunsSuccessfully()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Team with the name equal to project Id
            var team = new TeamBuilder()
                .WithName(project.Id.ToString())
                .Build();

            // Arrange SystemUser with Scripter role
            var systemUser = new SystemUserBuilder()
                .WithKantarScripterRoleProfile()
                .Build();

            // Arrange Study (linked to Project)
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .Build();

            // Arrange TeamMembership (SystemUser assigned to Team)
            var teamMembership = new TeamMembershipBuilder()
                .WithTeamMember(team, systemUser)
                .Build();

            // Set up Plugin Context
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PrimaryEntityName = study.LogicalName;

            // Simulate update by passing minimal Target with Id and updated statuscode
            var target = new KT_Study(study.Id);
            target.StatusCode = KT_Study_StatusCode.ReadyForScripting;

            pluginContext.InputParameters["Target"] = target;

            // Initialize all involved entities
            _context.Initialize(new Entity[] { project, team, systemUser, study, teamMembership });

            // Act
            _context.ExecutePluginWith<UpdateStudyStatusCodePreOperation>(pluginContext);

            // Assert: Ensure the status code is set to ReadyForScripting
            Assert.AreEqual(KT_Study_StatusCode.ReadyForScripting, target.StatusCode);
        }

        [TestMethod]
        public void UpdateStudyPreOperation_StatusCodeDraft_NoValidationMade()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Team
            var team = new TeamBuilder()
                .WithName(project.Id.ToString())
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            // Arrange SystemUser
            var systemUser = new SystemUserBuilder()
                .WithKantarScripterRoleProfile()
                .Build();

            // Arrange KTR_Project Team
            var projectTeamMem = new TeamMembershipBuilder()
                .WithTeamMember(team, systemUser)
                .Build();
            //projectTeamMem.KTR_TeamAccess = new EntityReference(Team.EntityLogicalName, team.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, team, study, projectTeamMem });

            // Act
            _context.ExecutePluginWith<UpdateStudyStatusCodePreOperation>(pluginContext);

            // Assert: Ensure the status code is still Draft
            Assert.AreEqual(KT_Study_StatusCode.Draft, study.StatusCode);
        }
    }
}
