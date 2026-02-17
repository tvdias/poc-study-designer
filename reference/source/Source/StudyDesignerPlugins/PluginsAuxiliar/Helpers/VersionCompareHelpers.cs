namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Study;
    using Microsoft.Xrm.Sdk;
    using spkl.Diffs;

    public static class VersionCompareHelpers
    {
        private static readonly Dictionary<string, KTR_ChangelogFieldChanged> PossibleQuestionsFieldsChanged = new Dictionary<string, KTR_ChangelogFieldChanged>(StringComparer.OrdinalIgnoreCase)
        {
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Id, KTR_ChangelogFieldChanged.QuestionId },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionRationale, KTR_ChangelogFieldChanged.QuestionQuestionRationale },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionText, KTR_ChangelogFieldChanged.QuestionQuestionText },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionTitle, KTR_ChangelogFieldChanged.QuestionQuestionTitle },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionType, KTR_ChangelogFieldChanged.QuestionQuestionType },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Name, KTR_ChangelogFieldChanged.QuestionQuestionVariableName },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionVersion2, KTR_ChangelogFieldChanged.QuestionQuestionVersion },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_ScripterNotes, KTR_ChangelogFieldChanged.QuestionScripterNotes },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_StandardOrCustom, KTR_ChangelogFieldChanged.QuestionStandardOrCustom },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_IsDummyQuestion, KTR_ChangelogFieldChanged.QuestionIsDummy },
            { KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Scriptlets, KTR_ChangelogFieldChanged.QuestionScriptlets },
        };

        private static readonly Dictionary<string, KTR_ChangelogFieldChanged> PossibleAnswersFieldsChanged = new Dictionary<string, KTR_ChangelogFieldChanged>(StringComparer.OrdinalIgnoreCase)
        {
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerId, KTR_ChangelogFieldChanged.AnswerAnswerId },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerLocation, KTR_ChangelogFieldChanged.AnswerAnswerLocation },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_AnswerText, KTR_ChangelogFieldChanged.AnswerAnswerTitle },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_CustomProperty, KTR_ChangelogFieldChanged.AnswerCustomerProperty },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EffectiveDate, KTR_ChangelogFieldChanged.AnswerEffectiveDate },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_EndDate, KTR_ChangelogFieldChanged.AnswerEndDate },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsActive, KTR_ChangelogFieldChanged.AnswerIsActive },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsExclusive, KTR_ChangelogFieldChanged.AnswerIsExclusive },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsFixed, KTR_ChangelogFieldChanged.AnswerIsFixed },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsOpen, KTR_ChangelogFieldChanged.AnswerIsOpen },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_IsTranslatable, KTR_ChangelogFieldChanged.AnswerIsTranslatable },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceId, KTR_ChangelogFieldChanged.AnswerSourceId },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_SourceName, KTR_ChangelogFieldChanged.AnswerSourceName },
            { KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_Version, KTR_ChangelogFieldChanged.AnswerVersion },
        };

        private static readonly Dictionary<string, KTR_ChangelogFieldChanged> PossibleManagedListFieldsChanged = new Dictionary<string, KTR_ChangelogFieldChanged>(StringComparer.OrdinalIgnoreCase)
        {
            { KTR_StudyQuestionManagedListSnapshot.Fields.KTR_Location, KTR_ChangelogFieldChanged.ManagedListLocation },
            { KTR_StudyQuestionManagedListSnapshot.Fields.KTR_DisplayOrder, KTR_ChangelogFieldChanged.ManagedListDisplayOrder },
            { KTR_StudyQuestionManagedListSnapshot.Fields.KTR_Name, KTR_ChangelogFieldChanged.ManagedListName },
        };

        private static readonly Dictionary<string, KTR_ChangelogFieldChanged> PossibleManagedListEntityFieldsChanged = new Dictionary<string, KTR_ChangelogFieldChanged>(StringComparer.OrdinalIgnoreCase)
        {
            { KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_DisplayOrder, KTR_ChangelogFieldChanged.ManagedListEntityDisplayOrder },
            { KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_Name, KTR_ChangelogFieldChanged.ManagedListEntityName },
        };

        public static VersionCompareResult CompareEntities(
            Dictionary<Guid, Entity> currentEntitiesDict,
            Dictionary<Guid, Entity> parentEntitiesDict,
            KTR_ChangelogRelatedObject changeLogRelatedEntity)
        {
            if ((currentEntitiesDict == null || currentEntitiesDict.Count == 0)
                && (parentEntitiesDict == null || parentEntitiesDict.Count == 0))
            {
                return null;
            }

            var addedIds = (currentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys
                .Except((parentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys);
            var removedIds = (parentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys
                .Except((currentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys);
            var commonIds = (parentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys
                .Intersect((currentEntitiesDict ?? new Dictionary<Guid, Entity>()).Keys);

            var currentCommonEntitiesDict = currentEntitiesDict
                .Where(x => commonIds.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            var parentCommonEntitiesDict = parentEntitiesDict
                .Where(x => commonIds.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            var fieldsChanged = CompareFieldsChanged(currentCommonEntitiesDict, parentCommonEntitiesDict, changeLogRelatedEntity);

            return new VersionCompareResult
            {
                RelatedObject = changeLogRelatedEntity,
                AddedEntityIds = addedIds,
                RemovedEntityIds = removedIds,
                CommonEntityIds = commonIds,
                FieldsChangedResults = fieldsChanged,
            };
        }

        private static IList<FieldChangedResult> CompareFieldsChanged(
            IDictionary<Guid, Entity> currentCommonEntities,
            IDictionary<Guid, Entity> parentCommonEntities,
            KTR_ChangelogRelatedObject changeLogRelatedEntity)
        {
            if ((currentCommonEntities == null || currentCommonEntities.Count == 0)
                && (parentCommonEntities == null || parentCommonEntities.Count == 0))
            {
                return new List<FieldChangedResult>();
            }

            // Ensure dictionaries are not null to avoid NullReferenceException
            var currentDict = currentCommonEntities ?? new Dictionary<Guid, Entity>();
            var parentDict = parentCommonEntities ?? new Dictionary<Guid, Entity>();

            var possibleFieldsChanged = new Dictionary<string, KTR_ChangelogFieldChanged>();
            switch (changeLogRelatedEntity)
            {
                case KTR_ChangelogRelatedObject.Question:
                    possibleFieldsChanged = PossibleQuestionsFieldsChanged;
                    break;
                case KTR_ChangelogRelatedObject.Answer:
                    possibleFieldsChanged = PossibleAnswersFieldsChanged;
                    break;
                case KTR_ChangelogRelatedObject.ManagedList:
                    possibleFieldsChanged = PossibleManagedListFieldsChanged;
                    break;
                case KTR_ChangelogRelatedObject.ManagedListEntity:
                    possibleFieldsChanged = PossibleManagedListEntityFieldsChanged;
                    break;
                default:
                    return new List<FieldChangedResult>();
            }

            var fieldsChangedResult = new List<FieldChangedResult>();
            foreach (var currentEntity in currentDict)
            {
                var entityId = currentEntity.Key;

                var attributes = currentEntity.Value.Attributes
                    .Where(x => possibleFieldsChanged.Keys.Contains(x.Key));

                var parentEntity = parentDict
                    .FirstOrDefault(x => x.Key == entityId);

                var fieldChanged = CheckWhichFieldChanged(
                    currentEntity.Value,
                    parentEntity.Value,
                    possibleFieldsChanged);

                if (fieldChanged != null && fieldChanged.Count > 0)
                {
                    fieldsChangedResult.AddRange(fieldChanged);
                }
            }

            return fieldsChangedResult;
        }

        private static IList<FieldChangedResult> CheckWhichFieldChanged(
            Entity currentEntity,
            Entity parentEntity,
            IDictionary<string, KTR_ChangelogFieldChanged> possibleFieldsChanged)
        {
            var fieldsChanged = new List<FieldChangedResult>();
            var attributes = currentEntity.Attributes
                    .Where(x => possibleFieldsChanged.Keys.Contains(x.Key));

            foreach (var attribute in attributes)
            {
                var attributeName = attribute.Key;

                var currentValue = currentEntity.GetAttributeValue<object>(attributeName);
                var parentValue = parentEntity.GetAttributeValue<object>(attributeName);

                // Special handling for lookups (EntityReference)
                if (currentValue is EntityReference currentRef && parentValue is EntityReference parentRef)
                {
                    if (currentRef.Id != parentRef.Id || currentRef.LogicalName != parentRef.LogicalName)
                    {
                        fieldsChanged.Add(MapFieldChangeResult(
                            currentEntity,
                            attributeName,
                            possibleFieldsChanged[attribute.Key],
                            parentValue.ToString(),
                            currentValue.ToString()));
                    }
                }
                // Handle OptionSetValue comparison
                else if (currentValue is OptionSetValue currentOpt && parentValue is OptionSetValue parentOpt)
                {
                    if (currentOpt.Value != parentOpt.Value)
                    {
                        fieldsChanged.Add(MapFieldChangeResult(
                            currentEntity,
                            attributeName,
                            possibleFieldsChanged[attribute.Key],
                            parentOpt.Value.ToString(),
                            currentOpt.Value.ToString()));
                    }
                }
                // General object comparison
                else if (!object.Equals(currentValue, parentValue))
                {
                    fieldsChanged.Add(MapFieldChangeResult(
                            currentEntity,
                            attributeName,
                            possibleFieldsChanged[attribute.Key],
                            parentValue != null ? parentValue.ToString() : null,
                            currentValue != null ? currentValue.ToString() : null));
                }
            }

            return fieldsChanged;
        }

        private static FieldChangedResult MapFieldChangeResult(
            Entity entity,
            string logicalName,
            KTR_ChangelogFieldChanged fieldChangedOption,
            string oldValue,
            string newValue)
        {
            return new FieldChangedResult
            {
                Entity = entity,
                FieldLogicalName = logicalName,
                FieldChanged = fieldChangedOption,
                OldValue = oldValue,
                NewValue = newValue
            };
        }

        //Note: We probably do not need this (int, int) tuple at all, need to use sort order anyway
        public static Dictionary<T, (int, int)> MyersDiffAlgorithm<T>(T[] listBefore, T[] listAfter)

        {
            //Run diff algorithm
            MyersDiff<T> diff = new MyersDiff<T>(listBefore, listAfter);
            var edits = diff.GetEditScript();

            Dictionary<T, (int, int)> changes = new Dictionary<T, (int, int)>(); // Key: Id, Value: (Index in Before, Index in After)

            foreach (var edit in edits)
            {
                for (int i = edit.LineA; i < edit.LineA + edit.CountA; i++)
                {
                    if (!changes.ContainsKey(listBefore[i]))
                    {
                        changes[listBefore[i]] = (i, -1);
                    }
                    else
                    {
                        (int, int) tupl = changes[listBefore[i]];
                        tupl.Item1 = i;
                        changes[listBefore[i]] = tupl;
                    }
                }

                for (int i = edit.LineB; i < edit.LineB + edit.CountB; i++)
                {
                    if (!changes.ContainsKey(listAfter[i]))
                    {
                        changes[listAfter[i]] = (-1, i);
                    }
                    else
                    {
                        (int, int) tupl = changes[listAfter[i]];
                        tupl.Item2 = i;
                        changes[listAfter[i]] = tupl;
                    }
                }
            }

            return changes;
        }
    }
}
