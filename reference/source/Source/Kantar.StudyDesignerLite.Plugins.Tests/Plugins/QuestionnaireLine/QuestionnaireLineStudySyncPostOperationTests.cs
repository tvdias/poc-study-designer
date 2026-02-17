using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineStudySyncPostOperationTests
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
        public void WhenQuestionnaireLineCreatedWithDraftStudies_ShouldCreateStudyLines()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine = new QuestionnaireLineBuilder(project).WithSortOrder(5).WithVariableName("SomeVar").Build();

            _context.Initialize(new List<Entity> { project, study, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var createdLines = _context.CreateQuery<KTR_StudyQuestionnaireLine>()
                .Where(x => x.KTR_QuestionnaireLine.Id == qLine.Id && x.KTR_Study.Id == study.Id)
                .ToList();

            Assert.AreEqual(1, createdLines.Count);
            Assert.AreEqual(5, createdLines[0].KTR_SortOrder);
        }

        [TestMethod]
        public void WhenMultipleQuestionnaireLinesCreated_ShouldCreateCorrespondingStudyLines()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();

            var qLine1 = new QuestionnaireLineBuilder(project).WithSortOrder(1).WithVariableName("Var1").Build();
            var qLine2 = new QuestionnaireLineBuilder(project).WithSortOrder(2).WithVariableName("Var2").Build();

            _context.Initialize(new List<Entity> { project, study });

            // Act
            foreach (var qLine in new[] { qLine1, qLine2 })
            {
                var pluginContext = _context.GetDefaultPluginContext();
                pluginContext.MessageName = nameof(ContextMessageEnum.Create);
                pluginContext.InputParameters["Target"] = qLine;
                _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);
            }

            // Assert
            var studyLines = _context.CreateQuery<KTR_StudyQuestionnaireLine>().ToList();
            Assert.AreEqual(2, studyLines.Count);
            Assert.IsTrue(studyLines.Any(x => x.KTR_QuestionnaireLine.Id == qLine1.Id));
            Assert.IsTrue(studyLines.Any(x => x.KTR_QuestionnaireLine.Id == qLine2.Id));
        }

        [TestMethod]
        public void WhenQuestionnaireLineDeactivated_ShouldDeactivateStudyLines()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine = new QuestionnaireLineBuilder(project).Build();
            var studyQ = new StudyQuestionnaireLineBuilder(study, qLine).Build();

            _context.Initialize(new List<Entity> { project, study, qLine, studyQ });

            var preImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active);

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id)
            {
                [KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive)
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KTR_StudyQuestionnaireLine>().FirstOrDefault(x => x.Id == studyQ.Id);
            Assert.AreEqual(KTR_StudyQuestionnaireLine_StateCode.Inactive, updated.StateCode);
        }

        [TestMethod]
        public void WhenQuestionnaireLineDeactivated_ShouldRecalculateSortOrder()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine1 = new QuestionnaireLineBuilder(project).WithSortOrder(0).Build();
            var qLine2 = new QuestionnaireLineBuilder(project).WithSortOrder(1).Build();
            var qLine3 = new QuestionnaireLineBuilder(project).WithSortOrder(2).Build();
            var qLine4 = new QuestionnaireLineBuilder(project).WithSortOrder(5).Build();
            var studyQ1 = new StudyQuestionnaireLineBuilder(study, qLine1).WithSortOrder(0).Build();
            var studyQ2 = new StudyQuestionnaireLineBuilder(study, qLine2).WithSortOrder(1).Build();
            var studyQ3 = new StudyQuestionnaireLineBuilder(study, qLine3).WithSortOrder(2).Build();
            var studyQ4 = new StudyQuestionnaireLineBuilder(study, qLine4).WithSortOrder(5).Build();

            _context.Initialize(new List<Entity> {
                project, study,
                qLine1, studyQ1,
                qLine2, studyQ2,
                qLine3, studyQ3,
                qLine4, studyQ4 });

            var preImage = qLine2.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active);
            var postImage = qLine2.Clone();
            postImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive);

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine2.Id)
            {
                [KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive)
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.PostEntityImages["PostImage"] = postImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KTR_StudyQuestionnaireLine>().FirstOrDefault(x => x.Id == studyQ4.Id);
            Assert.AreEqual(2, updated.KTR_SortOrder);
        }

        [TestMethod]
        public void WhenQuestionnaireLineReactivated_ShouldReactivateExistingStudyLines()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine = new QuestionnaireLineBuilder(project).WithSortOrder(7).Build();
            var studyQ = new StudyQuestionnaireLineBuilder(study, qLine).WithState(1).WithSortOrder(3).Build();

            _context.Initialize(new List<Entity> { project, study, qLine, studyQ });

            var preImage = qLine.Clone();
            var postImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive);
            postImage[KT_QuestionnaireLines.Fields.KTR_Project] = project.ToEntityReference();

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id)
            {
                [KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active),
                [KT_QuestionnaireLines.Fields.KTR_Project] = project.ToEntityReference()
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.PostEntityImages["PostImage"] = postImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KTR_StudyQuestionnaireLine>().FirstOrDefault(x => x.Id == studyQ.Id);
            Assert.AreEqual(KTR_StudyQuestionnaireLine_StateCode.Active, updated.StateCode);
        }

        [TestMethod]
        public void WhenSortOrderChanged_ShouldUpdateRelatedStudyLineOrders()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine = new QuestionnaireLineBuilder(project).WithSortOrder(2).Build();
            var studyQ = new StudyQuestionnaireLineBuilder(study, qLine).WithSortOrder(2).Build();

            _context.Initialize(new List<Entity> { project, study, qLine, studyQ });

            var preImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.KT_QuestionSortOrder] = 2;

            var postImage = qLine.Clone();
            postImage[KT_QuestionnaireLines.Fields.KT_QuestionSortOrder] = 10;

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id)
            {
                [KT_QuestionnaireLines.Fields.KT_QuestionSortOrder] = 10
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.PostEntityImages["PostImage"] = postImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KTR_StudyQuestionnaireLine>().FirstOrDefault(x => x.Id == studyQ.Id);
            Assert.AreEqual(10, updated.KTR_SortOrder);
        }

        [TestMethod]
        public void WhenNoDraftStudiesExist_ShouldNotCreateStudyLines()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithSortOrder(1).Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);

            // Assert
            var created = _context.CreateQuery<KTR_StudyQuestionnaireLine>().ToList();
            Assert.AreEqual(0, created.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void WhenQuestionnaireLineMissingRequiredField_ShouldThrowException()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();

            var brokenQL = new QuestionnaireLineBuilder(project)
                .WithSortOrder(1)
                .Build();

            // Broken field type that will cause the plugin to fail during CreateRequest
            brokenQL[KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = 12345; // invalid type (should be string)

            _context.Initialize(new Entity[] { project, study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = brokenQL;

            // Act - this will throw when the CreateRequest fails due to wrong attribute type
            _context.ExecutePluginWith<QuestionnaireLineStudySyncPostOperation>(pluginContext);
        }

    }
}
