using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;

namespace Kantar.StudyDesignerLite.Plugins.Tags
{
    public class DeleteTagsPreValidation : PluginBase
    {
        #region Declare Constants
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.DeleteTagsPreValidation";
        private const string MessageRestrictTagDelete = "Tag can't be deleted because it's associated with one or more question.";

        public DeleteTagsPreValidation() : base(typeof(DeleteTagsPreValidation)) { }
        #endregion Declare Constants

        #region Execute main method
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidOperationException(nameof(localContext));
            }
            
            ITracingService tracingService = localContext.TracingService;

            IOrganizationService currentUserService = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            EntityReference target = (EntityReference)context.InputParameters["Target"];

            if (target.LogicalName == KTR_Tag.EntityLogicalName)
            {
                var tagExistForQuestionsBank = IsQuestionBankContainTags(currentUserService, target.Id);
                // if current tag exst in any active question bank then display Delete message
                if (tagExistForQuestionsBank)
                {
                    throw new InvalidPluginExecutionException(MessageRestrictTagDelete);
                }
            }
        }
        #endregion Execute main method

        #region Main Operational Method
        private bool IsQuestionBankContainTags(IOrganizationService service, Guid tagId)
        {
            // Call Query expression method by passing tag id as a parameter
            var selectTagInQuestionBankQuery = selectTagInQuestionBank(tagId);
            var resultsTagsInQuestionsBank = service.RetrieveMultiple(selectTagInQuestionBankQuery);
            if (resultsTagsInQuestionsBank != null && resultsTagsInQuestionsBank.Entities.Count > 0)
                {
                     // If record exist in N:N (tag_questionbank) entity then return true or false as boolean value
                     return true;
                }
            else
                {
                    return false;
                }
        }
        #endregion Main Operational Method

        #region Query expression function
        private QueryExpression selectTagInQuestionBank(Guid tagId)
        {
            var query = new QueryExpression(KTR_Tag_KT_QuestionBank.EntityLogicalName);

            query.ColumnSet.AddColumn(KTR_Tag_KT_QuestionBank.Fields.KTR_Tag_KT_QuestionBankId);

            query.Criteria.AddCondition(KTR_Tag_KT_QuestionBank.Fields.KTR_TagId, ConditionOperator.Equal, tagId);
            // Add Link entity criteria for question bank to check question bank record is active or inactive
            var configQuestionLink = query.AddLink(KT_QuestionBank.EntityLogicalName, KTR_Tag_KT_QuestionBank.Fields.KT_QuestionBankId, KT_QuestionBank.Fields.KT_QuestionBankId, JoinOperator.Inner);

            configQuestionLink.LinkCriteria.AddCondition(KT_QuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionBank_StatusCode.Active);

            return query;
        }
        #endregion Query expression function
    }
}
