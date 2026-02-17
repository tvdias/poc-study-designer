using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages; // Added for UpdateRequest, ExecuteMultipleRequest
using Microsoft.Xrm.Sdk.Query;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;

namespace Kantar.StudyDesignerLite.Plugins.StudySubsetDefinition
{
    /// <summary>
    /// Recalculates subset HTML for Study Questionnaire Lines AND Study subset lists HTML when a Study Subset Definition is created/updated/deleted.
    /// </summary>
    public class CreateUpdateDeleteStudySubsetDefinitionPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.StudySubsetDefinition.CreateUpdateDeleteStudySubsetDefinitionPostOperation";

        public CreateUpdateDeleteStudySubsetDefinitionPostOperation() : base(typeof(CreateUpdateDeleteStudySubsetDefinitionPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null) { throw new InvalidPluginExecutionException(nameof(localContext)); }
            var tracing = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            var start = DateTime.UtcNow;
            tracing.Trace($"{PluginName} START Message={context.MessageName} Stage={context.Stage} Depth={context.Depth}");

            if (!context.InputParameters.Contains("Target"))
            {
                tracing.Trace("Target not present – exit.");
                return;
            }

            if (string.Equals(context.MessageName, nameof(ContextMessageEnum.Delete), StringComparison.Ordinal))
            {
                var targetRef = context.InputParameters["Target"] as EntityReference; // Delete supplies EntityReference
                if (targetRef == null || targetRef.LogicalName != KTR_StudySubsetDefinition.EntityLogicalName)
                {
                    tracing.Trace("Delete: Target not ktr_studysubsetdefinition EntityReference – exit.");
                    return;
                }

                if (context.Stage != (int)ContextStageEnum.PostOperation)
                {
                    tracing.Trace($"Delete received at unexpected Stage={context.Stage}. Register PostOperation with PreImage including ktr_study.");
                    return;
                }

                tracing.Trace("Delete PostOperation: reading Study Id from PreImage.");
                var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
                if (preImage == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage missing – ensure step registered with PreImage including ktr_study.");
                    return;
                }

                var studyRef = preImage.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinition.Fields.KTR_Study);
                if (studyRef == null)
                {
                    tracing.Trace("Delete PostOperation: PreImage lacks ktr_study – cannot rebuild.");
                    return;
                }

                tracing.Trace($"Delete PostOperation: Rebuilding HTML for Study={studyRef.Id} after deletion of subset definition {targetRef.Id}.");
                try
                {
                    UpdateStudySubsetListsHtml(service, tracing, studyRef.Id);
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Delete PostOperation: rebuild error {ex.Message}");
                    throw;
                }
                finally
                {
                    tracing.Trace($"{PluginName} END (Delete) ElapsedMs={(int)(DateTime.UtcNow - start).TotalMilliseconds}");
                }
                return;
            }

            // Create / Update path expects Entity Target
            var target = context.InputParameters["Target"] as Entity;
            if (target == null || target.LogicalName != KTR_StudySubsetDefinition.EntityLogicalName)
            {
                tracing.Trace("Create/Update: Target not ktr_studysubsetdefinition Entity – exit.");
                return;
            }

            Entity pre = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            var studyRefTarget = target.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinition.Fields.KTR_Study) ?? pre?.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinition.Fields.KTR_Study);
            if (studyRefTarget == null && target.Id != Guid.Empty)
            {
                try
                {
                    var retrieved = service.Retrieve(KTR_StudySubsetDefinition.EntityLogicalName, target.Id, new ColumnSet(KTR_StudySubsetDefinition.Fields.KTR_Study));
                    studyRefTarget = retrieved.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinition.Fields.KTR_Study);
                }
                catch (Exception ex)
                {
                    tracing.Trace($"Create/Update: retrieve failed {ex.Message}");
                }
            }
            if (studyRefTarget == null)
            {
                tracing.Trace("Create/Update: Study reference not resolved – exit.");
                return;
            }

            tracing.Trace($"Create/Update: Study {studyRefTarget.Id} impacted by {context.MessageName} – rebuilding HTML.");
            try
            {
                UpdateStudySubsetListsHtml(service, tracing, studyRefTarget.Id);
            }
            catch (Exception ex)
            {
                tracing.Trace($"Create/Update: rebuild error {ex.Message}");
                throw;
            }
            finally
            {
                tracing.Trace($"{PluginName} END ElapsedMs={(int)(DateTime.UtcNow - start).TotalMilliseconds}");
            }
        }

        #region Study Subset Lists HTML
        private void UpdateStudySubsetListsHtml(IOrganizationService service, ITracingService tracing, Guid studyId)
        {
            var subsetRepo = new SubsetRepository(service);
            var columns = new[]
            {
                KTR_StudySubsetDefinition.Fields.KTR_StudySubsetDefinitionId,
                KTR_StudySubsetDefinition.Fields.KTR_Study,
                KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition
            };

            var studySubsetDefs = subsetRepo.GetSubsetStudyAssociationByStudyId(studyId, columns) ?? new List<KTR_StudySubsetDefinition>();
            tracing.Trace($"UpdateStudySubsetListsHtml: SubsetDefinitionCount={studySubsetDefs.Count}");
            if (studySubsetDefs.Count == 0)
            {
                var clear = new Entity(KT_Study.EntityLogicalName, studyId) { [KT_Study.Fields.KTR_SubsetListsHtml] = string.Empty };
                service.Update(clear);
                tracing.Trace("UpdateStudySubsetListsHtml: Cleared HTML (no subset definitions).");
                return;
            }

            string html = BuildStudySubsetListsHtml(service, subsetRepo, studySubsetDefs);
            var studyUpdate = new Entity(KT_Study.EntityLogicalName, studyId) { [KT_Study.Fields.KTR_SubsetListsHtml] = html ?? string.Empty };
            service.Update(studyUpdate);
            tracing.Trace("UpdateStudySubsetListsHtml: HTML updated.");
        }

        private string BuildStudySubsetListsHtml(IOrganizationService service, ISubsetRepository subsetRepo, List<KTR_StudySubsetDefinition> studySubsetDefs)
        {
            if (studySubsetDefs == null || studySubsetDefs.Count == 0) { return string.Empty; }

            var parts = new List<string>();
            foreach (var ssd in studySubsetDefs)
            {
                var subsetDefRef = ssd.KTR_SubsetDefinition;
                if (subsetDefRef == null) { continue; }

                var subsetName = subsetDefRef.Name;
                if (string.IsNullOrWhiteSpace(subsetName))
                {
                    try
                    {
                        var subsetDef = service.Retrieve(KTR_SubsetDefinition.EntityLogicalName, subsetDefRef.Id, new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Name));
                        subsetName = subsetDef.GetAttributeValue<string>(KTR_SubsetDefinition.Fields.KTR_Name);
                    }
                    catch
                    {
                        subsetName = string.Empty;
                    }
                }

                var entities = subsetRepo.GetSubsetEntitiesByDefinitionIds(
                    new[] { subsetDefRef.Id },
                    new[]
                    {
                        KTR_SubsetEntities.Fields.KTR_SubsetEntitiesId,
                        KTR_SubsetEntities.Fields.KTR_Name,
                        KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion
                    }) ?? new List<KTR_SubsetEntities>();

                parts.Add(Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers.SubsetHtmlHelper.BuildSubsetDefinitionTable(subsetName, entities));
            }

            return string.Join("<br/>", parts.Where(p => !string.IsNullOrEmpty(p)));
        }
        #endregion
    }
}
