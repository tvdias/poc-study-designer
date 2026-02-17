using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Project;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Team = Microsoft.Xrm.Sdk.Entity;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project
{
    [TestClass]
    public class CreateOrDeleteTeamProjectPostOperationTests
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
        public void WhenAccessTeamTrue_ShouldCreateTeamAndGrantAccess()
        {
            var project = new ProjectBuilder()
                .WithAccessTeam(true)
                .Build();

            _context.Initialize(new List<Entity> { project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName)
            {
                Id = project.Id
            };
            pluginContext.PostEntityImages["Image"] = project;

            _context.ExecutePluginWith<CreateOrDeleteTeamProjectPostOperation>(pluginContext);

            var createdTeams = _context.CreateQuery("team").ToList();
            var updatedProject = _context.CreateQuery(KT_Project.EntityLogicalName).FirstOrDefault();

            Assert.AreEqual(1, createdTeams.Count);
            Assert.IsTrue(updatedProject.Contains(KT_Project.Fields.KTR_TeamAccess));
        }

        [TestMethod]
        public void WhenAccessTeamIsFalseAndNoExistingTeam_ShouldDoNothing()
        {
            var projectPre = new ProjectBuilder().WithAccessTeam(false).Build();
            var projectPost = new ProjectBuilder().WithAccessTeam(false).Build();
            projectPost.Id = projectPre.Id;

            _context.Initialize(new List<Entity> { projectPre });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName) { Id = projectPre.Id };
            pluginContext.PreEntityImages["Image"] = projectPre;
            pluginContext.PostEntityImages["Image"] = projectPost;

            _context.ExecutePluginWith<CreateOrDeleteTeamProjectPostOperation>(pluginContext);

            var teams = _context.CreateQuery("team").ToList();
            Assert.AreEqual(0, teams.Count);
        }
    }
}
