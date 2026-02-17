namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLineAnswer;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.QuestionnaireLine.QuestionnaireLineAddQuestionsOrModules;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public class QuestionnaireLineAddQuestionsOrModulesCustomAPI : PluginBase
    {
        private const string PluginName = nameof(QuestionnaireLineAddQuestionsOrModulesCustomAPI);

        public QuestionnaireLineAddQuestionsOrModulesCustomAPI()
            : base(typeof(QuestionnaireLineAddQuestionsOrModulesCustomAPI))
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

            tracingService.Trace($"Starting {PluginName}");
            Guid userId = context.InitiatingUserId;
            var isScripter = UserHasOnlyScripterRole(service, userId);

            AddQuestionsOrModulesRequest req = GetInputs(context, service, tracingService);

            //If partial success, errors will be returned but successful adds will still be committed
            List<string> errors = req.EntityType == EntityTypeEnum.Question ?
                AddQuestionsToProject(service, tracingService, req,isScripter) :
                AddModulesToProject(service, tracingService, req);

            if (errors.Count > 0)
            {
                string allErrors = string.Join("\n", errors.Distinct());
                tracingService.Trace($"Completed {PluginName} with errors: {allErrors}");
                throw new InvalidPluginExecutionException($"{allErrors}");
            }
        }

        public AddQuestionsOrModulesRequest GetInputs(IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracingService)
        {
            var reqParams = new AddQuestionsOrModulesRequest();
            reqParams.ProjectId = context.GetInputParameter<Guid>("projectId");

            var entity = service.Retrieve(KT_Project.EntityLogicalName, reqParams.ProjectId, new ColumnSet(KT_Project.Fields.Id));

            if (entity == null || entity.LogicalName != KT_Project.EntityLogicalName)
            {
                tracingService.Trace($"Project {reqParams.ProjectId} not found.");
                throw new InvalidPluginExecutionException($"Project {reqParams.ProjectId} not found.");
            }

            reqParams.SortOrder = context.GetInputParameter<int>("sortOrder");

            if (reqParams.SortOrder < -1)
            {
                tracingService.Trace($"Sort order invalid.");
                throw new InvalidPluginExecutionException($"Sort order invalid.");
            }

            // If -1, fetch highest sort order and append
            if (reqParams.SortOrder == -1)
            {
                var maxSort = GetHighestSortOrderForProject(service, reqParams.ProjectId);
                reqParams.SortOrder = maxSort + 1;
                tracingService.Trace($"SortOrder was -1, set to {reqParams.SortOrder}");
            }

            if (!context.InputParameters.Contains("entityType") ||
                !(context.InputParameters["entityType"] is string questionOrModule) ||
                Enum.TryParse<EntityTypeEnum>(questionOrModule, true, out var entityType) == false)
            {
                tracingService.Trace($"EntityType is invalid.");
                throw new InvalidPluginExecutionException($"EntityType is invalid.");
            }
            reqParams.EntityType = entityType;

            var rows = context.GetInputParameter<string>("rows");
            if (string.IsNullOrWhiteSpace(rows))
            {
                tracingService.Trace($"Questions or Modules should be provided.");
                throw new InvalidPluginExecutionException($"Questions or Modules should be provided.\n");
            }

            reqParams.Rows = JsonHelper.Deserialize<List<RowEntityRequest>>(rows, "rows")
                ?? throw new InvalidPluginExecutionException("Invalid Questions Request.");

            if (reqParams.Rows.Count() == 0)
            {
                tracingService.Trace($"Questions or Modules should be provided.");
                throw new InvalidPluginExecutionException($"Questions or Modules should be provided.\n");
            }

            return reqParams;
        }
        private bool UserHasOnlyScripterRole(IOrganizationService service, Guid userId)
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
            var businessRole = results.Entities.FirstOrDefault().GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole)?.Value;
            const int ScripterRoleValue = (int)KTR_KantarBusinessRole.KantarScripter;
            return businessRole == ScripterRoleValue;
        }
        private List<string> AddQuestionsToProject(IOrganizationService service, ITracingService tracingService, AddQuestionsOrModulesRequest request, bool isScripter)

        {

            //Get questions

            var questionIds = request.Rows.Select(id => id.Id).ToList();

            var questionBanks = GetQuestionBanks(questionIds, service, tracingService);

            var questionBankAnswers = GetQuestionBankAnswers(service, questionIds);

            // Validate duplicates
            ValidateQuestionDuplicate(service, request.ProjectId, questionBanks.Values.ToList());

            var requestCollection = new OrganizationRequestCollection();
            var index = request.SortOrder;
            var answersToInsert = new List<KTR_QuestionnaireLinesAnswerList>();
            // prepare insert questions

            foreach (var row in request.Rows)

            {
                var questionBank = (KT_QuestionBank)questionBanks[row.Id];
                var questionLine = QuestionnaireLineMapper.MapToEntity(
                   (KT_QuestionBank)questionBanks[row.Id],
                    request.ProjectId,
                   null,
                   index++);
                var isCustom = questionBank.GetAttributeValue<OptionSetValue>(KT_QuestionBank.Fields.KT_StandardOrCustom)?.Value == (int)KT_QuestionBank_KT_StandardOrCustom.Custom;
                if (isCustom && isScripter)
                {
                    tracingService.Trace($"Question {questionBank.Id} is custom → setting isdummy = true.");
                    questionLine[KT_QuestionnaireLines.Fields.KTR_IsDummyQuestion] = true;
                }
                requestCollection.Add(new CreateRequest { Target = questionLine });
                // prepare insert answers

                if (questionBankAnswers.TryGetValue(row.Id, out List<KTR_QuestionAnswerList> answers))

                {

                    foreach (var answer in answers)
                    {
                        var answerLine = answer.MapToEntity(questionLine.Id);
                        answersToInsert.Add(answerLine);

                    }

                }

            }
            var errors = ExecuteCreateQuestionnaireLinesAndMoveExisting(service, tracingService, request.ProjectId, request.SortOrder, requestCollection);
            CreateQuestionnaireLinesAnswers(service, answersToInsert);
            return errors;
        }
        private List<string> AddModulesToProject(IOrganizationService service, ITracingService tracingService, AddQuestionsOrModulesRequest request)
        {
            var moduleIds = request.Rows.Select(r => r.Id).ToList();

            //check if modules exist
            GetModules(moduleIds, service, tracingService);

            // Validate duplicates
            ValidateModuleDuplicate(service, request.ProjectId, moduleIds);

            var requestCollection = new OrganizationRequestCollection();
            var index = request.SortOrder;

            //Expand modules to questionIds and add them
            var moduleToQuestionIds = GetQuestionsIdsInModule(service, moduleIds);

            var answersToInsert = new List<KTR_QuestionnaireLinesAnswerList>();
            foreach (var moduleId in moduleIds)
            {
                var questionIds = moduleToQuestionIds[moduleId];

                var questionBanks = GetQuestionBanks(questionIds, service, tracingService);
                var questionBankAnswers = GetQuestionBankAnswers(service, questionIds);

                foreach (var questionId in questionIds)
                {
                    var questionLine = QuestionnaireLineMapper.MapToEntity(
                       (KT_QuestionBank)questionBanks[questionId],
                       request.ProjectId,
                       moduleId,
                       index++);

                    requestCollection.Add(new CreateRequest { Target = questionLine });

                    // prepare insert answers
                    if (questionBankAnswers.TryGetValue(questionId, out List<KTR_QuestionAnswerList> answers))
                    {
                        foreach (var answer in answers)
                        {
                            var answerLine = answer.MapToEntity(questionLine.Id);
                            answersToInsert.Add(answerLine);
                        }
                    }
                }
            }

            var errors = ExecuteCreateQuestionnaireLinesAndMoveExisting(service, tracingService, request.ProjectId, request.SortOrder, requestCollection);

            CreateQuestionnaireLinesAnswers(service, answersToInsert);

            return errors;
        }

        private Dictionary<Guid, Entity> GetQuestionBanks(
            List<Guid> questionIds,
            IOrganizationService service,
            ITracingService tracingService)
        {
            var query = new QueryExpression(KT_QuestionBank.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionBank.Fields.Id, ConditionOperator.In, questionIds.ToArray()),
                        new ConditionExpression(KT_QuestionBank.Fields.StateCode, ConditionOperator.Equal, (int)KT_QuestionBank_StateCode.Active)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            var dictionaryIds = results.Entities
                .ToDictionary(ent => ent.Id, ent => ent);
            
            if (results.Entities.Count != questionIds.Count)
            {
                var missingIds = string.Join(", ", questionIds.Where(qs => !dictionaryIds.ContainsKey(qs)).Select(qs => qs.ToString()));
                tracingService.Trace($"Question(s) with ids {missingIds} not found or inactive.");
                throw new InvalidPluginExecutionException($"Question(s) with ids {missingIds} not found or inactive.");
            }

            return dictionaryIds;
        }

        private Dictionary<Guid, Entity> GetModules(
            List<Guid> questionIds,
            IOrganizationService service,
            ITracingService tracingService)
        {
            var query = new QueryExpression(KT_Module.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Module.Fields.KT_ModuleId),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Module.Fields.Id, ConditionOperator.In, questionIds.ToArray()),
                        new ConditionExpression(KT_Module.Fields.StateCode, ConditionOperator.Equal, (int)KT_Module_StateCode.Active)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            var dictionaryIds = results.Entities.ToDictionary(ent => ent.Id, ent => ent);

            if (results.Entities.Count != questionIds.Count)
            {
                var missingIds = string.Join(", ", questionIds.Where(qs => !dictionaryIds.ContainsKey(qs)).Select(qs => qs.ToString()));
                tracingService.Trace($"Module(s) with ids {missingIds} not found or inactive.");
                throw new InvalidPluginExecutionException($"Module(s) with ids {missingIds} not found or inactive.");
            }

            return dictionaryIds;
        }

        private Dictionary<Guid, List<KTR_QuestionAnswerList>> GetQuestionBankAnswers(
            IOrganizationService service,
            List<Guid> questionIds)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Distinct = true,
            };

            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionAnswerList_StatusCode.Active);
            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            var dictionaryAnswerLists = results.Entities
                .Select(e => e.ToEntity<KTR_QuestionAnswerList>())
                .GroupBy(ent => ent.KTR_KT_QuestionBank.Id)
                .ToDictionary(group => group.Key, group => group.ToList());
            return dictionaryAnswerLists;
        }

        private Dictionary<Guid, List<Guid>> GetQuestionsIdsInModule(IOrganizationService service, List<Guid> moduleIds)
        {
            var query = new QueryExpression(KTR_ModuleQuestionBank.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    KTR_ModuleQuestionBank.Fields.KTR_QuestionBank,
                    KTR_ModuleQuestionBank.Fields.KTR_Module
                    ),
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_ModuleQuestionBank.Fields.KTR_Module, ConditionOperator.In,
                                moduleIds.ToArray()),
                            new ConditionExpression(KTR_ModuleQuestionBank.Fields.StateCode, ConditionOperator.Equal, (int)KTR_ModuleQuestionBank_StateCode.Active),
                        }
                    },
                Orders =
                {
                    new OrderExpression(KTR_ModuleQuestionBank.Fields.KTR_SortOrder, OrderType.Ascending)
                },
            };

            var results = service.RetrieveMultiple(query).Entities.Select(e => e.ToEntity<KTR_ModuleQuestionBank>());

            Dictionary<Guid, List<Guid>> moduleToQuestions = new Dictionary<Guid, List<Guid>>();
            foreach (var moduleQuestion in results)
            {
                if (!moduleToQuestions.ContainsKey(moduleQuestion.KTR_Module.Id))
                {
                    moduleToQuestions[moduleQuestion.KTR_Module.Id] = new List<Guid>();
                }
                moduleToQuestions[moduleQuestion.KTR_Module.Id].Add(moduleQuestion.KTR_QuestionBank.Id);
            }

            return moduleToQuestions;
        }

        private List<string> ExecuteCreateQuestionnaireLinesAndMoveExisting(
            IOrganizationService service,
            ITracingService tracingService,
            Guid projectId,
            int sortOrder,
            OrganizationRequestCollection requestCollection)
        {
            //Get the questions we'll need to move later
            var qLinesToMove = GetQuestionsToReorder(service, projectId, sortOrder);

            var executeMultipleCreateRequests = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                }
            };

            //Create new lines first (in case of errors, existing lines are not moved)
            var responseCreate = (ExecuteMultipleResponse)service.Execute(executeMultipleCreateRequests);
            var errorMessages = new List<string>();

            if (responseCreate.IsFaulted)
            {
                // Get all error messages
                errorMessages.AddRange(responseCreate.Responses
                    .Where(r => r.Fault != null).Select(resp => resp.Fault.Message));
            }

            var successResponses = responseCreate.Responses.Where(r => r.Fault == null).ToList();
            var skipped = responseCreate.Responses.Where(r => r.Fault != null).ToList();
            tracingService.Trace($"Created {successResponses.Count} new QuestionnaireLines and skipped creation of {skipped.Count} QuestionnaireLines");
            
            if (successResponses.Count == 0) //all failed
            {
                var allErrors = string.Join("\n", errorMessages);
                tracingService.Trace($"Creating QuestionnaireLines failed with errors: {allErrors}");
                throw new InvalidPluginExecutionException($"{allErrors}");
            }
            else if (skipped.Count != 0 && successResponses.Count != 0) //partial success
            {
                FixSortOrder(successResponses, executeMultipleCreateRequests, sortOrder, service);
                tracingService.Trace($"Fixed sort order");
            }

            //update sort order of existing lines to make space for new ones
            ReorderExistingQuestionnaireLines(qLinesToMove, successResponses.Count, service, tracingService);

            tracingService.Trace("Successfully created new QuestionnaireLines and moved existing QuestionnaireLines");

            return errorMessages;
        }

        private void FixSortOrder(List<ExecuteMultipleResponseItem> response, ExecuteMultipleRequest executedCreates, int sortOrder, IOrganizationService service)
        {
            var batch = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = false
                }
            };

            int index = sortOrder;
            foreach (var item in response)
            {
                var req = (CreateRequest)executedCreates.Requests[item.RequestIndex];
                var entity = (KT_QuestionnaireLines)req.Target;
                entity.KT_QuestionSortOrder = index++;

                batch.Requests.Add(new UpdateRequest { Target = entity });
            }

            var responseFixSortOrder = (ExecuteMultipleResponse)service.Execute(batch);
        }

        private EntityCollection GetQuestionsToReorder(IOrganizationService service, Guid projectId, int oldSortOrder)
        {
            var query = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId),
                        new ConditionExpression(KT_QuestionnaireLines.Fields.StateCode, ConditionOperator.Equal, (int)KT_QuestionnaireLines_StateCode.Active),
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder, ConditionOperator.GreaterEqual, oldSortOrder)
                    }
                }
            };
            return service.RetrieveMultiple(query);
        }

        private void ReorderExistingQuestionnaireLines(EntityCollection questionsToReorder, int amountToMove, IOrganizationService service, ITracingService tracing)
        {
            const string SortOrderField = KT_QuestionnaireLines.Fields.KT_QuestionSortOrder;

            var batch = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = false
                }
            };

            if (!questionsToReorder.Entities.Any())
            {
                tracing.Trace("No siblings to reorder.");
                return;
            }

            foreach (var record in questionsToReorder.Entities)
            {
                record[SortOrderField] = (int)record[SortOrderField] + amountToMove;

                batch.Requests.Add(new UpdateRequest { Target = record });
                tracing.Trace($"Queued update for ID: {record.Id}, {SortOrderField}: {(int)record[SortOrderField] - amountToMove} → {(int)record[SortOrderField]}");
            }

            var responseMoveQLines = (ExecuteMultipleResponse)service.Execute(batch);
            if (responseMoveQLines.IsFaulted)
            {
                tracing.Trace("Moving QuestionnaireLines failed");
                throw new InvalidPluginExecutionException("Moving QuestionnaireLines failed");
            }
        }

        private int GetHighestSortOrderForProject(IOrganizationService service, Guid projectId)
        {
            var query = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId)
                    }
                },
                Orders =
                {
                    new OrderExpression(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder, OrderType.Descending)
                },
                TopCount = 1
            };

            var results = service.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                return 0;
            }
            return results.Entities[0].GetAttributeValue<int>(KT_QuestionnaireLines.Fields.KT_QuestionSortOrder);
        }

        private void ValidateQuestionDuplicate(
            IOrganizationService service,
            Guid projectId,
            List<Entity> questionBanks)
        {
            // Fetch existing questionnaire lines (active + inactive)
            var existingQLs = GetQuestionnaireLinesForProject(
                service,
                projectId,
                new ColumnSet(
                    KT_QuestionnaireLines.Fields.KT_QuestionVariableName,
                    KT_QuestionnaireLines.Fields.KTR_QuestionBank,
                    KT_QuestionnaireLines.Fields.KT_StandardOrCustom)
            ).Entities;

            // Separate lookups and variable names
            var existingStandardsQuestionBankIds = new HashSet<Guid>(
                existingQLs
                    .Where(e => e.Contains(KT_QuestionnaireLines.Fields.KTR_QuestionBank))
                    .Where(e => e.Contains(KT_QuestionnaireLines.Fields.KT_StandardOrCustom) && e.GetAttributeValue<OptionSetValue>(KT_QuestionnaireLines.Fields.KT_StandardOrCustom)?.Value == (int)KT_QuestionnaireLines_KT_StandardOrCustom.Standard)
                    .Select(e => e.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_QuestionBank).Id)
            );

            var existingVariableNames = new HashSet<string>(
                existingQLs
                    .Select(e => e.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KT_QuestionVariableName)?.Trim().ToLower())
                    .Where(v => !string.IsNullOrEmpty(v))
            );

            // Loop through the incoming question bank records
            foreach (var q in questionBanks)
            {
                var qId = q.Id;
                var qName = q.GetAttributeValue<string>(KT_QuestionBank.Fields.KT_Name);
                var qVarName = qName?.Trim().ToLower();

                // If Standard question
                if (existingStandardsQuestionBankIds.Contains(qId))
                {
                    throw new InvalidPluginExecutionException(
                        $"Question '{qName}' already exists in the Project. Please go to inactivated questions view and activate it again instead.");
                }

                // Else if variable-name based duplicate (For custom question)
                if (!string.IsNullOrEmpty(qVarName) && existingVariableNames.Contains(qVarName))
                {
                    throw new InvalidPluginExecutionException(
                        $"Custom Question '{qName}' already exists in the Project. Please go to inactivated questions view and activate it again instead.");
                }
            }
        }

        private void ValidateModuleDuplicate(
            IOrganizationService service,
            Guid projectId,
            List<Guid> moduleIds)
        {
            // Fetch existing questionnaire lines (active + inactive)
            var existingModuleLines = GetQuestionnaireLinesForProject(
                service,
                projectId,
                new ColumnSet(KT_QuestionnaireLines.Fields.KTR_Module)
            ).Entities;

            // Existing module IDs
            var existingModuleIds = new HashSet<Guid>(
                existingModuleLines
                    .Where(e => e.Contains(KT_QuestionnaireLines.Fields.KTR_Module))
                    .Select(e => e.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_Module).Id)
            );

            foreach (var moduleId in moduleIds)
            {
                if (existingModuleIds.Contains(moduleId))
                {
                    // Get module name
                    var moduleName = service.Retrieve(KT_Module.EntityLogicalName, moduleId, new ColumnSet(KT_Module.Fields.KT_Name))
                        .GetAttributeValue<string>(KT_Module.Fields.KT_Name);

                    throw new InvalidPluginExecutionException(
                        $"Module '{moduleName}' already exists and can't be added to the Project. Please go to inactivated questions view and activate it again instead.");
                }
            }
        }

        #region Queries to Dataverse - QuestionnaireLines Answers
        private void CreateQuestionnaireLinesAnswers(
            IOrganizationService service,
            IList<KTR_QuestionnaireLinesAnswerList> answers)
        {
            if (answers == null || answers.Count == 0)
            {
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var answer in answers)
            {
                requestCollection.Add(new CreateRequest { Target = answer });
            }

            var bulkInsertAnswers = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            service.Execute(bulkInsertAnswers);
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLines
        private EntityCollection GetQuestionnaireLinesForProject(
            IOrganizationService service,
            Guid projectId,
            ColumnSet columns)
        {
            var query = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName)
            {
                ColumnSet = columns,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId),
                    }
                }
            };

            return service.RetrieveMultiple(query);
        }
        #endregion
    }
}
