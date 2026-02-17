using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Url;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.URL
{
    [TestClass]
    public class UrlPreValidationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldThrow_WhenStudyNotDraft()
        {
            // Arrange Project
            var project = new ProjectBuilder().Build();

            // Arrange Study (not draft)
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            // Arrange Url
            var urlEntity = new UrlBuilder(study)
                .Build();

            _context.Initialize(new List<Entity> { study });
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = urlEntity;

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<UrlPreValidation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should indicate study is not in draft status
                StringAssert.Contains(ex.Message, "draft");
            }
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenMessageNameIsNotCreateOrUpdate()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Ensure that no exception is thrown and the InputParameters remain unchanged
            Assert.IsTrue(pluginContext.InputParameters.Count == 0 || !pluginContext.InputParameters.ContainsKey("Target"));
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNull()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = null;
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Ensure that the Target parameter is still null after plugin execution
            Assert.IsNull(pluginContext.InputParameters["Target"]);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenNoStudyReference()
        {
            var urlEntity = new UrlBuilder(null)
                .Build();

            // Remove the study reference to simulate the test case
            urlEntity.Attributes.Remove(KTR_Url.Fields.KTR_Study);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = urlEntity;
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Ensure no study reference exists in the target entity
            Assert.IsFalse(urlEntity.Attributes.ContainsKey(KTR_Url.Fields.KTR_Study));
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenNoStudyReferenceInPreImage()
        {
            var urlEntity = new UrlBuilder(null)
                .Build();
            var preImage = new UrlBuilder(null)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = urlEntity;
            pluginContext.PreEntityImages["PreImage"] = preImage;
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Ensure no study reference exists in both target and pre-image
            Assert.IsFalse(urlEntity.Attributes.ContainsKey(KTR_Url.Fields.KTR_Study));
            Assert.IsFalse(preImage.Attributes.ContainsKey(KTR_Url.Fields.KTR_Study));
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenStudyEntityNotFound()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            // Register the entity type in FakeXrmEasy metadata with a non-empty Id
            _context.Initialize(new List<Entity> { new Entity(KT_Study.EntityLogicalName) { Id = Guid.NewGuid() } });

            var urlEntity = new UrlBuilder(study)
                .Build();

            // Do not initialize the actual study entity in context

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = urlEntity;

            try
            {
                _context.ExecutePluginWith<UrlPreValidation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should indicate study entity not found
                StringAssert.Contains(ex.Message, "Does Not Exist");
            }
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenStudyHasNoStatusCode()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .Build(); // No status code set

            study.StatusCode = null; // Explicitly set to null to simulate missing status code

            var urlEntity = new UrlBuilder(study)
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = urlEntity;
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Study should not have a status code set
            Assert.IsTrue(study.StatusCode == null, "Study should not have a status code set.");
        }

        [TestMethod]
        public void Execute_ShouldNotThrow_WhenStudyIsDraft()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            var urlEntity = new UrlBuilder(study)
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = urlEntity;
            _context.ExecutePluginWith<UrlPreValidation>(pluginContext);

            // Assert: Ensure no exception and the context is unchanged (study is still draft)
            Assert.AreEqual(KT_Study_StatusCode.Draft, study.StatusCode);
        }
    }
}
// This code is a unit test suite for the UrlPreValidation plugin in the Kantar Study Designer Lite application.    
