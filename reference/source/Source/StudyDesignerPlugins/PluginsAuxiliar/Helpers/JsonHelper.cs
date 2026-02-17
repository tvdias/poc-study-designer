using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    public static class JsonHelper
    {
        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    Converters = new[] { new StringEnumConverter() },
                    Formatting = Formatting.None
                };

                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (JsonException ex)
            {
                throw new InvalidPluginExecutionException($"Failed to serialize object: {ex.Message}");
            }
        }

        public static T Deserialize<T>(string json, string contextName = "input", JsonSerializerSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidPluginExecutionException($"Empty or null JSON in {contextName}.");
            }

            try
            {
                if (settings == null)
                {
                    settings = new JsonSerializerSettings
                    {
                        Converters = new[] { new StringEnumConverter() }
                    };
                }

                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (JsonException ex)
            {
                throw new InvalidPluginExecutionException($"Failed to deserialize {contextName}: {ex.Message}");
            }
        }
    }
}