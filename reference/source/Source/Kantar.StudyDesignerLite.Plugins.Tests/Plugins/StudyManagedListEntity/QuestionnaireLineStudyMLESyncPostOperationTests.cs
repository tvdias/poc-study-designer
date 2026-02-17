using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.StudyManagedListEntity;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.StudyManagedListEntity
{
    [TestClass]
    public class QuestionnaireLineStudyMLESyncPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Init()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void When_StudyMLE_Deactivated_ShouldDeactivateLinkedQLMLE()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var studyId = new StudyBuilder(project).Build();
            var qline = new QuestionnaireLineBuilder(project).Build();

            var ml = new ManagedListBuilder(project).Build();
            var mleId = new ManagedListEntityBuilder(ml).Build();
            
            var pre = new StudyManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Active)
                .Build();

            var post = new StudyManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Inactive)
                .Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active)
                .Build();

            _context.Initialize(new List<Entity> { qlmle });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = pre;
            pluginContext.PostEntityImages["PostImage"] = post;

            pluginContext.InputParameters["Target"] = new Entity(pre.LogicalName, pre.Id);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudyMLESyncPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
                                  .First();

            Assert.AreEqual(
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                updated.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [TestMethod]
        public void When_NoStateChange_ShouldNotUpdateQLMLE()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var studyId = new StudyBuilder(project).Build();
            var qline = new QuestionnaireLineBuilder(project).Build();

            var ml = new ManagedListBuilder(project).Build();
            var mleId = new ManagedListEntityBuilder(ml).Build();
            
            var pre = new StudyManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Active)
                .Build();

            var post = new StudyManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Active)
                .Build();

            var qlmle = new QuestionnaireLineManagedListEntityBuilder(mleId)
                .WithStudy(studyId)
                .WithStateCode(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active)
                .Build();

            _context.Initialize(new List<Entity> { qlmle });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = pre;
            pluginContext.PostEntityImages["PostImage"] = post;
            pluginContext.InputParameters["Target"] = new Entity(pre.LogicalName, pre.Id);

            // Act
            _context.ExecutePluginWith<QuestionnaireLineStudyMLESyncPostOperation>(pluginContext);

            // Assert
            var unchanged = _context.CreateQuery(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName).First();
            Assert.AreEqual(
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active,
                unchanged.GetAttributeValue<OptionSetValue>("statecode").Value);
        }
    }
}
