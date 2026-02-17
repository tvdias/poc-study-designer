using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Microsoft.Xrm.Sdk;
using Moq;
using Microsoft.Xrm.Sdk.Query;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class StudyNewVersionCustomAPITests
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
        public void CreateNewStudy_ValidStudyId_CreatesNewVersionStudySuccessfully()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            // Completed parent study
            var parentStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStateCode(KT_Study_StateCode.Active)
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithVersion(1)
                .Build();

            parentStudy.KTR_MasterStudy = parentStudy.ToEntityReference();

            _context.Initialize(new List<Entity> { parentStudy });

            var pluginContext = MockPluginContext(parentStudy.Id.ToString());
            _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);

            var updatedStudy = _service.Retrieve("kt_study", parentStudy.Id, new ColumnSet("statuscode", "statecode", KT_Study.Fields.KTR_MasterStudy));
            Assert.AreEqual((int)parentStudy.StatusCode, ((OptionSetValue)updatedStudy["statuscode"]).Value); // Status should not be updated
            Assert.AreEqual((int)parentStudy.StateCode, ((OptionSetValue)updatedStudy["statecode"]).Value); // State should not be updated

            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_new_version_study"));
            var newStudyId = pluginContext.OutputParameters["ktr_new_version_study"] as string;
            Assert.IsNotNull(newStudyId);
            Assert.AreNotEqual(parentStudy.Id.ToString(), newStudyId);
            Assert.IsTrue(updatedStudy.Attributes.Contains(KT_Study.Fields.KTR_MasterStudy), "KTR_MasterStudy should be set.");

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CreateNewStudy_MissingInputParameters_ThrowsException()
        {
            var pluginContext = MockPluginContext(string.Empty); // Missing studyId
            _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CreateNewStudy_InvalidStudyId_ThrowsException()
        {
            var pluginContext = MockPluginContext("invalid-guid"); // Invalid GUID
            _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);
        }

        [TestMethod]
        public void CreateNewStudy_ValidStudyId_UpdatesOldStudyToInactive()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            // Completed parent study
            var parentStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStateCode(KT_Study_StateCode.Active)
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithVersion(1)
                .Build();

            parentStudy.KTR_MasterStudy = parentStudy.ToEntityReference();

            _context.Initialize(new List<Entity> { parentStudy });

            var pluginContext = MockPluginContext(parentStudy.Id.ToString());
            _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);

            var updatedStudy = _service.Retrieve("kt_study", parentStudy.Id, new ColumnSet("statuscode", "statecode"));
            Assert.AreEqual((int)parentStudy.StatusCode, ((OptionSetValue)updatedStudy["statuscode"]).Value); // Status should not be updated
            Assert.AreEqual((int)parentStudy.StateCode, ((OptionSetValue)updatedStudy["statecode"]).Value); // State should not be updated
        }

        [TestMethod]
        public void CreateNewStudy_WhenDraftChildExists_ThrowsException()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            // Completed parent study
            var parentStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStatusCode(KT_Study_StatusCode.Completed)
                .Build();

            // Draft child study already linked to parent
            var draftChildStudy = new StudyBuilder(project)
                .WithName("Study v2 Draft")
                .WithStatusCode(KT_Study_StatusCode.Draft)
                .WithParentStudy(parentStudy)
                .Build();

            _context.Initialize(new Entity[]
                {
                    draftChildStudy, parentStudy,
                     });

            var pluginContext = MockPluginContext(parentStudy.Id.ToString());

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
            {
                _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);
            });

            Assert.AreEqual(
            "A draft version of this study already exists. Please update the draft or delete it before creating a new version.",
            exception.Message
            );
        }

        [TestMethod]
        public void SetMasterStudy_WhenNull_UpdatesToSelfReference()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var originalStudy = new StudyBuilder(project)
                .WithName("Study v1")
                .WithStateCode(KT_Study_StateCode.Active)
                .WithStatusCode(KT_Study_StatusCode.ReadyForScripting)
                .WithVersion(1)
                .Build();

            _context.Initialize(new List<Entity> { originalStudy });

            var pluginContext = MockPluginContext(originalStudy.Id.ToString());

            // Act
            _context.ExecutePluginWith<StudyNewVersionCustomAPI>(pluginContext);

            // Assert - retrieve updated parent
            var updatedStudy = _service.Retrieve(KT_Study.EntityLogicalName, originalStudy.Id, new ColumnSet(KT_Study.Fields.KTR_MasterStudy));
            Assert.IsTrue(updatedStudy.Attributes.Contains(KT_Study.Fields.KTR_MasterStudy), "KTR_MasterStudy should be set.");

            var masterRef = (EntityReference)updatedStudy[KT_Study.Fields.KTR_MasterStudy];
            Assert.AreEqual(originalStudy.Id, masterRef.Id, "KTR_MasterStudy should point to the same study.");

            // Also assert that new study was created
            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_new_version_study"));
            var newStudyId = pluginContext.OutputParameters["ktr_new_version_study"] as string;
            Assert.IsNotNull(newStudyId);
            Assert.AreNotEqual(originalStudy.Id.ToString(), newStudyId, "New study should have a different ID.");
        }

        [TestMethod]
        public void CopyFieldworkLanguages_CopiesLanguagesToNewStudy()
        {
            // Arrange
            var oldStudyId = Guid.NewGuid();
            var newStudyId = Guid.NewGuid();

            var language1 = new FieldworkLanguagesBuilder(oldStudyId)
                .WithName("English")
                .WithCode("EN")
                .Build();

            var language2 = new FieldworkLanguagesBuilder(oldStudyId)
                .WithName("French")
                .WithCode("FR")
                .Build();

            var fwLangCollection = new EntityCollection(new List<Entity> { language1, language2 });

            var serviceMock = new Mock<IOrganizationService>();
            var createdEntities = new List<Entity>();

            // Mock RetrieveMultiple to return languages only for the expected query
            serviceMock
                .Setup(s => s.RetrieveMultiple(It.Is<QueryExpression>(q =>
                    q.EntityName == "ktr_fieldworklanguages" &&
                    q.Criteria.Conditions.Count > 0 &&
                    q.Criteria.Conditions[0].AttributeName == "ktr_study" &&
                    ((EntityReference)language1["ktr_study"]).Id == oldStudyId
                )))
                .Returns(fwLangCollection);

            // Capture created entities
            serviceMock
                .Setup(s => s.Create(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntities.Add(e))
                .Returns(Guid.NewGuid());

            var tracingMock = new Mock<ITracingService>();
            var plugin = new StudyNewVersionCustomAPI();

            // Act
            var method = typeof(StudyNewVersionCustomAPI).GetMethod("CopyFieldworkLanguages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(plugin, new object[] { serviceMock.Object, tracingMock.Object, oldStudyId, newStudyId });

            // Assert
            Assert.AreEqual(2, createdEntities.Count, "Should create two new fieldwork language entities for the new study.");

            var names = new HashSet<string>();
            foreach (var lang in createdEntities)
            {
                Assert.AreEqual("ktr_fieldworklanguages", lang.LogicalName);
                Assert.AreEqual(newStudyId, ((EntityReference)lang["ktr_study"]).Id, "Language should be linked to new study.");
                names.Add(lang.GetAttributeValue<string>("ktr_name"));
            }
            CollectionAssert.AreEquivalent(new[] { "English", "French" }, new List<string>(names));
        }

        private XrmFakedPluginExecutionContext MockPluginContext(string studyId)
        {
            return new XrmFakedPluginExecutionContext
            {
                MessageName = "ktr_study_new_version", // Message that triggers the plugin
                InputParameters = new ParameterCollection
                {
                    { "ktr_oldstudy_id", studyId } // Study ID to be passed into the plugin
                },
                OutputParameters = new ParameterCollection
                {
                    { "ktr_new_version_study", null } // Output to store new version study ID
                }
            };
        }
    }
}
