using Kantar.StudyDesignerLite.Plugins.SubsetDefinition;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using FakeXrmEasy;
using System;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.SubsetDefinition
{
    [TestClass]
    public class SubsetDefinitionDeletePreValidationTest
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }
        
        [TestMethod]
        public void Delete_NoRelatedSubsets_NoUpdates()
        {
            // Arrange: create a subset definition that is being deleted
            var subsetDefinition = new SubsetDefinitionBuilder()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters["Target"] = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinition.Id);

            // Initialize context with only the subset definition and no related ktr_questionnairelinesubset records
            _context.Initialize(new Entity[] { subsetDefinition });

            // Act: execute plugin
            _context.ExecutePluginWith<SubsetDefinitionDeletePreValidation>(pluginContext);

            // Assert: no ktr_questionnairelinesubset records exist
            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(true)
            };
            query.Criteria.AddCondition(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId, Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, subsetDefinition.Id);

            var results = _service.RetrieveMultiple(query);
            Assert.AreEqual(0, results.Entities.Count);
        }

        [TestMethod]
        public void Delete_RelatedSubsets_AreClearedAndSetUsesFullList()
        {
            // Arrange: create subset definition and related questionnaire line subset entities
            var subsetDefinition = new SubsetDefinitionBuilder()
                .Build();

            var relatedSubset1 = new QuestionnaireLineSubsetBuilder()
                .WithSubsetDefinition(subsetDefinition) // builder should set KTR_SubsetDefinitionId to the entity ref
                .WithUsesFullList(false)
                .Build();

            var relatedSubset2 = new QuestionnaireLineSubsetBuilder()
                .WithSubsetDefinition(subsetDefinition)
                .WithUsesFullList(false)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters["Target"] = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinition.Id);

            // Initialize context with subset definition and related subset records
            _context.Initialize(new Entity[] { subsetDefinition, relatedSubset1, relatedSubset2 });

            // Sanity check before executing plugin
            var before1 = _service.Retrieve(KTR_QuestionnaireLineSubset.EntityLogicalName, relatedSubset1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Assert.IsTrue(before1.Attributes.ContainsKey(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId));
            Assert.IsFalse(before1.GetAttributeValue<bool>(KTR_QuestionnaireLineSubset.Fields.KTR_UsesFullList));

            // Act: execute plugin
            _context.ExecutePluginWith<SubsetDefinitionDeletePreValidation>(pluginContext);

            // Assert: related subsets should have KTR_SubsetDefinitionId cleared and KTR_UsesFullList = true
            var after1 = _service.Retrieve(KTR_QuestionnaireLineSubset.EntityLogicalName, relatedSubset1.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            var after2 = _service.Retrieve(KTR_QuestionnaireLineSubset.EntityLogicalName, relatedSubset2.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

            // KTR_SubsetDefinitionId should no longer be present
            Assert.IsFalse(after1.Attributes.ContainsKey(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId));
            Assert.IsTrue(after1.GetAttributeValue<bool>(KTR_QuestionnaireLineSubset.Fields.KTR_UsesFullList));

            Assert.IsFalse(after2.Attributes.ContainsKey(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId));
            Assert.IsTrue(after2.GetAttributeValue<bool>(KTR_QuestionnaireLineSubset.Fields.KTR_UsesFullList));
        }
    }
}
