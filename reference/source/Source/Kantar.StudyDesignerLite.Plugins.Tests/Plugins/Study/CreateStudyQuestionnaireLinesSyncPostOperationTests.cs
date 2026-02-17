namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    using System;
    using System.Linq;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.Study;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;

    [TestClass]
    public class CreateStudyQuestionnaireLinesSyncPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_NoParent_Success()
        {

            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            var qLines1 = new QuestionnaireLineBuilder(project)
                .WithState(0) // Active
                .WithVariableName("TestVariable1")
                .Build();

            var qLines2 = new QuestionnaireLineBuilder(project)
                .WithState(1) // Inactive
                .WithVariableName("TestVariable2")
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", study }
            };

            _context.Initialize(new Entity[]
                {
                    study, project,
                    qLines1, qLines2
                });

            var plugin = new CreateStudyQuestionnaireLinesSyncPostOperation();

            // Act
            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(pluginContext);

            // Assert
            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLine.EntityLogicalName,
                ColumnSet = new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_Name,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_Study,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine,
                                          KTR_StudyQuestionnaireLine.Fields.StateCode,
                                          KTR_StudyQuestionnaireLine.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, study.Id)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);

            var studyQuestionnaireLines = results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionnaireLine>())
                .ToList();
            Assert.IsTrue(studyQuestionnaireLines.Count == 2);
            var activeLine = studyQuestionnaireLines.Where(x => x.StateCode == KTR_StudyQuestionnaireLine_StateCode.Active);
            Assert.IsTrue(activeLine.Count() == 1);
            Assert.IsTrue(activeLine.First().KTR_QuestionnaireLine.Id == qLines1.Id);
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_Success()
        {

            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            var qLines1 = new QuestionnaireLineBuilder(project)
                .WithState(0) // Active
                .WithVariableName("TestVariable1")
                .Build();

            var qLines2 = new QuestionnaireLineBuilder(project)
                .WithState(1) // Inactive
                .WithVariableName("TestVariable2")
                .Build();

            var qLines3 = new QuestionnaireLineBuilder(project)
                .WithState(0) // Active
                .WithVariableName("TestVariable3")
                .Build();

            var qLines4 = new QuestionnaireLineBuilder(project)
                .WithState(1) // Inactive
                .WithVariableName("TestVariable4")
                .Build();

            //Parent Study
            var parent = new StudyBuilder(project)
                .WithName("Study 1")
                .Build();

            var studyQLines1 = new StudyQuestionnaireLineBuilder(parent, qLines1)
                .WithState(0) // Both Active
                .Build();

            var studyQLines2 = new StudyQuestionnaireLineBuilder(parent, qLines2)
                .WithState(1) // Both Inactive
                .Build();

            var studyQLines3 = new StudyQuestionnaireLineBuilder(parent, qLines3)
                .WithState(1) // Active in Parent, Inactive in Study
                .Build();

            var studyQLines4 = new StudyQuestionnaireLineBuilder(parent, qLines4)
                .WithState(0) // Inactive in Parent, Active in Study
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 2")
                .WithParentStudy(parent)
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", study }
            };

            _context.Initialize(new Entity[]
                {
                    parent, study, project,
                    qLines1, qLines2, qLines3, qLines4,
                    studyQLines1, studyQLines2, studyQLines3, studyQLines4
                });

            var plugin = new CreateStudyQuestionnaireLinesSyncPostOperation();

            // Act
            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(pluginContext);

            // Assert
            var query = new QueryExpression()
            {
                EntityName = KTR_StudyQuestionnaireLine.EntityLogicalName,
                ColumnSet = new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_Name,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_Study,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_SortOrder,
                                          KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine,
                                          KTR_StudyQuestionnaireLine.Fields.StateCode,
                                          KTR_StudyQuestionnaireLine.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_StudyQuestionnaireLine.Fields.KTR_Study, ConditionOperator.Equal, study.Id)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);

            var studyQuestionnaireLines = results.Entities
                .Select(e => e.ToEntity<KTR_StudyQuestionnaireLine>())
                .ToList();
            Assert.IsTrue(studyQuestionnaireLines.Count == 4);
            var activeLine = studyQuestionnaireLines.Where(x => x.StateCode == KTR_StudyQuestionnaireLine_StateCode.Active);
            Assert.IsTrue(activeLine.Count() == 1);
            Assert.IsTrue(activeLine.First().KTR_QuestionnaireLine.Id == qLines1.Id);
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_InheritInactiveManagedListEntity_FromParent()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();
            var parentStudy = new StudyBuilder(project).WithName("Parent").Build();
            var childStudy = new StudyBuilder(project).WithName("Child").WithParentStudy(parentStudy).Build();
            var managedList = new ManagedListBuilder(project).WithName("ML1").Build();
            var qlManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();
            var mle = new ManagedListEntityBuilder(managedList)
                .WithAnswerText("A1")
                .WithStateCode(KTR_ManagedListEntity_StateCode.Inactive)
                .WithStatusCode(KTR_ManagedListEntity_StatusCode.Inactive)
                .Build();

            // Parent already has Study Managed List Entity set inactive
            var parentStudyMLE = new StudyManagedListEntityBuilder(mle).Build();
            parentStudyMLE.KTR_Study = parentStudy.ToEntityReference();

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = childStudy;

            _context.Initialize(new Entity[] { project, parentStudy, childStudy, qLine, managedList, qlManagedList, mle, parentStudyMLE });

            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            var childStudyMles = _context.CreateQuery<KTR_StudyManagedListEntity>().Where(x => x.KTR_Study.Id == childStudy.Id).ToList();
            Assert.AreEqual(1, childStudyMles.Count, "Should copy one managed list entity.");
            Assert.AreEqual(KTR_StudyManagedListEntity_StateCode.Inactive, childStudyMles[0].StateCode, "State should inherit inactive.");
            Assert.AreEqual(KTR_StudyManagedListEntity_StatusCode.Inactive, childStudyMles[0].StatusCode, "Status should inherit inactive.");
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_NoManagedListEntities_NoCopy()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();
            var study = new StudyBuilder(project).WithName("Study ML").Build();
            // Intentionally no managed list / managed list entity

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, study, qLine });

            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            var studyMles = _context.CreateQuery<KTR_StudyManagedListEntity>().Where(x => x.KTR_Study.Id == study.Id).ToList();
            Assert.AreEqual(0, studyMles.Count, "No managed list entities should be created when none exist.");
        }

        [TestMethod]
        public void CopyQuestionnaireLineManagedListEntities_ParentHasNoRecords_NoCopy()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var parentStudy = new StudyBuilder(project).WithName("Parent Study").Build();
            var childStudy = new StudyBuilder(project).WithName("Child Study").WithParentStudy(parentStudy).Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = childStudy;

            // Initialize without any parent QuestionnaireLineManagedListEntity records
            _context.Initialize(new Entity[] { project, parentStudy, childStudy, qLine });

            // Act
            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            // Assert - no records copied
            var childRecords = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                 .Where(x => x.KTR_StudyId != null && x.KTR_StudyId.Id == childStudy.Id)
                 .ToList();
            Assert.AreEqual(0, childRecords.Count, "No Questionnaire Line Managed List Entity records should be copied when parent has none.");
        }

        [TestMethod]
        public void CopyQuestionnaireLineManagedListEntities_NoParent_NoMLE_NoRecords()
        {
            // Arrange - no Managed List Entities present
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithName("Child Study").Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();
            var managedList = new ManagedListBuilder(project).WithName("ML1").Build();
            var qlManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, study, qLine, managedList, qlManagedList });

            // Act
            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            // Assert - no Questionnaire Line Managed List Entity records created
            var created = _context.CreateQuery<KTR_QuestionnaireLinemanAgedListEntity>()
                 .Where(x => x.KTR_StudyId != null && x.KTR_StudyId.Id == study.Id)
                 .ToList();
            Assert.AreEqual(0, created.Count, "No records should be created when no Managed List Entities exist.");
        }

        // Additional tests to increase coverage on new branches

        [TestMethod]
        public void CreateStudyQuestionnaireLines_NoQuestionnaireLines_EarlyExit()
        {
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).WithName("Study Without QLines").Build();
            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = study;
            _context.Initialize(new Entity[] { project, study });

            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            Assert.AreEqual(0, _context.CreateQuery<KTR_StudyQuestionnaireLine>().Count(), "No study questionnaire lines should be created.");
            Assert.AreEqual(0, _context.CreateQuery<KTR_StudyManagedListEntity>().Count(), "No study managed list entities should be created.");
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_QuestionnaireLineManagedListWithoutManagedListReference_EarlyExit()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();
            var study = new StudyBuilder(project).WithName("Study Missing ML Ref").Build();

            // Build a questionnaire line managed list record WITHOUT managed list reference
            var qlManagedListNoML = new KTR_QuestionnaireLinesHaRedList
            {
                Id = Guid.NewGuid(),
                KTR_QuestionnaireLine = qLine.ToEntityReference()
            };

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = study;

            _context.Initialize(new Entity[] { project, study, qLine, qlManagedListNoML });

            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            // Should create study questionnaire line but no study managed list entities since managedListIds.Count ==0 branch
            Assert.AreEqual(1, _context.CreateQuery<KTR_StudyQuestionnaireLine>().Count(), "One study questionnaire line should be created.");
            Assert.AreEqual(0, _context.CreateQuery<KTR_StudyManagedListEntity>().Count(), "No study managed list entities should be created when managed list ref missing.");
        }

        [TestMethod]
        public void CreateStudyQuestionnaireLines_ParentStudyWithManagedLists_NoParentStudyMLEs_CopiesActive()
        {
            var project = new ProjectBuilder().Build();
            var parentStudy = new StudyBuilder(project).WithName("Parent").Build();
            var childStudy = new StudyBuilder(project).WithName("Child").WithParentStudy(parentStudy).Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).WithVariableName("Q1").Build();
            var managedList = new ManagedListBuilder(project).WithName("ML1").Build();
            var qlManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();
            var mleActive = new ManagedListEntityBuilder(managedList).WithAnswerText("ActiveAns").Build();

            var ctx = _context.GetDefaultPluginContext();
            ctx.MessageName = nameof(ContextMessageEnum.Create);
            ctx.InputParameters["Target"] = childStudy;

            // Note: No parent study managed list entity records provided
            _context.Initialize(new Entity[] { project, parentStudy, childStudy, qLine, managedList, qlManagedList, mleActive });

            _context.ExecutePluginWith<CreateStudyQuestionnaireLinesSyncPostOperation>(ctx);

            var childStudyMles = _context.CreateQuery<KTR_StudyManagedListEntity>().Where(x => x.KTR_Study.Id == childStudy.Id).ToList();
            Assert.AreEqual(0, childStudyMles.Count, "Managed list entity should be copied as active when parent has no override.");
        }
    }
}
