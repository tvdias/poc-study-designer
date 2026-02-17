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
    public class QuestionnaireLineQLMLESyncPreValidationTests
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
        public void WhenQuestionnaireLineDeleted_ShouldDeactivateQLMLEs()
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

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.InputParameters["Target"] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, qLine.Id);
            pluginContext.PreEntityImages["PreImage"] = qLine.Clone(); // <-- PreImage added

            // Act
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPreValidation>(pluginContext);

            // Assert
            var updatedQLMLE = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                .FirstOrDefault(x => x.Id == qlmle.Id);

            Assert.IsNotNull(updatedQLMLE);
            Assert.AreEqual(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive, updatedQLMLE.StateCode);
        }

        [TestMethod]
        public void WhenMultipleQLMLEsExist_ShouldDeactivateStudyMLEOnlyWhenAllDeleted()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithStatusCode((KT_Study_StatusCode)1).Build();

            var qLine1 = new QuestionnaireLineBuilder(project).Build();
            var qLine2 = new QuestionnaireLineBuilder(project).Build();

            var mL = new ManagedListBuilder(project).Build();
            var mLE = new ManagedListEntityBuilder(mL).Build();

            var qlmle1 = new QuestionnaireLineManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .WithQuestionnaireLine(qLine1)
                .Build();

            var qlmle2 = new QuestionnaireLineManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .WithQuestionnaireLine(qLine2)
                .Build();

            var studyMle = new StudyManagedListEntityBuilder(mLE)
                .WithStudy(study)
                .Build();

            _context.Initialize(new List<Entity> { project, study, qLine1, qLine2, qlmle1, qlmle2, studyMle });

            // Act - delete first QL
            var pluginContext1 = _context.GetDefaultPluginContext();
            pluginContext1.MessageName = "Delete";
            pluginContext1.InputParameters["Target"] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, qLine1.Id);
            pluginContext1.PreEntityImages["PreImage"] = qLine1.Clone(); // <-- PreImage added
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPreValidation>(pluginContext1);

            // Assert - StudyMLE should still be active
            var studyMleAfterFirst = _context.CreateQuery<KTR_StudyManagedListEntity>().First(x => x.Id == studyMle.Id);
            Assert.AreEqual(KTR_StudyManagedListEntity_StateCode.Active, studyMleAfterFirst.StateCode);

            // Act - delete second QL
            var pluginContext2 = _context.GetDefaultPluginContext();
            pluginContext2.MessageName = "Delete";
            pluginContext2.InputParameters["Target"] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, qLine2.Id);
            pluginContext2.PreEntityImages["PreImage"] = qLine2.Clone(); // <-- PreImage added
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPreValidation>(pluginContext2);

            // Assert - StudyMLE should now be inactive
            var studyMleAfterSecond = _context.CreateQuery<KTR_StudyManagedListEntity>().First(x => x.Id == studyMle.Id);
            Assert.AreEqual(KTR_StudyManagedListEntity_StateCode.Inactive, studyMleAfterSecond.StateCode);
        }

        [TestMethod]
        public void WhenNoQLMLEsExist_ShouldDoNothing()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.InputParameters["Target"] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, qLine.Id);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineQLMLESyncPreValidation>(pluginContext);

            // Assert - nothing deactivated
            var qlmles = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>().ToList();
            Assert.AreEqual(0, qlmles.Count);
        }
    }
}
