namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLineSubset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Plugin that handles post-operation logic for QuestionnaireLineSubset entity changes.
    /// Updates HTML content for questionnaire lines and study subset lists when subsets are created, updated, deleted, or deactivated.
    /// </summary>
    public class QuestionnaireLineSubsetPostOperation : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireLineSubsetPostOperation"/> class.
        /// </summary>
        public QuestionnaireLineSubsetPostOperation()
            : base(typeof(QuestionnaireLineSubsetPostOperation))
        {
        }

        /// <summary>
        /// Executes the plugin logic for QuestionnaireLineSubset operations.
        /// Generates HTML content for answer subsets and updates the related StudyQuestionnaireLine and Study entities.
        /// </summary>
        /// <param name="localContext">The local plugin context containing execution details and services.</param>
        /// <exception cref="InvalidPluginExecutionException">Thrown when localContext is null.</exception>
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            var tracingService = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;
            var subsetRepository = new SubsetRepository(service);

            Entity sourceEntity = null;
            if (string.Equals(context.MessageName, nameof(ContextMessageEnum.Delete), StringComparison.Ordinal))
            {
                if (context.PreEntityImages.Contains("PreImage"))
                {
                    sourceEntity = context.PreEntityImages["PreImage"];
                    tracingService.Trace("Using PreImage for Delete operation.");
                }
            }
            else
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
                {
                    sourceEntity = targetEntity;
                }
            }

            if (sourceEntity == null)
            {
                tracingService.Trace("Source entity is missing.");
                return;
            }

            if (sourceEntity.LogicalName != KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                tracingService.Trace("Incorrect entity. Expected ktr_questionnairelinesubset.");
                return;
            }

            // Prefer parent from Source; if missing (e.g., deactivate), fall back to PreImage
            Guid questionnaireLineId;
            var parentAttrName = KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId;

            // study from Source first
            var studyRef = sourceEntity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLineSubset.Fields.KTR_Study);
            if ((studyRef == null || studyRef.Id == Guid.Empty) && context.PreEntityImages.Contains("PreImage"))
            {
                var pre = context.PreEntityImages["PreImage"];
                studyRef = pre.GetAttributeValue<EntityReference>(KTR_QuestionnaireLineSubset.Fields.KTR_Study);
            }

            EntityReference parentRef = null;
            if (sourceEntity.Attributes.Contains(parentAttrName) &&
                sourceEntity[parentAttrName] is EntityReference srcParent &&
                srcParent.Id != Guid.Empty)
            {
                parentRef = srcParent;
            }
            else if (context.PreEntityImages.Contains("PreImage"))
            {
                var pre = context.PreEntityImages["PreImage"];
                parentRef = pre.GetAttributeValue<EntityReference>(parentAttrName);
            }

            if (parentRef == null || parentRef.Id == Guid.Empty)
            {
                tracingService.Trace("Subset missing parent questionnaire line in both Source and PreImage; skipping.");
                return;
            }

            questionnaireLineId = parentRef.Id;
            tracingService.Trace($"Resolved Questionnaire Line Id: {questionnaireLineId}");

            // Ensure study is set
            if (studyRef == null || studyRef.Id == Guid.Empty)
            {
                tracingService.Trace("Study reference missing in both Source and PreImage; skipping.");
                return;
            }

            var qaRepo = new QuestionnaireLineAnswerListRepository(service);
            var answers = qaRepo.GetQuestionnaireLinesAnswerLists(service, questionnaireLineId);
            var questionnaireLineSubsetsWithLocation = subsetRepository.GetQuestionnaireLineSubsetsWithLocation(studyRef.Id);

            // Now put the subsets in 2 vars, one for rows and one for columns
            var subsetsForLine = questionnaireLineSubsetsWithLocation.ContainsKey(questionnaireLineId)
                ? questionnaireLineSubsetsWithLocation[questionnaireLineId]
                : new System.Collections.Generic.List<QuestionnaireLineSubsetWithLocation>();

            var subsetsAsRows = subsetsForLine
                .Where(s => string.Equals(s.Location, "Row", StringComparison.Ordinal))
                .ToList();

            var subsetsAsColumns = subsetsForLine
                .Where(s => string.Equals(s.Location, "Column", StringComparison.Ordinal))
                .ToList();

            var htmlContent = HtmlGenerationHelper.GenerateAnswerSubsetListHtml(answers, subsetsAsRows, subsetsAsColumns);

            tracingService.Trace($"Generated HTML content. {htmlContent}");

            var query = new QueryExpression(KTR_StudyQuestionnaireLine.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_StudyQuestionnaireLine.Fields.Id),
                Criteria = new FilterExpression(LogicalOperator.And)
            };
            query.Criteria.AddCondition(KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, studyRef.Id);
            query.Criteria.AddCondition(KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine, ConditionOperator.Equal, questionnaireLineId);

            var results = service.RetrieveMultiple(query).Entities;
            if (results == null || results.Count == 0)
            {
                tracingService.Trace("No matching ktr_studyquestionnaireline found; nothing to update.");
                return;
            }

            var studyQLineId = results[0].Id;
            var update = new Entity(KTR_StudyQuestionnaireLine.EntityLogicalName, studyQLineId)
            {
                [KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml] = htmlContent ?? string.Empty
            };

            service.Update(update);
            tracingService.Trace("Updated ktr_subsethtml for ktr_studyquestionnaireline.");

            try
            {
                UpdateStudySubsetListsHtml(service, tracingService, studyRef.Id);
                tracingService.Trace("Rebuilt KT_Study.KTR_SubsetListsHtml after QuestionnaireLineSubset change.");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error rebuilding KT_Study.KTR_SubsetListsHtml: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates the subset lists HTML field on the Study entity by rebuilding the HTML content
        /// for all active subset definitions associated with the study.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracing">The tracing service for logging.</param>
        /// <param name="studyId">The ID of the study to update.</param>
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

            var html = BuildStudySubsetListsHtml(service, subsetRepo, studySubsetDefs, studyId);
            var studyUpdate = new Entity(KT_Study.EntityLogicalName, studyId) { [KT_Study.Fields.KTR_SubsetListsHtml] = html ?? string.Empty };
            service.Update(studyUpdate);
            tracing.Trace("UpdateStudySubsetListsHtml: HTML updated.");
        }

        /// <summary>
        /// Builds the HTML content for study subset lists by generating tables for each subset definition
        /// that has active questionnaire line subsets.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="subsetRepo">The subset repository.</param>
        /// <param name="studySubsetDefs">The list of study subset definitions.</param>
        /// <param name="studyId">The ID of the study.</param>
        /// <returns>The generated HTML content, or empty string if no valid subsets exist.</returns>
        private string BuildStudySubsetListsHtml(IOrganizationService service, ISubsetRepository subsetRepo, List<KTR_StudySubsetDefinition> studySubsetDefs, Guid studyId)
        {
            if (studySubsetDefs == null || studySubsetDefs.Count == 0) { return string.Empty; }

            var parts = new List<string>();
            foreach (var ssd in studySubsetDefs)
            {
                var subsetDefRef = ssd.KTR_SubsetDefinition;
                if (subsetDefRef == null) { continue; }

                // Include only if there is at least one ACTIVE QuestionnaireLineSubset for this subset and current study
                if (!HasActiveQuestionnaireLineSubsetForStudy(service, studyId, subsetDefRef.Id))
                {
                    continue;
                }

                var subsetName = subsetDefRef.Name;
                if (string.IsNullOrWhiteSpace(subsetName))
                {
                    try
                    {
                        var subsetDef = service.Retrieve(
                            KTR_SubsetDefinition.EntityLogicalName, subsetDefRef.Id,
                            new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Name));
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

                parts.Add(SubsetHtmlHelper.BuildSubsetDefinitionTable(subsetName, entities));
            }

            return string.Join("<br/>", parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        /// <summary>
        /// Checks whether there is at least one active QuestionnaireLineSubset record
        /// for the specified study and subset definition.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="studyId">The ID of the study.</param>
        /// <param name="subsetDefinitionId">The ID of the subset definition.</param>
        /// <returns>True if at least one active record exists; otherwise, false.</returns>
        private bool HasActiveQuestionnaireLineSubsetForStudy(IOrganizationService service, Guid studyId, Guid subsetDefinitionId)
        {
            var query = new QueryExpression(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineSubsetId),
                Criteria = new FilterExpression(LogicalOperator.And),
                NoLock = true
            };

            query.Criteria.AddCondition(KTR_QuestionnaireLineSubset.Fields.KTR_Study, ConditionOperator.Equal, studyId);
            query.Criteria.AddCondition(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId, ConditionOperator.Equal, subsetDefinitionId);
            // Only active records should count
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var hasAny = service.RetrieveMultiple(query).Entities.Any();
            return hasAny;
        }
    }
}
