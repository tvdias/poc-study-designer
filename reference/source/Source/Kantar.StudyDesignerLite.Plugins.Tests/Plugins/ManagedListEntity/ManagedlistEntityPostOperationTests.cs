namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedListEntity
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Services.Description;
    using FakeXrmEasy;
    using FakeXrmEasy.Extensions;
    using Kantar.StudyDesignerLite.Plugins.ManagedListEntity;
    using Kantar.StudyDesignerLite.Plugins.StudyManagedListEntity;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class ManagedlistEntityPostOperationTests
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
        public void Should_Deactivate_RelatedtoDraftStudy_Sucessfully()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .Build();

            var preManagedlistEntity = new ManagedListEntityBuilder()
                .Build();
            var postManagedlistEntity = new ManagedListEntityBuilder()
                .WithId(preManagedlistEntity.Id)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .Build();

            var studyMLEntity = new StudyManagedListEntityBuilder(preManagedlistEntity)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Active)
                .WithStatusCode(KTR_StudyManagedListEntity_StatusCode.Active)
                .WithStudy(study)
                .Build();

            var qL = new QuestionnaireLineBuilder(project).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qlMLEntity = new QuestionnaireLineManagedListEntityBuilder(preManagedlistEntity)
                .WithStateCode(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active)
                .WithStatusCode(KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Active)
                .WithStudy(study)
                .Build();

            _context.Initialize(new List<Entity>
            {
                project,
                study,                
                preManagedlistEntity,  
                studyMLEntity,
                qlMLEntity
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preManagedlistEntity;
            pluginContext.PostEntityImages["PostImage"] = postManagedlistEntity;

            pluginContext.InputParameters["Target"] = new Entity(preManagedlistEntity.LogicalName, preManagedlistEntity.Id);

            // Act
            _context.ExecutePluginWith<ManagedlistEntityPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
                .First();
            Assert.AreEqual(
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                updated.GetAttributeValue<OptionSetValue>("statecode").Value);
        }

        [TestMethod]
        public void Should_NotDeactivate_RelatedtoNonDraftStudy_Sucessfully()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project)
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .Build();

            var preManagedlistEntity = new ManagedListEntityBuilder()
                .Build();
            var postManagedlistEntity = new ManagedListEntityBuilder()
                .WithId(preManagedlistEntity.Id)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .Build();

            var studyMLEntity = new StudyManagedListEntityBuilder(preManagedlistEntity)
                .WithStateCode(KTR_StudyManagedListEntity_StateCode.Active)
                .WithStatusCode(KTR_StudyManagedListEntity_StatusCode.Active)
                .WithStudy(study)
                .Build();

            var qL = new QuestionnaireLineBuilder(project).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qlMLEntity = new QuestionnaireLineManagedListEntityBuilder(preManagedlistEntity)
                .WithStateCode(KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active)
                .WithStatusCode(KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Active)
                .WithStudy(study)
                .Build();

            _context.Initialize(new List<Entity>
            {
                project,
                study,
                preManagedlistEntity,
                studyMLEntity,
                qlMLEntity
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.PreEntityImages["PreImage"] = preManagedlistEntity;
            pluginContext.PostEntityImages["PostImage"] = postManagedlistEntity;

            pluginContext.InputParameters["Target"] = new Entity(preManagedlistEntity.LogicalName, preManagedlistEntity.Id);

            // Act
            _context.ExecutePluginWith<ManagedlistEntityPostOperation>(pluginContext);

            // Assert StudyManagedListEntity is still Active
            var updatedStudyMLEntity = _context.CreateQuery(KTR_StudyManagedListEntity.EntityLogicalName)
                .First(e => e.Id == studyMLEntity.Id);
            Assert.AreEqual(
                (int)KTR_StudyManagedListEntity_StateCode.Active,
                updatedStudyMLEntity.GetAttributeValue<OptionSetValue>("statecode").Value);

            // Assert QLMLEntity is still Active
            var updatedQLMLEntity = _context.CreateQuery(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
                .First(e => e.Id == qlMLEntity.Id);
            Assert.AreEqual(
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active,
                updatedQLMLEntity.GetAttributeValue<OptionSetValue>("statecode").Value);
        }
    }
}
