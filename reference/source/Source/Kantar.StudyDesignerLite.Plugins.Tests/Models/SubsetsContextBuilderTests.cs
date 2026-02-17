namespace Kantar.StudyDesignerLite.Plugins.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class SubsetsContextBuilderTests
    {
        private static KTR_QuestionnaireLinemanAgedListEntity CreateQLManagedListEntity(
            Guid managedListId,
            string managedListName,
            Guid managedListEntityId,
            string managedListEntityName,
            Guid questionnaireLineId,
            string questionnaireLineName)
        {
            var qlle = new KTR_QuestionnaireLinemanAgedListEntity
            {
                Id = Guid.NewGuid()
            };

            var ml = new KTR_ManagedList
            {
                Id = managedListId
            };
            ml.Attributes["name"] = managedListName;

            var mle = new KTR_ManagedListEntity
            {
                Id = managedListEntityId
            };
            mle.Attributes["name"] = managedListEntityName;

            var ql = new KT_QuestionnaireLines
            {
                Id = questionnaireLineId
            };
            ql.Attributes["name"] = questionnaireLineName;

            qlle.KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, ml.Id);
            qlle.KTR_ManagedListEntity = new EntityReference(KTR_ManagedListEntity.EntityLogicalName, mle.Id);
            qlle.KTR_QuestionnaireLine = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, ql.Id);

            return qlle;
        }

        private static IList<KTR_SubsetDefinition> CreateExistingSubsetDefinitions(
            IEnumerable<(string name, string signature)> items,
            Guid? managedListId = null,
            Guid? masterStudyId = null)
        {
            var list = new List<KTR_SubsetDefinition>();
            foreach (var (name, signature) in items)
            {
                var def = new KTR_SubsetDefinition
                {
                    Id = Guid.NewGuid()
                };
                def.Attributes["ktr_name"] = name;
                def.Attributes["ktr_filtersignature"] = signature;
                if (managedListId.HasValue)
                {
                    def.KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, managedListId.Value);
                }
                if (masterStudyId.HasValue)
                {
                    def.KTR_MasterStudyId = new EntityReference(KT_Study.EntityLogicalName, masterStudyId.Value);
                }
                list.Add(def);
            }
            return list;
        }

        [TestMethod]
        public void ProcessExistingSubsetDefinitions_ReturnExisting()
        {
            // Arrange
            var study = new KT_Study
            {
                Id = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a163"),
                KT_Name = "Study-1"
            };
            var mlId = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a160");
            var qlId = Guid.NewGuid();
            var subsetDefinitionId = Guid.NewGuid();
            var mleId1 = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a161");
            var mleId2 = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a162");

            var managedListName = "ML1";
            var qlName = "QL-A";
            var mleName1 = "Entity-1";
            var mleName2 = "Entity-2";

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>
            {
                CreateQLManagedListEntity(mlId, managedListName, mleId1, mleName1, qlId, qlName),
                CreateQLManagedListEntity(mlId, managedListName, mleId2, mleName2, qlId, qlName)
            };

            var existent = new List<KTR_SubsetDefinition>()
            {
               new KTR_SubsetDefinition
               {
                   Id = Guid.NewGuid(),
                   KTR_FilterSignature = "4726951b4f1091e75013675eb6f82b5036ba5e792fb5be1bc40df5ec0b8c5d23",
                   KTR_SubsetDefinitionId = subsetDefinitionId,
                   KTR_Name = managedListName + "SUB1",
                   KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, mlId),
               },
            };

            var builder = new SubsetsContextBuilder(entities, study);

            // Act
            builder.ProcessExistingSubsetDefinitions(existent);
            builder.ProcessExistingStudySubsetDefinitions(new List<KTR_StudySubsetDefinition>());
            var result = builder.Build();

            // Assert
            Assert.AreEqual(1, result.Count);
            var first = result.First();
            Assert.AreEqual(first.SubsetName, managedListName + "SUB1");
            Assert.AreEqual(first.Hash, "4726951b4f1091e75013675eb6f82b5036ba5e792fb5be1bc40df5ec0b8c5d23");
            Assert.AreEqual(first.SubsetDefinitionId, subsetDefinitionId);
            Assert.IsTrue(first.IsReused);
            Assert.IsFalse(first.IsNewSubset);
        }

        [TestMethod]
        public void ProcessExistingSubsetDefinitions_Marks_Reused_When_Signature_Matches()
        {
            // Arrange
            var study = new KT_Study
            {
                Id = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a163")
            };
            var mlId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var mleId = Guid.NewGuid();

            var managedListName = "ML-Reuse";
            var qlName = "QL-Reuse";
            var mleName = "Entity-Reuse";

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>
            {
                CreateQLManagedListEntity(mlId, managedListName, mleId, mleName, qlId, qlName)
            };

            var builder = new SubsetsContextBuilder(entities, study);

            var contexts = builder.ProcessExistingSubsetDefinitions(new List<KTR_SubsetDefinition>());
            Assert.AreEqual(1, contexts.Count);
            var contextHash = contexts[0].Hash;

            var subsetName = "ExistingSubsetName";
            var existent = CreateExistingSubsetDefinitions(new[] { (subsetName, contextHash) }, mlId, study.Id);

            // Act
            var processed = builder.ProcessExistingSubsetDefinitions(existent);

            // Assert
            Assert.AreEqual(1, processed.Count);
            var ctx = processed[0];
            Assert.IsFalse(ctx.IsNewSubset);
            Assert.IsTrue(ctx.IsReused);
            Assert.AreNotEqual(Guid.Empty, ctx.SubsetDefinitionId);
            Assert.AreEqual(subsetName, ctx.SubsetName);
        }

        [TestMethod]
        public void ProcessExistingQuestionnaireLineSubset_Creates_New_On_Missing()
        {
            // Arrange
            var study = new KT_Study
            {
                Id = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a163")
            };
            var mlId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var mleId = Guid.NewGuid();

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>
            {
                CreateQLManagedListEntity(mlId, "ML-Q", mleId, "Entity-Q", qlId, "QL-Q")
            };

            var builder = new SubsetsContextBuilder(entities, study);

            builder.ProcessExistingSubsetDefinitions(new List<KTR_SubsetDefinition>());

            // Act
            var processed = builder.ProcessExistingQuestionnaireLineSubset(new List<KTR_QuestionnaireLineSubset>());

            // Assert
            Assert.AreEqual(1, processed.Count);
            var ctx = processed[0];
            Assert.IsTrue(ctx.NewQuestionnaireLineSubsets.Count > 0);
            Assert.AreEqual(0, ctx.ExistingQuestionnaireLineSubsets.Count);
        }

        [TestMethod]
        public void ProcessExistingSubsetEntities_Creates_New_On_Missing()
        {
            // Arrange
            var study = new KT_Study
            {
                Id = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a163")
            };
            var mlId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var mleId1 = Guid.NewGuid();
            var mleId2 = Guid.NewGuid();

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>
            {
                CreateQLManagedListEntity(mlId, "ML-Entities", mleId1, "Entity-1", qlId, "QL-Entities"),
                CreateQLManagedListEntity(mlId, "ML-Entities", mleId2, "Entity-2", qlId, "QL-Entities")
            };

            var builder = new SubsetsContextBuilder(entities, study);

            // Make sure we have a SubsetDefinitionId in context
            builder.ProcessExistingSubsetDefinitions(new List<KTR_SubsetDefinition>());

            // Act
            var processed = builder.ProcessExistingSubsetEntities(new List<KTR_SubsetEntities>());

            // Assert
            Assert.AreEqual(1, processed.Count);
            var ctx = processed[0];
            Assert.AreEqual(2, ctx.NewSubsetEntities.Count);
            Assert.AreEqual(0, ctx.ExistingSubsetEntities.Count);
        }

        [TestMethod]
        public void ProcessManagedListEntities_Marks_FullList_When_Counts_Match()
        {
            // Arrange
            var study = new KT_Study
            {
                Id = Guid.Parse("8e8d7499-4a16-47c9-9c88-d5285906a163")
            };
            var mlId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var mleId1 = Guid.NewGuid();
            var mleId2 = Guid.NewGuid();

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity>
            {
                CreateQLManagedListEntity(mlId, "ML-Full", mleId1, "Entity-1", qlId, "QL-Full"),
                CreateQLManagedListEntity(mlId, "ML-Full", mleId2, "Entity-2", qlId, "QL-Full")
            };

            var builder = new SubsetsContextBuilder(entities, study);
            var managedListDefinition = new KTR_ManagedList()
            {
                Id = mlId,
                KTR_Name = "ML-Full"
            };
            var smlEntities = new List<KTR_ManagedListEntity>
            {
                new KTR_ManagedListEntity { Id = mleId1, KTR_ManagedList = managedListDefinition.ToEntityReference() },
                new KTR_ManagedListEntity { Id = mleId2, KTR_ManagedList = managedListDefinition.ToEntityReference() }
            };

            // Act
            var processed = builder.ProcessManagedListEntities(smlEntities);

            // Assert
            Assert.AreEqual(1, processed.Count);
            var ctx = processed[0];
            Assert.IsTrue(ctx.IsFullList);
        }
    }
}
