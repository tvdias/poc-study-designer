namespace Kantar.StudyDesignerLite.Plugins.Study
{

    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class StudyAbandonOrReworkCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.StudyAbandonOrReworkCustomAPI";

        private const int STATUS_REASON_ABANDON = (int)KT_Study_StatusCode.Abandon;
        private const int STATUS_REASON_REWORK = (int)KT_Study_StatusCode.Rework;

        public StudyAbandonOrReworkCustomAPI()
            : base(typeof(StudyAbandonOrReworkCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.CurrentUserService;

            tracingService.Trace($"{PluginName} triggered");

            if (context.InputParameters.Contains("ktr_study_id") &&
                context.InputParameters["ktr_study_id"] is string studyIdString &&
                Guid.TryParse(studyIdString, out Guid studyId) &&
                context.InputParameters.Contains("ktr_new_status_reason_study") &&
                context.InputParameters["ktr_new_status_reason_study"] is int newStatusReason)
            {
                tracingService.Trace($"Processing StudyId: {studyId} with StatusReason: {newStatusReason}");

                // Retrieve the study record with statuscode and statecode
                Entity study = service.Retrieve(KT_Study.EntityLogicalName, studyId, new ColumnSet(KT_Study.Fields.StateCode, KT_Study.Fields.StatusCode));

                if (study == null)
                {
                    throw new InvalidPluginExecutionException("Study not found.");
                }

                if (newStatusReason == STATUS_REASON_ABANDON)
                {
                    tracingService.Trace("Executing Abandon logic...");

                    // Create the update entity for Abandon status
                    Entity update = new Entity(KT_Study.EntityLogicalName, studyId)
                    {
                        [KT_Study.Fields.StatusCode] = new OptionSetValue(STATUS_REASON_ABANDON)
                    };

                    service.Update(update);
                    context.OutputParameters["ktr_study_status_confirmation"] = "Study successfully abandoned.";
                }

                else if (newStatusReason == STATUS_REASON_REWORK)
                {
                    tracingService.Trace("Executing Rework logic...");

                    // Retrieve full study with all attributes
                    var fullStudy = service.Retrieve(KT_Study.EntityLogicalName, studyId, new ColumnSet(true)).ToEntity<KT_Study>();
                    if (!fullStudy.Attributes.TryGetValue(KT_Study.Fields.KTR_MasterStudy, out var masterStudy) || masterStudy == null)
                    {
                        tracingService.Trace("MasterStudy not set. Updating to self-reference.");
                        Entity updateMasterStudy = new Entity(KT_Study.EntityLogicalName, fullStudy.Id)
                        {
                            [KT_Study.Fields.KTR_MasterStudy] = fullStudy.ToEntityReference()
                        };
                        service.Update(updateMasterStudy);
                        fullStudy[KT_Study.Fields.KTR_MasterStudy] = fullStudy.ToEntityReference();
                    }
                    // Mark the original Study as Rework (Inactive)
                    Entity originalUpdate = new Entity(KT_Study.EntityLogicalName, studyId)
                    {
                        [KT_Study.Fields.StatusCode] = new OptionSetValue(STATUS_REASON_REWORK)
                    };
                    service.Update(originalUpdate);

                    // Create new Study entity for the draft
                    Entity newDraftStudy = new Entity(KT_Study.EntityLogicalName);

                    foreach (var attr in fullStudy.Attributes)
                    {
                        if (attr.Key == KT_Study.Fields.KT_StudyId ||
                            attr.Key == KT_Study.Fields.StatusCode ||
                            attr.Key == KT_Study.Fields.StateCode ||
                            attr.Key == KT_Study.Fields.KTR_StudyVersionNumber || // skip auto-number
                            attr.Key == KT_Study.Fields.OwnerId)
                        {
                            continue;
                        }

                        newDraftStudy[attr.Key] = attr.Value;
                    }

                    newDraftStudy[KT_Study.Fields.KTR_ParentStudy] = fullStudy.ToEntityReference(); // Set parent study reference

                    // Ensure project is copied
                    if (fullStudy.Attributes.Contains(KT_Study.Fields.KT_Project))
                    {
                        newDraftStudy[KT_Study.Fields.KT_Project] = fullStudy[KT_Study.Fields.KT_Project];
                    }

                    newDraftStudy[KT_Study.Fields.OwnerId] = new EntityReference(SystemUser.EntityLogicalName, context.InitiatingUserId);
                    tracingService.Trace($"Owner set to InitiatingUserId: {context.InitiatingUserId}");

                    Guid newStudyId = service.Create(newDraftStudy);
                    tracingService.Trace($"New Draft Study created: {newStudyId}");

                    // Return the new study ID as part of the response
                    context.OutputParameters["ktr_new_reworkstudy"] = newStudyId.ToString();

                    context.OutputParameters["ktr_study_status_confirmation"] = "Study marked for rework and draft version created.";
                }

                else
                {
                    tracingService.Trace("Invalid status reason received.");
                    throw new InvalidPluginExecutionException("Invalid status reason provided.");
                }
            }
            else
            {
                tracingService.Trace("Missing or invalid input parameters.");
                throw new InvalidPluginExecutionException("Missing required input parameters: ktr_study_id or ktr_new_status_reason_study.");
            }
        }
    }
}
