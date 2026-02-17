namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Project
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Microsoft.Xrm.Sdk;

    public class ProjectService
    {
        private readonly ITracingService _tracing;
        private readonly IOrganizationService _service;

        private const string EntityLogicalName = "kt_questionnairelines";
        private const string SortOrderFieldName = "kt_questionsortorder";

        public ProjectService(
            IOrganizationService service,
            ITracingService tracing)
        {
            _service = service;
            _tracing = tracing;
        }

        public IList<Guid> ReorderProjectQuestionnaire(Guid projectId)
        {
            var qlRepository = new QuestionnaireLineRepository(_service);

            var questionnaireLines = qlRepository.GetQuestionnaireLinesByProjectId(
               projectId,
               columns: new string[]
                {
                    KT_QuestionnaireLines.Fields.Id,
                    KT_QuestionnaireLines.Fields.KT_QuestionSortOrder,
                    KT_QuestionnaireLines.Fields.CreatedOn,
                    KT_QuestionnaireLines.Fields.KT_QuestionVariableName,
                });

            if (questionnaireLines == null || questionnaireLines.Count == 0)
            {
                _tracing.Trace($"No QuestionnaireLines found in Project: {projectId}.");
                return new List<Guid>();
            }

            var reorderService = new ReorderService(
                _service,
                _tracing,
                EntityLogicalName,
                SortOrderFieldName);

            var ids = questionnaireLines
                .OrderBy(ql => ql.KT_QuestionSortOrder)
                .ThenBy(ql => ql.CreatedOn)
                .ThenBy(ql => ql.KT_QuestionVariableName)
                .Select(ql => ql.Id)
                .ToList();

            var success = reorderService.ReorderEntities(ids);

            return ids;
        }
    }
}
