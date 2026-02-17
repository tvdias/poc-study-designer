using System;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.Common
{
    public class SetNameFromLookupPreValidation : PluginBase
    {
        public SetNameFromLookupPreValidation() : base(typeof(SetNameFromLookupPreValidation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            ITracingService tracingService = localContext.TracingService;
            IOrganizationService orgService = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            tracingService.Trace("Pre-Validation plugin execution started.");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
            {
                HandleSetNameFromLookup(targetEntity, orgService, tracingService, context);
            }
        }

        private void HandleSetNameFromLookup(Entity targetEntity, IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context)
        {
            var fieldMappings = GetFieldMappingsByEntity(targetEntity, context);

            string lookupFieldName = fieldMappings.LookupField;
            string lookupEntityNameField = fieldMappings.LookupEntityNameField;
            string targetEntityNameField = fieldMappings.TargetEntityNameField;

            if (!string.IsNullOrEmpty(lookupFieldName) && !string.IsNullOrEmpty(lookupEntityNameField) && !string.IsNullOrEmpty(targetEntityNameField))
            {
                EntityReference lookupRef = null;

                if (targetEntity.Attributes.Contains(lookupFieldName) && targetEntity[lookupFieldName] is EntityReference lookupFromTarget)
                {
                    lookupRef = lookupFromTarget;
                    tracingService.Trace("Lookup field '{0}' found in target entity with ID: {1}", lookupFieldName, lookupRef.Id);
                }
                else
                {
                    var existingEntity = service.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet(lookupFieldName));
                    if (existingEntity != null && existingEntity.Attributes.Contains(lookupFieldName) && existingEntity[lookupFieldName] is EntityReference lookupFromExisting)
                    {
                        lookupRef = lookupFromExisting;
                        tracingService.Trace("Lookup field '{0}' retrieved from existing entity with ID: {1}", lookupFieldName, lookupRef.Id);
                    }
                    else
                    {
                        tracingService.Trace("Lookup field '{0}' not found on existing entity, cannot proceed.", lookupFieldName);
                        return;
                    }
                }

                var lookupEntity = service.Retrieve(lookupRef.LogicalName, lookupRef.Id, new ColumnSet(lookupEntityNameField));
                if (lookupEntity != null && lookupEntity.Attributes.Contains(lookupEntityNameField))
                {
                    var lookupEntityName = lookupEntity.GetAttributeValue<string>(lookupEntityNameField);
                    tracingService.Trace("Retrieved name '{0}' from lookup entity.", lookupEntityName);

                    targetEntity[targetEntityNameField] = lookupEntityName;
                    tracingService.Trace("Set name field '{0}' on target entity to '{1}'.", targetEntityNameField, lookupEntityName);
                }
                else
                {
                    tracingService.Trace("Name field '{0}' not found on lookup entity.", lookupEntityNameField);
                }
            }
            else
            {
                tracingService.Trace("No field mappings found for entity '{0}'.", targetEntity.LogicalName);
            }
        }

        private EntityFieldMapping GetFieldMappingsByEntity(Entity targetEntity, IPluginExecutionContext context)
        {
            EntityFieldMapping fieldMapping = null;
            switch (targetEntity.LogicalName)
            {
                case KTR_ProductConfigQuestion.EntityLogicalName:
                    fieldMapping = new EntityFieldMapping
                    {
                        LookupField = KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion,
                        LookupEntityNameField = KTR_ProductConfigQuestion.Fields.KTR_Name,
                        TargetEntityNameField = KTR_ProductConfigQuestion.Fields.KTR_Name,
                    };
                    break;
                case KTR_ProjectProductConfig.EntityLogicalName:
                    fieldMapping = new EntityFieldMapping
                    {
                        LookupField = KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion,
                        LookupEntityNameField = KTR_ProductConfigQuestion.Fields.KTR_Name,
                        TargetEntityNameField = KTR_ProjectProductConfig.Fields.KTR_Name,
                    };
                    break;
                case KTR_ProductTemplateLine.EntityLogicalName:
                    var line = (context.InputParameters["Target"] as Entity).ToEntity<KTR_ProductTemplateLine>();

                    if (line.KTR_Type == KTR_ProductTemplateLineType.Module)
                    {
                        fieldMapping = new EntityFieldMapping
                        {
                            LookupField = KTR_ProductTemplateLine.Fields.KTR_KT_Module,
                            LookupEntityNameField = KT_Module.Fields.KT_Name,
                        };
                    }
                    else if (line.KTR_Type == KTR_ProductTemplateLineType.Question)
                    {
                        fieldMapping = new EntityFieldMapping
                        {
                            LookupField = KTR_ProductTemplateLine.Fields.KTR_KT_QuestionBank,
                            LookupEntityNameField = KT_QuestionBank.Fields.KT_Name,
                        };
                    }
                    else
                    {
                        return null;
                    }

                    fieldMapping.TargetEntityNameField = KTR_ProductTemplateLine.Fields.KTR_Name;
                    break;
            }
            return fieldMapping;
        }
    }
}
