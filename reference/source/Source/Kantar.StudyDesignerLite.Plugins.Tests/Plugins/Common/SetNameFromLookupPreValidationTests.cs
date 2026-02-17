using System;
using System.Reflection;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    [TestClass]
    public class SetNameFromLookupPreValidationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                _context = new XrmFakedContext();
                _service = _context.GetOrganizationService();
                _tracingService = new Mock<ITracingService>();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Console.WriteLine("LoaderException: " + loaderEx?.Message);
                }
                throw;
            }
        }

        [TestMethod]
        public void SetNameProductConfigQuestion_ValidLookupName_SetTargetEntityName()
        {
            // Arrange
            var lookupName = "Lookup Name";
            var lookupEntity = new KTR_ConfigurationQuestion
            {
                Id = Guid.NewGuid(),
                KTR_Name = lookupName
            };

            var targetEntity = new KTR_ProductConfigQuestion
            {
                Id = Guid.NewGuid(),
                KTR_ConfigurationQuestion = new EntityReference(lookupEntity.LogicalName, lookupEntity.Id)
            };

            _context.Initialize(new Entity[] { lookupEntity });

            // Act
            _context.ExecutePluginWithTarget<SetNameFromLookupPreValidation>(targetEntity);

            // Assert
            Assert.AreEqual(lookupName, targetEntity.KTR_Name);
        }

        [TestMethod]
        public void SetNameProjectProductConfig_ValidLookupName_SetTargetEntityName()
        {
            // Arrange
            var lookupName = "Lookup Name";
            var lookupEntity = new KTR_ConfigurationQuestion
            {
                Id = Guid.NewGuid(),
                KTR_Name = lookupName
            };

            var targetEntity = new KTR_ProjectProductConfig
            {
                Id = Guid.NewGuid(),
                KTR_ConfigurationQuestion = new EntityReference(lookupEntity.LogicalName, lookupEntity.Id)
            };

            _context.Initialize(new Entity[] { lookupEntity });

            // Act
            _context.ExecutePluginWithTarget<SetNameFromLookupPreValidation>(targetEntity);

            // Assert
            Assert.AreEqual(lookupName, targetEntity.KTR_Name);
        }

        [TestMethod]
        public void ExecutePlugin_LookupEntityExistsButNameIsMissing_NoChanges()
        {
            // Arrange
            var lookupEntity = new KTR_ConfigurationQuestion
            {
                Id = Guid.NewGuid()
            };

            var targetEntity = new KTR_ProductConfigQuestion
            {
                Id = Guid.NewGuid(),
                KTR_ConfigurationQuestion = new EntityReference(lookupEntity.LogicalName, lookupEntity.Id)
            };

            _context.Initialize(new Entity[] { lookupEntity });

            // Act
            _context.ExecutePluginWithTarget<SetNameFromLookupPreValidation>(targetEntity);

            // Assert
            Assert.IsFalse(targetEntity.Attributes.Contains("ktr_name"), "Target entity name should not be set.");
        }
    }
}
