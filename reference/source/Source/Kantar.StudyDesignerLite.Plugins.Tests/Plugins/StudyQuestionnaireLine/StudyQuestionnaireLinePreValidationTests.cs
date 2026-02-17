using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.StudyQuestionnaireLine
{
    [TestClass]
    public class StudyQuestionnaireLinePreValidationTests
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
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();
            var questionnaireLine = new KT_QuestionnaireLines();

            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;

            try
            {
                _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);
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
            _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);

            // Assert: Ensure that no exception is thrown and the InputParameters remain unchanged
            Assert.IsTrue(pluginContext.InputParameters.Count == 0 || !pluginContext.InputParameters.ContainsKey("Target"));
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNull()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = null;
            _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);

            // Assert: Ensure that the Target parameter is still null after plugin execution
            Assert.IsNull(pluginContext.InputParameters["Target"]);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenNoStudyReference()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();
            var questionnaireLine = new KT_QuestionnaireLines();
            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            studyQuestionnaireLine.Attributes.Remove(KTR_StudyQuestionnaireLine.Fields.KTR_Study);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;
            _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);

            // Assert: Ensure no study reference exists in the target entity
            Assert.IsFalse(studyQuestionnaireLine.Attributes.ContainsKey(KTR_StudyQuestionnaireLine.Fields.KTR_Study));
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenNoStudyReferenceInPreImage()
        {
            // Register the entity type in FakeXrmEasy metadata with a non-empty Id
            _context.Initialize(new List<Entity> { new Entity(KT_Study.EntityLogicalName) { Id = Guid.NewGuid() } });

            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();
            var questionnaireLine = new KT_QuestionnaireLines();
            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();
            var preImage = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            preImage.Attributes.Remove(KTR_StudyQuestionnaireLine.Fields.KTR_Study);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            try
            {
                _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should indicate missing study reference
                StringAssert.Contains(ex.Message, "study");
            }
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenStudyEntityNotFound()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();
            var questionnaireLine = new KT_QuestionnaireLines();

            // Register the entity type in FakeXrmEasy metadata with a non-empty Id
            _context.Initialize(new List<Entity> { new Entity(KT_Study.EntityLogicalName) { Id = Guid.NewGuid() } });

            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;

            try
            {
                _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);
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
                .Build();

            study.StatusCode = null; // Explicitly remove status code

            var questionnaireLine = new KT_QuestionnaireLines();

            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;
            _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);

            // Assert: Study should not have a status code set
            Assert.IsFalse(study.Attributes.ContainsKey("statuscode") && study.StatusCode != null);
        }

        [TestMethod]
        public void Execute_ShouldNotThrow_WhenStudyIsDraft()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();
            var questionnaireLine = new KT_QuestionnaireLines();

            var studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(study, questionnaireLine)
                .Build();

            _context.Initialize(new List<Entity> { study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = studyQuestionnaireLine;
            _context.ExecutePluginWith<StudyQuestionnaireLinePreValidation>(pluginContext);

            // Assert: Ensure no exception and the context is unchanged (study is still draft)
            Assert.AreEqual(KT_Study_StatusCode.Draft, study.StatusCode);
        }
    }
}
