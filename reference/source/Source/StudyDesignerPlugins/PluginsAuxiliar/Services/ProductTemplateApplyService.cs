using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services
{
    public class ProductTemplateApplyService
    {
        private readonly ITracingService _tracing;
        private readonly DataverseContext _dataverseContext;
        private readonly IOrganizationService _service;

        public ProductTemplateApplyService(
            DataverseContext dataverseContext,
            IOrganizationService service,
            ITracingService tracing)
        {
            _dataverseContext = dataverseContext;
            _service = service;
            _tracing = tracing;
        }

        /// <summary>
        /// Apply Product Template logic and rules
        /// </summary>
        /// <param name="productTemplate">Template to apply</param>
        /// <param name="dependencyRulesToApply">Dependency rules to apply</param>
        /// <returns></returns>
        public IList<TemplateLineQuestionResult> ApplyProductTemplate(
            Guid productTemplateId,
            IList<KTR_DependencyRule> dependencyRulesToApply)
        {
            var productTemplateLines = GetProductTemplateLines(productTemplateId);

            var templateQuestions = ExtractQuestionsFromTemplateLines(productTemplateLines);

            ApplyTemplateLines(templateQuestions);

            var questionsResult = ApplyDependencyRules(dependencyRulesToApply, templateQuestions);

            var questionsToInclude = questionsResult
                .Where(x => x.Value.IsIncluded)
                .ToDictionary(x => x.Key, x => x.Value);

            ReorderQuestions(questionsToInclude);

            return questionsToInclude
                .MapToResult();
        }

        /// <summary>
        /// If a ConfigQuestion = MultiChoice, then DependencyRuleAnswers should match exactly the user's answers
        /// </summary>
        /// <param name="dependencyRules"></param>
        /// <param name="dependencyRuleAnswers"></param>
        /// <returns></returns>
        public List<KTR_DependencyRule> FilterExactMatchMultiChoiceDependencyRules(
            IList<KTR_DependencyRule> dependencyRules,
            IList<Guid> answerIds)
        {
            var filteredRules = new List<KTR_DependencyRule>();

            if (dependencyRules == null || dependencyRules.Count == 0
               || answerIds == null || answerIds.Count == 0)
            {
                return filteredRules;
            }

            var dependencyRulesAnswers = GetDependencyRuleAnswers(dependencyRules);

            foreach (var rule in dependencyRules)
            {
                var ruleAnswerIds = dependencyRulesAnswers
                    .Where(x => x.KTR_DependencyRule != null
                        && x.KTR_ConfigurationAnswer != null
                        && x.KTR_DependencyRule.Id == rule.Id)
                    .Select(x => x.KTR_ConfigurationAnswer.Id)
                    .ToList();

                if (ruleAnswerIds.All(x => answerIds.Contains(x)))
                {
                    filteredRules.Add(rule);
                }
            }

            return filteredRules;
        }

        private IDictionary<Guid, TemplateLineQuestionContext> ExtractQuestionsFromTemplateLines(
            IList<KTR_ProductTemplateLine> productTemplateLines)
        {
            if (productTemplateLines == null || productTemplateLines.Count == 0)
            {
                return new Dictionary<Guid, TemplateLineQuestionContext>();
            }

            var questionsDict = new Dictionary<Guid, TemplateLineQuestionContext>();

            var order = 0;
            foreach (var line in productTemplateLines
                                    .OrderBy(q => q.KTR_DisplayOrder)
                                    .ThenBy(q => q.CreatedOn))
            {
                switch (line.KTR_Type)
                {
                    case KTR_ProductTemplateLineType.Question:
                        var questionId = line.KTR_KT_QuestionBank.Id;
                        var questionOrder = order++;
                        var questionCreatedOn = line.CreatedOn.GetValueOrDefault();
                        if (!questionsDict.ContainsKey(questionId))
                        {
                            questionsDict[questionId] = new TemplateLineQuestionContext(
                                line,
                                line.KTR_KT_QuestionBank.Id,
                                null,
                                questionOrder,
                                questionCreatedOn);
                        }
                        break;
                    case KTR_ProductTemplateLineType.Module:
                        var moduleId = line.KTR_KT_Module.Id;
                        var moduleQuestions = GetModuleQuestionBanks(moduleId);

                        var sortedModuleQuestions = moduleQuestions
                            .OrderBy(x => x.KTR_SortOrder).ToList();

                        foreach (var moduleQuestion in sortedModuleQuestions)
                        {
                            var moduleQuestionId = moduleQuestion.KTR_QuestionBank.Id;
                            var moduleQuestionOrder = order++;
                            var moduleQuestionCreatedOn = moduleQuestion.CreatedOn.GetValueOrDefault();
                            if (!questionsDict.ContainsKey(moduleQuestionId))
                            {
                                questionsDict[moduleQuestionId] = new TemplateLineQuestionContext(
                                    line,
                                    moduleQuestionId,
                                    line.KTR_KT_Module.Id,
                                    moduleQuestionOrder,
                                    moduleQuestionCreatedOn);
                            }
                        }
                        break;
                }
            }

            return questionsDict;
        }

        private void ApplyTemplateLines(IDictionary<Guid, TemplateLineQuestionContext> templateQuestions)
        {
            foreach (var item in templateQuestions)
            {
                if (item.Value.ProductTemplateLine.KTR_IncludeByDefault.GetValueOrDefault())
                {
                    item.Value.IsIncluded = true;
                }
            }
        }

        private IDictionary<Guid, TemplateLineQuestionContext> ApplyDependencyRules(
            IList<KTR_DependencyRule> dependencyRules,
            IDictionary<Guid, TemplateLineQuestionContext> templateQuestions)
        {
            if (dependencyRules == null || dependencyRules.Count == 0)
            {
                return templateQuestions;
            }

            var orderedRules = dependencyRules
                .OrderBy(x => x.KTR_Classification != KTR_DependencyRule_KTR_Classification.Primary)
                .ThenBy(x => x.CreatedOn)
                .ToList();

            foreach (var rule in orderedRules)
            {
                if (rule.KTR_Type == KTR_DependencyRule_KTR_Type.Exclude)
                {
                    switch (rule.KTR_ContentType)
                    {
                        case KTR_DependencyRule_KTR_ContentType.Question:
                            ExcludeQuestions(templateQuestions, new List<Guid> { rule.KTR_KT_QuestionBank.Id });
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Module:
                            var questionsToRemove = templateQuestions.Values
                                .Where(x => x.ModuleId == rule.KTR_KT_Module.Id)
                                .Select(x => x.QuestionId)
                                .ToList();

                            ExcludeQuestions(templateQuestions, questionsToRemove);
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Tag:
                            var tag = rule.KTR_Tag;

                            var tagQuestions = GetAllQuestionBanksByTagIdAndQuestionIds(
                                tag.Id,
                                templateQuestions.Keys.Cast<Guid>().ToList());

                            if (tagQuestions != null && tagQuestions.Count > 0)
                            {
                                ExcludeQuestions(templateQuestions, tagQuestions.Select(x => x.Id).ToList());
                            }
                            break;
                    }
                }
                else if (rule.KTR_Type == KTR_DependencyRule_KTR_Type.Include)
                {
                    switch (rule.KTR_ContentType)
                    {
                        case KTR_DependencyRule_KTR_ContentType.Question:
                            IncludeQuestions(templateQuestions, new List<Guid> { rule.KTR_KT_QuestionBank.Id });
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Module:
                            var questionsToInclude = templateQuestions.Values
                                .Where(x => x.ModuleId == rule.KTR_KT_Module.Id)
                                .Select(x => x.QuestionId)
                                .ToList();

                            IncludeQuestions(templateQuestions, questionsToInclude);
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Tag:
                            var tag = rule.KTR_Tag;

                            var tagQuestions = GetAllQuestionBanksByTagId(tag.Id);

                            if (tagQuestions != null && tagQuestions.Count > 0)
                            {
                                IncludeQuestions(templateQuestions, tagQuestions.Select(x => x.Id).ToList());
                            }
                            break;
                    }
                }
            }

            return templateQuestions;
        }

        private void ExcludeQuestions(IDictionary<Guid, TemplateLineQuestionContext> questionsFromTemplate, List<Guid> questionIds)
        {
            foreach (var q in questionIds)
            {
                questionsFromTemplate.Remove(q);
            }
        }

        private void IncludeQuestions(IDictionary<Guid, TemplateLineQuestionContext> questionsFromTemplate, List<Guid> questionIds)
        {
            foreach (var q in questionIds)
            {
                if (questionsFromTemplate.ContainsKey(q))
                {
                    questionsFromTemplate[q].IsIncluded = true;
                }
            }
        }

        private void ReorderQuestions(IDictionary<Guid, TemplateLineQuestionContext> questions)
        {
            int order = 1;
            foreach (var q in questions.Values
                .OrderBy(q => q.DisplayOrder)
                .ThenBy(q => q.CreatedOn))
            {
                q.DisplayOrder = order++;
            }
        }

        #region Queries to Dataverse - Product Template Line
        private IList<KTR_ProductTemplateLine> GetProductTemplateLines(
            Guid productTemplateId)
        {
            return _dataverseContext
                .CreateQuery<KTR_ProductTemplateLine>()
                .Where(x => x.KTR_ProductTemplate.Id == productTemplateId
                    && x.StatusCode == KTR_ProductTemplateLine_StatusCode.Active)
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Module-Question Bank
        private IList<KTR_ModuleQuestionBank> GetModuleQuestionBanks(
            Guid moduleId)
        {
            return _dataverseContext
                .CreateQuery<KTR_ModuleQuestionBank>()
                .Where(x => x.StatusCode == KTR_ModuleQuestionBank_StatusCode.Active
                    && x.KTR_Module.Id == moduleId)
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Question Bank
        private IList<KT_QuestionBank> GetAllQuestionBanksByTagId(
            Guid tagId)
        {
            var query = from q in _dataverseContext.CreateQuery<KT_QuestionBank>()
                        join qt in _dataverseContext.CreateQuery<KTR_Tag_KT_QuestionBank>()
                            on q.Id equals qt.KT_QuestionBankId
                        where q.StatusCode == KT_QuestionBank_StatusCode.Active
                            && qt.KTR_TagId == tagId
                        select q;

            return query.ToList();
        }

        private IList<KT_QuestionBank> GetAllQuestionBanksByTagIdAndQuestionIds(
            Guid tagId,
            IList<Guid> questionBankIds)
        {
            if (questionBankIds == null || questionBankIds.Count == 0)
            {
                return new List<KT_QuestionBank>();
            }

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionBank.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition(KT_QuestionBank.Fields.KT_QuestionBankId, ConditionOperator.In, questionBankIds.Cast<object>().ToArray());
            query.Criteria.AddCondition(KT_QuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionBank_StatusCode.Active);

            var linkTag = query.AddLink(
                KTR_Tag_KT_QuestionBank.EntityLogicalName,
                KT_QuestionBank.Fields.KT_QuestionBankId,
                KTR_Tag_KT_QuestionBank.Fields.KT_QuestionBankId);

            linkTag.LinkCriteria.AddCondition(KTR_Tag_KT_QuestionBank.Fields.KTR_TagId, ConditionOperator.Equal, tagId);

            var results = _service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionBank>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Dependency Rules Answer
        private IList<KTR_DependencyRuleAnswer> GetDependencyRuleAnswers(
            IList<KTR_DependencyRule> dependencyRules)
        {
            if (dependencyRules == null || dependencyRules.Count == 0)
            {
                return new List<KTR_DependencyRuleAnswer>();
            }

            var dependencyRulesIds = dependencyRules
                .Select(d => d.Id)
                .Distinct()
                .ToArray();

            var query = new QueryExpression()
            {
                EntityName = KTR_DependencyRuleAnswer.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition(KTR_DependencyRuleAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRuleAnswer_StatusCode.Active);
            query.Criteria.AddCondition(KTR_DependencyRuleAnswer.Fields.KTR_DependencyRule, ConditionOperator.In, dependencyRulesIds.Cast<object>().ToArray());
            
            var results = _service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRuleAnswer>())
                .ToList();
        }
        #endregion
    }
}
