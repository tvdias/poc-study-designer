namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Partial class for Subset Repository QL Subsets operations
    /// </summary>
    public partial class SubsetRepository : ISubsetRepository
    {
        public List<Guid> BulkInsertQLSubsets(IList<KTR_QuestionnaireLineSubset> qlSubsets)
        {
            var entities = qlSubsets.Select(sd => sd.ToEntity<Entity>()).ToList();

            var entitiesColletion = new EntityCollection(entities)
            {
                EntityName = KTR_QuestionnaireLineSubset.EntityLogicalName
            };

            var request = new CreateMultipleRequest
            {
                Targets = entitiesColletion
            };

            var response = (CreateMultipleResponse)_service.Execute(request);
            return response.Ids.ToList();
        }

        public List<KTR_QuestionnaireLineSubset> GetQLSubsetsByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_QuestionnaireLineSubset.Fields.Id,
                    KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId,
                    KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId,
                    KTR_QuestionnaireLineSubset.Fields.KTR_StudyMaster,
                    KTR_QuestionnaireLineSubset.Fields.KTR_Study,
                };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLineSubset.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLineSubset.Fields.KTR_Study,
                            ConditionOperator.Equal, studyId)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_QuestionnaireLineSubset>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_QuestionnaireLineSubset>())
                        .ToList();
        }

        public void DeleteQLSubset(Guid qlSubsetId)
        {
            _service.Delete(KTR_QuestionnaireLineSubset.EntityLogicalName, qlSubsetId);
        }

        public void BulkDeleteQLSubset(IList<Guid> qlSubsetIds)
        {
            if (qlSubsetIds == null || qlSubsetIds.Count == 0)
            {
                return;
            }

            var request = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                },
                Requests = new OrganizationRequestCollection()
            };

            foreach (var id in qlSubsetIds)
            {
                request.Requests.Add(new DeleteRequest
                {
                    Target = new EntityReference(
                        KTR_QuestionnaireLineSubset.EntityLogicalName,
                        id)
                });
            }

            _service.Execute(request);
        }
    }
}
