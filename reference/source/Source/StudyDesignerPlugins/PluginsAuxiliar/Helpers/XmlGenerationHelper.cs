namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Microsoft.Xrm.Sdk;

    public static class XmlGenerationHelper
    {
        public static string GenerateStudyXml(StudyXmlData data)
        {
            if (data?.Study == null)
            {
                throw new ArgumentNullException(nameof(data), "Study data cannot be null");
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var studyElement = new XElement("Study");
            doc.Add(studyElement);

            AddStudyBasicInfo(studyElement, data.Study);

            AddFieldworkMarketInfo(studyElement, data.Study);

            AddFieldworkLanguages(studyElement, data.Languages);

            AddScripterNotes(studyElement, data.Study);

            AddProjectInfo(studyElement, data.Project);

            AddSubsets(studyElement, data.Subsets, data.SubsetEntities);

            AddQuestionnaireLines(studyElement, data.QuestionnaireLinesSnapshot, data.QuestionnaireLineAnswersSnapshot, data.Subsets);

            return doc.Declaration + Environment.NewLine + doc.ToString();
        }

        private static void AddStudyBasicInfo(XElement studyElement, KT_Study study)
        {
            studyElement.Add(new XElement("Name", new XCData(GetSafeValue(study.KT_Name))));

            studyElement.Add(new XElement("Version", GetSafeValue(study.KTR_VersionNumber)));

            studyElement.Add(new XElement("Category", GetSafeValue(study.FormattedValues.ContainsKey(KT_Study.Fields.KTR_StudyCategory)
                ? study.FormattedValues[KT_Study.Fields.KTR_StudyCategory]
                : study.KTR_StudyCategory?.ToString())));
        }

        private static void AddFieldworkMarketInfo(XElement studyElement, KT_Study study)
        {
            studyElement.Add(new XElement("FieldworkMarket", GetSafeValue(study.FormattedValues.ContainsKey(KT_Study.Fields.KTR_StudyFieldworkMarket)
                ? study.FormattedValues[KT_Study.Fields.KTR_StudyFieldworkMarket]
                : study.KTR_StudyFieldworkMarketName)));

            studyElement.Add(new XElement("MaconomyJobNumber", GetSafeValue(study.KTR_FinancialJobNR)));

            studyElement.Add(new XElement("ProjectOperationsURL", new XCData(GetSafeValue(study.KTR_ProjectOperationsUrl))));
        }

        private static void AddFieldworkLanguages(XElement studyElement, IEnumerable<KTR_Language> languages)
        {
            var languagesElement = new XElement("FieldworkLanguages");
            studyElement.Add(languagesElement);

            if (languages != null)
            {
                foreach (var language in languages)
                {
                    languagesElement.Add(new XElement("Language", GetSafeValue(language.KTR_LocaleCode)));
                }
            }
        }

        private static void AddScripterNotes(XElement studyElement, KT_Study study)
        {
            studyElement.Add(new XElement("StudyScripterNotes", new XCData(HtmlGenerationHelper.AddLineBreaksAndFormat(GetSafeValue(study.KTR_ScripTeRNotes)))));
        }

        private static void AddProjectInfo(XElement studyElement, KT_Project project)
        {
            var projectElement = new XElement("Project");
            studyElement.Add(projectElement);

            if (project != null)
            {
                projectElement.Add(new XElement("ProjectName", GetSafeValue(project.KT_Name)));
                projectElement.Add(new XElement("ProjectVersion", GetSafeValue(project.VersionNumber)));
                projectElement.Add(new XElement("Client", new XCData(GetSafeValue(project.KTR_ClientAccountName))));
                projectElement.Add(new XElement("Description", GetSafeValue(project.KT_Description)));
                projectElement.Add(new XElement("Methodology", GetSafeValue(project.FormattedValues.ContainsKey(KT_Project.Fields.KTR_Methodology)
                    ? project.FormattedValues[KT_Project.Fields.KTR_Methodology]
                    : project.KTR_Methodology?.ToString())));
                projectElement.Add(new XElement("CommissioningMarket", GetSafeValue(project.FormattedValues.ContainsKey(KT_Project.Fields.KT_CommissioningMarket)
                    ? project.FormattedValues[KT_Project.Fields.KT_CommissioningMarket]
                    : project.KT_CommissioningMarket?.ToString())));
            }
        }

        #region Keeping commented out incase we need it later - ?? don't know what this is for
        //Keeping commented out incase we need it later
        //private static void AddManagedLists(XElement studyElement, IEnumerable<KTR_ManagedList> managedLists, 
        //    IDictionary<Guid, IList<KTR_ManagedListEntity>> managedListEntitiesGrouped, 
        //    IEnumerable<KTR_SubsetDefinition> studySubsets,
        //    IDictionary<Guid, IList<SubsetEntityWithManagedListEntity>> subsetEntitiesGrouped)
        //{
        //    var managedListsElement = new XElement("ManagedLists");
        //    studyElement.Add(managedListsElement);

        //    if (managedLists != null)
        //    {
        //        foreach (var managedList in managedLists)
        //        {
        //            var managedListElement = new XElement("ManagedList");
        //            managedListsElement.Add(managedListElement);

        //            managedListElement.Add(new XElement("Name", GetSafeValue(managedList.KTR_Name)));
        //            managedListElement.Add(new XElement("Description", GetSafeValue(managedList.KTR_Description)));
        //            managedListElement.Add(new XElement("IsAutoGenerated", GetSafeValue(managedList.KTR_IsAutoGenerated)));
        //            managedListElement.Add(new XElement("SourceType", GetSafeValue(managedList.FormattedValues.ContainsKey(KTR_ManagedList.Fields.KTR_SourceType)
        //                ? managedList.FormattedValues[KTR_ManagedList.Fields.KTR_SourceType]
        //                : managedList.KTR_SourceType?.ToString())));

        //            // Add entities for this managed list
        //            var entitiesElement = new XElement("Entities");
        //            managedListElement.Add(entitiesElement);

        //            if (managedListEntitiesGrouped != null && managedListEntitiesGrouped.ContainsKey(managedList.Id))
        //            {
        //                foreach (var entity in managedListEntitiesGrouped[managedList.Id])
        //                {
        //                    var entityElement = new XElement("Entity");
        //                    entitiesElement.Add(entityElement);

        //                    entityElement.Add(new XElement("EntityCode", GetSafeValue(entity.KTR_AnswerCode)));
        //                    entityElement.Add(new XElement("EntityName", GetSafeValue(entity.KTR_AnswerText)));
        //                    entityElement.Add(new XElement("DisplayOrder", GetSafeValue(entity.KTR_DisplayOrder)));
        //                }
        //            }

        //            // Add subsets for this managed list
        //            var subsetsElement = new XElement("Subsets");
        //            managedListElement.Add(subsetsElement);

        //            if (studySubsets != null)
        //            {
        //                var relatedSubsets = studySubsets.Where(s => s.KTR_ManagedList?.Id == managedList.Id);
        //                foreach (var subset in relatedSubsets)
        //                {
        //                    var subsetElement = new XElement("Subset");
        //                    subsetsElement.Add(subsetElement);

        //                    subsetElement.Add(new XElement("Name", GetSafeValue(subset.KTR_Name)));

        //                    // Add entities for this subset
        //                    var subsetEntitiesElement = new XElement("Entities");
        //                    subsetElement.Add(subsetEntitiesElement);

        //                    if (subsetEntitiesGrouped != null && subsetEntitiesGrouped.ContainsKey(subset.Id))
        //                    {
        //                        foreach (var subsetEntity in subsetEntitiesGrouped[subset.Id])
        //                        {
        //                            var subsetEntityElement = new XElement("Entity");
        //                            subsetEntitiesElement.Add(subsetEntityElement);

        //                            subsetEntityElement.Add(new XElement("EntityCode", GetSafeValue(subsetEntity.EntityCode)));
        //                            subsetEntityElement.Add(new XElement("EntityName", GetSafeValue(subsetEntity.EntityName)));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion

        private static void AddSubsets(
            XElement studyElement,
            IList<KTR_StudySubsetDefinitionSnapshot> studySubsetSnapshots,
            IList<KTR_StudySubsetEntitiesSnapshot> studySubsetEntitiesSnapshots)
        {
            // Create a single <Subsets> container
            var subsetsRootElement = new XElement("Subsets");
            studyElement.Add(subsetsRootElement);

            if (studySubsetSnapshots == null || studySubsetEntitiesSnapshots == null)
            {
                return;
            }

            var subsetList = new List<XmlSubsetFields>();
            var subsetEntitiesList = new List<XmlSubsetEntityFields>();

            foreach (var subset in studySubsetSnapshots)
            {
                if (!subsetList.Where(x => x.SubsetId == subset.KTR_SubsetDefinition2.Id).Any())
                {
                    subsetList.Add(new XmlSubsetFields
                    {
                        SubsetId = subset.KTR_SubsetDefinition2.Id,
                        Name = subset.KTR_Name
                    });

                    var subsetEntities = studySubsetEntitiesSnapshots
                        .Where(e => e.KTR_SubsetDefinitionSnapshot.Id == subset.Id)
                        .ToList();

                    foreach (var entity in subsetEntities)
                    {
                        if (!subsetEntitiesList.Where(x => x.SubsetId == subset.Id).Any())
                        {
                            subsetEntitiesList.Add(new XmlSubsetEntityFields
                            {
                                SubsetEntityId = entity.KTR_SubsetEntities.Id,
                                SubsetId = subset.KTR_SubsetDefinition2.Id,
                                DisplayOrder = entity.KTR_DisplayOrder.GetValueOrDefault(0),
                                AnswerCode = entity.KTR_AnswerCode,
                                AnswerText = entity.KTR_AnswerText
                            });
                        }
                    }
                }
            }

            if (subsetList.Count > 0 && subsetEntitiesList.Count > 0)
            {
                foreach (var subsetItem in subsetList)
                {
                    var subsetElement = new XElement("Subset");
                    subsetsRootElement.Add(subsetElement);

                    subsetElement.Add(new XElement("Name", GetSafeValue(subsetItem.Name)));

                    // Add entities inside subset
                    var subsetEntitiesElement = new XElement("Entities");
                    subsetElement.Add(subsetEntitiesElement);

                    var orderedSubsetEntities = subsetEntitiesList
                        .Where(e => e.SubsetId == subsetItem.SubsetId)
                        .OrderBy(x => x.DisplayOrder);

                    foreach (var subsetEntityItem in orderedSubsetEntities)
                    {
                        var subsetEntityElement = new XElement("Entity");
                        subsetEntitiesElement.Add(subsetEntityElement);

                        subsetEntityElement.Add(new XElement("EntityCode", GetSafeValue(subsetEntityItem.AnswerCode)));
                        subsetEntityElement.Add(new XElement("EntityName", GetSafeValue(subsetEntityItem.AnswerText)));
                    }
                }
            }
        }

        private static void AddQuestionnaireLines(
            XElement studyElement,
            IEnumerable<KTR_StudyQuestionnaireLineSnapshot> questionnaireLinesSnapshot,
            IDictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>> answersSnapshot,
            IList<KTR_StudySubsetDefinitionSnapshot> subsetSnapshots)
        {
            var questionnaireLinesElement = new XElement("QuestionnaireLines");
            studyElement.Add(questionnaireLinesElement);

            if (questionnaireLinesSnapshot != null)
            {
                foreach (var lineSnp in questionnaireLinesSnapshot)
                {
                    var questionnaireLineElement = new XElement("QuestionnaireLine");
                    questionnaireLinesElement.Add(questionnaireLineElement);

                    questionnaireLineElement.Add(new XElement("QuestionVariableName", GetSafeValue(lineSnp.KTR_Name)));
                    questionnaireLineElement.Add(new XElement("QuestionOrder", GetSafeValue(lineSnp.KTR_SortOrder)));
                    questionnaireLineElement.Add(new XElement("StandardOrCustomIndicator", GetSafeValue(lineSnp.KTR_StandardOrCustom)));
                    questionnaireLineElement.Add(new XElement("QuestionTitle", new XCData(GetSafeValue(lineSnp.KTR_QuestionTitle))));
                    questionnaireLineElement.Add(new XElement("QuestionType", GetSafeValue(lineSnp.KTR_QuestionType)));
                    questionnaireLineElement.Add(new XElement("QuestionText", new XCData(GetSafeValue(lineSnp.KTR_QuestionText))));
                    questionnaireLineElement.Add(new XElement("QuestionVersion", GetSafeValue(lineSnp.KTR_QuestionVersion2)));

                    AddScripterNotesSection(questionnaireLineElement, lineSnp);

                    questionnaireLineElement.Add(new XElement("QuestionRationale", GetSafeValue(lineSnp.KTR_QuestionRationale)));
                    questionnaireLineElement.Add(new XElement("AnswerList", new XCData(GetSafeValue(lineSnp.KTR_AnswerList))));

                    // Add individual Answer elements
                    if (answersSnapshot != null && answersSnapshot.ContainsKey(lineSnp.Id) && answersSnapshot[lineSnp.Id].Any())
                    {
                        var answersElement = new XElement("Answers");
                        questionnaireLineElement.Add(answersElement);

                        foreach (var answer in answersSnapshot[lineSnp.Id].OrderBy(a => a.KTR_DisplayOrder))
                        {
                            var answerElement = new XElement("Answer");
                            answersElement.Add(answerElement);

                            answerElement.Add(new XElement("AnswerCode", GetSafeValue(answer.KTR_Name)));
                            answerElement.Add(new XElement("AnswerLocation", GetSafeValue(answer.KTR_AnswerLocation)));
                            answerElement.Add(new XElement("AnswerText", GetSafeValue(answer.KTR_AnswerText)));
                            answerElement.Add(new XElement("DisplayOrder", GetSafeValue(answer.KTR_DisplayOrder)));
                            answerElement.Add(new XElement("IsActive", GetSafeValue(answer.KTR_IsActive)));
                            answerElement.Add(new XElement("IsExclusive", GetSafeValue(answer.KTR_IsExclusive)));
                            answerElement.Add(new XElement("CustomProperty", GetSafeValue(answer.KTR_CustomProperty)));
                            answerElement.Add(new XElement("IsOpen", GetSafeValue(answer.KTR_IsOpen)));
                            answerElement.Add(new XElement("IsTranslatable", GetSafeValue(answer.KTR_IsTranslatable)));
                            answerElement.Add(new XElement("IsFixed", GetSafeValue(answer.KTR_IsFixed)));
                            answerElement.Add(new XElement("SourceId", GetSafeValue(answer.KTR_SourceId)));
                            answerElement.Add(new XElement("SourceName", GetSafeValue(answer.KTR_SourceName)));
                            answerElement.Add(new XElement("EffectiveDate", GetSafeValue(answer.KTR_EffectiveDate)));
                            answerElement.Add(new XElement("Version", GetSafeValue(answer.KTR_Version)));
                        }
                    }

                    if (subsetSnapshots != null && subsetSnapshots.Count > 0)
                    {
                        var subsetsElement = new XElement("Subsets");
                        questionnaireLineElement.Add(subsetsElement);

                        var qlSubsets = subsetSnapshots
                            .Where(x => x.KTR_QuestionnaireLinesNaPsHot.Id == lineSnp.Id)
                            .ToList();

                        foreach (var subset in qlSubsets)
                        {
                            var subsetElement = new XElement("Subset");
                            subsetsElement.Add(subsetElement);

                            subsetElement.Add(new XElement("Name", GetSafeValue(subset.KTR_Name)));

                            if (!string.IsNullOrEmpty(subset.KTR_ManagedListLocation))
                            {
                                subsetElement.Add(new XElement("Location", GetSafeValue(subset.KTR_ManagedListLocation)));
                            }

                            if (!string.IsNullOrEmpty(subset.KTR_ManagedListNameLabel))
                            {
                                subsetElement.Add(new XElement("ManagedListName", GetSafeValue(subset.KTR_ManagedListNameLabel)));
                            }
                        }
                    }

                    questionnaireLineElement.Add(new XElement("IsDummyQuestion", GetSafeValue(lineSnp.KTR_IsDummyQuestion)));
                    questionnaireLineElement.Add(new XElement("Scriptlets", new XCData(GetSafeValue(lineSnp.KTR_Scriptlets))));
                }
            }
        }

        // Scripter Notes - TO BE ADDED
        private static void AddScripterNotesSection(XElement questionnaireLineElement, KTR_StudyQuestionnaireLineSnapshot qlineSnapshot)
        {
            var scripterNotesElement = new XElement("ScripterNotes");
            questionnaireLineElement.Add(scripterNotesElement);

            scripterNotesElement.Add(new XElement("RowSortOrder", GetSafeValue(qlineSnapshot.KTR_RowSortOrder)));

            scripterNotesElement.Add(new XElement("ColumnSortOrder", GetSafeValue(qlineSnapshot.KTR_ColumnSortOrder)));

            scripterNotesElement.Add(new XElement("AnswerMin", GetSafeValue(qlineSnapshot.KTR_AnswerMin)));

            scripterNotesElement.Add(new XElement("AnswerMax", GetSafeValue(qlineSnapshot.KTR_AnswerMax)));

            scripterNotesElement.Add(new XElement("QuestionFormatDetails", new XCData(HtmlGenerationHelper.AddLineBreaksAndFormat(GetSafeValue(qlineSnapshot.KTR_QuestionFormatDetails)))));

            scripterNotesElement.Add(new XElement("Notes", new XCData(HtmlGenerationHelper.AddLineBreaksAndFormat(GetSafeValue(qlineSnapshot.KTR_ScripterNotes)))));

            scripterNotesElement.Add(new XElement("CustomNotes", new XCData(HtmlGenerationHelper.AddLineBreaksAndFormat(GetSafeValue(qlineSnapshot.KTR_CustomNotes)))));
        }

        /// <summary>
        /// Gets a safe string value, handling nulls and converting to string.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Safe string representation</returns>
        private static string GetSafeValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is string stringValue)
            {
                return stringValue ?? string.Empty;
            }

            return value.ToString() ?? string.Empty;
        }
    }
}
