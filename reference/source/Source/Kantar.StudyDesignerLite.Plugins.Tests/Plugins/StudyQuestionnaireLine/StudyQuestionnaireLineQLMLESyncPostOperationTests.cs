using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.StudyQuestionnaireLine
{
    [TestClass]
    public class StudyQuestionnaireLineQLMLESyncPostOperationTests
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
        public void WhenStudyQuestionnaireLineDeactivated_ShouldDeactivateQLMLEs()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithStatusCode((KT_Study_StatusCode)1) // Draft
                .Build();

            var questionnaireLine = new QuestionnaireLineBuilder(project).Build();
            var studyQuestionnaireLine =
                new StudyQuestionnaireLineBuilder(study, questionnaireLine).Build();

            var managedList = new ManagedListBuilder(project).Build();
            var mle = new ManagedListEntityBuilder(managedList).Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mle)
                .WithStudy(study)
                .WithQuestionnaireLine(questionnaireLine)
                .Build(); // Active

            _context.Initialize(new List<Entity>
            {
                project,
                study,
                questionnaireLine,
                studyQuestionnaireLine,
                managedList,
                mle,
                qlmle
            });

            var preImage = studyQuestionnaireLine.Clone();
            preImage[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Active);

            var target = new Entity(
                KTR_StudyQuestionnaireLine.EntityLogicalName,
                studyQuestionnaireLine.Id);

            target[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<StudyQuestionnaireLineQLMLESyncPostOperation>(
                pluginContext);

            // Assert
            var updatedQLMLE = _context
                .CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                .FirstOrDefault(x => x.Id == qlmle.Id);

            Assert.IsNotNull(updatedQLMLE);
            Assert.AreEqual(
                KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                updatedQLMLE.StateCode);
        }

        [TestMethod]
        public void WhenAllQLMLEsDeactivated_ShouldDeactivateStudyMLE()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithStatusCode((KT_Study_StatusCode)1)
                .Build();

            var questionnaireLine = new QuestionnaireLineBuilder(project).Build();
            var studyQuestionnaireLine =
                new StudyQuestionnaireLineBuilder(study, questionnaireLine).Build();

            var managedList = new ManagedListBuilder(project).Build();
            var mle = new ManagedListEntityBuilder(managedList).Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mle)
                .WithStudy(study)
                .WithQuestionnaireLine(questionnaireLine)
                .Build(); // Active

            var studyMle = new StudyManagedListEntityBuilder(mle)
                .WithStudy(study)
                .Build(); // Active

            _context.Initialize(new List<Entity>
            {
                project,
                study,
                questionnaireLine,
                studyQuestionnaireLine,
                managedList,
                mle,
                qlmle,
                studyMle
            });

            var preImage = studyQuestionnaireLine.Clone();
            preImage[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Active);

            var target = new Entity(
                KTR_StudyQuestionnaireLine.EntityLogicalName,
                studyQuestionnaireLine.Id);

            target[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<StudyQuestionnaireLineQLMLESyncPostOperation>(
                pluginContext);

            // Assert
            var updatedStudyMle = _context
                .CreateQuery<KTR_StudyManagedListEntity>()
                .FirstOrDefault(x => x.Id == studyMle.Id);

            Assert.IsNotNull(updatedStudyMle);
            Assert.AreEqual(
                KTR_StudyManagedListEntity_StateCode.Inactive,
                updatedStudyMle.StateCode);
        }

        [TestMethod]
        public void WhenNoQLMLEsExist_ShouldDoNothing()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).Build();
            var questionnaireLine = new QuestionnaireLineBuilder(project).Build();
            var studyQuestionnaireLine =
                new StudyQuestionnaireLineBuilder(study, questionnaireLine).Build();

            _context.Initialize(new List<Entity>
            {
                project,
                study,
                questionnaireLine,
                studyQuestionnaireLine
            });

            var preImage = studyQuestionnaireLine.Clone();
            preImage[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Active);

            var target = new Entity(
                KTR_StudyQuestionnaireLine.EntityLogicalName,
                studyQuestionnaireLine.Id);

            target[KTR_StudyQuestionnaireLine.Fields.StateCode] =
                new OptionSetValue((int)KTR_StudyQuestionnaireLine_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preImage;
            pluginContext.InputParameters["Target"] = target;

            // Act
            _context.ExecutePluginWith<StudyQuestionnaireLineQLMLESyncPostOperation>(
                pluginContext);

            // Assert
            var qlmles = _context
                .CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                .ToList();

            Assert.AreEqual(0, qlmles.Count);
        }
    }
}
