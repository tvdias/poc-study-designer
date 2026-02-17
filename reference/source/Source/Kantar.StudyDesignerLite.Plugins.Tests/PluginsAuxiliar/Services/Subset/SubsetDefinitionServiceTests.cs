namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Services.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Moq;

    [TestClass]
    public class SubsetDefinitionServiceTests
    {
        private readonly Mock<ITracingService> _tracing;
        private readonly Mock<ISubsetRepository> _repository;
        private readonly Mock<IQuestionnaireLineManagedListEntityRepository> _qLMLErepository;
        private readonly Mock<IStudyRepository> _studyRepository;
        private readonly Mock<IManagedListEntityRepository> _managedListEntityRepository;
        private readonly SubsetDefinitionService _service;

        private const string HelperTypeName = "Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset.SubsetDefinitionService";

        private static Type GetHelperType() =>
            typeof(SubsetDefinitionService).Assembly.GetType(HelperTypeName, throwOnError: true);

        private static object Invoke(string methodName, SubsetDefinitionService obj, params object[] args)
        {
            var t = GetHelperType();
            var m = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"Method {methodName} not found.");
            return m.Invoke(obj, args);
        }

        public SubsetDefinitionServiceTests()
        {
            _tracing = new Mock<ITracingService>();
            _repository = new Mock<ISubsetRepository>();
            _qLMLErepository = new Mock<IQuestionnaireLineManagedListEntityRepository>();
            _studyRepository = new Mock<IStudyRepository>();
            _managedListEntityRepository = new Mock<IManagedListEntityRepository>();

            _service = new SubsetDefinitionService(
                _tracing.Object,
                _repository.Object,
                _qLMLErepository.Object,
                _studyRepository.Object,
                _managedListEntityRepository.Object);
        }

        [TestMethod]
        public void GetStudy_ReturnExpected()
        {
            // Arrange
            var id = Guid.NewGuid();

            var expectedStudy = new KT_Study() { Id = id, KT_Name = "Test Study" };

            var columns = new string[]
            {
                KT_Study.Fields.Id,
                KT_Study.Fields.KTR_MasterStudy,
                KT_Study.Fields.KT_Name,
                KT_Study.Fields.KT_Project,
            };

            _studyRepository.Setup(x => x.Get(id, columns)).Returns(expectedStudy);
            // Act
            var value = (KT_Study)Invoke("GetStudy", this._service, id);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(id, value.Id);
            _studyRepository.Verify(x => x.Get(id, columns), Times.Once);
            _tracing.Verify(x => x.Trace($"Study found: {expectedStudy.Id} - {expectedStudy.KT_Name}"), Times.Once);
            _tracing.Verify(x => x.Trace($"Study with ID: {id} not found."), Times.Never);
        }

        [TestMethod]
        public void GetStudy_ReturnNull_ThrowException()
        {
            // Arrange
            var id = Guid.NewGuid();

            var columns = new string[]
            {
                KT_Study.Fields.Id,
                KT_Study.Fields.KTR_MasterStudy,
                KT_Study.Fields.KT_Name,
                KT_Study.Fields.KT_Project,
            };

            _studyRepository.Setup(x => x.Get(id, columns)).Returns(null as KT_Study);

            // Act
            var value = Assert.ThrowsException<TargetInvocationException>(() =>
            {
                Invoke("GetStudy", this._service, id);
            });

            // Assert
            var exception = value.InnerException as ArgumentException;
            Assert.IsNotNull(exception);
            Assert.AreEqual($"Study with ID {id} not found.", exception.Message);
        }

        [TestMethod]
        public void ProcessManagedListEntities_ReturnExpected()
        {
            // Arrange
            var managedListId = Guid.NewGuid();
            var study = new KT_Study()
            {
                Id = Guid.NewGuid(),
                KT_Name = "Study",
                KT_Project = new EntityReference("KT_Project", Guid.NewGuid())
            };

            var expectedMLEntities = new List<KTR_ManagedListEntity>
            {
                new KTR_ManagedListEntity
                {
                    Id = Guid.NewGuid(),
                    KTR_ManagedList = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, managedListId)
                },
                new KTR_ManagedListEntity
                {
                    Id = Guid.NewGuid() ,
                    KTR_ManagedList = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, managedListId)
                },
            };

            var questionnaireLine = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "Questionnaire Line"
            };

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>()
            {
                new KTR_QuestionnaireLinemanAgedListEntity
                {
                    Id = Guid.NewGuid(),
                    KTR_ManagedList = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, managedListId),
                    KTR_ManagedListEntity = expectedMLEntities[0].ToEntityReference(),
                    KTR_QuestionnaireLine = questionnaireLine.ToEntityReference()
                },
                new KTR_QuestionnaireLinemanAgedListEntity
                {
                    Id = Guid.NewGuid(),
                    KTR_ManagedList = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, managedListId),
                    KTR_ManagedListEntity = expectedMLEntities[1].ToEntityReference(),
                    KTR_QuestionnaireLine = questionnaireLine.ToEntityReference()
                }
            };

            var contexts = new SubsetsContextBuilder(entities, study);

            _managedListEntityRepository.Setup(x => x.GetByManagedListId(managedListId, null)).Returns(expectedMLEntities);

            // Act
            var value = (SubsetsContextBuilder)Invoke("ProcessManagedListEntities", this._service, entities, contexts);

            // Assert
            Assert.IsNotNull(value);
            value.ProcessExistingStudySubsetDefinitions(new List<KTR_StudySubsetDefinition>());
            var builded = value.Build();
            Assert.AreEqual(1, builded.Count);
            Assert.IsTrue(builded.First().IsFullList);
            _managedListEntityRepository.Verify(x => x.GetByManagedListId(managedListId, null), Times.Exactly(1));
        }

        [TestMethod]
        public void GetSubsetAssociationBySubseIds_ReturnExpected()
        {
            // Arrange
            Guid[] associationsIds = { Guid.NewGuid() };

            var expected = new List<KTR_StudySubsetDefinition>
                    {
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                    };

            _repository.Setup(x => x.GetSubsetAssociationBySubsetIds(associationsIds, null)).Returns(expected);

            // Act
            var value = (List<KTR_StudySubsetDefinition>)Invoke("GetSubsetAssociationBySubsetIds", this._service, associationsIds);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);
        }

        [TestMethod]
        public void GetSubsetAssociationBySubseIds_NotFounded_ReturnEmpty()
        {
            // Arrange
            Guid[] associationsIds = { Guid.NewGuid() };

            var expected = new List<KTR_StudySubsetDefinition>
            {
                new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
            };

            _repository.Setup(x => x.GetSubsetAssociationBySubsetIds(associationsIds, null)).Returns(null as List<KTR_StudySubsetDefinition>);

            // Act
            var value = (List<KTR_StudySubsetDefinition>)Invoke("GetSubsetAssociationBySubsetIds", this._service, associationsIds);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void GetSubsetAssociationsByStudyId_ReturnExpected()
        {
            // Arrange
            var studyId = Guid.NewGuid();

            var expected = new List<KTR_StudySubsetDefinition>
                    {
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                    };

            _repository.Setup(x => x.GetSubsetStudyAssociationByStudyId(studyId, null)).Returns(expected);

            // Act
            var value = (List<KTR_StudySubsetDefinition>)Invoke("GetSubsetStudyAssociationByStudyId", this._service, studyId);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);
        }

        [TestMethod]
        public void GetSubsetAssociationsByStudyId_NotFounded_ReturnEmpty()
        {
            // Arrange
            var studyId = Guid.NewGuid();

            var expected = new List<KTR_StudySubsetDefinition>
                    {
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                        new KTR_StudySubsetDefinition { Id = Guid.NewGuid()},
                    };

            _repository.Setup(x => x.GetSubsetStudyAssociationByStudyId(studyId, null)).Returns(null as List<KTR_StudySubsetDefinition>);

            // Act
            var value = (List<KTR_StudySubsetDefinition>)Invoke("GetSubsetStudyAssociationByStudyId", this._service, studyId);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void GetSubsetDefinitions_NotFounded_ReturnEmpty()
        {
            // Arrange
            var masterStudyId = Guid.NewGuid();

            var study = new KT_Study() { Id = masterStudyId, KT_Name = "Test Study" };

            var columns = new string[]
           {
                KTR_SubsetDefinition.Fields.Id,
                KTR_SubsetDefinition.Fields.KTR_MasterStudyId,
                KTR_SubsetDefinition.Fields.KTR_Name,
                KTR_SubsetDefinition.Fields.KTR_FilterSignature,
                KTR_SubsetDefinition.Fields.KTR_UsageCount
           };

            _repository.Setup(x => x.GetByMasterStudyId(masterStudyId, columns)).Returns(null as List<KTR_SubsetDefinition>);

            // Act
            var value = (List<KTR_SubsetDefinition>)Invoke("GetSubsetDefinitions", this._service, study);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void GetQuestionnaireLinemanAgedListEntities_ReturnExpected()
        {
            // Arrange
            var studyId = Guid.NewGuid();

            var columns = new string[]
           {
                KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine
           };

            var expected = new List<KTR_QuestionnaireLinemanAgedListEntity>
                    {
                        new KTR_QuestionnaireLinemanAgedListEntity { Id = Guid.NewGuid()},
                        new KTR_QuestionnaireLinemanAgedListEntity { Id = Guid.NewGuid()},
                    };

            _qLMLErepository.Setup(x => x.GetByStudyId(studyId, columns)).Returns(expected);

            // Act
            var value = (List<KTR_QuestionnaireLinemanAgedListEntity>)Invoke("GetQuestionnaireLineManagedListEntities", this._service, studyId);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);
        }

        [TestMethod]
        public void GetQuestionnaireLinemanAgedListEntities_NotFounded_ReturnEmpty()
        {
            // Arrange
            var studyId = Guid.NewGuid();

            var columns = new string[]
           {
                KTR_QuestionnaireLinemanAgedListEntity.Fields.Id,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList,
                KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_QuestionnaireLine
           };

            var expected = null as List<KTR_QuestionnaireLinemanAgedListEntity>;

            _qLMLErepository.Setup(x => x.GetByStudyId(studyId, columns)).Returns(expected);

            // Act
            var value = (List<KTR_QuestionnaireLinemanAgedListEntity>)Invoke("GetQuestionnaireLineManagedListEntities", this._service, studyId);

            // Assert
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
            _tracing.Verify(x => x.Trace($"No QuestionnaireLinemanAgedListEntities found for the study {studyId}."), Times.Once);
        }

        [TestMethod]
        public void DeleteQLSubsetes_DeleteExpectredAmount()
        {
            // Arrange
            var idDelete1 = Guid.NewGuid();
            var idDelete2 = Guid.NewGuid();
            var idToKeep = Guid.NewGuid();
            var study = new KT_Study() { Id = Guid.NewGuid() };
            var expectedQLSubsets = new List<KTR_QuestionnaireLineSubset>
            {
                new KTR_QuestionnaireLineSubset
                {
                    Id = idDelete1,
                    KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid()),
                    KTR_Study = study.ToEntityReference()
                },
                new KTR_QuestionnaireLineSubset
                {
                    Id = idDelete2,
                    KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid()),
                    KTR_Study = study.ToEntityReference()
                },
                new KTR_QuestionnaireLineSubset
                {
                    Id = idToKeep,
                    KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid()),
                    KTR_Study = study.ToEntityReference()
                },
            };

            var contexts = new List<SubsetCreationContext>()
            {
                new SubsetCreationContext
                {
                    Study = study,
                    ExistingQuestionnaireLineSubsets = new List<KTR_QuestionnaireLineSubset>()
                    {
                        new KTR_QuestionnaireLineSubset
                        {
                            Id = idToKeep,
                            KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid()),
                            KTR_Study = study.ToEntityReference()
                        }
                    }
                }
            };

            var ids = new List<Guid> { idDelete1, idDelete2 };
            _repository.Setup(x => x.GetQLSubsetsByStudyId(study.Id, null)).Returns(expectedQLSubsets);
            _repository.Setup(x => x.BulkDeleteQLSubset(ids));

            // Act
            Invoke("DeleteQLSubsetes", this._service, contexts);

            // Assert
            _repository.Verify(x => x.GetQLSubsetsByStudyId(study.Id, null), Times.Once);
            _repository.Verify(x => x.BulkDeleteQLSubset(ids), Times.Once);
        }

        [TestMethod]
        public void DeleteQLSubsetes_NotExist_DoNotDelete()
        {
            // Arrange
            var expected = new List<KTR_QuestionnaireLineSubset>();
            var study = new KT_Study() { Id = Guid.NewGuid() };
            _repository.Setup(x => x.GetQLSubsetsByStudyId(study.Id, null)).Returns(expected);
            var contexts = new List<SubsetCreationContext>()
            {
                new SubsetCreationContext
                {
                    Study = study,
                    ExistingQuestionnaireLineSubsets = new List<KTR_QuestionnaireLineSubset>()
                    {
                        new KTR_QuestionnaireLineSubset
                        {
                            Id = Guid.NewGuid(),
                            KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid()),
                            KTR_Study = study.ToEntityReference()
                        }
                    }
                }
            };
            // Act
            Invoke("DeleteQLSubsetes", this._service, contexts);

            // Assert
            _repository.Verify(x => x.GetQLSubsetsByStudyId(study.Id, null), Times.Once);
            _repository.Verify(x => x.BulkDeleteQLSubset(It.IsAny<IList<Guid>>()), Times.Never);
        }

        [TestMethod]
        public void DeleteSubsetEntities_DeleteExpectredAmount()
        {
            // Arrange
            var idDelete1 = Guid.NewGuid();
            var idDelete2 = Guid.NewGuid();
            var studyIds = new List<Guid>() { idDelete1, idDelete2 };
            var expected = new List<KTR_SubsetEntities>
                    {
                        new KTR_SubsetEntities
                        {
                            Id = idDelete1,
                            KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid())
                        },
                        new KTR_SubsetEntities
                        {
                            Id = idDelete2,
                            KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, Guid.NewGuid())
                        },
                    };
            _repository.Setup(x => x.GetSubsetEntitiesByDefinitionIds(studyIds.ToArray(), null)).Returns(expected);
            _repository.Setup(x => x.BulkDeleteSubsetEntity(studyIds));

            // Act
            Invoke("DeleteSubsetEntities", this._service, studyIds);

            // Assert
            _repository.Verify(x => x.GetSubsetEntitiesByDefinitionIds(studyIds.ToArray(), null), Times.Once);
            _repository.Verify(x => x.BulkDeleteSubsetEntity(studyIds), Times.Once);
        }

        [TestMethod]
        public void DeleteSubsetEntities_NotExist_DoNotDelete()
        {
            // Arrange
            var idDelete1 = Guid.NewGuid();
            var studyIds = new List<Guid>() { idDelete1 };

            var expected = null as List<KTR_SubsetEntities>;
            _repository.Setup(x => x.GetSubsetEntitiesByDefinitionIds(studyIds.ToArray(), null)).Returns(expected);
            _repository.Setup(x => x.BulkDeleteSubsetEntity(studyIds));

            // Act
            Invoke("DeleteSubsetEntities", this._service, studyIds);

            // Assert
            _repository.Verify(x => x.GetSubsetEntitiesByDefinitionIds(studyIds.ToArray(), null), Times.Once);
            _repository.Verify(x => x.BulkDeleteSubsetEntity(It.IsAny<IList<Guid>>()), Times.Never);
        }

        [TestMethod]
        public void DeleteStudySubset_DeleteExpectredAmount()
        {
            // Arrange
            var idDelete1 = Guid.NewGuid();
            var idDelete2 = Guid.NewGuid();
            var ids = new List<Guid> { idDelete1, idDelete2 };

            var associationsToDelete = new List<KTR_StudySubsetDefinition>
                    {
                        new KTR_StudySubsetDefinition
                        {
                            Id = idDelete1,
                        },
                        new KTR_StudySubsetDefinition
                        {
                            Id = idDelete2,
                        },
                    };
            _repository.Setup(x => x.BulkDeleteSubsetStudyAssociation(ids));

            // Act
            Invoke("DeleteSubsetStudyAssociation", this._service, associationsToDelete);

            // Assert
            _repository.Verify(x => x.BulkDeleteSubsetStudyAssociation(ids), Times.Once);
        }

        [TestMethod]
        public void ProcessSubsetLogic_ReturnExpectResult()
        {
            // Arrange
            // Arrange Study
            var masterStudy = new KT_Study()
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522941"),
                KT_Project = new EntityReference("kt_project", Guid.NewGuid())
            };
            var study = new KT_Study()
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522940"),
                KT_Project = masterStudy.KT_Project,
                KTR_MasterStudy = masterStudy.ToEntityReference()
            };
            var study2 = new KT_Study()
            {
                Id = Guid.NewGuid(),
                KT_Project = masterStudy.KT_Project,
                KTR_MasterStudy = masterStudy.ToEntityReference()
            };

            _studyRepository.Setup(x => x.Get(study.Id, It.IsAny<string[]>())).Returns(study);

            // Arange Managed List
            var managedList1 = new EntityReference("ktr_managedlist")
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522941"),
                Name = "BRAND_LIST"
            };
            var managedList2 = new EntityReference("ktr_managedlist")
            {
                Id = Guid.NewGuid(),
                Name = "BRAND_LIST2"
            };

            var jameson1 = new KTR_ManagedListEntity
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522942"),
                KTR_ManagedList = managedList1
            };

            var bushmills1 = new KTR_ManagedListEntity
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522943"),
                KTR_ManagedList = managedList1
            };

            var jack1 = new KTR_ManagedListEntity
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522944"),
                KTR_ManagedList = managedList1
            };

            var jameson2 = new KTR_ManagedListEntity
            {
                Id = Guid.NewGuid(),
                KTR_ManagedList = managedList2
            };

            var bushmills2 = new KTR_ManagedListEntity
            {
                Id = Guid.NewGuid(),
                KTR_ManagedList = managedList2
            };

            var jack2 = new KTR_ManagedListEntity
            {
                Id = Guid.NewGuid(),
                KTR_ManagedList = managedList2
            };

            var managedListEntitiesResponse1 = new List<KTR_ManagedListEntity>()
            {
                jameson1,
                bushmills1,
                jack1
            };
            var managedListEntitiesResponse2 = new List<KTR_ManagedListEntity>()
            {
                jameson2,
                bushmills2,
                jack2
            };
            _managedListEntityRepository.Setup(x => x.GetByManagedListId(managedList1.Id, It.IsAny<string[]>()))
                .Returns(managedListEntitiesResponse1);

            _managedListEntityRepository.Setup(x => x.GetByManagedListId(managedList2.Id, It.IsAny<string[]>()))
                .Returns(managedListEntitiesResponse2);

            // Arange Questionnaire Lines
            var questionnaireLineA = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineA"
            };
            var questionnaireLineB = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineB"
            };
            var questionnaireLineC = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineC"
            };
            var questionnaireLineD = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineD"
            };
            var questionnaireLineE = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineE"
            };
            var questionnaireLineF = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineE"
            };

            // Arrange Subset Definitions ML1
            var subsetDefinitionIdToDeleteML1 = Guid.NewGuid();
            var subsetDefinitionIdCantDeleteML1 = Guid.NewGuid();
            var subsetDefinitionSnapshotIdML1 = Guid.NewGuid();
            var existentSubsetDefinitionML1 = Guid.NewGuid();
            // Arrange Subset Definitions ML2
            var subsetDefinitionIdToDeleteML2 = Guid.NewGuid();
            var subsetDefinitionSnapshotIdML2 = Guid.NewGuid();

            var subsetDefinitionsResponse = new List<KTR_SubsetDefinition>()
            {
                {
                    new KTR_SubsetDefinition() // Exists for questionnaireLineC REUSED
                    {
                        Id = existentSubsetDefinitionML1,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB1",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){jameson1,bushmills1}, masterStudy.Id),
                        KTR_ManagedList = managedList1
                    }
                },
                {
                    new KTR_SubsetDefinition() // DELETE THIS ONE
                    {
                        Id = subsetDefinitionIdToDeleteML1,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB3",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){jameson1,bushmills1,jack1}, masterStudy.Id),
                        KTR_ManagedList = managedList1
                    }
                },
                {
                    new KTR_SubsetDefinition() // CANT DELETE THIS ONE
                    {
                        Id = subsetDefinitionIdCantDeleteML1,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB2",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){bushmills1,jack1}, masterStudy.Id),
                        KTR_ManagedList = managedList1
                    }
                },
                {
                    new KTR_SubsetDefinition() // CANT DELETE THIS ONE IS A SNAPSHOT
                    {
                        Id = subsetDefinitionSnapshotIdML1,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB4",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){jameson1,jack1}, masterStudy.Id),
                        KTR_ManagedList = managedList1,
                        KTR_EverInSnapshot = true
                    }
                },
                {
                    new KTR_SubsetDefinition() // DELETE THIS ONE
                    {
                        Id = subsetDefinitionIdToDeleteML2,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LIST2SUB3",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){jameson2}, masterStudy.Id),
                        KTR_ManagedList = managedList2
                    }
                },
                {
                    new KTR_SubsetDefinition() // CANT DELETE THIS
                    {
                        Id = subsetDefinitionSnapshotIdML2,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LIST2SUB4",
                        KTR_FilterSignature = GenerateHash(new List<KTR_ManagedListEntity>(){jameson2,jack2}, masterStudy.Id),
                        KTR_ManagedList = managedList2,
                        KTR_EverInSnapshot = true
                    }
                },
            };
            _repository.Setup(x => x.GetByMasterStudyId(masterStudy.Id, It.IsAny<string[]>())).Returns(subsetDefinitionsResponse);

            // Arrange Subset Associations
            var associationsResponse = new List<KTR_StudySubsetDefinition>()
            {
                {
                    new KTR_StudySubsetDefinition() // Exist SubsetDefinition
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, existentSubsetDefinitionML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Should Be Deleted
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Have another Study association
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Have Snapshot
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Should Be Deleted
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML2),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Have Snapshot
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML2),
                    }
                }
            };
            var deletationAssociationsResponse = new List<KTR_StudySubsetDefinition>()
            {
                {
                    new KTR_StudySubsetDefinition() // Same Study
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Is in both Studies
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Is in both Studies
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study2.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Same Study
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML2),
                    }
                }
            };
            _repository.Setup(x => x.GetSubsetStudyAssociationByStudyId(study.Id, It.IsAny<string[]>())).Returns(associationsResponse);

            _repository.Setup(x => x.GetSubsetAssociationBySubsetIds(
                                new[]
                                {
                                    subsetDefinitionIdToDeleteML1,
                                    subsetDefinitionIdCantDeleteML1,
                                    subsetDefinitionIdToDeleteML2,
                                },
                                It.IsAny<string[]>()))
                .Returns(deletationAssociationsResponse);

            var qlSubsetEntitiesResponse = new List<KTR_QuestionnaireLineSubset>()
            {
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, existentSubsetDefinitionML1),
                        KTR_QuestionnaireLineId = questionnaireLineB.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                },
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                        KTR_QuestionnaireLineId = questionnaireLineA.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                },
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                        KTR_QuestionnaireLineId = questionnaireLineF.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                },
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML1),
                        KTR_QuestionnaireLineId = questionnaireLineE.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                },
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML2),
                        KTR_QuestionnaireLineId = questionnaireLineB.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                },
                {
                    new KTR_QuestionnaireLineSubset()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML2),
                        KTR_QuestionnaireLineId = questionnaireLineE.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                }
            };
            _repository.Setup(x => x.GetQLSubsetsByStudyId(study.Id, It.IsAny<string[]>())).Returns(qlSubsetEntitiesResponse);

            // Arrange Subset Entities
            var subsetEntitiesResponse = new List<KTR_SubsetEntities>()
            {
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                        KTR_ManagedListEntity = jameson1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                        KTR_ManagedListEntity = bushmills1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML1),
                        KTR_ManagedListEntity = jack1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, existentSubsetDefinitionML1),
                        KTR_ManagedListEntity = jameson1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, existentSubsetDefinitionML1),
                        KTR_ManagedListEntity = bushmills1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML1),
                        KTR_ManagedListEntity = jameson1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML1),
                        KTR_ManagedListEntity = jack1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                        KTR_ManagedListEntity = bushmills1.ToEntityReference()
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDeleteML1),
                        KTR_ManagedListEntity = jack1.ToEntityReference(),
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDeleteML2),
                        KTR_ManagedListEntity = jameson2.ToEntityReference(),
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML2),
                        KTR_ManagedListEntity = jameson2.ToEntityReference(),
                    }
                },
                {
                    new KTR_SubsetEntities()
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionSnapshotIdML2),
                        KTR_ManagedListEntity = jack2.ToEntityReference(),
                    }
                }
            };

            _repository.Setup(x => x.GetSubsetEntitiesByMLEntityIds(
                new[]
                {
                    jameson1.Id,
                    bushmills1.Id,
                    bushmills2.Id,
                    jameson2.Id,
                    jack2.Id
                },
                It.IsAny<string[]>()))
                .Returns(subsetEntitiesResponse);
            // Arrange Questionnaire Line Managed List Entities
            var qLManagedListEntitiesResponse = new List<KTR_QuestionnaireLinemanAgedListEntity>()
            {
                //B and D are equal.
                CreateQLManagedListEntity(managedList1, jameson1, questionnaireLineB, study, "ListBJameson", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList1, bushmills1, questionnaireLineB, study, "ListBBushmills", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList1, jameson1, questionnaireLineC, study, "ListCJameson", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList1, jameson1, questionnaireLineD, study, "ListDJameson", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList1, bushmills1, questionnaireLineD, study, "ListDBushmills", Guid.NewGuid()),

                CreateQLManagedListEntity(managedList2, bushmills2, questionnaireLineD, study, "List2DBushmills", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList2, jameson2, questionnaireLineD, study, "List2DJameson", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList2, jack2, questionnaireLineD, study, "ListJack2D", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList2, jameson2, questionnaireLineA, study, "ListJameson2A", Guid.NewGuid()),
                CreateQLManagedListEntity(managedList2, bushmills2, questionnaireLineA, study, "ListBushmills2A", Guid.NewGuid()),
            };
            _qLMLErepository.Setup(x => x.GetByStudyId(study.Id, It.IsAny<string[]>())).Returns(qLManagedListEntitiesResponse);

            // Arrange Delete Setups
            var subsetEntitiesToDeleteResponse = subsetEntitiesResponse.Where(se =>
                se.KTR_SubsetDeFinTion.Id == subsetDefinitionIdToDeleteML1 ||
                se.KTR_SubsetDeFinTion.Id == subsetDefinitionIdToDeleteML2).ToList();

            _repository.Setup(x => x.GetSubsetEntitiesByDefinitionIds(
                new[]
                {
                    subsetDefinitionIdToDeleteML1,
                    subsetDefinitionIdToDeleteML2
                },
                It.IsAny<string[]>()))
                .Returns(subsetEntitiesToDeleteResponse);
            var contexts = new List<SubsetCreationContext>();
            // Act
            var result = _service.ProcessSubsetLogic(study.Id);
            // Assert
            Assert.IsNotNull(result);
            var bLSub1 = result.First(r => r.SubsetName == "BRAND_LISTSUB1");
            var bLSub5 = result.First(r => r.SubsetName == "BRAND_LISTSUB5");
            var bL2Sub5 = result.First(r => r.SubsetName == "BRAND_LIST2SUB5");
            var bL2Sub6 = result.First(r => r.SubsetName == "BRAND_LIST2SUB6");
            Assert.AreEqual(4, result.Count);
            Assert.IsNotNull(bLSub5);
            Assert.AreEqual(-1, bLSub5.UsageCount);
            Assert.AreEqual(1, bLSub5.EntityCount);
            Assert.AreNotEqual(Guid.Empty, bLSub5.SubsetId);
            Assert.IsFalse(bLSub5.UsesFullList);

            Assert.IsNotNull(bLSub1);
            Assert.AreNotEqual(Guid.Empty, bLSub1.SubsetId);
            Assert.AreEqual(-1, bLSub1.UsageCount);
            Assert.AreEqual(2, bLSub1.EntityCount);

            Assert.IsNotNull(bL2Sub5);
            Assert.AreEqual(-1, bL2Sub5.UsageCount);
            Assert.AreEqual(3, bL2Sub5.EntityCount);
            Assert.AreNotEqual(Guid.Empty, bL2Sub5.SubsetId);
            Assert.IsTrue(bL2Sub5.UsesFullList);

            Assert.IsNotNull(bL2Sub6);
            Assert.AreEqual(-1, bL2Sub6.UsageCount);
            Assert.AreEqual(2, bL2Sub6.EntityCount);
            Assert.AreNotEqual(Guid.Empty, bL2Sub6.SubsetId);
            Assert.IsFalse(bL2Sub6.UsesFullList);

            // Assert SubsetDefinition Deletes
            var subsetDefDeletedIds = new List<Guid> { subsetDefinitionIdToDeleteML1, subsetDefinitionIdToDeleteML2 };
            _repository.Verify(x => x.BulkDelete(subsetDefDeletedIds), Times.Once);
            // Assert Association Deletes
            var deleteSubsetStudyAssociationIds = associationsResponse.Where(y =>
                y.KTR_SubsetDefinition.Id == subsetDefinitionIdToDeleteML1
                || y.KTR_SubsetDefinition.Id == subsetDefinitionIdCantDeleteML1
                || y.KTR_SubsetDefinition.Id == subsetDefinitionSnapshotIdML1
                || y.KTR_SubsetDefinition.Id == subsetDefinitionIdToDeleteML2
                || y.KTR_SubsetDefinition.Id == subsetDefinitionSnapshotIdML2)
                .Select(z => z.Id).ToList();

            _repository.Verify(x => x.BulkDeleteSubsetStudyAssociation(deleteSubsetStudyAssociationIds), Times.Once);

            // Assert QL Subsets Deletes
            var deleteQLSubsetIds = qlSubsetEntitiesResponse.Where(y =>
                y.KTR_SubsetDefinitionId.Id == subsetDefinitionIdToDeleteML1
                || y.KTR_SubsetDefinitionId.Id == subsetDefinitionIdCantDeleteML1
                || y.KTR_SubsetDefinitionId.Id == subsetDefinitionSnapshotIdML1
                || y.KTR_SubsetDefinitionId.Id == subsetDefinitionIdToDeleteML2
                || y.KTR_SubsetDefinitionId.Id == subsetDefinitionSnapshotIdML2)
                .Select(z => z.Id).ToList();

            _repository.Verify(x => x.BulkDeleteQLSubset(deleteQLSubsetIds), Times.Once);

            var deleteQLSubset = qlSubsetEntitiesResponse.Select(y => y.KTR_SubsetDefinitionId.Id).ToList();
            _repository.Verify(x => x.BulkDeleteQLSubset(deleteQLSubset), Times.Never);

            // Assert Subsets entities Deletes
            var subsetEntitiesToDeleteResponseIds = subsetEntitiesToDeleteResponse.Select(se => se.Id).ToList();
            _repository.Verify(x => x.BulkDeleteSubsetEntity(subsetEntitiesToDeleteResponseIds), Times.Once);

            var notDeleteSubsetEntities = subsetEntitiesResponse.Where(se =>
                se.KTR_SubsetDeFinTion.Id == existentSubsetDefinitionML1
                || se.KTR_SubsetDeFinTion.Id == subsetDefinitionSnapshotIdML1
                || se.KTR_SubsetDeFinTion.Id == subsetDefinitionIdCantDeleteML1
                || se.KTR_SubsetDeFinTion.Id == subsetDefinitionSnapshotIdML2);

            var notDeleteSubsetEntitiesIds = notDeleteSubsetEntities.Select(ne => ne.Id).ToList();
            _repository.Verify(x => x.BulkDeleteSubsetEntity(notDeleteSubsetEntitiesIds), Times.Never);

            // Assert Insert calls
            _repository.Verify(
                x => x.BulkInsert(It.Is<IList<KTR_SubsetDefinition>>(
                    list => list.Count == 3
                    && list.Count(l => l.KTR_FilterSignature == GenerateHash(new List<KTR_ManagedListEntity>() { jameson1 }, masterStudy.Id)) == 1
                    && list.Count(l => l.KTR_FilterSignature == GenerateHash(new List<KTR_ManagedListEntity>() { jameson2, bushmills2 }, masterStudy.Id)) == 1
                    && list.Count(l => l.KTR_FilterSignature == GenerateHash(new List<KTR_ManagedListEntity>() { jameson2, bushmills2, jack2 }, masterStudy.Id)) == 1)),
                Times.Once);

            _repository.Verify(
                x => x.BulkInsertSubsetStudyAssociation(It.Is<IList<KTR_StudySubsetDefinition>>(list =>
                    list.Count == 3
                    && list.Count(l => l.KTR_SubsetDefinition.Id == bLSub5.SubsetId) == 1
                    && list.Count(l => l.KTR_SubsetDefinition.Id == bL2Sub5.SubsetId) == 1
                    && list.Count(l => l.KTR_SubsetDefinition.Id == bL2Sub6.SubsetId) == 1)),
                Times.Once);

            _repository.Verify(
                x => x.BulkInsertQLSubsets(It.Is<IList<KTR_QuestionnaireLineSubset>>(
                    list => list.Count == 4
                    && list.Count(l => l.KTR_SubsetDefinitionId.Id == bLSub5.SubsetId && l.KTR_QuestionnaireLineId.Id == questionnaireLineC.Id) == 1
                    && list.Count(l => l.KTR_SubsetDefinitionId.Id == bLSub1.SubsetId && l.KTR_QuestionnaireLineId.Id == questionnaireLineD.Id) == 1
                    && list.Count(l => l.KTR_SubsetDefinitionId.Id == bL2Sub6.SubsetId && l.KTR_QuestionnaireLineId.Id == questionnaireLineA.Id) == 1
                    && list.Count(l => l.KTR_SubsetDefinitionId.Id == bL2Sub5.SubsetId && l.KTR_QuestionnaireLineId.Id == questionnaireLineD.Id) == 1)),
                Times.Once);

            _repository.Verify(
                x => x.BulkInsertSubsetEntities(It.Is<IList<KTR_SubsetEntities>>(list =>
                    list.Count == 6
                    && list.Count(l => l.KTR_SubsetDeFinTion.Id == bLSub5.SubsetId) == 1
                    && list.Count(l => l.KTR_SubsetDeFinTion.Id == bL2Sub5.SubsetId) == 3
                    && list.Count(l => l.KTR_SubsetDeFinTion.Id == bL2Sub6.SubsetId) == 2)),
                Times.Once);
        }

        [TestMethod]
        public void ProcessSubsetLogic_EmptyEntities_ReturnEmptyList()
        {
            // Arrange
            // Arrange Study
            var masterStudy = new KT_Study()
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522941"),
                KT_Project = new EntityReference("kt_project", Guid.NewGuid())
            };
            var study = new KT_Study()
            {
                Id = Guid.Parse("4e9325eb-1a89-45a0-bea8-56edb5522940"),
                KT_Project = masterStudy.KT_Project,
                KTR_MasterStudy = masterStudy.ToEntityReference()
            };
            _studyRepository.Setup(x => x.Get(study.Id, It.IsAny<string[]>())).Returns(study);

            // Arange Questionnaire Lines
            var questionnaireLineA = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineA"
            };
            var questionnaireLineB = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineB"
            };
            var questionnaireLineC = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineC"
            };
            var questionnaireLineD = new KT_QuestionnaireLines()
            {
                Id = Guid.NewGuid(),
                KT_Name = "QuestionnaireLineD"
            };

            // Arrange Subset Definitions
            var subsetDefinitionIdToDelete = Guid.NewGuid();
            var subsetDefinitionIdCantDelete = Guid.NewGuid();
            var subsetDefinitionSnapshotId = Guid.NewGuid();
            var existentSubsetDefinition = Guid.NewGuid();
            var subsetDefinitionsResponse = new List<KTR_SubsetDefinition>()
            {
                {
                    new KTR_SubsetDefinition() // DELETE THIS ONE
                    {
                        Id = subsetDefinitionIdToDelete,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB3",
                        KTR_FilterSignature = "hash3",
                        KTR_EverInSnapshot = false
                    }
                },
                {
                    new KTR_SubsetDefinition() // CANT DELETE THIS ONE
                    {
                        Id = subsetDefinitionIdCantDelete,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB2",
                        KTR_FilterSignature = "hash2",
                        KTR_EverInSnapshot = false
                    }
                },
                {
                    new KTR_SubsetDefinition() // CANT DELETE THIS SNAPSHOT ONE
                    {
                        Id = subsetDefinitionSnapshotId,
                        KTR_MasterStudyId = masterStudy.ToEntityReference(),
                        KTR_Name = "BRAND_LISTSUB4",
                        KTR_FilterSignature = "hash4",
                        KTR_EverInSnapshot = true
                    }
                },
            };
            _repository.Setup(x => x.GetByMasterStudyId(masterStudy.Id, It.IsAny<string[]>())).Returns(subsetDefinitionsResponse);

            // Arrange Subset Associations
            var associationsResponse = new List<KTR_StudySubsetDefinition>()
            {
                {
                    new KTR_StudySubsetDefinition() // Same Study
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = new EntityReference(KT_Study.EntityLogicalName, study.Id),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDelete),
                    }
                }
            };
            var deletationAssociationsResponse = new List<KTR_StudySubsetDefinition>()
            {
                {
                    new KTR_StudySubsetDefinition() // Same Study
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = new EntityReference(KT_Study.EntityLogicalName, study.Id),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDelete),
                    }
                },
                {
                    new KTR_StudySubsetDefinition() // Another Study
                    {
                        Id = Guid.NewGuid(),
                        KTR_Study = new EntityReference(KT_Study.EntityLogicalName, Guid.NewGuid()),
                        KTR_SubsetDefinition = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdCantDelete),
                    }
                }
            };
            _repository.Setup(x => x.GetSubsetStudyAssociationByStudyId(study.Id, It.IsAny<string[]>())).Returns(associationsResponse);
            _repository.Setup(x => x.GetSubsetAssociationBySubsetIds(
                                new[] { subsetDefinitionIdToDelete, subsetDefinitionIdCantDelete },
                                It.IsAny<string[]>()))
                .Returns(deletationAssociationsResponse);

            var qlSubsetEntitiesResponse = new List<KTR_QuestionnaireLineSubset>()
            {
                {
                    new KTR_QuestionnaireLineSubset() // To Delete
                    {
                        Id = Guid.NewGuid(),
                        KTR_SubsetDefinitionId = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDelete),
                        KTR_QuestionnaireLineId = questionnaireLineA.ToEntityReference(),
                        KTR_Study = study.ToEntityReference(),
                        KTR_StudyMaster = masterStudy.ToEntityReference()
                    }
                }
            };
            _repository.Setup(x => x.GetQLSubsetsByStudyId(study.Id, It.IsAny<string[]>())).Returns(qlSubsetEntitiesResponse);

            // Arrange Questionnaire Line Managed List Entities
            var qLManagedListEntitiesResponse = new List<KTR_QuestionnaireLinemanAgedListEntity>();
            _qLMLErepository.Setup(x => x.GetByStudyId(study.Id, It.IsAny<string[]>())).Returns(qLManagedListEntitiesResponse);

            // Arrange Delete Setups
            var subsetEntitiesToDeleteResponse = new List<KTR_SubsetEntities>()
            {
                new KTR_SubsetEntities
                {
                    Id = Guid.NewGuid(),
                    KTR_SubsetDeFinTion = new EntityReference(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionIdToDelete)
                }
            };
            _repository.Setup(x => x.GetSubsetEntitiesByDefinitionIds(new[] { subsetDefinitionIdToDelete }, It.IsAny<string[]>()))
                .Returns(subsetEntitiesToDeleteResponse);
            var contexts = new List<SubsetCreationContext>();
            // Act
            var result = _service.ProcessSubsetLogic(study.Id);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            // Assert Delete calls
            var deleteSubsetStudyAssociationId = associationsResponse
                .Select(y => y.KTR_SubsetDefinition.Id).ToList();

            _repository.Verify(x => x.BulkDeleteSubsetStudyAssociation(deleteSubsetStudyAssociationId), Times.Never);

            // Assert Insert calls
            _repository.Verify(
                x => x.BulkInsert(It.IsAny<IList<KTR_SubsetDefinition>>()),
                Times.Never);

            _repository.Verify(
                x => x.BulkInsertSubsetStudyAssociation(It.IsAny<IList<KTR_StudySubsetDefinition>>()),
                Times.Never);

            _repository.Verify(
                x => x.BulkInsertQLSubsets(It.IsAny<IList<KTR_QuestionnaireLineSubset>>()),
                Times.Never);

            _repository.Verify(
                x => x.BulkInsertSubsetEntities(It.IsAny<IList<KTR_SubsetEntities>>()),
                Times.Never);
        }

        private static KTR_QuestionnaireLinemanAgedListEntity CreateQLManagedListEntity(
            EntityReference managedList,
            KTR_ManagedListEntity entity,
            KT_QuestionnaireLines questionnaireLine,
            KT_Study study,
            string entityName,
            Guid managedListId)
        {
            return new KTR_QuestionnaireLinemanAgedListEntity
            {
                Id = managedListId,
                KTR_ManagedList = managedList,
                KTR_ManagedListEntity = entity.ToEntityReference(),
                KTR_QuestionnaireLine = questionnaireLine.ToEntityReference(),
                KTR_StudyId = study.ToEntityReference(),
                KTR_Name = entityName,
                FormattedValues = new FormattedValueCollection()
                        {
                              { KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedList, managedList.Name}
                        }
            };
        }

        private string GenerateHash(IList<KTR_ManagedListEntity> entities, Guid masterStudyId)
        {
            var orderedIds = entities
                .Select(e => e.Id)
                .OrderBy(id => id)
                .ToList();

            var rawData = $"{masterStudyId}|{entities.First().KTR_ManagedList.Id}|{string.Join("|", orderedIds)}";

            return EncodeHelper.ComputeSha256Hash(rawData);
        }
    }
}

