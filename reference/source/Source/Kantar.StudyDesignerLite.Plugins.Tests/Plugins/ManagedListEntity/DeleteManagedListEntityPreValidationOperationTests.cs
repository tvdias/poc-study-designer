using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ManagedListEntity;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedListEntity
{
    [TestClass]
    public class DeleteManagedListEntityPreValidationOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;
        private KT_Project _project;
        private KTR_ManagedList _managedList;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
            _project = new ProjectBuilder().Build();
            _managedList = new ManagedListBuilder(_project).WithName("Test Managed List").Build();
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenMessageNameIsNotDelete()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create"; // Not Delete
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetParameterMissing()
        {
            // Arrange
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection(); // No Target

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNotEntityReference()
        {
            // Arrange
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", "NotAnEntityReference" }
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNull()
        {
            // Arrange
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", null } // Null target
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - Should complete without exception
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_ShouldThrowException_WhenActiveQuestionnaireLineAssociationsExist()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity, questionnaireLine, questionnaireLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenActiveQuestionnaireLineAssociationsExist_WithCorrectMessage()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity, questionnaireLine, questionnaireLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with 1 question(s) or referenced in snapshot history."));
            }
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenSubsetsExist_WithCorrectMessage()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            var subsetDefinition = new SubsetDefinitionBuilder().Build();
            var subsetEntity = new SubsetEntitiesBuilder(managedListEntity, subsetDefinition).Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity, subsetDefinition, subsetEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with 1 subset definitions(s)."));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_ShouldThrowException_WhenEverInSnapshotIsTrue()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(true)
                .Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenEverInSnapshotIsTrue_WithCorrectMessage()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(true)
                .Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with [1] question(s) or referenced in snapshot history."));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void Execute_ShouldThrowException_WhenManagedListExistsInSnapshots()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            
            var study = new StudyBuilder(_project).Build();
            var studyQuestionnaireLineSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine).Build();
            var studyQuestionManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(studyQuestionnaireLineSnapshot)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .Build();

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study, studyQuestionnaireLineSnapshot, 
                studyQuestionManagedListSnapshot 
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
        }

        [TestMethod]
        public void Execute_ShouldAllowDeletion_WhenNoActiveAssociationsAndNoSnapshots()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldAllowDeletion_WhenNoManagedListReference()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder()
                .WithEverInSnapshot(false)
                .Build(); // No managed list reference

            _context.Initialize(new List<Entity> { managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ValidateSnapshotHistory_ShouldAllowDeletion_WhenManagedListHasNoProject()
        {
            // Arrange
            var managedListWithoutProject = new ManagedListBuilder(_project)
                .WithId(Guid.NewGuid())
                .Build();
            managedListWithoutProject.KTR_Project = null; // Remove project reference

            var managedListEntity = new ManagedListEntityBuilder(managedListWithoutProject).Build();

            _context.Initialize(new List<Entity> { managedListWithoutProject, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ValidateSnapshotHistory_ShouldAllowDeletion_WhenNoQuestionnaireLineAssociations()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldNotAllowDeletion_WhenInactiveQuestionnaireLineAssociationsExist()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var inactiveQuestionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            inactiveQuestionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity, questionnaireLine, inactiveQuestionnaireLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should throw exception
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
          _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext)
             );

            Assert.IsTrue(ex.Message.Contains("Cannot delete"));
        
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WithMultipleActiveAssociations()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            var questionnaireLine1 = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLine2 = new QuestionnaireLineBuilder(_project).WithVariableName("Q2").Build();
            var questionnaireLineManagedList1 = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine1).Build();
            var questionnaireLineManagedList2 = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine2).Build();

            _context.Initialize(new List<Entity> { 
                _project, _managedList, managedListEntity, 
                questionnaireLine1, questionnaireLine2,
                questionnaireLineManagedList1, questionnaireLineManagedList2 
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with 2 question(s) or referenced in snapshot history."));
            }
        }

        [TestMethod]
        public void GetManagedList_ShouldReturnNull_WhenManagedListNotFound()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder()
                .WithManagedList(new EntityReference(KTR_ManagedList.EntityLogicalName, Guid.NewGuid()))
                .Build();

            _context.Initialize(new List<Entity> { managedListEntity }); // No managed list in context

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception (GetManagedList returns null, method continues)
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldAllowDeletion_WhenEverInSnapshotIsNull()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            // EverInSnapshot is null by default

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldAllowDeletion_WhenEverInSnapshotIsFalse()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();

            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldThrowException_WhenSnapshotHistoryHasReferences_WithCorrectMessage()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            
            var study = new StudyBuilder(_project).Build();
            var studyQuestionnaireLineSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine).Build();
            var studyQuestionManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(studyQuestionnaireLineSnapshot)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .Build();

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study, studyQuestionnaireLineSnapshot, 
                studyQuestionManagedListSnapshot 
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with 1 question(s) or referenced in snapshot history."));
            }
        }

        [TestMethod]
        public void ValidateSnapshotHistory_ShouldAllowDeletion_WhenEmptyQuestionnaireLineAssociationsList()
        {
            // Arrange - Test the case where GetManagedListSnapshotReferences receives empty list
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();

            // Initialize without any questionnaire line associations
            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldHandleComplexScenario_WithMixedActiveAndInactiveAssociations()
        {
            // Arrange - Test with both active and inactive associations
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine1 = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLine2 = new QuestionnaireLineBuilder(_project).WithVariableName("Q2").Build();
            
            var activeQuestionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine1).Build();
            var inactiveQuestionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine2).Build();
            inactiveQuestionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;

            _context.Initialize(new List<Entity> { 
                _project, _managedList, managedListEntity, 
                questionnaireLine1, questionnaireLine2,
                activeQuestionnaireLineManagedList, inactiveQuestionnaireLineManagedList 
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act & Assert - Should throw exception due to active association
            try
            {
                _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Cannot delete: Currently associated with 2 question(s) or referenced in snapshot history."));
            }
        }

        [TestMethod]
        public void Execute_ShouldNotAllowDeletion_WhenInactiveSnapshotsExist()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            // Make the questionnaire line managed list inactive so it doesn't trigger the first validation
            questionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;
            
            var study = new StudyBuilder(_project).Build();
            var studyQuestionnaireLineSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine).Build();
            var inactiveStudyQuestionManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(studyQuestionnaireLineSnapshot)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .WithStatusCode(KTR_StudyQuestionManagedListSnapshot_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study, studyQuestionnaireLineSnapshot, 
                inactiveStudyQuestionManagedListSnapshot 
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };
            // Act - Should throw exception
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
          _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext)
             );

            Assert.IsTrue(ex.Message.Contains("Cannot delete"));
        }

        [TestMethod]
        public void Execute_ShouldNotAllowDeletion_WhenStudyHasNoActiveQuestionnaireLineSnapshots()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            questionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;
            
            var study = new StudyBuilder(_project).Build();
            // Create an inactive questionnaire line snapshot
            var inactiveStudyQuestionnaireLineSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine)
                .WithStatusCode(KTR_StudyQuestionnaireLinesNaPsHot_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study, inactiveStudyQuestionnaireLineSnapshot
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should throw exception
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
          _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext)
             );

            Assert.IsTrue(ex.Message.Contains("Cannot delete"));
        }
     
        [TestMethod]
        public void Execute_ShouldReturn_WhenEntityLogicalNameIsIncorrect()
        {
            // Arrange
            // Create a valid managed list entity first so the retrieve operation succeeds
            var managedListEntity = new ManagedListEntityBuilder(_managedList).Build();
            _context.Initialize(new List<Entity> { _project, _managedList, managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                // Use an entity reference with wrong logical name but valid ID
                { "Target", new EntityReference("wrong_entity_name", managedListEntity.Id) }
            };

            // Act - Should complete without exception
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldHandleGetManagedListException()
        {
            // Arrange - Create a managed list entity that references a non-existent managed list
            var nonExistentManagedListRef = new EntityReference(KTR_ManagedList.EntityLogicalName, Guid.NewGuid());
            var managedListEntity = new ManagedListEntityBuilder()
                .WithManagedList(nonExistentManagedListRef)
                .WithEverInSnapshot(false)
                .Build();

            _context.Initialize(new List<Entity> { managedListEntity });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should complete without exception (GetManagedList returns null on exception)
            _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext);

            // Assert - If we reach here, no exception was thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldHandleEmptyQuestionnaireLineSnapshotIds()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            questionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;            
            var study = new StudyBuilder(_project).Build();
            // Don't create any questionnaire line snapshots for the study

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };

            // Act - Should throw exception
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
          _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext)
             );

            Assert.IsTrue(ex.Message.Contains("Cannot delete"));
        }

        [TestMethod]
        public void Execute_ShouldNotA_WhenManagedListSnapshotIsInactiveButQuestionnaireLineSnapshotIsActive()
        {
            // Arrange
            var managedListEntity = new ManagedListEntityBuilder(_managedList)
                .WithEverInSnapshot(false)
                .Build();
            var questionnaireLine = new QuestionnaireLineBuilder(_project).WithVariableName("Q1").Build();
            var questionnaireLineManagedList = new QuestionnaireLineManagedListBuilder(_project, _managedList, questionnaireLine).Build();
            questionnaireLineManagedList.StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive;
            
            var study = new StudyBuilder(_project).Build();
            var activeStudyQuestionnaireLineSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine)
                .WithStatusCode(KTR_StudyQuestionnaireLinesNaPsHot_StatusCode.Active)
                .Build();
            
            // Create inactive managed list snapshot
            var inactiveStudyQuestionManagedListSnapshot = new StudyQuestionManagedListSnapshotBuilder(activeStudyQuestionnaireLineSnapshot)
                .WithQuestionnaireLineManagedList(questionnaireLineManagedList)
                .WithStatusCode(KTR_StudyQuestionManagedListSnapshot_StatusCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> 
            { 
                _project, _managedList, managedListEntity, questionnaireLine, 
                questionnaireLineManagedList, study, activeStudyQuestionnaireLineSnapshot, 
                inactiveStudyQuestionManagedListSnapshot
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", managedListEntity.ToEntityReference() }
            };
            // Act - Should throw exception
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
          _context.ExecutePluginWith<DeleteManagedListEntityPreValidationOperation>(pluginContext)
             );

            Assert.IsTrue(ex.Message.Contains("Cannot delete"));
        }
    }
}
