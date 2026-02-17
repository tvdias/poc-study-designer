using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class UpdateStudyPostOperationTests
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
        public void DeleteRelatedFieldworkMarketLanguage_WhenFieldworkMarketChanged_DeletesRelatedLanguages()
        {
            // Arrange Project
            var project = new ProjectBuilder().Build();

            // Arrange Study (post-image)
            var newMarketId = Guid.NewGuid();
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithFieldworkMarket(newMarketId)
                .Build();

            // Arrange Study (pre-image)
            var oldMarketId = Guid.NewGuid();
            var studyPreImage = new StudyBuilder(project)
                .WithName("Study 1")
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithFieldworkMarket(oldMarketId)
                .Build();

            // Arrange related Fieldwork Market Language (should be deleted)
            var fieldworkLanguage = new Entity("ktr_fieldworklanguages")
            {
                Id = Guid.NewGuid()
            };
            fieldworkLanguage["ktr_fieldworkmarket"] = new EntityReference("ktr_fieldworkmarket", oldMarketId);
            fieldworkLanguage["ktr_study"] = new EntityReference("kt_study", studyPreImage.Id);

            // Arrange unrelated Fieldwork Market Language (should NOT be deleted)
            var unrelatedLanguage = new Entity("ktr_fieldworklanguages")
            {
                Id = Guid.NewGuid()
            };
            unrelatedLanguage["ktr_fieldworkmarket"] = new EntityReference("ktr_fieldworkmarket", Guid.NewGuid());
            unrelatedLanguage["ktr_study"] = new EntityReference("kt_study", studyPreImage.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = study;
            pluginContext.PreEntityImages["Image"] = studyPreImage;

            _context.Initialize(new Entity[] { study, studyPreImage, fieldworkLanguage, unrelatedLanguage });

            // Mock QueryExpression execution for ktr_fieldworklanguages
            _context.AddExecutionMock<Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest>(req =>
            {
                var retrieveMultipleRequest = req as Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest;
                var query = retrieveMultipleRequest?.Query as QueryExpression;
                if (query != null && query.EntityName == "ktr_fieldworklanguages")
                {
                    // Assert the query contains the expected conditions
                    Assert.IsTrue(query.Criteria.Conditions.Any(c =>
                        c.AttributeName == "ktr_fieldworkmarket" &&
                        c.Values.Contains(oldMarketId)));

                    Assert.IsTrue(query.Criteria.Conditions.Any(c =>
                        c.AttributeName == "ktr_study" &&
                        c.Values.Contains(studyPreImage.Id)));

                    // Return the expected entities
                    var result = new EntityCollection(new[] { fieldworkLanguage });
                    return new Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    {
                        Results = { ["EntityCollection"] = result }
                    };
                }
                // Default: return empty
                return new Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                {
                    Results = { ["EntityCollection"] = new EntityCollection() }
                };
            });

            // Act
            _context.ExecutePluginWith<UpdateStudyPostOperation>(pluginContext);

            // Assert
            var remainingLanguages = _context.CreateQuery("ktr_fieldworklanguages").ToList();
            Assert.IsFalse(remainingLanguages.Any(e => e.Id == fieldworkLanguage.Id), "Related Fieldwork Market Language should be deleted.");
            Assert.IsTrue(remainingLanguages.Any(e => e.Id == unrelatedLanguage.Id), "Unrelated Fieldwork Market Language should NOT be deleted.");
        }
    }
}
