namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Services
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Moq;

    [TestClass]
    public class ReorderServiceTests
    {
        private Mock<IOrganizationService> _serviceMock;
        private Mock<ITracingService> _tracingMock;
        private ReorderService _reorderService;

        private const string EntityName = "ktr_questionnaireline";
        private const string SortOrderField = "sortorder";

        [TestInitialize]
        public void Setup()
        {
            _serviceMock = new Mock<IOrganizationService>();
            _tracingMock = new Mock<ITracingService>();

            _reorderService = new ReorderService(
                _serviceMock.Object,
                _tracingMock.Object,
                EntityName,
                SortOrderField);
        }

        [TestMethod]
        public void ReorderEntities_ShouldCallExecuteMultiple_WithCorrectData()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            OrganizationRequest capturedRequest = null;

            _serviceMock
                .Setup(s => s.Execute(It.IsAny<OrganizationRequest>()))
                .Callback<OrganizationRequest>(req => capturedRequest = req)
                .Returns(new OrganizationResponse());

            _serviceMock
                .Setup(s => s.Execute(It.IsAny<OrganizationRequest>()))
                .Callback<OrganizationRequest>(req =>
                {
                    capturedRequest = req as ExecuteMultipleRequest;
                })
                .Returns(new ExecuteMultipleResponse
                {
                    ["Responses"] = new OrganizationResponseCollection(),
                    ["IsFaulted"] = false
                });

            // Act
            var success = _reorderService.ReorderEntities(ids);

            // Assert
            Assert.IsTrue(success);
            _tracingMock.Verify(t => t.Trace($"Reordering complete for {EntityName}."), Times.Once);
            _serviceMock.Verify(s => s.Execute(It.IsAny<ExecuteMultipleRequest>()), Times.Once);

            // Optional: verify the sort order mapping
            var executeRequest = capturedRequest as ExecuteMultipleRequest;
            if (executeRequest != null)
            {
                for (var i = 0; i < ids.Count; i++)
                {
                    var updateReq = executeRequest.Requests[i] as UpdateRequest;
                    Assert.IsNotNull(updateReq);
                    var entity = updateReq.Target;
                    Assert.AreEqual(ids[i], entity.Id);
                    Assert.AreEqual(i, entity[SortOrderField]);
                }
            }
        }
    }
}
