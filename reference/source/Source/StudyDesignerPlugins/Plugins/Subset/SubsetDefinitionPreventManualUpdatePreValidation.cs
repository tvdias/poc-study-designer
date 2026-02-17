namespace Kantar.StudyDesignerLite.Plugins.Subset
{
    using System;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Prevent manual updates to Subset Definition records via the UI.
    /// Depth equal 1 indicates the operation is triggered from the UI,
    /// </summary>
    public class SubsetDefinitionPreventManualUpdatePreValidation : PluginBase
    {
        public SubsetDefinitionPreventManualUpdatePreValidation()
            : base(typeof(SubsetDefinitionPreventManualUpdatePreValidation))
        { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var tracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;

            if (context.Depth == 1)
            {
                tracingService.Trace($"{KTR_SubsetDefinition.EntitySchemaName} change attempt blocked.");

                throw new InvalidPluginExecutionException("Change to this record is not allowed from the UI.");
            }

            tracingService.Trace($"{KTR_SubsetDefinition.EntitySchemaName} change attempt allowed.");
        }

    }
}
