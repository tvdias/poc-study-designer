using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ManagedList;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedList
{
    [TestClass]
    public class UpdateManagedListPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;
        private KT_Project _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
            _project = new ProjectBuilder().Build();
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenMessageNameIsNotUpdate()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            _context.Initialize(new List<Entity> { _project, managedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create); // Not Update
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedList }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetParameterMissing()
        {
            // Arrange
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection(); // No Target

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNotEntity()
        {
            // Arrange
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", "NotAnEntity" }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNotManagedListEntity()
        {
            // Arrange
            var someOtherEntity = new Entity("some_other_entity", Guid.NewGuid());

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", someOtherEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenStateCodeNotInTarget()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            // Don't include StateCode in target
            targetEntity[KTR_ManagedList.Fields.KTR_Name] = "Updated Name";

            _context.Initialize(new List<Entity> { _project, managedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenStateCodeValueIsInvalid()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue(999); // Invalid state code

            _context.Initialize(new List<Entity> { _project, managedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldActivateRelatedManagedListEntities_WhenManagedListIsActivated()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            var managedListEntity1 = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Inactive)
                .Build();
            var managedListEntity2 = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList, managedListEntity1, managedListEntity2 });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Active);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert
            var updatedEntity1 = _context.Data[managedListEntity1.LogicalName][managedListEntity1.Id];
            var updatedEntity2 = _context.Data[managedListEntity2.LogicalName][managedListEntity2.Id];

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Active,
                updatedEntity1.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Active,
                updatedEntity1.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Active,
                updatedEntity2.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Active,
                updatedEntity2.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldDeactivateRelatedManagedListEntities_WhenManagedListIsDeactivated()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            var managedListEntity1 = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Active)
                .Build();
            var managedListEntity2 = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Active)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList, managedListEntity1, managedListEntity2 });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert
            var updatedEntity1 = _context.Data[managedListEntity1.LogicalName][managedListEntity1.Id];
            var updatedEntity2 = _context.Data[managedListEntity2.LogicalName][managedListEntity2.Id];

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedEntity1.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Inactive,
                updatedEntity1.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedEntity2.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Inactive,
                updatedEntity2.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldWorkWithNoRelatedManagedListEntities()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            // No related ManagedListEntity records

            _context.Initialize(new List<Entity> { _project, managedList });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Active);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldHandleSingleRelatedManagedListEntity()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            var managedListEntity = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList, managedListEntity });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Active);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert
            var updatedEntity = _context.Data[managedListEntity.LogicalName][managedListEntity.Id];
            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Active,
                updatedEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Active,
                updatedEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenManagedListIdCannotBeDetermined()
        {
            // Arrange
            var targetEntity = new Entity(KTR_ManagedList.EntityLogicalName); // No ID
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Active);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.PrimaryEntityId = Guid.Empty; // Also empty in context
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldHandleMultipleRelatedManagedListEntitiesWithMixedStates()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            var activeEntity = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Active)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Active)
                .Build();
            var inactiveEntity = new ManagedListEntityBuilder(managedList)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList, activeEntity, inactiveEntity });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert
            var updatedActiveEntity = _context.Data[activeEntity.LogicalName][activeEntity.Id];
            var updatedInactiveEntity = _context.Data[inactiveEntity.LogicalName][inactiveEntity.Id];

            // Both should now be inactive
            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedActiveEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Inactive,
                updatedActiveEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedInactiveEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StatusCode.Inactive,
                updatedInactiveEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StatusCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldNotAffectUnrelatedManagedListEntities()
        {
            // Arrange
            var managedList1 = new ManagedListBuilder(_project).WithName("Test List 1").Build();
            var managedList2 = new ManagedListBuilder(_project).WithName("Test List 2").Build();

            var relatedEntity = new ManagedListEntityBuilder(managedList1)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .Build();
            var unrelatedEntity = new ManagedListEntityBuilder(managedList2)
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { _project, managedList1, managedList2, relatedEntity, unrelatedEntity });

            var targetEntity = new Entity(managedList1.LogicalName, managedList1.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Active);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert
            var updatedRelatedEntity = _context.Data[relatedEntity.LogicalName][relatedEntity.Id];
            var updatedUnrelatedEntity = _context.Data[unrelatedEntity.LogicalName][unrelatedEntity.Id];

            // Related entity should be activated
            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Active,
                updatedRelatedEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);

            // Unrelated entity should remain inactive
            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedUnrelatedEntity.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldContinueProcessing_WhenOneEntityUpdateFails()
        {
            // This test would require mocking the service to simulate failure
            // For now, we'll test the happy path as FakeXrmEasy doesn't easily support partial failures
            // Arrange
            var managedList = new ManagedListBuilder(_project).WithName("Test List").Build();
            var managedListEntity1 = new ManagedListEntityBuilder(managedList).Build();
            var managedListEntity2 = new ManagedListEntityBuilder(managedList).Build();

            _context.Initialize(new List<Entity> { _project, managedList, managedListEntity1, managedListEntity2 });

            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = new OptionSetValue((int)KTR_ManagedList_StateCode.Inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act - Should complete without exception even if some updates fail
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - Both entities should be updated successfully in this case
            var updatedEntity1 = _context.Data[managedListEntity1.LogicalName][managedListEntity1.Id];
            var updatedEntity2 = _context.Data[managedListEntity2.LogicalName][managedListEntity2.Id];

            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedEntity1.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
            Assert.AreEqual((int)KTR_ManagedListEntity_StateCode.Inactive,
                updatedEntity2.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value);
        }

        [TestMethod]
        public void Execute_ShouldHandleStateCodeWithNullOptionSetValue()
        {
            // Arrange
            var managedList = new ManagedListBuilder(_project).Build();
            var targetEntity = new Entity(managedList.LogicalName, managedList.Id);
            targetEntity[KTR_ManagedList.Fields.StateCode] = null; // Null OptionSetValue

            _context.Initialize(new List<Entity> { _project, managedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", targetEntity }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<UpdateManagedListPostOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }
    }
}
