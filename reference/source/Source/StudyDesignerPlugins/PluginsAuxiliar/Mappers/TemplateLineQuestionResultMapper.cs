using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers
{
    internal static class TemplateLineQuestionResultMapper
    {
        internal static IList<TemplateLineQuestionResult> MapToResult(this IDictionary<Guid, TemplateLineQuestionContext> contexts)
        {
            var result = new List<TemplateLineQuestionResult>();

            foreach (var context in contexts)
            {
                var mappedResult = context.MapToResult();
                result.Add(mappedResult);
            }

            return result;
        }

        internal static TemplateLineQuestionResult MapToResult(this KeyValuePair<Guid, TemplateLineQuestionContext> context)
        {
            return new TemplateLineQuestionResult
            {
                ProductTemplateLine = context.Value.ProductTemplateLine,
                QuestionId = context.Value.QuestionId,
                ModuleId = context.Value.ModuleId,
                DisplayOrder = context.Value.DisplayOrder,
                CreatedOn = context.Value.CreatedOn,
            };
        }
    }
}
