namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    using System.Collections.Generic;
    using System.Linq;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class QuestionnaireLineManagedListPreOperationTests
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
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void ShouldThrowIfLocalContextIsNull()
        {
            var plugin = new QuestionnaireLineManagedListPreOperation();
            plugin.Execute(null);
        }

        [TestMethod]
        public void ShouldReturnIfTargetEntityMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters.Clear();

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            // Assert: No entities should be changed in the context
            Assert.AreEqual(0, _context.Data.Count);
        }

        [TestMethod]
        public void ShouldReturnIfEntityLogicalNameIsNotExpected()
        {
            var wrongEntity = new Entity("wrong_entity");
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = wrongEntity;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            // Assert: The wrong entity should not have a KTR_Name attribute set
            Assert.IsFalse(wrongEntity.Attributes.ContainsKey("ktr_name"));
        }

        [TestMethod]
        public void ShouldDoNothingIfMessageIsNotCreateOrUpdate()
        {
            var project = new ProjectBuilder().Build();
            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.InputParameters["Target"] = qlManagedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            // Assert: KTR_Name should not be set
            Assert.IsFalse(qlManagedList.Attributes.ContainsKey("ktr_name"));
        }

        [TestMethod]
        public void ShouldSetKTRNameFromManagedListName()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("ManagedListNameValue").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .WithLocation(100000000, "Row")
                .Build();

            _context.Initialize(new List<Entity> { managedList, qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = qlManagedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.AreEqual("ManagedListNameValue - Row", qlManagedList.KTR_Name);
        }
        [TestMethod]
        public void ShouldUpdateRelatedQuestionnaireLines_WhenManagedListNameChanges()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("ManagedList").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList).WithLocation(100000000, "Row")
                .Build();
            _context.Initialize(new List<Entity> { managedList, qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = managedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            var updatedQl = _context.CreateQuery<KTR_QuestionnaireLinesHaRedList>().FirstOrDefault(x => x.Id == qlManagedList.Id);
            var ktrName = updatedQl.GetAttributeValue<string>("ktr_name");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ktrName), "ktr_name should have been updated by the plugin.");
            Assert.IsTrue(ktrName.StartsWith("ManagedList - Row"),
                $"Expected 'ktr_name' to start with 'ManagedList - Row', but was '{ktrName}'.");
        }
        [TestMethod]
        public void ShouldNotSetKTRNameIfManagedListNameIsNullOrWhitespace()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("   ").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .Build();

            _context.Initialize(new List<Entity> { managedList, qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = qlManagedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.IsTrue(string.IsNullOrWhiteSpace(qlManagedList.KTR_Name));
        }

        [TestMethod]
        public void ShouldNotSetKTRNameIfManagedListReferenceIsNullOrEmptyGuid()
        {
            var project = new ProjectBuilder().Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .Build();

            _context.Initialize(new List<Entity> { qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = qlManagedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.IsTrue(string.IsNullOrWhiteSpace(qlManagedList.KTR_Name));
        }

        [TestMethod]
        public void ShouldCatchExceptionAndThrowInvalidPluginExecutionException()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .Build();

            _context.Initialize(new List<Entity> { qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = qlManagedList;

            Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext));
        }

        [TestMethod]
        public void ShouldSetKTRNameFromManagedListNameOnUpdate()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("ManagedListNameValue").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .WithLocation(100000000, "Row")
                .Build();

            _context.Initialize(new List<Entity> { managedList, qlManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = qlManagedList;

            // Add PreImage to context
            pluginContext.PreEntityImages["PreImage"] = qlManagedList;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.AreEqual("ManagedListNameValue - Row", $"{qlManagedList.KTR_Name}");
        }

        [TestMethod]
        public void ShouldSetKTRNameFromManagedListNameOnUpdate_ManagedListFromPreImage()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("ManagedListNameValue").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithLocation(100000000, "Row")
                .Build();

            var preImage = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .WithLocation(100000000, "Row")
                .Build();

            _context.Initialize(new List<Entity> { managedList, qlManagedList, preImage });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = qlManagedList;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.AreEqual("ManagedListNameValue - Row", $"{qlManagedList.KTR_Name}");
        }

        [TestMethod]
        public void ShouldSetKTRNameFromManagedListNameOnUpdate_LocationFromPreImage()
        {
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).WithName("ManagedListNameValue").Build();

            var qlManagedList = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .Build();

            var preImage = new QuestionnaireLineSharedListBuilder()
                .WithProject(project)
                .WithManagedList(managedList)
                .WithLocation(100000000, "Row")
                .Build();

            _context.Initialize(new List<Entity> { managedList, qlManagedList, preImage });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = qlManagedList;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            _context.ExecutePluginWith<QuestionnaireLineManagedListPreOperation>(pluginContext);

            Assert.AreEqual("ManagedListNameValue - Row", $"{qlManagedList.KTR_Name}");
        }
    }
}
