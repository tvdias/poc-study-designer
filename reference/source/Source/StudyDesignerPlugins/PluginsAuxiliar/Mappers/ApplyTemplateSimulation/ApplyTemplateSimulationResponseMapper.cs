using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product.ApplyTemplateSimulation;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.ApplyTemplateSimulation
{
    public static class ApplyTemplateSimulationResponseMapper
    {
        public static ApplyTemplateSimulationResponse MapToResponse(
            this IList<TemplateLineQuestionResult> result,
            IList<KT_QuestionBank> questions,
            IList<KTR_QuestionAnswerList> answers,
            IList<KT_Module> modules)
        {
            return new ApplyTemplateSimulationResponse
            {
                Questions = result
                    .Select(x => x.MapQuestionToResponse(questions, answers, modules))
                    .OrderBy(x => x.DisplayOrder)
                    .ToList()
            };
        }

        private static QuestionResponse MapQuestionToResponse(
            this TemplateLineQuestionResult result,
            IList<KT_QuestionBank> questions,
            IList<KTR_QuestionAnswerList> answers,
            IList<KT_Module> modules)
        {
            var question = questions
                .FirstOrDefault(x => x.Id == result.QuestionId);

            return new QuestionResponse
            {
                Id = result.QuestionId,
                QuestionTitle = question?.KT_QuestionTitle,
                DisplayOrder = result.DisplayOrder,
                Module = result.MapModuleToResponse(modules),
                Answers = result.MapAnswerToResponse(answers),
                QuestionType = question?.KT_QuestionType,
                SingleOrMultiCoded = question?.KT_SingleOrMultiCode,
                QuestionText = question?.KT_DefaultQuestionText,
                AnswerMin = question?.KTR_AnswerMin,
                AnswerMax = question?.KTR_AnswerMax,
                QuestionFormatDetails = question?.KTR_QuestionFormatDetails,
                CustomNotes = question?.KTR_CustomNotes,
                QuestionRationale = question?.KT_QuestionRationale
            };
        }

        private static ModuleResponse MapModuleToResponse(
            this TemplateLineQuestionResult result,
            IList<KT_Module> modules)
        {
            if ((result.ModuleId == null || result.ModuleId == Guid.Empty)
                && (modules == null || modules.Count == 0))
            {
                return null;
            }

            var module = modules
                .FirstOrDefault(x => x.Id == result.ModuleId);

            return module == null
                ? null
                : new ModuleResponse
                {
                    Id = result.ModuleId.GetValueOrDefault(),
                    Name = module.KT_Name
                };
        }

        private static IList<AnswerResponse> MapAnswerToResponse(
            this TemplateLineQuestionResult result,
            IList<KTR_QuestionAnswerList> answers)
        {
            if (result.QuestionId == Guid.Empty || answers == null || answers.Count == 0)
            {
                return new List<AnswerResponse>();
            }

            var questionAnswers = answers
                .Where(x => x.KTR_KT_QuestionBank.Id == result.QuestionId)
                .ToList();

            var answersResponse = new List<AnswerResponse>();
            foreach (var qa in questionAnswers)
            {
                var a = qa.MapAnswerToResponse();
                answersResponse.Add(a);
            }

            return answersResponse;
        }

        private static AnswerResponse MapAnswerToResponse(
            this KTR_QuestionAnswerList answer)
        {
            if (answer == null)
            {
                return null;
            }

            return new AnswerResponse
            {
                Id = answer.Id,
                Text = answer.KTR_AnswerText,
            };
        }
    }
}
