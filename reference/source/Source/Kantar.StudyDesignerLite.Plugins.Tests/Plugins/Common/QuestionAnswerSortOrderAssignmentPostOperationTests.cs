using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    [TestClass]
    public partial class QuestionAnswerSortOrderAssignmentPostOperationTests
    {
        protected XrmFakedContext _context;
        protected IOrganizationService _service;
        protected Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }
    }
}
