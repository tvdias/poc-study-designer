using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Helpers
{
    [TestClass]
    public class EntityHelpersTests
    {
        [TestMethod]
        public void ExtractLookupIds_ShouldReturnCorrectIds_WhenValidLookupsArePresent()
        {
            // Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var entities = new List<Entity>
            {
                new Entity("custom_entity") { ["custom_lookup"] = guid1 },
                new Entity("custom_entity") { ["custom_lookup"] = guid2 }
            };

            // Act
            var result = EntityHelpers.JoinIds(entities, "custom_lookup");

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(guid1));
            Assert.IsTrue(result.Contains(guid2));
        }

        [TestMethod]
        public void ExtractLookupIds_ShouldReturnEmptyList_WhenNoEntitiesProvided()
        {
            // Arrange
            var entities = new List<Entity>();

            // Act
            var result = EntityHelpers.JoinIds(entities, "custom_lookup");

            // Assert
            Assert.IsTrue(result.Count == 0);
        }
    }
}
