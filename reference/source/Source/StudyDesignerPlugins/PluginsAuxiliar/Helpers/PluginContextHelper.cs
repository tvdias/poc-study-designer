using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    public static class PluginContextHelper
    {
        public static T GetInputParameter<T>(this IPluginExecutionContext context, string key)
        {
            if (context.InputParameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            throw new InvalidPluginExecutionException($"Missing or invalid input parameter: {key}");
        }
    }
}
