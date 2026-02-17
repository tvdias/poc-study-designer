using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionBank;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionBank
{
    [TestClass]
    public class AddSuffixToQuestionVariableNamePostOperationTests
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
       
        public void ExecutePluginUpdate_WhenStandardOrCustomIsCustom_AppendsCustSuffix()
        {
            var entity = new QuestionnaireLineBuilder()
                .WithVariableName("BaseName")
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                .Build();

            _context.Initialize(new List<Entity> { entity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_QuestionnaireLines.EntityLogicalName)
            {
                Id = entity.Id,
                Attributes = { { KT_QuestionnaireLines.Fields.KT_QuestionVariableName, entity.KT_QuestionVariableName } }
            };

            _context.ExecutePluginWith<AddSuffixToQuestionVariableNamePostOperation>(pluginContext);

            var updatedEntity = _context.Data[KT_QuestionnaireLines.EntityLogicalName][entity.Id];
            Assert.AreEqual("BaseName_CUST", updatedEntity[KT_QuestionnaireLines.Fields.KTR_XmlVariableName]);
        }


        [TestMethod]
        public void ExecutePluginUpdate_WhenStandardOrCustomIsStandard_DoesNotAppendCustSuffix()
        {
            var entity = new QuestionnaireLineBuilder()
                .WithVariableName("BaseName")
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Standard)
                .Build();

            _context.Initialize(new List<Entity> { entity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_QuestionnaireLines.EntityLogicalName)
            {
                Id = entity.Id,
                Attributes = { { KT_QuestionnaireLines.Fields.KT_QuestionVariableName, entity.KT_QuestionVariableName } }
            };

            _context.ExecutePluginWith<AddSuffixToQuestionVariableNamePostOperation>(pluginContext);

            var updatedEntity = _context.Data[KT_QuestionnaireLines.EntityLogicalName][entity.Id];
            Assert.AreEqual("BaseName", updatedEntity[KT_QuestionnaireLines.Fields.KTR_XmlVariableName]);
        }

        [TestMethod]
        public void ExecutePluginUpdate_WhenNameFieldIsMissing_DoesNotThrow()
        {
            var entity = new QuestionnaireLineBuilder()
                 .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Standard)
                 .Build();

            _context.Initialize(new List<Entity> { entity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_QuestionnaireLines.EntityLogicalName) { Id = entity.Id };

            _context.ExecutePluginWith<AddSuffixToQuestionVariableNamePostOperation>(pluginContext);

            // Assert: XmlVariableName should remain unchanged (null or not set)
            var updatedEntity = _context.Data[KT_QuestionnaireLines.EntityLogicalName][entity.Id];
            Assert.IsFalse(updatedEntity.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_XmlVariableName));
        }

        [TestMethod]
        public void ExecutePluginCreate_CustomQuestion_AppendsCustSuffix()
        {
            var entity = new QuestionnaireLineBuilder()
                .WithVariableName("CreateName")
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                .Build();

            _context.Initialize(new List<Entity> { entity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = new Entity(KT_QuestionnaireLines.EntityLogicalName)
            {
                Id = entity.Id,
                Attributes = { { KT_QuestionnaireLines.Fields.KT_QuestionVariableName, entity.KT_QuestionVariableName } }
            };

            _context.ExecutePluginWith<AddSuffixToQuestionVariableNamePostOperation>(pluginContext);

            var updatedEntity = _context.Data[KT_QuestionnaireLines.EntityLogicalName][entity.Id];
            Assert.AreEqual("CreateName_CUST", updatedEntity[KT_QuestionnaireLines.Fields.KTR_XmlVariableName]);
        }

        [TestMethod]
        public void ExecutePluginUpdate_CustomQuestion_WithoutNameChange()
        {
            var entity = new QuestionnaireLineBuilder()
                .WithVariableName("UnchangedName")
                .WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom.Custom)
                .WithXmlVariableName("UnchangedName_CUST")
                .Build();

            _context.Initialize(new List<Entity> { entity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_QuestionnaireLines.EntityLogicalName)
            {
                Id = entity.Id,
                Attributes = { { KT_QuestionnaireLines.Fields.KT_QuestionVariableName, entity.KT_QuestionVariableName } }
            };

            _context.ExecutePluginWith<AddSuffixToQuestionVariableNamePostOperation>(pluginContext);

            var updatedEntity = _context.Data[KT_QuestionnaireLines.EntityLogicalName][entity.Id];
            Assert.AreEqual("UnchangedName_CUST", updatedEntity[KT_QuestionnaireLines.Fields.KTR_XmlVariableName]);
        }

    }
}
