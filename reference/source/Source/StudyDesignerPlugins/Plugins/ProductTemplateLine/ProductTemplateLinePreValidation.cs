using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kantar.StudyDesignerLite.Plugins.ProductTemplateLine
{
    public class ProductTemplateLinePreValidation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ProductTemplateLinePreValidation";
        private const string MessageInvalidEntity = "This association between Product Template and Product Template Line is invalid.";
        private const string MessageDuplicateModuleEntity = "This module is already associated!";
        private const string MessageDuplicateQuestionEntity = "This question is already associated!";
        private const string MessageModuleContainsAssociatedQuestions = "This module contains questions that are already associated!";
        private const string MessageQuestionAlreadyInModule = "This question is already associated via a module!";

        public ProductTemplateLinePreValidation()
            : base(typeof(ProductTemplateLinePreValidation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;

            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService currentUserService = localPluginContext.CurrentUserService;

            Entity entity = (Entity)context.InputParameters["Target"];
                
            tracingService.Trace($"{PluginName} {entity.LogicalName}");

            if (entity.LogicalName == KTR_ProductTemplateLine.EntityLogicalName)
            {
                var line = (context.InputParameters["Target"] as Entity).ToEntity<KTR_ProductTemplateLine>();

                if (IsInvalidEntity(line))
                {
                    throw new InvalidPluginExecutionException(MessageInvalidEntity);
                }

                if (IsDuplicate(currentUserService, line))
                {
                    switch (line.KTR_Type)
                    {
                        case KTR_ProductTemplateLineType.Module:
                            throw new InvalidPluginExecutionException(MessageDuplicateModuleEntity);
                        case KTR_ProductTemplateLineType.Question:
                            throw new InvalidPluginExecutionException(MessageDuplicateQuestionEntity);
                    }
                }

                if (IsModuleContainingAssociatedQuestions(currentUserService, line))
                {
                    throw new InvalidPluginExecutionException(MessageModuleContainsAssociatedQuestions);
                }

                if (IsQuestionAlreadyInAssociatedModules(currentUserService, line))
                {
                    throw new InvalidPluginExecutionException(MessageQuestionAlreadyInModule);
                }
            }
        }

        private bool IsInvalidEntity(KTR_ProductTemplateLine line)
        {
            return line.KTR_ProductTemplate.Id == Guid.Empty
                || line.KTR_Type is null
                || (line.KTR_Type == KTR_ProductTemplateLineType.Module && line.KTR_KT_Module is null)
                || (line.KTR_Type == KTR_ProductTemplateLineType.Question && line.KTR_KT_QuestionBank is null)
                || (line.KTR_Type == KTR_ProductTemplateLineType.Module && line.KTR_KT_QuestionBank != null)
                || (line.KTR_Type == KTR_ProductTemplateLineType.Question && line.KTR_KT_Module != null);
        }

        private bool IsDuplicate(IOrganizationService currentUserService, KTR_ProductTemplateLine line)
        {
            var query = SelectLinesByTemplateAndTypeQuery(line);

            var results = currentUserService.RetrieveMultiple(query);

            return results.Entities.Count > 0;
        }

        private bool IsModuleContainingAssociatedQuestions(IOrganizationService service, KTR_ProductTemplateLine line)
        {
            if (line.KTR_Type != KTR_ProductTemplateLineType.Module)
            {
                return false;
            }

            var selectQuestionsInModuleQuery = SelectQuestionsInModuleQuery(line.KTR_KT_Module.Id);
            var resultsQuestionsInModule = service.RetrieveMultiple(selectQuestionsInModuleQuery);

            var questionIds = JoinModuleQuestionIds(resultsQuestionsInModule);
            if (questionIds.Count() == 0)
            {
                return false;
            }

            var selectLinesByTemplateAndQuestionsQuery = SelectLinesByTemplateAndQuestionsQuery(line.KTR_ProductTemplate.Id, questionIds);
            var resultsLines = service.RetrieveMultiple(selectLinesByTemplateAndQuestionsQuery);

            if (resultsLines.Entities.Count > 0)
            {
                return true;
            }

            var selectModulesFromLinesQuery = SelectModulesFromLinesQuery(line.KTR_ProductTemplate.Id);
            var resultsModulesFromLines = service.RetrieveMultiple(selectModulesFromLinesQuery);
            var moduleIds = JoinModuleIds(resultsModulesFromLines);

            if (moduleIds.Count() == 0)
            {
                return false;
            }

            var selectQuestionInModulesByQuestionIdQuery = SelectQuestionInModulesByQuestionIdQuery(moduleIds, questionIds);
            var resultQuestion = service.RetrieveMultiple(selectQuestionInModulesByQuestionIdQuery);

            return resultQuestion.Entities.Count > 0;
        }

        private bool IsQuestionAlreadyInAssociatedModules(IOrganizationService service, KTR_ProductTemplateLine line)
        {
            if (line.KTR_Type != KTR_ProductTemplateLineType.Question)
            {
                return false;
            }

            var questionId = line.KTR_KT_QuestionBank.Id;
            
            var selectModulesFromLinesQuery = SelectModulesFromLinesQuery(line.KTR_ProductTemplate.Id);
            var resultsModulesFromLines = service.RetrieveMultiple(selectModulesFromLinesQuery);
            var moduleIds = JoinModuleIds(resultsModulesFromLines);

            if (moduleIds.Count() == 0)
            {
                return false;
            }

            var selectQuestionInModulesByQuestionIdQuery = SelectQuestionInModulesByQuestionIdQuery(moduleIds, new List<Guid> { questionId });
            var resultQuestion = service.RetrieveMultiple(selectQuestionInModulesByQuestionIdQuery);

            return resultQuestion.Entities.Count > 0;
        }

        #region Queries

        private QueryExpression SelectLinesByTemplateAndTypeQuery(KTR_ProductTemplateLine line)
        {
            var query = new QueryExpression(KTR_ProductTemplateLine.EntityLogicalName);
            query.TopCount = 1;

            query.ColumnSet.AddColumn(KTR_ProductTemplateLine.Fields.KTR_ProductTemplateLineId);

            query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_ProductTemplate, ConditionOperator.Equal, line.KTR_ProductTemplate.Id);

            switch (line.KTR_Type)
            {
                case KTR_ProductTemplateLineType.Module:
                    query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_KT_Module, ConditionOperator.Equal, line.KTR_KT_Module.Id);
                    break;
                case KTR_ProductTemplateLineType.Question:
                    query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_KT_QuestionBank, ConditionOperator.Equal, line.KTR_KT_QuestionBank.Id);
                    break;
                default:
                    break;
            }

            return query;
        }

        private QueryExpression SelectLinesByTemplateAndQuestionsQuery(Guid productTemplateId, IEnumerable<Guid> questionIds)
        {
            var query = new QueryExpression(KTR_ProductTemplateLine.EntityLogicalName);
            query.TopCount = 1;

            query.ColumnSet.AddColumn(KTR_ProductTemplateLine.Fields.KTR_ProductTemplateLineId);

            query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_ProductTemplate, ConditionOperator.Equal, productTemplateId);
            query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_KT_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            return query;
        }

        private QueryExpression SelectQuestionsInModuleQuery(Guid moduleId)
        {
            var query = new QueryExpression(KTR_ModuleQuestionBank.EntityLogicalName);

            query.ColumnSet.AddColumn(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank);

            query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_Module, ConditionOperator.Equal, moduleId);

            return query;
        }

        private QueryExpression SelectQuestionInModulesByQuestionIdQuery(IList<Guid> moduleIds, IList<Guid> questionIds)
        {
            var query = new QueryExpression(KTR_ModuleQuestionBank.EntityLogicalName);
            query.TopCount = 1;

            query.ColumnSet.AddColumn(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank);

            query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_Module, ConditionOperator.In, moduleIds.Cast<object>().ToArray());

            if (questionIds.Count == 1)
            {
                query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank, ConditionOperator.Equal, questionIds.First());
            }
            else
            {
                query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());
            }

            return query;
        }

        private QueryExpression SelectModulesFromLinesQuery(Guid productTemplateId)
        {
            var query = new QueryExpression(KTR_ProductTemplateLine.EntityLogicalName);

            query.ColumnSet.AddColumn(KTR_ProductTemplateLine.Fields.KTR_KT_Module);

            query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_Type, ConditionOperator.Equal, (int)KTR_ProductTemplateLineType.Module);
            query.Criteria.AddCondition(KTR_ProductTemplateLine.Fields.KTR_ProductTemplate, ConditionOperator.Equal, productTemplateId);

            return query;
        }

        #endregion

        #region Auxiliar

        public IList<Guid> JoinModuleQuestionIds(EntityCollection resultsQuestions)
        {
            var questionIds = new List<Guid>();
            foreach (var entity in resultsQuestions.Entities)
            {
                if (entity.Contains(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank) && entity[KTR_ModuleQuestionBank.Fields.KTR_QuestionBank] is EntityReference questionRef)
                {
                    questionIds.Add(questionRef.Id);
                }
            }
            return questionIds;
        }

        public IList<Guid> JoinModuleIds(EntityCollection resultsModules)
        {
            var moduleIds = new List<Guid>();
            foreach (var entity in resultsModules.Entities)
            {
                if (entity.Contains(KTR_ProductTemplateLine.Fields.KTR_KT_Module) && entity[KTR_ProductTemplateLine.Fields.KTR_KT_Module] is EntityReference questionRef)
                {
                    moduleIds.Add(questionRef.Id);
                }
            }
            return moduleIds;
        }

        #endregion 
    }
}
