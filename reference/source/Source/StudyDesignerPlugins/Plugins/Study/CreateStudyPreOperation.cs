namespace Kantar.StudyDesignerLite.Plugins.Study
{
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pre-operation plugin for creating a study.
    /// Set the user-friendly study version based on the master study or as the first version.
    /// No need to invoke update since we are in pre-operation and modifying the target entity directly after creation.
    /// [IMPORTANT]FetchXML is better because it calculates MAX() directly in the database and returns a single aggregated record, instead of retrieving and processing all rows in the plugin.
    /// </summary>
    public class CreateStudyPreOperation : PluginBase
    {
        private static readonly string s_pluginName = typeof(CreateStudyPreOperation).FullName;

        public CreateStudyPreOperation() : base(typeof(CreateStudyPreOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var tracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.SystemUserService;

            if (!context.InputParameters.TryGetValue("Target", out Entity entity) || entity == null)
            {
                tracingService.Trace($"{s_pluginName}: Target entity not found");
                return;
            }

            tracingService.Trace($"{s_pluginName} {entity.LogicalName}");

            if (entity.LogicalName == KT_Study.EntityLogicalName)
            {
                var study = entity.ToEntity<KT_Study>();

                if (context.MessageName == nameof(ContextMessageEnum.Create))
                {
                    SetUserFriendlyStudyVersion(service, study, tracingService);
                }
            }
        }

        #region Set User Friendly Study Version
        private void SetUserFriendlyStudyVersion(IOrganizationService service, KT_Study study, ITracingService tracingService)
        {
            var version = 1;

            if (study.Contains(KT_Study.Fields.KTR_MasterStudy) && study[KT_Study.Fields.KTR_MasterStudy] != null)
            {
                tracingService.Trace("Study has a parent, checking master study and version.");

                var masterStudyRef = (EntityReference)study[KT_Study.Fields.KTR_MasterStudy];
                if (masterStudyRef != null)
                {
                    tracingService.Trace($"Master Study ID: {masterStudyRef.Id}");

                    var fetchXml = $@"
                        <fetch aggregate='true'>
                          <entity name='{KT_Study.EntityLogicalName}'>
                            <attribute name='{KT_Study.Fields.KTR_VersionNumber}' alias='maxversion' aggregate='max' />
                            <filter>
                              <condition attribute='{KT_Study.Fields.KTR_MasterStudy}' operator='eq' value='{masterStudyRef.Id}' />
                            </filter>
                          </entity>
                        </fetch>";

                    // Find the max version number
                    var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    var maxVersion = result.Entities.FirstOrDefault()?.GetAttributeValue<AliasedValue>("maxversion")?.Value as int? ?? 0;
                    version = maxVersion + 1;

                }
                else
                {
                    tracingService.Trace("Parent does not contain a Master Study.");
                }
            }

            study[KT_Study.Fields.KTR_VersionNumber] = version;
            tracingService.Trace($"SetUserFriendlyStudyVersion executed. Final Version: {version}");
        }
        #endregion
    }
}
