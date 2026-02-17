namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.StudyManagedListEntity
{
    using System;
    using System.Collections.Generic;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.StudyManagedListEntity;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Newtonsoft.Json;

    [TestClass]
    public class ValidateStudyTemplateCustomAPITests
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
        public void Execute_WithValidStudyManagedListEntities_ShouldReturnGroupedStudyIds()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study1 = new StudyBuilder(project).Build();
            var study2 = new StudyBuilder(project).Build();

            var ml = new ManagedListBuilder(project).Build();
            var mle1 = new ManagedListEntityBuilder(ml).Build();
            var mle2 = new ManagedListEntityBuilder(ml).Build();

            var smle1 = new StudyManagedListEntityBuilder(mle1)
                .WithStudy(study1)
                .Build();

            var smle2 = new StudyManagedListEntityBuilder(mle2)
                .WithStudy(study1)
                .Build();

            var smle3 = new StudyManagedListEntityBuilder(mle1)
                .WithStudy(study2)
                .Build();

            _context.Initialize(new List<Entity> { study1, study2, smle1, smle2, smle3 });

            var inputIds = new List<Guid> { smle1.Id, smle2.Id, smle3.Id };
            var inputJson = JsonConvert.SerializeObject(inputIds);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "ktr_validate_study_template";
            pluginContext.InputParameters = new ParameterCollection
            {
                ["StudyManagedListEntityIds"] = inputJson
            };

            // Act
            _context.ExecutePluginWith<ValidateStudyTemplateCustomAPI>(pluginContext);

            // Assert
            Assert.IsTrue(pluginContext.OutputParameters.ContainsKey("StudyIds"), "Output parameter 'StudyIds' should exist.");

            var resultJson = (string)pluginContext.OutputParameters["StudyIds"];
            var resultIds = JsonConvert.DeserializeObject<List<Guid>>(resultJson);

            Assert.AreEqual(2, resultIds.Count, "Should return 2 unique study IDs.");
            CollectionAssert.Contains(resultIds, study1.Id);
            CollectionAssert.Contains(resultIds, study2.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_MissingInputParameter_ShouldThrow()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "ktr_validate_study_template";
            pluginContext.InputParameters = new ParameterCollection();

            _context.ExecutePluginWith<ValidateStudyTemplateCustomAPI>(pluginContext);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_InvalidJson_ShouldThrow()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.InputParameters = new ParameterCollection
            {
                ["StudyManagedListEntityIds"] = "NotJson"
            };

            _context.ExecutePluginWith<ValidateStudyTemplateCustomAPI>(pluginContext);
        }

        [TestMethod]
        public void Execute_StudyMissing_ShouldSkipWithoutError()
        {
            var project = new ProjectBuilder().Build();

            var ml = new ManagedListBuilder(project).Build();
            var mle1 = new ManagedListEntityBuilder(ml).Build();

            var sml = new StudyManagedListEntityBuilder(mle1).Build(); // no study linked

            _context.Initialize(new List<Entity> { sml });

            var inputJson = JsonConvert.SerializeObject(new List<Guid> { sml.Id });
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.InputParameters = new ParameterCollection
            {
                ["StudyManagedListEntityIds"] = inputJson
            };

            // Act
            _context.ExecutePluginWith<ValidateStudyTemplateCustomAPI>(pluginContext);

            // Assert
            var resultJson = (string)pluginContext.OutputParameters["StudyIds"];
            var resultIds = JsonConvert.DeserializeObject<List<Guid>>(resultJson);

            Assert.AreEqual(0, resultIds.Count, "Should return no study IDs when none are linked.");
        }
    }
}
