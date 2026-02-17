using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ManagedListEntity;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedListEntity
{
    [TestClass]
    public class UpdateManagedListEntityPreValidationOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private KT_Project _project;
        private KTR_ManagedList _managedList;

        [TestInitialize]
        public void Init()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _project = new ProjectBuilder().Build();
            _managedList = new ManagedListBuilder(_project).WithName("ML").Build();
        }

        private XrmFakedPluginExecutionContext GetUpdateContext(Entity target, string preImageName = "PreImage", Entity preImage = null)
        {
            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = "Update";
            ctx.InputParameters = new ParameterCollection { { "Target", target } };
            if (preImage != null)
            {
                ctx.PreEntityImages = new EntityImageCollection
                {
                    { preImageName, preImage }
                };
            }
            return ctx;
        }

        [TestMethod]
        public void Should_Return_When_Message_Not_Update()
        {
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode("A1").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle });
            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = "Create";
            ctx.InputParameters = new ParameterCollection { { "Target", mle } };
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Return_When_Target_Not_ManagedListEntity()
        {
            var other = new Entity("account") { Id = Guid.NewGuid() };
            _context.Initialize(new List<Entity> { other });
            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = "Update";
            ctx.InputParameters = new ParameterCollection { { "Target", other } };
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Return_When_AnswerCode_Not_In_Target()
        {
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode("OLD").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle.Id };
            var ctx = GetUpdateContext(target);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Return_When_AnswerCode_Unchanged_CaseInsensitive()
        {
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode("Code1").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "code1";
            var pre = mle.ToEntity<Entity>();
            var ctx = GetUpdateContext(target, preImage: pre);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Should_Block_When_Duplicate_AnswerCode_In_Same_List()
        {
            var mle1 = new ManagedListEntityBuilder(_managedList).WithAnswerCode("A1").Build();
            var mle2 = new ManagedListEntityBuilder(_managedList).WithAnswerCode("B1").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle1, mle2 });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle2.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "A1";
            var pre = mle2.ToEntity<Entity>();
            var ctx = GetUpdateContext(target, preImage: pre);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
        }

        [TestMethod]
        public void Should_Allow_When_New_AnswerCode_Unique()
        {
            var mle1 = new ManagedListEntityBuilder(_managedList).WithAnswerCode("A1").Build();
            var mle2 = new ManagedListEntityBuilder(_managedList).WithAnswerCode("B1").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle1, mle2 });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle2.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "C1";
            var pre = mle2.ToEntity<Entity>();
            var ctx = GetUpdateContext(target, preImage: pre);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Handle_No_PreImage_Fallback_Retrieve()
        {
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode("OLD").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "NEW";
            var ctx = GetUpdateContext(target, preImage: null);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Handle_Target_ManagedList_Change()
        {
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode("OLD").Build();
            var otherList = new ManagedListBuilder(_project).WithName("Other").Build();
            _context.Initialize(new List<Entity> { _project, _managedList, otherList, mle });
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "NEW";
            target[KTR_ManagedListEntity.Fields.KTR_ManagedList] = new EntityReference(otherList.LogicalName, otherList.Id);
            var pre = mle.ToEntity<Entity>();
            var ctx = GetUpdateContext(target, preImage: pre);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Should_Allow_When_OldAnswerCode_Null_New_Populated_By_PA()
        {
            // Arrange
            var mle = new ManagedListEntityBuilder(_managedList).WithAnswerCode(null).Build();
            _context.Initialize(new List<Entity> { _project, _managedList, mle });

            // Target update simulating Power Automate setting the first AnswerCode
            var target = new Entity(KTR_ManagedListEntity.EntityLogicalName) { Id = mle.Id };
            target[KTR_ManagedListEntity.Fields.KTR_AnswerCode] = "PA_SET_CODE"; // new value from PA

            // Pre-image simulating old record
            var pre = mle.ToEntity<Entity>();

            // Act
            var ctx = GetUpdateContext(target, preImage: pre);
            _context.ExecutePluginWith<UpdateManagedListEntityPreValidationOperation>(ctx);

            // Assert
            Assert.IsTrue(true, "Update should be allowed when old AnswerCode is null and new AnswerCode is set by Power Automate");
        }
    }
}
