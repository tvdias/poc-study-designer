namespace Kantar.StudyDesignerLite.Plugins.StudyManagedListEntity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;

    public class ValidateStudyTemplateCustomAPI : PluginBase
    {
        private const string PluginName = nameof(ValidateStudyTemplateCustomAPI);

        public ValidateStudyTemplateCustomAPI()
            : base(typeof(ValidateStudyTemplateCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracing = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.CurrentUserService;

            tracing.Trace($"Starting {PluginName}");

            try
            {
                var request = GetRequest(context, tracing);
                tracing.Trace($"Received {request.StudyManagedListEntityIds.Count} StudyManagedListEntityIds.");

                var grouped = GroupStudyManagedListEntitiesByStudy(service, tracing, request.StudyManagedListEntityIds);
                var studyIds = grouped.Keys.ToList();

                tracing.Trace($"Found {studyIds.Count} unique Studies from provided IDs.");

                // Return the list of study IDs to PCF
                context.OutputParameters["StudyIds"] = JsonHelper.Serialize(studyIds);
                tracing.Trace($"Returning StudyIds to caller: {string.Join(", ", studyIds)}");
            }
            catch (Exception ex)
            {
                tracing.Trace($"Error in {PluginName}: {ex}");
                throw new InvalidPluginExecutionException($"Error in {PluginName}: {ex.Message}");
            }
        }

        private ValidateStudyTemplateRequest GetRequest(IPluginExecutionContext context, ITracingService tracing)
        {
            tracing.Trace("Reading input parameters...");

            if (!context.InputParameters.Contains("StudyManagedListEntityIds"))
            {
                throw new InvalidPluginExecutionException("Input parameter 'StudyManagedListEntityIds' is required.");
            }

            var raw = context.GetInputParameter<string>("StudyManagedListEntityIds");
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new InvalidPluginExecutionException("No StudyManagedListEntityIds were provided.");
            }

            try
            {
                var ids = JsonHelper.Deserialize<List<Guid>>(raw, "StudyManagedListEntityIds")
                         ?? throw new InvalidPluginExecutionException("Invalid StudyManagedListEntityIds JSON format.");

                return new ValidateStudyTemplateRequest { StudyManagedListEntityIds = ids };
            }
            catch
            {
                throw new InvalidPluginExecutionException("Failed to parse StudyManagedListEntityIds parameter.");
            }
        }

        private Dictionary<Guid, List<Guid>> GroupStudyManagedListEntitiesByStudy(
            IOrganizationService service,
            ITracingService tracing,
            List<Guid> studyManagedListEntityIds)
        {
            var grouped = new Dictionary<Guid, List<Guid>>();
            tracing.Trace("Grouping StudyManagedListEntityIds by related Study...");

            foreach (var id in studyManagedListEntityIds)
            {
                var entity = service.Retrieve(
                    KTR_StudyManagedListEntity.EntityLogicalName,
                    id,
                    new ColumnSet(KTR_StudyManagedListEntity.Fields.KTR_Study));

                var studyRef = entity.GetAttributeValue<EntityReference>(KTR_StudyManagedListEntity.Fields.KTR_Study);
                if (studyRef == null)
                { continue; }

                if (!grouped.ContainsKey(studyRef.Id))
                { grouped[studyRef.Id] = new List<Guid>(); }

                grouped[studyRef.Id].Add(id);
            }

            return grouped;
        }
    }

    internal class ValidateStudyTemplateRequest
    {
        public List<Guid> StudyManagedListEntityIds { get; set; }
    }
}
