namespace Kantar.StudyDesignerLite.Plugins.SubsetEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums; // Use ContextMessageEnum and ContextStageEnum
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Rebuilds ktr_subsetlistshtml on all impacted Studies when a ktr_subsetentities record is created/updated/deleted.
    /// </summary>
    public class CreateUpdateDeleteSubsetEntitiesPostOperation : PluginBase
    {
        private static readonly string s_pluginName = typeof(CreateUpdateDeleteSubsetEntitiesPostOperation).FullName;

        public CreateUpdateDeleteSubsetEntitiesPostOperation() : base(typeof(CreateUpdateDeleteSubsetEntitiesPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null) { throw new InvalidPluginExecutionException(nameof(localContext)); }
            var tracing = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            tracing.Trace($"{s_pluginName} START Message={context.MessageName} Stage={context.Stage} Depth={context.Depth}");

            if (!context.InputParameters.Contains("Target"))
            {
                tracing.Trace("No Target parameter – exit.");
                return;
            }

            if (string.Equals(context.MessageName, nameof(ContextMessageEnum.Delete), StringComparison.Ordinal))
            {
                if (!(context.InputParameters["Target"] is EntityReference targetRef) || targetRef.LogicalName != KTR_SubsetEntities.EntityLogicalName)
                {
                    tracing.Trace("Delete: Target not ktr_subsetentities EntityReference – exit.");
                    return;
                }

                if (context.Stage != (int)ContextStageEnum.PostOperation)
                {
                    tracing.Trace($"Delete received at unexpected Stage={context.Stage}. Register only PostOperation with PreImage including ktr_subsetdefinition.");
                    return;
                }

                tracing.Trace("Delete PostOperation: reading subset definition id from PreImage.");
                var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
                if (preImage == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage missing – ensure step registered with PreImage including ktr_subsetdefinition.");
                    return;
                }

                var subsetDefRef = preImage.GetAttributeValue<EntityReference>(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion);
                if (subsetDefRef == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage lacks ktr_subsetdefinition – cannot rebuild.");
                    return;
                }

                tracing.Trace($"Delete PostOperation: rebuilding studies for SubsetDefinition={subsetDefRef.Id} after deletion of subset entity {targetRef.Id}.");
                try
                {
                    RebuildStudiesForSubsetDefinition(service, tracing, subsetDefRef.Id);
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Delete PostOperation: rebuild error {ex.Message}");
                    throw;
                }
                finally
                {
                    tracing.Trace($"{s_pluginName} END (Delete)");
                }
                return;
            }

            // CREATE / UPDATE path (Target is Entity)

            if (!(context.InputParameters["Target"] is Entity target) || target.LogicalName != KTR_SubsetEntities.EntityLogicalName)
            {
                tracing.Trace("Create/Update: Target not ktr_subsetentities Entity – exit.");
                return;
            }

            var pre = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            var subsetDefFromTarget = target.GetAttributeValue<EntityReference>(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion)
                ?? pre?.GetAttributeValue<EntityReference>(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion);
            if (subsetDefFromTarget == null && target.Id != Guid.Empty)
            {
                try
                {
                    var retrieved = service.Retrieve(KTR_SubsetEntities.EntityLogicalName, target.Id,
                        new ColumnSet(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion));
                    subsetDefFromTarget = retrieved.GetAttributeValue<EntityReference>(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion);
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Create/Update: retrieve subset entity failed: {ex.Message}");
                }
            }
            if (subsetDefFromTarget == null)
            {
                tracing.Trace("Create/Update: subset definition not resolved – exit.");
                return;
            }

            if (string.Equals(context.MessageName, nameof(ContextMessageEnum.Create), StringComparison.Ordinal) ||
                string.Equals(context.MessageName, nameof(ContextMessageEnum.Update), StringComparison.Ordinal))
            {
                tracing.Trace($"{context.MessageName}: rebuilding studies for SubsetDefinition={subsetDefFromTarget.Id}.");
                RebuildStudiesForSubsetDefinition(service, tracing, subsetDefFromTarget.Id);
                tracing.Trace($"{s_pluginName} END ({context.MessageName})");
            }
        }

        private void RebuildStudiesForSubsetDefinition(IOrganizationService service, ITracingService tracing, Guid subsetDefinitionId)
        {
            var studyIds = GetImpactedStudyIds(service, subsetDefinitionId);
            tracing.Trace($"Impacted study count={studyIds.Count}");
            if (studyIds.Count == 0)
            {
                tracing.Trace("No impacted studies found – nothing to rebuild.");
                return;
            }

            var request = new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection(),
                ReturnResponses = false,
            };
            foreach (var studyId in studyIds)
            {
                var html = BuildStudySubsetListsHtmlForStudy(service, studyId);
                tracing.Trace($"HTML = {html}");
                var update = new Entity(KT_Study.EntityLogicalName, studyId)
                {
                    [KT_Study.Fields.KTR_SubsetListsHtml] = html ?? string.Empty
                };
                request.Requests.Add(new UpdateRequest
                {
                    Target = update,
                });
            }

            tracing.Trace($"Executing transaction with {request.Requests.Count} update requests.");
            service.Execute(request);
        }

        private List<Guid> GetImpactedStudyIds(IOrganizationService service, Guid subsetDefinitionId)
        {
            var query = new QueryExpression(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_StudySubsetDefinition.Fields.KTR_Study),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition, ConditionOperator.Equal, subsetDefinitionId)
                    }
                },
                NoLock = true
            };
            return service.RetrieveMultiple(query).Entities
                .Select(e => e.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinition.Fields.KTR_Study))
                .Where(r => r != null)
                .Select(r => r.Id)
                .Distinct()
                .ToList();
        }

        private string BuildStudySubsetListsHtmlForStudy(IOrganizationService service, Guid studyId)
        {
            var subsetRepo = new SubsetRepository(service);

            var studySubsetDefs = subsetRepo.GetSubsetStudyAssociationByStudyId(
                studyId,
                new[]
                {
                    KTR_StudySubsetDefinition.Fields.KTR_StudySubsetDefinitionId,
                    KTR_StudySubsetDefinition.Fields.KTR_Study,
                    KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition
                }) ?? new List<KTR_StudySubsetDefinition>();

            if (studySubsetDefs.Count == 0) { return string.Empty; }

            var parts = new List<string>();
            foreach (var ssd in studySubsetDefs)
            {
                var subsetDefRef = ssd.KTR_SubsetDefinition;
                if (subsetDefRef == null) { continue; }

                //-----------------
                // Skip if no QuestionnaireLineSubset record exists for this study and subset definition
                var existsQlsQuery = new QueryExpression(KTR_QuestionnaireLineSubset.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(false),
                    Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.KTR_Study, ConditionOperator.Equal, studyId),
                            new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId, ConditionOperator.Equal, subsetDefRef.Id),
                            new ConditionExpression(KTR_QuestionnaireLineSubset.Fields.StateCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLineSubset_StateCode.Active)
                        }
                    },
                    NoLock = true,
                    TopCount = 1
                };

                var hasQlsAssociation = service.RetrieveMultiple(existsQlsQuery).Entities.Count > 0;
                if (!hasQlsAssociation) { continue; }
                //-------------------

                var subsetName = subsetDefRef.Name;
                if (string.IsNullOrWhiteSpace(subsetName))
                {
                    try
                    {
                        var subsetDef = service.Retrieve(
                            KTR_SubsetDefinition.EntityLogicalName,
                            subsetDefRef.Id,
                            new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Name));
                        subsetName = subsetDef.GetAttributeValue<string>(KTR_SubsetDefinition.Fields.KTR_Name);
                    }
                    catch
                    {
                        subsetName = string.Empty;
                    }
                }

                var subsetEntities = subsetRepo.GetSubsetEntitiesByDefinitionIds(
                    new[] { subsetDefRef.Id },
                    new[]
                    {
                        KTR_SubsetEntities.Fields.KTR_SubsetEntitiesId,
                        KTR_SubsetEntities.Fields.KTR_Name,
                        KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion
                    }) ?? new List<KTR_SubsetEntities>();

                parts.Add(Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers.SubsetHtmlHelper.BuildSubsetDefinitionTable(subsetName, subsetEntities));
            }

            return string.Join("<br/>", parts.Where(p => !string.IsNullOrEmpty(p)));
        }
    }
}
