using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Helpers
{
    [TestClass]
    public class JsonHelperTests
    {
        [TestMethod]
        public void JsonHelper_Serialize_Success()
        {
            // Arrange
            var expectedDate = DateTime.UtcNow;
            var test = new TestClass(123, "test name", expectedDate);

            // Act
            var result = JsonHelper.Serialize(test);

            // Assert
            Assert.IsNotNull(result);
            StringAssert.Contains(result, "\"Id\":123");
            StringAssert.Contains(result, "\"Name\":\"test name\"");
            StringAssert.Contains(result, expectedDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        }

        [TestMethod]
        public void JsonHelper_Deserialize_Success()
        {
            // Arrange
            var expectedDate = new DateTime(2025, 6, 1, 14, 30, 0, DateTimeKind.Utc);
            var json = $"{{\"Id\":1,\"Name\":\"Test\",\"Date\":\"{expectedDate:O}\"}}";

            // Act
            var result = JsonHelper.Deserialize<TestClass>(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(expectedDate, result.Date);
        }

        private class TestClass
        {
            public TestClass(int id, string name, DateTime date)
            {
                Id = id;
                Name = name;
                Date = date;
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

    }
}
