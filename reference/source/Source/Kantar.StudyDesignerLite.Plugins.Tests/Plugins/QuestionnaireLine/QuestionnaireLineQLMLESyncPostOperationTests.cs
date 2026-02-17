using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineQLMLESyncPostOperationTests
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
        public void WhenQuestionnaireLineDeactivated_ShouldDeactivateQLMLEs()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build(); // Draft
            var qLine = new QuestionnaireLineBuilder(project).Build();
            var mL = new ManagedListBuilder(project).Build();
            var mLE = new ManagedListEntityBuilder(mL).Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .WithQuestionnaireLine(qLine)
                .Build(); // Active

            _context.Initialize(new List<Entity> { project, study, qLine, qlmle });

            var preImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active);

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id);
            target[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPostOperation>(pluginContext);

            // Assert
            var updatedQLMLE = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                .FirstOrDefault(x => x.Id == qlmle.Id);

            Assert.IsNotNull(updatedQLMLE);
            Assert.AreEqual(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive, updatedQLMLE.StateCode);
        }

        [TestMethod]
        public void WhenAllQLMLEsDeactivated_ShouldDeactivateStudyMLE()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();
            var qLine = new QuestionnaireLineBuilder(project).Build();

            var mL = new ManagedListBuilder(project).Build();
            var mLE = new ManagedListEntityBuilder(mL).Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .WithQuestionnaireLine(qLine)
                .Build(); // Active

            var studyMle = new StudyManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .Build(); // Active

            _context.Initialize(new List<Entity> { project, study, qLine, qlmle, studyMle });

            var preImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active);

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id);
            target[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPostOperation>(pluginContext);

            // Assert
            var updatedStudyMLE = _context.CreateQuery<KTR_StudyManagedListEntity>()
                .FirstOrDefault(x => x.Id == studyMle.Id);

            Assert.IsNotNull(updatedStudyMLE);
            Assert.AreEqual(KTR_StudyManagedListEntity_StateCode.Inactive, updatedStudyMLE.StateCode);
        }

        [TestMethod]
        public void WhenNoQLMLEsExist_ShouldDoNothing()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var preImage = qLine.Clone();
            preImage[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Active);

            var target = new Entity(KT_QuestionnaireLines.EntityLogicalName, qLine.Id);
            target[KT_QuestionnaireLines.Fields.StateCode] = new OptionSetValue((int)KT_QuestionnaireLines_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPostOperation>(pluginContext);

            // Assert - nothing deactivated, so QLMLE query returns 0
            var qlmles = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>().ToList();
            Assert.AreEqual(0, qlmles.Count);
        }
    }
}
