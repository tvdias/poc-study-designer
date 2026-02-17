namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    using System;
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Plugin that validates questionnaire line updates by Scripter-only users.
    /// Blocks Scripters from modifying non-dummy questions unless it's a reorder operation.
    /// </summary>
    public class QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation : PluginBase
    {
        public static readonly string UpdateStepID = "a4cd3dcd-f4a8-f011-bbd2-6045bd8a585f";

        public QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation()
            : base(typeof(QuestionnaireLinesBlockScripterOnNonDummyQuestionsPreValidation))
        { }

        /// <summary>
        /// Executes the plugin logic to validate questionnaire line updates.
        /// </summary>
        /// <param name="localContext">The local plugin context containing execution details.</param>
        /// <exception cref="InvalidPluginExecutionException">Thrown when Scripter-only users attempt to modify non-dummy questions.</exception>
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            { throw new InvalidPluginExecutionException(nameof(localContext)); }

            var tracingService = localContext.TracingService;
            var service = localContext.SystemUserService;
            var context = localContext.PluginExecutionContext;

            // Run only on Update, PreValidation stage
            if (context.MessageName != "Update" || context.Stage != 10)
            {
                tracingService.Trace("Not Update or not PreValidation stage. Exiting.");
                return;
            }

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target))
            {
                tracingService.Trace("Target entity is missing. Exiting.");
                return;
            }

            if (target.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                tracingService.Trace($"Entity is not kt_questionnaireline: {target.LogicalName}. Exiting.");
                return;
            }

            // Get logged-in user
            var loggedInUserId = context.InitiatingUserId;
            tracingService.Trace($"Logged-in user ID: {loggedInUserId}");

            // Check if user has only Scripter role
            if (!UserHasScripterRole(service, loggedInUserId))
            {
                tracingService.Trace("User is not Scripter-only → allowing update.");
                return;
            }
            tracingService.Trace("User is Scripter-only → checking if question is dummy.");

            // Get PreImage
            var preImage = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            if (preImage == null || !preImage.Contains("ktr_isdummyquestion"))
            {
                tracingService.Trace("PreImage missing or does not contain 'ktr_isdummyquestion'. Exiting.");
                return;
            }

            bool isDummy = preImage.GetAttributeValue<bool>(KT_QuestionnaireLines.Fields.KTR_IsDummyQuestion);

            bool isReorderOnly = target.Attributes.Contains(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);

            if (!isDummy && !isReorderOnly)
            {
                tracingService.Trace("Non-dummy question detected and update is not reorder. Blocking update.");
                throw new InvalidPluginExecutionException("Scripters with only Scripter role cannot modify non-dummy questions.");
            }

            tracingService.Trace("Dummy question or reorder update → update allowed.");
        }

        /// <summary>
        /// Determines whether the specified user has the Scripter business role.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns><c>true</c> if the user has the Scripter role; otherwise, <c>false</c>.</returns>
        private bool UserHasScripterRole(IOrganizationService service, Guid userId)
        {
            var query = new QueryExpression(SystemUser.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(SystemUser.Fields.KTR_BusinessRole),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(SystemUser.Fields.SystemUserId, ConditionOperator.Equal, userId)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            if (results.Entities.Count == 0 || results.Entities.Count > 1)
            { return false; }

            var businessRole = results.Entities.FirstOrDefault()
                .GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole)?.Value;

            const int ScripterRoleValue = (int)KTR_KantarBusinessRole.KantarScripter;
            return businessRole == ScripterRoleValue;
        }
    }
}
