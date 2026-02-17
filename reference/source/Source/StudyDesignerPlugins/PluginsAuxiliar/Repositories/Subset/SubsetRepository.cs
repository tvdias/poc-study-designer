namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Partial class for Subset Repository
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class SubsetRepository : ISubsetRepository
    {
        private readonly IOrganizationService _service;

        public SubsetRepository(IOrganizationService service)
        {
            _service = service;
        }

        public IList<KTR_SubsetDefinition> GetByMasterStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[] { KTR_SubsetDefinition.Fields.Id };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_SubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_SubsetDefinition.Fields.KTR_MasterStudyId, ConditionOperator.Equal, studyId)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_SubsetDefinition>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_SubsetDefinition>())
                        .ToList();
        }

        public List<Guid> BulkInsert(IList<KTR_SubsetDefinition> subsetDefinition)
        {
            var entities = subsetDefinition
                .Select(sd => sd.ToEntity<Entity>())
                .ToList();

            var entitiesColletion = new EntityCollection(entities)
            {
                EntityName = KTR_SubsetDefinition.EntityLogicalName
            };

            var request = new CreateMultipleRequest
            {
                Targets = entitiesColletion
            };

            var response = (CreateMultipleResponse)_service.Execute(request);
            return response.Ids.ToList();
        }

        public void Delete(Guid subsetDefinitionId)
        {
            _service.Delete(
                KTR_SubsetDefinition.EntityLogicalName,
                subsetDefinitionId);
        }

        public void BulkDelete(IList<Guid> subsetDefinitionIds)
        {
            var request = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                },
                Requests = new OrganizationRequestCollection()
            };

            foreach (var subsetDefinitionId in subsetDefinitionIds)
            {
                request.Requests.Add(new DeleteRequest
                {
                    Target = new EntityReference(
                        KTR_SubsetDefinition.EntityLogicalName,
                        subsetDefinitionId)
                });
            }

            _service.Execute(request);
        }

        public List<KTR_StudySubsetDefinition> GetSubsetStudyAssociationByStudyId(Guid studyId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_StudySubsetDefinition.Fields.Id,
                    KTR_StudySubsetDefinition.Fields.KTR_Study,
                    KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition,
                };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_StudySubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudySubsetDefinition.Fields.KTR_Study,
                            ConditionOperator.Equal, studyId)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_StudySubsetDefinition>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_StudySubsetDefinition>())
                        .ToList();
        }

        public List<KTR_StudySubsetDefinition> GetSubsetAssociationBySubsetIds(Guid[] associationIds, string[] columns = null)
        {
            if (associationIds == null || associationIds.Length == 0)
            {
                return new List<KTR_StudySubsetDefinition>();
            }
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_StudySubsetDefinition.Fields.Id,
                    KTR_StudySubsetDefinition.Fields.KTR_Study,
                    KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition,
                };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_StudySubsetDefinition.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition,
                            ConditionOperator.In, associationIds)
                    }
                }
            };

            var results = _service.RetrieveMultiple(query);
            return results == null ?
                    new List<KTR_StudySubsetDefinition>() :
                    results.Entities
                        .Select(e => e.ToEntity<KTR_StudySubsetDefinition>())
                        .ToList();
        }

        public List<Guid> BulkInsertSubsetStudyAssociation(IList<KTR_StudySubsetDefinition> subsetDefinitionAssociations)
        {
            var entities = subsetDefinitionAssociations
                .Select(sd => sd.ToEntity<Entity>())
                .ToList();

            var entitiesColletion = new EntityCollection(entities)
            {
                EntityName = KTR_StudySubsetDefinition.EntityLogicalName
            };

            var request = new CreateMultipleRequest
            {
                Targets = entitiesColletion
            };

            var response = (CreateMultipleResponse)_service.Execute(request);
            return response.Ids.ToList();
        }

        public void DeleteSubsetStudyAssociation(Guid subsetDefinitionAssociationId)
        {
            _service.Delete(
                KTR_StudySubsetDefinition.EntityLogicalName,
                subsetDefinitionAssociationId);
        }

        public void BulkDeleteSubsetStudyAssociation(IList<Guid> subsetDefinitionAssociationIds)
        {
            if (subsetDefinitionAssociationIds == null || subsetDefinitionAssociationIds.Count == 0)
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

            foreach (var associationId in subsetDefinitionAssociationIds)
            {
                request.Requests.Add(new DeleteRequest
                {
                    Target = new EntityReference(
                        KTR_StudySubsetDefinition.EntityLogicalName,
                        associationId)
                });
            }

            _service.Execute(request);
        }

        public List<string> GetSubsetNamesByQuestionnaireLineId(Guid questionnaireLineId, Guid? justCreatedSubsetDefinitionId = null)
        {
            var subsetLinkQuery = new QueryExpression(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId,
                            ConditionOperator.Equal,
                            questionnaireLineId)
                    }
                }
            };

            var subsetLinkResults = _service.RetrieveMultiple(subsetLinkQuery)?.Entities ?? Enumerable.Empty<Entity>();

            var subsetDefinitionIds = subsetLinkResults
                .Select(e => e.GetAttributeValue<EntityReference>(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId))
                .Where(er => er != null && er.Id != Guid.Empty)
                .Select(er => er.Id)
                .ToList();

            // Ensure we include the just-created record id from the current pipeline operation
            if (justCreatedSubsetDefinitionId.HasValue && justCreatedSubsetDefinitionId.Value != Guid.Empty)
            {
                subsetDefinitionIds.Add(justCreatedSubsetDefinitionId.Value);
            }

            subsetDefinitionIds = subsetDefinitionIds
                .Distinct()
                .ToList();

            if (subsetDefinitionIds.Count == 0)
            {
                return new List<string>();
            }

            // Fetch names for all collected subset definition ids
            var subsetDefQuery = new QueryExpression(KTR_SubsetDefinition.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Name),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(
                            KTR_SubsetDefinition.Fields.Id,
                            ConditionOperator.In,
                            subsetDefinitionIds.ToArray())
                    }
                }
            };

            var subsetDefResults = _service.RetrieveMultiple(subsetDefQuery)?.Entities;
            if (subsetDefResults == null || subsetDefResults.Count == 0)
            {
                return new List<string>();
            }

            return subsetDefResults
                .Select(e => e.GetAttributeValue<string>(KTR_SubsetDefinition.Fields.KTR_Name))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
