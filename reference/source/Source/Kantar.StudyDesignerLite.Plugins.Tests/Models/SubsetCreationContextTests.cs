namespace Kantar.StudyDesignerLite.Plugins.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class SubsetCreationContextTests
    {
        private static EntityReference Ref(Guid id, string logicalName)
        {
            return new EntityReference(logicalName, id);
        }

        private static string ComputeSha256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private KT_Study CreateStudy(Guid studyId, Guid? masterStudyId, string studyLogicalName = KT_Study.EntityLogicalName)
        {
            var study = new KT_Study(new { })
            {
                Id = studyId,
                KTR_MasterStudy = masterStudyId.HasValue ? new EntityReference(studyLogicalName, masterStudyId.Value) : null,
                KT_Project = Ref(Guid.NewGuid(), KT_Project.EntityLogicalName)
            };
            return study;
        }

        private KTR_QuestionnaireLinemanAgedListEntity CreateQLMLE(Guid managedListId, Guid managedListEntityId, Guid questionnaireLineId)
        {
            var e = new KTR_QuestionnaireLinemanAgedListEntity(new { })
            {
                KTR_ManagedList = Ref(managedListId, KTR_ManagedList.EntityLogicalName),
                KTR_ManagedListEntity = Ref(managedListEntityId, "ktr_managedlistentity"), // logical name of managed list entity
                KTR_QuestionnaireLine = Ref(questionnaireLineId, KT_QuestionnaireLines.EntityLogicalName)
            };
            return e;
        }

        [TestMethod]
        public void Constructor_SetsExpectedFields()
        {
            // Arrange
            var masterStudyId = Guid.NewGuid();
            var studyId = Guid.NewGuid();
            var study = CreateStudy(studyId, masterStudyId);

            var managedListId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var entityIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var entities = entityIds
                .Select(id => CreateQLMLE(managedListId, id, qlId))
                .ToList();

            var orderedIds = entityIds.OrderBy(x => x).ToArray();
            var raw = $"{masterStudyId}|{managedListId}|{string.Join("|", orderedIds)}";
            var expectedHash = ComputeSha256(raw);

            // Act
            var ctx = new SubsetCreationContext(entities, study);

            // Assert
            Assert.AreNotEqual(Guid.Empty, ctx.SubsetDefinitionId);
            Assert.IsTrue(ctx.IsNewSubset);
            Assert.AreEqual(entities.Count, ctx.EntityCount);
            Assert.AreEqual(managedListId, ctx.KTR_ManagedList.Id);
            CollectionAssert.AreEquivalent(entityIds.Select(id => new EntityReference("ktr_managedlistentity", id)).ToList(), ctx.KTR_ManagedListEntities);
            Assert.AreEqual(expectedHash, ctx.Hash);
            Assert.AreEqual(study, ctx.Study);
            Assert.AreEqual(masterStudyId, ctx.MasterStudyId);
        }

        [TestMethod]
        public void Constructor_UsesSelfIdWhenNoMasterStudy()
        {
            // Arrange
            var studyId = Guid.NewGuid();
            var study = CreateStudy(studyId, null);

            var managedListId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var entityIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var entities = entityIds.Select(id => CreateQLMLE(managedListId, id, qlId)).ToList();

            var orderedIds = entityIds.OrderBy(x => x).ToArray();
            var raw = $"{studyId}|{managedListId}|{string.Join("|", orderedIds)}";
            var expectedHash = ComputeSha256(raw);

            // Act
            var ctx = new SubsetCreationContext(entities, study);

            // Assert
            Assert.AreEqual(studyId, ctx.MasterStudyId);
            Assert.AreEqual(expectedHash, ctx.Hash);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_Throws_WhenMultipleQuestionnaireLines()
        {
            // Arrange: two different QuestionnaireLine Ids
            var study = CreateStudy(Guid.NewGuid(), Guid.NewGuid());
            var managedListId = Guid.NewGuid();

            var e1 = CreateQLMLE(managedListId, Guid.NewGuid(), Guid.NewGuid());
            var e2 = CreateQLMLE(managedListId, Guid.NewGuid(), Guid.NewGuid()); // different QL Id

            var entities = new List<KTR_QuestionnaireLinemanAgedListEntity> { e1, e2 };

            // Act
            // Should throw InvalidOperationException with message about multiple Questionnaire Lines
            var _ = new SubsetCreationContext(entities, study);
        }

        [TestMethod]
        public void ToEntity_MapsFieldsCorrectly()
        {
            // Arrange
            var study = CreateStudy(Guid.NewGuid(), Guid.NewGuid());
            var managedListId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var entityIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var entities = entityIds.Select(id => CreateQLMLE(managedListId, id, qlId)).ToList();

            var ctx = new SubsetCreationContext(entities, study)
            {
                SubsetName = "Test Subset",
                IsFullList = true,
                UsageCount = 42
            };

            // Act
            var subsetEntity = ctx.ToEntity();

            // Assert
            Assert.AreEqual(ctx.SubsetDefinitionId, subsetEntity.Id);
            Assert.AreEqual("Test Subset", subsetEntity.KTR_Name);
            Assert.AreEqual(ctx.Hash, subsetEntity.KTR_FilterSignature);
            Assert.AreEqual(ctx.KTR_ManagedList.Id, subsetEntity.KTR_ManagedList.Id);
            Assert.IsNotNull(subsetEntity.KTR_MasterStudyId);
            Assert.AreEqual(ctx.MasterStudyId, subsetEntity.KTR_MasterStudyId.Id);
            Assert.AreEqual(ctx.EntityCount, subsetEntity.KTR_EntityCount);
            Assert.AreEqual(true, subsetEntity.KTR_IsFullList);
            Assert.AreEqual(study.KT_Project.Id, subsetEntity.KTR_Project.Id);
            Assert.AreEqual(42, subsetEntity.KTR_UsageCount);
        }

        [TestMethod]
        public void ToResponse_MapsFieldsCorrectly()
        {
            // Arrange
            var study = CreateStudy(Guid.NewGuid(), Guid.NewGuid());
            var managedListId = Guid.NewGuid();
            var qlId = Guid.NewGuid();
            var entities = new[] { Guid.NewGuid() }
                .Select(id => CreateQLMLE(managedListId, id, qlId))
                .ToList();

            var ctx = new SubsetCreationContext(entities, study)
            {
                SubsetName = "Subset Resp",
                UsageCount = 5,
                IsFullList = false,
                IsReused = true
            };

            // Act
            var resp = ctx.ToResponse();

            // Assert
            Assert.AreEqual(ctx.SubsetDefinitionId, resp.SubsetId);
            Assert.AreEqual("Subset Resp", resp.SubsetName);
            Assert.AreEqual(ctx.EntityCount, resp.EntityCount);
            Assert.AreEqual(5, resp.UsageCount);
            Assert.AreEqual(false, resp.UsesFullList);
            Assert.AreEqual(true, resp.IsReused);
        }
    }
}
