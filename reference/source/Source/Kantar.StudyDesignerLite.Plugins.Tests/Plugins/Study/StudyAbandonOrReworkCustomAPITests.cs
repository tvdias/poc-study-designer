using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Microsoft.Xrm.Sdk;
using Moq;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class StudyAbandonOrReworkCustomAPITests
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
        public void StudyAbandon_ValidStatusReason_AbandonsStudySuccessfully()
        {
            var studyId = Guid.NewGuid();
            var study = new KT_Study()
            {
                Id = studyId,
                KT_StudyId = studyId,
                StatusCode = KT_Study_StatusCode.Draft,
                StateCode = KT_Study_StateCode.Active,
                KTR_VersionNumber = 1
            };

            _context.Initialize(new List<Entity> { study });

            var pluginContext = MockPluginContext(studyId, 847610005); // Abandon status reason
            _context.ExecutePluginWith<StudyAbandonOrReworkCustomAPI>(pluginContext);

            var updatedStudy = _service.Retrieve("kt_study", studyId, new ColumnSet("statuscode", "statecode"));
            Assert.AreEqual(847610005, ((OptionSetValue)updatedStudy["statuscode"]).Value); // Status is Abandoned
            Assert.AreEqual(0, ((OptionSetValue)updatedStudy["statecode"]).Value); // State is Inactive

            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_study_status_confirmation"));
            var response = pluginContext.OutputParameters["ktr_study_status_confirmation"] as string;
            Assert.AreEqual("Study successfully abandoned.", response);          

        }

        [TestMethod]
        public void StudyRework_ValidStatusReason_CreatesDraftStudySuccessfully()
        {
            var studyId = Guid.NewGuid();

            var study = new KT_Study()
            {
                Id = studyId,
                KT_StudyId = studyId,
                KTR_VersionNumber = 1
            };

            var fullStudy = new KT_Study()
            {
                Id = studyId,
                KT_StudyId = studyId,
                KTR_VersionNumber = 1
            };

            _context.Initialize(new List<Entity> { study, fullStudy });

            // Set statecode/statuscode AFTER creation
            var updateStudy = new KT_Study(studyId)
            {
                StatusCode = KT_Study_StatusCode.Draft,
                StateCode = KT_Study_StateCode.Active,
            };
            _service.Update(updateStudy);

            var pluginContext = MockPluginContext(studyId, 847610006); // Rework status reason
            _context.ExecutePluginWith<StudyAbandonOrReworkCustomAPI>(pluginContext);

            var updatedStudy = _service.Retrieve("kt_study", studyId, new ColumnSet("statuscode", "statecode", KT_Study.Fields.KTR_MasterStudy));
            Assert.AreEqual(847610006, ((OptionSetValue)updatedStudy["statuscode"]).Value); // Status is Rework
            Assert.AreEqual(0, ((OptionSetValue)updatedStudy["statecode"]).Value); // State is Inactive
            Assert.IsTrue(updatedStudy.Attributes.Contains(KT_Study.Fields.KTR_MasterStudy), "KTR_MasterStudy should be set.");

            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_study_status_confirmation"));
            var response = pluginContext.OutputParameters["ktr_study_status_confirmation"] as string;
            Assert.AreEqual("Study marked for rework and draft version created.", response);

            Assert.IsTrue(pluginContext.OutputParameters.Contains("ktr_new_reworkstudy"));
            var newDraftStudyId = pluginContext.OutputParameters["ktr_new_reworkstudy"] as string;
            Assert.IsNotNull(newDraftStudyId);
            Assert.AreNotEqual(studyId.ToString(), newDraftStudyId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void StudyStatus_InvalidStatusReason_ThrowsException()
        {
            var studyId = Guid.NewGuid();
            var study = new Entity("kt_study")
            {
                Id = studyId,
                ["kt_studyid"] = studyId,
                ["statuscode"] = new OptionSetValue(1), // Status 'Active'
                ["statecode"] = new OptionSetValue(0)  // State 'Active'
            };

            _context.Initialize(new List<Entity> { study });

            var pluginContext = MockPluginContext(studyId, 999999999); // Invalid status reason
            _context.ExecutePluginWith<StudyAbandonOrReworkCustomAPI>(pluginContext);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void StudyStatus_MissingInputParameters_ThrowsException()
        {
            var pluginContext = MockPluginContext(Guid.Empty, 0); // Missing studyId and status reason
            _context.ExecutePluginWith<StudyAbandonOrReworkCustomAPI>(pluginContext);
        }

        private XrmFakedPluginExecutionContext MockPluginContext(Guid studyId, int newStatusReason)
        {
            return new XrmFakedPluginExecutionContext
            {
                MessageName = "ktr_abandon_or_rework_study",
                InputParameters = new ParameterCollection
                {
                    { "ktr_study_id", studyId.ToString() },
                    { "ktr_new_status_reason_study", newStatusReason }
                },
                OutputParameters = new ParameterCollection
                {
                    { "ktr_study_status_confirmation", null },
                    { "ktr_new_reworkstudy", null }
                }
            };
        }

    }
}
