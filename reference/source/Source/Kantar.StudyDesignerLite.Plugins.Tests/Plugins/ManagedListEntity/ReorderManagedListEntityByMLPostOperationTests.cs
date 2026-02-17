namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedListEntity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.ManagedListEntity;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class ReorderManagedListEntityByMLPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Init()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void Should_Reorder_Sucessfully()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            var managedList = new ManagedListBuilder(project)
                .Build();

            var managedListEntity1 = new ManagedListEntityBuilder()
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithManagedList(managedList)
                .WithDisplayOrder(2)
                .Build();
            var managedListEntity2 = new ManagedListEntityBuilder()
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithManagedList(managedList)
                .WithDisplayOrder(7)
                .Build();
            var newManagedListEntity = new ManagedListEntityBuilder()
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithManagedList(managedList)
                .WithDisplayOrder(888)
                .Build();

            _context.Initialize(new List<Entity>
            {
                project,
                managedList,
                managedListEntity1,
                managedListEntity2,
                newManagedListEntity
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.InputParameters["Target"] = newManagedListEntity;
            pluginContext.MessageName = "Create";
            
            // Act
            _context.ExecutePluginWith<ReorderManagedListEntityByMLPostOperation>(pluginContext);

            // Assert
            var ordered = _context.CreateQuery("ktr_managedlistentity")
                .OrderBy(e => e.GetAttributeValue<int?>("ktr_displayorder"))
                .ToList();

            Assert.AreEqual(3, ordered.Count); // 2 existing + 1 new
            Assert.AreEqual(0, ordered[0].GetAttributeValue<int?>("ktr_displayorder")); // smallest first
        }

    }
}
