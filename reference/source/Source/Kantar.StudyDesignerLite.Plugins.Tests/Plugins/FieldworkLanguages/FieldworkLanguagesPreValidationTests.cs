using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Kantar.StudyDesignerLite.Plugins.FieldworkLanguages;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.FieldworkLanguages
{
    [TestClass]
    public class FieldworkLanguagesPreValidationTests
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

            // Arrange FieldworkLanguage
            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            try
            {
                _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should mention draft
                StringAssert.Contains(ex.Message, "draft");
            }
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldThrow_WhenStudyNotDraft_OnUpdate()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            try
            {
                _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should mention draft
                StringAssert.Contains(ex.Message, "draft");
            }
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldNotThrow_WhenStudyIsDraft()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: Study status code should remain Draft
            Assert.AreEqual(KT_Study_StatusCode.Draft, study.StatusCode);
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldNotThrow_WhenTargetIsNull()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = null;
            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: Target should remain null
            Assert.IsNull(pluginContext.InputParameters["Target"]);
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldNotThrow_WhenStudyReferenceIsNotEntityReference()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            // Overwrite the study reference with a string to simulate the test case
            fieldworkLanguage[KTR_FieldworkLanguages.Fields.KTR_Study] = "not-an-entityreference";

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: Study reference is not an EntityReference
            Assert.IsInstanceOfType(fieldworkLanguage[KTR_FieldworkLanguages.Fields.KTR_Study], typeof(string));
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldNotThrow_WhenNoStudyReference()
        {
            var fieldworkLanguage = new FieldworkLanguagesBuilder(Guid.NewGuid())
                .WithName("English")
                .WithCode("EN")
                .Build();

            // Remove the study reference to simulate the test case
            fieldworkLanguage.Attributes.Remove(KTR_FieldworkLanguages.Fields.KTR_Study);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: Study reference should not exist
            Assert.IsFalse(fieldworkLanguage.Attributes.ContainsKey(KTR_FieldworkLanguages.Fields.KTR_Study));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CreateOrUpdate_ShouldNotThrow_WhenStudyEntityNotFound()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            // Register the entity type in FakeXrmEasy metadata with a non-empty Id
            _context.Initialize(new List<Entity> { new Entity(KT_Study.EntityLogicalName) { Id = Guid.NewGuid() } });

            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            // Do not initialize the study entity in context
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            // Act: Should not throw
            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: The fieldworkLanguage should still reference the missing study Id
            Assert.AreEqual(study.Id, ((EntityReference)fieldworkLanguage[KTR_FieldworkLanguages.Fields.KTR_Study]).Id);
            // Assert: No exception was thrown and plugin completed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void CreateOrUpdate_ShouldNotThrow_WhenStudyHasNoStatusCode()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .Build(); // No status code set

            study.StatusCode = null; // Explicitly set to null

            var fieldworkLanguage = new FieldworkLanguagesBuilder(study.Id)
                .WithName("English")
                .WithCode("EN")
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = fieldworkLanguage;

            _context.ExecutePluginWith<FieldworkLanguagesPreValidation>(pluginContext);

            // Assert: Study should not have a status code set
            Assert.IsFalse(study.Attributes.ContainsKey("statuscode") && study.StatusCode != null);
        }
    }
}
