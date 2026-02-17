using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Converters
{
    public class QuestionRequestConverter : JsonConverter<QuestionRequest>
    {
        public override QuestionRequest ReadJson(JsonReader reader, Type objectType, QuestionRequest existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            var operationToken = jo.Properties()
                .FirstOrDefault(p => string.Equals(p.Name, "Origin", StringComparison.OrdinalIgnoreCase));

            if (operationToken == null)
            {
                throw new JsonSerializationException("Missing 'Origin' field.");
            }

            var originString = operationToken.Value.ToString();

            if (!Enum.TryParse<QuestionRequestOrigin>(originString, ignoreCase: true, out var origin))
            {
                throw new JsonSerializationException($"Invalid Origin value: '{originString}'");
            }
            
            QuestionRequest result;
            switch (origin)
            {
                case QuestionRequestOrigin.New:
                    result = new NewQuestionRequest();
                    break;
                case QuestionRequestOrigin.QuestionBank:
                    result = new ExistingQuestionRequest();
                    break;
                default:
                    throw new JsonSerializationException($"Unsupported Origin value: {origin}");
            }

            serializer.Populate(jo.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, QuestionRequest value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
