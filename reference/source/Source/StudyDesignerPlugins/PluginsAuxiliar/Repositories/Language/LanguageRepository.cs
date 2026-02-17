namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Language
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Repository for KTR_Language entity operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LanguageRepository : ILanguageRepository
    {
        private readonly IOrganizationService _service;

        public LanguageRepository(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IEnumerable<KTR_Language> GetStudyLanguages(Guid studyId)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_Language.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_Language.Fields.Id,
                    KTR_Language.Fields.KTR_LanguageName,
                    KTR_Language.Fields.KTR_LocaleCode)
            };

            query.Criteria.AddCondition(
                KTR_Language.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_Language_StateCode.Active
            );

            var fwLink = query.AddLink(
                KTR_FieldworkLanguages.EntityLogicalName,
                KTR_Language.Fields.Id,
                KTR_FieldworkLanguages.Fields.KTR_Language
            );
            fwLink.JoinOperator = JoinOperator.Inner;

            fwLink.LinkCriteria.AddCondition(
                KTR_FieldworkLanguages.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_FieldworkLanguages_StateCode.Active
            );

            var studyLink = fwLink.AddLink(
                KT_Study.EntityLogicalName,
                KTR_FieldworkLanguages.Fields.KTR_Study,
                KT_Study.Fields.Id
            );
            studyLink.JoinOperator = JoinOperator.Inner;

            studyLink.LinkCriteria.AddCondition(
                KT_Study.Fields.Id,
                ConditionOperator.Equal,
                studyId
            );

            var results = _service.RetrieveMultiple(query);
            return results.Entities.Select(e => e.ToEntity<KTR_Language>()).ToList();
        }
    }
}
