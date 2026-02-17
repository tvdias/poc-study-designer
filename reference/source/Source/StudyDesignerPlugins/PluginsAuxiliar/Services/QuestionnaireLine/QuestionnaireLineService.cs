namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Services;
    using System.Web.Services.Description;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerList;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class QuestionnaireLineService
    {
        private readonly ITracingService _tracing;
        private readonly IOrganizationService _service;
        private readonly QuestionnaireLineAnswerListRepository _questionAnswerRepository;
        private readonly ManagedListRepository _managedListRepository;

        public QuestionnaireLineService(
            IOrganizationService service,
            ITracingService tracing,
            QuestionnaireLineAnswerListRepository questionAnswerRepository,
            ManagedListRepository managedListRepository)
        {
            _service = service;
            _tracing = tracing;
            _questionAnswerRepository = questionAnswerRepository;
            _managedListRepository = managedListRepository;
        }

        public void RegenerateHtmlField(IList<Guid> questionnaireLineIds)
        {
            if (questionnaireLineIds.Count == 0)
            {
                return;
            }

            foreach (var questionnaireLineId in questionnaireLineIds)
            {
                var questionToUpdate = (KT_QuestionnaireLines)_service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId, new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionType));

                if (questionToUpdate == null)
                {
                    _tracing.Trace("QuestionnaireLineId is missing or empty.");
                    throw new InvalidPluginExecutionException("QuestionnaireLineId is missing or empty.");
                }

                var answers = _questionAnswerRepository.GetQuestionnaireLinesAnswerLists(_service, questionnaireLineId);
                _tracing.Trace($"Found {answers.Count} answers.");

                var managedListsAsRows = _managedListRepository.GetManagedListsByLocation(_service, questionnaireLineId, KTR_Location.Row);
                _tracing.Trace($"Found {managedListsAsRows.Count} managed List as Rows.");

                var managedListsAsColumns = _managedListRepository.GetManagedListsByLocation(_service, questionnaireLineId, KTR_Location.Column);
                _tracing.Trace($"Found {managedListsAsColumns.Count} managed List as Columns.");

                //Update Answer HTML
                questionToUpdate.KTR_AnswerList = HtmlGenerationHelper.GenerateAnswerListHtml(answers, managedListsAsRows, managedListsAsColumns);
                _service.Update(questionToUpdate);
            }
        }
    }
}
