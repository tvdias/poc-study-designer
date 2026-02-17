namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine
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
    public class QuestionnaireLineManagedListEntityRepository : IQuestionnaireLineManagedListEntityRepository
    {
        private readonly IOrganizationService _service;
        // Tracing service for logging due to the complexity of this repository
        private readonly ITracingService _tracing;

        public QuestionnaireLineManagedListEntityRepository(IOrganizationService service, ITracingService tracing)
        {
            _service = service;
            _tracing = tracing;
        }

        public IList<KTR_QuestionnaireLinemanAgedListEntity> GetByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[] { KTR_QuestionnaireLinemanAgedListEntity.Fields.Id };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                            ConditionOperator.Equal, studyId),
                        new ConditionExpression(
                            KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode,
                            ConditionOperator.Equal, (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            if (results == null || results.Entities == null || results.Entities.Count == 0)
            {
                _tracing.Trace("No QLMLEs found for the given StudyId.");
                return new List<KTR_QuestionnaireLinemanAgedListEntity>();
            }

            var qlmlEntities = results.Entities
                        .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                        .ToList();
            
            var managedListIds = qlmlEntities
                .Select(e => e.KTR_ManagedList.Id)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
            
            var questionnaireLineIds = qlmlEntities
                .Where(e => e.KTR_QuestionnaireLine != null && e.KTR_QuestionnaireLine.Id != Guid.Empty)
                .Select(e => e.KTR_QuestionnaireLine.Id)
                .Distinct()
                .ToList();
            
            var qlMls = GetQuestionnaireLineManagedLists(managedListIds, questionnaireLineIds);
            
            if (qlMls == null || qlMls.Count == 0)
            {
                return qlmlEntities;
            }

            var validQlmlEntities = qlmlEntities
                .Where(qlml => qlMls.Any(qlmlRel =>
                    qlml.KTR_ManagedList != null
                    && qlmlRel.KTR_ManagedList != null
                    && qlml.KTR_QuestionnaireLine != null
                    && qlmlRel.KTR_QuestionnaireLine != null
                    && qlmlRel.KTR_ManagedList.Id == qlml.KTR_ManagedList.Id
                    && qlmlRel.KTR_QuestionnaireLine.Id == qlml.KTR_QuestionnaireLine.Id))
                .ToList();

            return validQlmlEntities;
        }

        // Dataverse can't handle inner joins with multiple condtions... that's why this workaround is needed
        private IList<KTR_QuestionnaireLinesHaRedList> GetQuestionnaireLineManagedLists(List<Guid> managedListIds, List<Guid> questionnaireLineIds, string[] columns = null)
        {
            if ((managedListIds == null || managedListIds.Count == 0) || (questionnaireLineIds == null || questionnaireLineIds.Count == 0))
            {
                return new List<KTR_QuestionnaireLinesHaRedList>();
            }

            if (columns == null || columns.Length == 0)
            {
                columns = new string[] {
                    KTR_QuestionnaireLinesHaRedList.Fields.Id,
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine,
                    KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList
                };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinesHaRedList.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLinesHaRedList.Fields.KTR_ManagedList,
                            ConditionOperator.In, managedListIds.Cast<object>().ToArray()),
                        new ConditionExpression(
                            KTR_QuestionnaireLinesHaRedList.Fields.KTR_QuestionnaireLine,
                            ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                        new ConditionExpression(
                            KTR_QuestionnaireLinesHaRedList.Fields.StateCode,
                            ConditionOperator.Equal, (int)KTR_QuestionnaireLinesHaRedList_StateCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null ?
                    new List<KTR_QuestionnaireLinesHaRedList>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_QuestionnaireLinesHaRedList>())
                        .ToList();
        }

        public List<KTR_QuestionnaireLinemanAgedListEntity> GetByEntityId(Guid mlEntityId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.Id
                };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity, ConditionOperator.Equal, mlEntityId),
                        new ConditionExpression(KTR_QuestionnaireLinemanAgedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Active)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);

            return results == null ?
                new List<KTR_QuestionnaireLinemanAgedListEntity>() :
                results.Entities
                    .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                    .ToList();
        }

        public void BulkUpdateStatus(
            IEnumerable<KTR_QuestionnaireLinemanAgedListEntity> entities,
            KTR_QuestionnaireLinemanAgedListEntity_StateCode state,
            KTR_QuestionnaireLinemanAgedListEntity_StatusCode status)
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
                var update = new Entity(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, row.Id)
                {
                    [KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode] = new OptionSetValue((int)state),
                    [KTR_QuestionnaireLinemanAgedListEntity.Fields.StatusCode] = new OptionSetValue((int)status)
                };

                batch.Requests.Add(new UpdateRequest { Target = update });
            }

            if (batch.Requests.Any())
            {
                _service.Execute(batch);
            }
        }

        public List<KTR_QuestionnaireLinemanAgedListEntity>
        GetDraftStudyQLMLEsByEntityId(Guid mlEntityId)
        {
            // Get all ACTIVE QLMLE for this MLE
            var qlMles = GetByEntityId(
                mlEntityId,
                new string[]
                {
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId
                }
            );

            // Filter only QLMLEs whose Study is Draft
            var draftOnly = new List<KTR_QuestionnaireLinemanAgedListEntity>();

            foreach (var qlmle in qlMles)
            {
                var studyRef = qlmle.GetAttributeValue<EntityReference>(
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId);

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
                    draftOnly.Add(qlmle);
                }
            }

            return draftOnly;
        }

        // For QL deactivation
        public List<KTR_QuestionnaireLinemanAgedListEntity>
        GetDraftStudyQLMLEsByQuestionnaireLineId(Guid questionnaireLineId)
        {
            var query = new QueryExpression(
                KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine
                )
            };

            // QLMLE must be Active
            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active);

            // QLMLE must belong to the deleted/deactivated QuestionnaireLine
            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine,
                ConditionOperator.Equal,
                questionnaireLineId);

            // Join to Study to enforce Draft only
            var studyLink = query.AddLink(
                KT_Study.EntityLogicalName,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                KT_Study.Fields.Id,
                JoinOperator.Inner);

            studyLink.LinkCriteria.AddCondition(
                KT_Study.Fields.StatusCode,
                ConditionOperator.Equal,
                (int)KT_Study_StatusCode.Draft);

            var results = _service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                .ToList();
        }

        //For QL deletion (needs fetch)
        public List<KTR_QuestionnaireLinemanAgedListEntity> GetActiveQLMLEsForPreDelete(Guid questionnaireLineId, ITracingService tracing)
        {
            tracing.Trace($"Fetching QLMLEs for QuestionnaireLine {questionnaireLineId} using FetchXML pre-delete");

            string fetchXml = $@"
             <fetch>
              <entity name='ktr_questionnairelinemanagedlistentity'>
                <attribute name='ktr_questionnairelinemanagedlistentityid' />
                <attribute name='ktr_studyid' />
                <attribute name='ktr_managedlistentity' />
                <link-entity name='kt_questionnairelines' from='kt_questionnairelinesid' to='ktr_questionnaireline' link-type='inner'>
                  <filter>
                    <condition attribute='kt_questionnairelinesid' operator='eq' value='{questionnaireLineId}' />
                  </filter>
                </link-entity>
                <filter>
                  <condition attribute='statecode' operator='eq' value='0' /> <!-- Active -->
                </filter>
              </entity>
            </fetch>";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));

            tracing.Trace($"QLMLEs fetched: {results.Entities.Count}");

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                .ToList();
        }

        public bool HasActiveQLMLEs(Guid studyId, Guid mleId)
        {
            var query = new QueryExpression(
                KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(false),
                TopCount = 1
            };

            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                ConditionOperator.Equal,
                studyId);

            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                ConditionOperator.Equal,
                mleId);

            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active);

            var result = _service.RetrieveMultiple(query);

            return result.Entities.Any();
        }

        public List<KTR_QuestionnaireLinemanAgedListEntity>
        GetActiveByStudyAndQuestionnaireLine(Guid studyId, Guid questionnaireLineId)
        {
            var query = new QueryExpression(
                KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                    KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine
                )
            };

            // Study filter
            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_StudyId,
                ConditionOperator.Equal,
                studyId);

            // Questionnaire Line filter
            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine,
                ConditionOperator.Equal,
                questionnaireLineId);

            // Active only
            query.Criteria.AddCondition(
                KTR_QuestionnaireLinemanAgedListEntity.Fields.StateCode,
                ConditionOperator.Equal,
                (int)KTR_QuestionnaireLinemanAgedListEntity_StateCode.Active);

            var results = _service.RetrieveMultiple(query);

            return results == null
                ? new List<KTR_QuestionnaireLinemanAgedListEntity>()
                : results.Entities
                    .Select(e => e.ToEntity<KTR_QuestionnaireLinemanAgedListEntity>())
                    .ToList();
        }
    }
}
