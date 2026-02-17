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
    /// Partial class for Subset Repository Subset Entities operations
    /// </summary>
    public partial class SubsetRepository : ISubsetRepository
    {
        public List<Guid> BulkInsertSubsetEntities(IList<KTR_SubsetEntities> subsetEntities)
        {
            var entities = subsetEntities.Select(sd => sd.ToEntity<Entity>()).ToList();

            var entitiesColletion = new EntityCollection(entities)
            {
                EntityName = KTR_SubsetEntities.EntityLogicalName
            };

            var request = new CreateMultipleRequest
            {
                Targets = entitiesColletion
            };

            var response = (CreateMultipleResponse)_service.Execute(request);
            return response.Ids.ToList();
        }

        public List<KTR_SubsetEntities> GetSubsetEntitiesByDefinitionIds(Guid[] subsetIds, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_SubsetEntities.Fields.Id,
                    KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion,
                    KTR_SubsetEntities.Fields.KTR_ManagedListEntity,
                };
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_SubsetEntities.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion,
                            ConditionOperator.In, subsetIds)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_SubsetEntities>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_SubsetEntities>())
                        .ToList();
        }

        public void DeleteSubsetEntity(Guid subsetEntityId)
        {
            _service.Delete(
                KTR_SubsetEntities.EntityLogicalName,
                subsetEntityId);
        }

        public void BulkDeleteSubsetEntity(IList<Guid> subsetEntityIds)
        {
            if (subsetEntityIds == null || subsetEntityIds.Count == 0)
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

            foreach (var id in subsetEntityIds)
            {
                request.Requests.Add(new DeleteRequest
                {
                    Target = new EntityReference(
                        KTR_SubsetEntities.EntityLogicalName,
                        id)
                });
            }

            _service.Execute(request);
        }

        public List<KTR_SubsetEntities> GetSubsetEntitiesByMLEntityIds(Guid[] mlEntityIds, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_SubsetEntities.Fields.Id,
                    KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion,
                    KTR_SubsetEntities.Fields.KTR_ManagedListEntity,
                };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_SubsetEntities.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_SubsetEntities.Fields.KTR_ManagedListEntity,
                            ConditionOperator.In, mlEntityIds)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_SubsetEntities>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_SubsetEntities>())
                        .ToList();
        }
    }
}
