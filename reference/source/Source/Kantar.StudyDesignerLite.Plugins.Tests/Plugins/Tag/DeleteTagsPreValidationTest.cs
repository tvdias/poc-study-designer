using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tags;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Tag
{
    [TestClass]
    public class DeleteTagsPreValidationTest
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
        public void ExecuteCdsPlugin_TagAssociatedWithQuestionBank_ThrowsException()
        {

            // Arrange Tag
            var tagEntity = new TagBuilder()
                .WithName("Tag 1")
                .Build();

            var questionBankEntity = new QuestionBankBuilder()
                .WithStandardOrCustom(KT_QuestionBank_KT_StandardOrCustom.Standard)
                .WithName("Question1")
                .Build();

            var tagQuestionBankEntity = new TagQuestionBankBuilder()
                .WithTagAndQuestionBank(tagEntity, questionBankEntity)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.InputParameters["Target"] = new EntityReference(tagEntity.LogicalName, tagEntity.Id);

            _context.Initialize(new Entity[] { tagEntity, questionBankEntity, tagQuestionBankEntity });

            // Act
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() => _context.ExecutePluginWith<DeleteTagsPreValidation>(pluginContext));
            
            // Assert
            Assert.AreEqual("Tag can't be deleted because it's associated with one or more question.", exception.Message.ToString());
        }
    }
}
