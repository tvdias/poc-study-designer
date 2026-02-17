namespace Kantar.StudyDesignerLite.Plugins.Tests.TestHelpers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FakeXrmEasy;
    using Microsoft.Xrm.Sdk;

    public static class PluginContextFactory
    {
        public static XrmFakedPluginExecutionContext Create(
            string pluginName,
            IDictionary<string, object> inputParameters = null,
            IDictionary<string, object> outputParameters = null)
        {
            return new XrmFakedPluginExecutionContext
            {
                MessageName = pluginName,
                InputParameters = MapParameters(inputParameters),
                OutputParameters = MapParameters(outputParameters),
            };
        }

        private static ParameterCollection MapParameters(IDictionary<string, object> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return null;
            }

            var paramCollection = new ParameterCollection();
            foreach (var item in parameters)
            {
                paramCollection.Add(item.Key, item.Value);
            }
            return paramCollection;
        }
    }
}
