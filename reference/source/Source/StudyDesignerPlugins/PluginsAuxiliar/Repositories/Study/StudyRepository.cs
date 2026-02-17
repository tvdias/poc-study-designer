namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class StudyRepository : IStudyRepository
    {
        private readonly IOrganizationService _service;

        public StudyRepository(IOrganizationService service)
        {
            _service = service;
        }

        public KT_Study Get(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KT_Study.Fields.Id,
                    KT_Study.Fields.KT_Name
                };
            }

            var entity = _service.Retrieve(
                KT_Study.EntityLogicalName,
                studyId,
                new ColumnSet(columns));

            return entity.ToEntity<KT_Study>();
        }

        public KT_Study Get(Guid studyId)
        {
            var entity = _service.Retrieve(
                KT_Study.EntityLogicalName,
                studyId,
                new ColumnSet(true));

            return entity.ToEntity<KT_Study>();
        }

        public List<KTR_StudyManagedListEntity> GetByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[] { KTR_StudyManagedListEntity.Fields.Id };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyManagedListEntity.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                        new ConditionExpression(KTR_StudyManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_StudyManagedListEntity_StatusCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null ?
                new List<KTR_StudyManagedListEntity>() :
                results.Entities
                    .Select(e => e.ToEntity<KTR_StudyManagedListEntity>())
                    .ToList();
        }

        public void UpdateStudyXml(Guid studyId, string xmlContent)
        {
            var studyToUpdate = new KT_Study()
            {
                Id = studyId
            };

            studyToUpdate[KT_Study.Fields.KTR_StudyXml] = xmlContent;
            _service.Update(studyToUpdate);
        }
    }
}
