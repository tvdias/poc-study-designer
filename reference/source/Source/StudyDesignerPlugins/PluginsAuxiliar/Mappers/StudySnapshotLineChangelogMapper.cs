using System;
using System.Collections.Generic;
using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Study;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers
{
    public static class StudySnapshotLineChangelogMapper
    {
        public static KTR_StudySnapshotLineChangelog MapQuestionAdded(
            Guid currentStudyId,
            Guid formerStudyId,
            KTR_StudyQuestionnaireLineSnapshot currentSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Question,
                KTR_Change = KTR_ChangelogType.QuestionAdded,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentSnapshot.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = null,
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
            };
        }
        public static KTR_StudySnapshotLineChangelog MapModuleAdded(
            Guid currentStudyId,
            Guid formerStudyId,
            KTR_StudyQuestionnaireLineSnapshot currentSnapshot,
            Guid moduleId)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Module,
                KTR_Change = KTR_ChangelogType.ModuleAdded,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentSnapshot.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = null,
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_Module2 = new EntityReference(KT_Module.EntityLogicalName, moduleId)
            };
        }
        public static KTR_StudySnapshotLineChangelog MapAnswerAdded(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyQuestionAnswerListSnapshot currentQlAnswerSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Answer,
                KTR_Change = KTR_ChangelogType.AnswerAdded,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlAnswerSnapshot.Id),
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
            };
        }

        public static KTR_StudySnapshotLineChangelog MapManagedListAdded(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyQuestionManagedListSnapshot currentQlManagedListSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedList,
                KTR_Change = KTR_ChangelogType.ManagedListAdded,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_CurrentVersionManagedList = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlManagedListSnapshot.Id),
                KTR_FormerVersionManagedList = null
            };
        }

        public static KTR_StudySnapshotLineChangelog MapManagedListEntityAdded(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyManagedListEntitiesSnapshot currentMLEntitysnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedListEntity,
                KTR_Change = KTR_ChangelogType.ManagedListEntityAdded,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = null,
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_CurrentVersionManagedList = null,
                KTR_FormerVersionManagedList = null,
                KTR_CurrentVersionManagedListEntity = new EntityReference(KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName, currentMLEntitysnapshot.Id),
                KTR_FormerVersionManagedListEntity = null
            };
        }

        public static KTR_StudySnapshotLineChangelog MapQuestionRemoved(
            Guid currentStudyId,
            Guid formerStudyId,
            KTR_StudyQuestionnaireLineSnapshot formerSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Question,
                KTR_Change = KTR_ChangelogType.QuestionRemoved,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = null,
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerSnapshot.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
            };
        }
        public static KTR_StudySnapshotLineChangelog MapModuleRemoved(
            Guid currentStudyId,
            Guid formerStudyId,
            KTR_StudyQuestionnaireLineSnapshot formerSnapshot,
            Guid moduleId)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Module,
                KTR_Change = KTR_ChangelogType.ModuleRemoved,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = null,
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerSnapshot.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_Module2 = new EntityReference(KT_Module.EntityLogicalName, moduleId)
            };
        }
        public static KTR_StudySnapshotLineChangelog MapAnswerRemoved(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference formerQlSnapshotRef,
            KTR_StudyQuestionAnswerListSnapshot formerQlAnswerSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Answer,
                KTR_Change = KTR_ChangelogType.AnswerRemoved,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlAnswerSnapshot.Id),
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
            };
        }

        public static KTR_StudySnapshotLineChangelog MapManagedListRemoved(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference formerQlSnapshotRef,
            KTR_StudyQuestionManagedListSnapshot formerQlManagedListSnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedList,
                KTR_Change = KTR_ChangelogType.ManagedListRemoved,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_CurrentVersionManagedList = null,
                KTR_FormerVersionManagedList = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerQlManagedListSnapshot.Id)
            };
        }

        public static KTR_StudySnapshotLineChangelog MapManagedListEntityRemoved(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyManagedListEntitiesSnapshot formerMLEntitysnapshot)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedListEntity,
                KTR_Change = KTR_ChangelogType.ManagedListEntityRemoved,
                KTR_FieldChanged = null,
                KTR_CurrentStudyQuestionnaireSnapshotLine = null,
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = null,
                KTR_NewValue2 = null,
                KTR_CurrentVersionManagedList = null,
                KTR_FormerVersionManagedList = null,
                KTR_CurrentVersionManagedListEntity = null,
                KTR_FormerVersionManagedListEntity = new EntityReference(KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName, formerMLEntitysnapshot.Id)
            };
        }

        public static KTR_StudySnapshotLineChangelog MapModifiedQuestion(
            Guid currentStudyId,
            Guid formerStudyId,
            KTR_StudyQuestionnaireLineSnapshot currentSnapshot,
            KTR_StudyQuestionnaireLineSnapshot formerSnapshot,
            FieldChangedResult fieldChange)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Question,
                KTR_Change = KTR_ChangelogType.FieldChangeQuestion,
                KTR_FieldChanged = fieldChange.FieldChanged,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentSnapshot.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, formerSnapshot.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_OldValue2 = fieldChange.OldValue,
                KTR_NewValue2 = fieldChange.NewValue,
            };
        }

        public static KTR_StudySnapshotLineChangelog MapModifiedAnswer(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyQuestionAnswerListSnapshot currentQlAnswerSnapshot,
            FieldChangedResult fieldChange)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.Answer,
                KTR_Change = KTR_ChangelogType.FieldChangeAnswer,
                KTR_FieldChanged = fieldChange.FieldChanged,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlAnswerSnapshot.Id),
                KTR_FormerStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlAnswerSnapshot.Id),
                KTR_OldValue2 = fieldChange.OldValue,
                KTR_NewValue2 = fieldChange.NewValue,
            };
        }

        public static KTR_StudySnapshotLineChangelog MapModifiedManagedList(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyQuestionManagedListSnapshot currentQlManagedListSnapshot,
            FieldChangedResult fieldChange)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedList,
                KTR_Change = KTR_ChangelogType.FieldChangeManagedList,
                KTR_FieldChanged = fieldChange.FieldChanged,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_CurrentVersionManagedList = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlManagedListSnapshot.Id),
                KTR_FormerVersionManagedList = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlManagedListSnapshot.Id),
                KTR_OldValue2 = fieldChange.OldValue,
                KTR_NewValue2 = fieldChange.NewValue,
            };
        }

        public static KTR_StudySnapshotLineChangelog MapModifiedManagedListEntity(
            Guid currentStudyId,
            Guid formerStudyId,
            EntityReference currentQlSnapshotRef,
            KTR_StudyManagedListEntitiesSnapshot snapshot,
            FieldChangedResult fieldChange)
        {
            return new KTR_StudySnapshotLineChangelog
            {
                KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, formerStudyId),
                KTR_RelatedObject = KTR_ChangelogRelatedObject.ManagedListEntity,
                KTR_Change = KTR_ChangelogType.FieldChangeManagedListEntity,
                KTR_FieldChanged = fieldChange.FieldChanged,
                KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentQlSnapshotRef.Id),
                KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                KTR_FormerStudyQuestionAnswerListSnapshot = null,
                KTR_CurrentVersionManagedListEntity = new EntityReference(KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName, snapshot.Id),
                KTR_FormerVersionManagedListEntity = new EntityReference(KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName, snapshot.Id),
                KTR_OldValue2 = fieldChange.OldValue,
                KTR_NewValue2 = fieldChange.NewValue
            };
        }

        public static List<KTR_StudySnapshotLineChangelog> MapModifiedQuestionOrder(
            Guid currentStudyId,
            Guid parentStudyId,
            Dictionary<Guid, Entity> currentDict,
            Dictionary<Guid, Entity> parentDict,
            Dictionary<Guid, (int, int)> diffResult)
        {
            List<KTR_StudySnapshotLineChangelog> logs = new List<KTR_StudySnapshotLineChangelog>();
            foreach (var diffId in diffResult)
            {
                logs.Add(new KTR_StudySnapshotLineChangelog
                {
                    KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                    KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, parentStudyId),
                    KTR_RelatedObject = KTR_ChangelogRelatedObject.Question,
                    KTR_Change = KTR_ChangelogType.QuestionReordered,
                    KTR_FieldChanged = null,
                    KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentDict[diffId.Key].Id),
                    KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, parentDict[diffId.Key].Id),
                    KTR_CurrentStudyQuestionAnswerListSnapshot = null,
                    KTR_FormerStudyQuestionAnswerListSnapshot = null,
                    KTR_OldValue2 = ((KTR_StudyQuestionnaireLineSnapshot)parentDict[diffId.Key]).KTR_SortOrder?.ToString() ?? "-1",
                    KTR_NewValue2 = ((KTR_StudyQuestionnaireLineSnapshot)currentDict[diffId.Key]).KTR_SortOrder?.ToString() ?? "-1",
                });

            }
            return logs;
        }

        public static List<KTR_StudySnapshotLineChangelog> MapModifiedAnswerOrder(
            Guid currentStudyId,
            Guid parentStudyId,
            Dictionary<Guid, Entity> currentDict,
            Dictionary<Guid, Entity> parentDict,
            Dictionary<Guid, (int, int)> diffResult)
        {
            List<KTR_StudySnapshotLineChangelog> logs = new List<KTR_StudySnapshotLineChangelog>();
            foreach (var diffId in diffResult)
            {
                var parentSnapshot = (KTR_StudyQuestionAnswerListSnapshot)parentDict[diffId.Key];
                var currentSnapshot = (KTR_StudyQuestionAnswerListSnapshot)currentDict[diffId.Key];

                logs.Add(new KTR_StudySnapshotLineChangelog
                {
                    KTR_CurrentStudy = new EntityReference(KT_Study.EntityLogicalName, currentStudyId),
                    KTR_FormerStudy = new EntityReference(KT_Study.EntityLogicalName, parentStudyId),
                    KTR_RelatedObject = KTR_ChangelogRelatedObject.Answer,
                    KTR_Change = KTR_ChangelogType.AnswerReordered,
                    KTR_FieldChanged = null,
                    KTR_CurrentStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, currentSnapshot.KTR_QuestionnaireLinesNaPsHot.Id),
                    KTR_FormerStudyQuestionnaireSnapshotLine = new EntityReference(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, parentSnapshot.KTR_QuestionnaireLinesNaPsHot.Id),
                    KTR_CurrentStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName, currentSnapshot.Id),
                    KTR_FormerStudyQuestionAnswerListSnapshot = new EntityReference(KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName, parentSnapshot.Id),
                    KTR_OldValue2 = parentSnapshot.KTR_DisplayOrder?.ToString() ?? "-1",
                    KTR_NewValue2 = currentSnapshot.KTR_DisplayOrder?.ToString() ?? "-1",
                });

            }
            return logs;
        }
    }
}
