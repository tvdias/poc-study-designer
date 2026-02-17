namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.StudyManagedlistEntity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class StudyManagedlistEntityRepository
    {
        private readonly IOrganizationService _service;

        public StudyManagedlistEntityRepository(IOrganizationService service)
        {
            _service = service;
        }

        public List<KTR_StudyManagedListEntity> GetByEntityId(
            Guid mlEntityId,
            string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_StudyManagedListEntity.Fields.Id,
                    KTR_StudyManagedListEntity.Fields.KTR_Study
                };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_StudyManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity, ConditionOperator.Equal, mlEntityId),
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

        public void BulkUpdateStatus(
            IEnumerable<KTR_StudyManagedListEntity> entities,
            KTR_StudyManagedListEntity_StateCode state,
            KTR_StudyManagedListEntity_StatusCode status)
        {
            if (entities == null)
            {
                return;
            }

            var batch = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            foreach (var row in entities)
            {
                var update = new Entity(KTR_StudyManagedListEntity.EntityLogicalName, row.Id)
                {
                    [KTR_StudyManagedListEntity.Fields.StateCode] = new OptionSetValue((int)state),
                    [KTR_StudyManagedListEntity.Fields.StatusCode] = new OptionSetValue((int)status)
                };

                batch.Requests.Add(new UpdateRequest { Target = update });
            }

            if (batch.Requests.Any())
            {
                _service.Execute(batch);
            }
        }

        public List<KTR_StudyManagedListEntity> GetDraftStudyMLEsByEntityId(Guid mlEntityId)
        {
            var allStudyMles = GetByEntityId(
                mlEntityId,
                new string[]
                {
                    KTR_StudyManagedListEntity.Fields.Id,
                    KTR_StudyManagedListEntity.Fields.KTR_Study
                });

            var draftOnly = new List<KTR_StudyManagedListEntity>();

            foreach (var studyMle in allStudyMles)
            {
                var studyRef = studyMle.GetAttributeValue<EntityReference>(
                    KTR_StudyManagedListEntity.Fields.KTR_Study);

                if (studyRef == null)
                {
                    continue;
                }

                var study = _service.Retrieve(
                    studyRef.LogicalName,
                    studyRef.Id,
                    new ColumnSet(KT_Study.Fields.StatusCode));

                var studyStatus = study.GetAttributeValue<OptionSetValue>(
                    KT_Study.Fields.StatusCode)?.Value;

                if (studyStatus == (int)KT_Study_StatusCode.Draft)
                {
                    draftOnly.Add(studyMle);
                }
            }

            return draftOnly;
        }

        public KTR_StudyManagedListEntity GetByStudyAndMLE(Guid studyId, Guid mleId)
        {
            var query = new QueryExpression
            {
                EntityName = KTR_StudyManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_StudyManagedListEntity.Fields.Id,
                    KTR_StudyManagedListEntity.Fields.KTR_Study,
                    KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity
                ),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression(KTR_StudyManagedListEntity.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                new ConditionExpression(KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity, ConditionOperator.Equal, mleId)
            }
                }
            };

            var result = _service.RetrieveMultiple(query);

            return result.Entities
                .Select(e => e.ToEntity<KTR_StudyManagedListEntity>())
                .FirstOrDefault();
        }

        public List<KTR_StudyManagedListEntity> GetByStudyId(
            Guid studyId,
            string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new[]
                {
                    KTR_StudyManagedListEntity.Fields.Id,
                    KTR_StudyManagedListEntity.Fields.KTR_Study,
                    KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity
                };
            }

            var query = new QueryExpression
            {
                EntityName = KTR_StudyManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudyManagedListEntity.Fields.KTR_Study,
                            ConditionOperator.Equal,
                            studyId),

                        new ConditionExpression(
                            KTR_StudyManagedListEntity.Fields.StateCode,
                            ConditionOperator.Equal,
                            (int)KTR_StudyManagedListEntity_StateCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null
                ? new List<KTR_StudyManagedListEntity>()
                : results.Entities
                    .Select(e => e.ToEntity<KTR_StudyManagedListEntity>())
                    .ToList();
        }
    }
}
