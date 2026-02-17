namespace Kantar.StudyDesignerLite.Plugins.Tests.TestHelpers
{
    using System.Collections.Generic;
    using FakeXrmEasy;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

    public static class XrmFakedContextHelper
    {
        public static XrmFakedContext Mock(
            this XrmFakedContext context,
            IEnumerable<Entity> entities = null,
            bool isFaulted = false)
        {
            if (entities != null)
            {
                context.Initialize(entities);
            }

            context.AddExecutionMock<ExecuteMultipleRequest>(req =>
                new ExecuteMultipleResponse
                {
                    ["Responses"] = new ExecuteMultipleResponseItemCollection(),
                    ["IsFaulted"] = isFaulted
                });

            return context;
        }
    }
}
