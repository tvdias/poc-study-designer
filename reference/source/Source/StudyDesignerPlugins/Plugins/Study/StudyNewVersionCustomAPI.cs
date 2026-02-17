namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class StudyNewVersionCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.StudyNewVersionCustomAPI";

        private const int STATUS_REASON_DRAFT = (int)KT_Study_StatusCode.Draft;

        public StudyNewVersionCustomAPI()
            : base(typeof(StudyNewVersionCustomAPI))
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

            tracingService.Trace($"{PluginName} triggered.");

            if (!context.InputParameters.Contains("ktr_oldstudy_id") ||
                !(context.InputParameters["ktr_oldstudy_id"] is string studyIdString) ||
                !Guid.TryParse(studyIdString, out Guid studyId))
            {
                tracingService.Trace("Missing or invalid input parameter: ktr_oldstudy_id");
                throw new InvalidPluginExecutionException("Missing or invalid input parameter: ktr_oldstudy_id.");
            }

            tracingService.Trace($"Fetching Study with ID: {studyId}");
            var fullStudy = service.Retrieve(KT_Study.EntityLogicalName, studyId, new ColumnSet(true)).ToEntity<KT_Study>();

            if (fullStudy == null)
            {
                tracingService.Trace("Study not found.");
                throw new InvalidPluginExecutionException("Study not found.");
            }

            // Check if a draft version already exists
            if (DraftChildStudyExists(service, studyId, tracingService))
            {
                tracingService.Trace("A draft version of this study already exists.");
                throw new InvalidPluginExecutionException("A draft version of this study already exists. Please update the draft or delete it before creating a new version.");
            }

            if ((fullStudy.StatusCode != KT_Study_StatusCode.Completed &&
                fullStudy.StatusCode != KT_Study_StatusCode.ApprovedForLaunch &&
                fullStudy.StatusCode != KT_Study_StatusCode.ReadyForScripting) ||
                !IsMostRecent(fullStudy, service))
            {
                tracingService.Trace("Not the most recent Completed/ApprovedForLaunch/ReadyForScripting Study.");
                throw new InvalidPluginExecutionException("Not the most recent Completed/ApprovedForLaunch/ReadyForScripting Study.");
            }

            // Ensure fullStudy has KTR_MasterStudy set before creating the new version
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

            // Create new Study
            Entity newStudy = new Entity(KT_Study.EntityLogicalName);
            foreach (var attr in fullStudy.Attributes)
            {
                if (attr.Key == KT_Study.Fields.KT_StudyId ||
                    attr.Key == KT_Study.Fields.StatusCode ||
                    attr.Key == KT_Study.Fields.StateCode ||
                    attr.Key == KT_Study.Fields.KTR_StudyVersionNumber ||
                    attr.Key == KT_Study.Fields.OwnerId ||
                    attr.Key == KT_Study.Fields.KTR_IsSnapshotCreated)
                {
                    continue;
                }

                newStudy[attr.Key] = attr.Value;
            }

            newStudy[KT_Study.Fields.KTR_ParentStudy] = fullStudy.ToEntityReference(); // Set parent study reference

            // Ensure project is copied
            if (fullStudy.Attributes.Contains(KT_Study.Fields.KT_Project))
            {
                newStudy[KT_Study.Fields.KT_Project] = fullStudy[KT_Study.Fields.KT_Project];
            }

            newStudy[KT_Study.Fields.OwnerId] = new EntityReference(SystemUser.EntityLogicalName, context.InitiatingUserId);
            tracingService.Trace($"Owner set to InitiatingUserId: {context.InitiatingUserId}");

            Guid newStudyId = service.Create(newStudy);
            tracingService.Trace($"New Study created with ID: {newStudyId}");

            // Copy Fieldwork Market Languages from old study to new study
            CopyFieldworkLanguages(service, tracingService, studyId, newStudyId);

            // Return new study ID
            context.OutputParameters["ktr_new_version_study"] = newStudyId.ToString();
            tracingService.Trace("Returning new Study ID.");
        }

        private bool DraftChildStudyExists(IOrganizationService service, Guid parentStudyId, ITracingService tracing)
        {
            tracing.Trace($"Checking for draft children of Study: {parentStudyId}");

            QueryExpression query = new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Study.Fields.KT_StudyId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KTR_ParentStudy, ConditionOperator.Equal, parentStudyId),
                        new ConditionExpression(KT_Study.Fields.StatusCode, ConditionOperator.Equal, STATUS_REASON_DRAFT)
                    }
                }
            };

            EntityCollection results = service.RetrieveMultiple(query);
            tracing.Trace($"Found {results.Entities.Count} draft children.");
            return results.Entities.Count > 0;
        }

        private bool IsMostRecent(KT_Study study, IOrganizationService service)
        {
            // Check if this study is the most recent version
            QueryExpression query = new QueryExpression(KT_Study.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Study.Fields.KT_StudyId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Study.Fields.KT_Project, ConditionOperator.Equal, study.KT_Project.Id),
                        new ConditionExpression(KT_Study.Fields.KTR_MasterStudy, ConditionOperator.Equal, study.KTR_MasterStudy?.Id ?? Guid.Empty),
                        new ConditionExpression(KT_Study.Fields.KTR_VersionNumber, ConditionOperator.GreaterThan, study.KTR_VersionNumber),
                        new ConditionExpression(KT_Study.Fields.StatusCode, ConditionOperator.In, new int[] {
                            (int)KT_Study_StatusCode.ReadyForScripting,
                            (int)KT_Study_StatusCode.ApprovedForLaunch,
                            (int)KT_Study_StatusCode.Completed
                        })
                    }
                }
            };
            EntityCollection results = service.RetrieveMultiple(query);
            return results.Entities.Count == 0;
        }

        private void CopyFieldworkLanguages(IOrganizationService service, ITracingService tracingService, Guid oldStudyId, Guid newStudyId)
        {
            tracingService.Trace("Copying related ktr_fieldworklanguages to new study.");

            QueryExpression fwLangQuery = new QueryExpression(KTR_FieldworkLanguages.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_FieldworkLanguages.Fields.KTR_Study, ConditionOperator.Equal, oldStudyId)
                    }
                }
            };

            EntityCollection fwLangs = service.RetrieveMultiple(fwLangQuery);

            foreach (var oldFwLang in fwLangs.Entities)
            {
                Entity newFwLang = new Entity(KTR_FieldworkLanguages.EntityLogicalName);
                foreach (var attr in oldFwLang.Attributes)
                {
                    // Skip primary key,relationship to old study, statecode, and statuscode due to Dataverse limitations
                    if (attr.Key == KTR_FieldworkLanguages.Fields.Id
                        || attr.Key == KTR_FieldworkLanguages.Fields.KTR_Study
                        || attr.Key == KTR_FieldworkLanguages.Fields.StateCode
                        || attr.Key == KTR_FieldworkLanguages.Fields.StatusCode)
                    {
                        continue;
                    }

                    newFwLang[attr.Key] = attr.Value;
                }
                // Set relationship to new study
                newFwLang[KTR_FieldworkLanguages.Fields.KTR_Study] = new EntityReference(KT_Study.EntityLogicalName, newStudyId);

                var newId = service.Create(newFwLang);
                tracingService.Trace($"Created new ktr_fieldworklanguages with ID: {newId}");

                newFwLang.Id = newId;

                // Update statecode and statuscode after creation due to Dataverse limitations on create inactive records
                var oldState = oldFwLang.GetAttributeValue<OptionSetValue>(KTR_FieldworkLanguages.Fields.StateCode);

                if (oldState != null &&
                    oldState.Value == (int)KTR_FieldworkLanguages_StateCode.Inactive)
                {
                    var setStateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(KTR_FieldworkLanguages.EntityLogicalName, newId),
                        State = new OptionSetValue((int)KTR_FieldworkLanguages_StateCode.Inactive),
                        Status = new OptionSetValue((int)KTR_FieldworkLanguages_StatusCode.Inactive)
                    };
                    service.Execute(setStateRequest);
                    tracingService?.Trace($"Inactivate KTR_FieldworkLanguages: {newId}");
                }
            }

            tracingService.Trace($"Copied {fwLangs.Entities.Count} ktr_fieldworklanguages to new study.");
        }
    }
}
