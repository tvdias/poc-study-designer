namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Language;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Project;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetDefinitionSnapshot;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetEntitiesSnapshot;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Service that orchestrates Study XML data collection and generation.
    /// Coordinates multiple repository calls to build complete study data model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StudyXMLService : IStudyXMLService
    {
        private readonly IStudyRepository _studyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IManagedListRepository _managedListRepository;
        private readonly ISubsetDefinitionSnapshotRepository _subsetDefinitionSnapshotRepository;
        private readonly ISubsetEntitiesSnapshotRepository _subsetEntitiesSnapshotRepository;
        private readonly ITracingService _tracingService;
        private readonly IQuestionnaireLineSnapshotRepository _questionnaireLineSnapshotRepository;
        private readonly IQuestionnaireLineAnswerSnapshotRepository _answerSnapshotRepository;

        public StudyXMLService(
            IStudyRepository studyRepository,
            IProjectRepository projectRepository,
            ILanguageRepository languageRepository,
            IManagedListRepository managedListRepository,
            IQuestionnaireLineSnapshotRepository questionnaireLineSnapshotRepository,
            IQuestionnaireLineAnswerSnapshotRepository answerSnapshotRepository,
            ISubsetDefinitionSnapshotRepository subsetDefinitionSnapshotRepository,
            ISubsetEntitiesSnapshotRepository subsetEntitiesSnapshotRepository,
            ITracingService tracingService = null)
        {
            _studyRepository = studyRepository ?? throw new ArgumentNullException(nameof(studyRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
            _managedListRepository = managedListRepository ?? throw new ArgumentNullException(nameof(managedListRepository));
            _answerSnapshotRepository = answerSnapshotRepository ?? throw new ArgumentNullException(nameof(answerSnapshotRepository)); 
            _subsetDefinitionSnapshotRepository = subsetDefinitionSnapshotRepository ?? throw new ArgumentNullException(nameof(subsetDefinitionSnapshotRepository));
            _subsetEntitiesSnapshotRepository = subsetEntitiesSnapshotRepository ?? throw new ArgumentNullException(nameof(subsetEntitiesSnapshotRepository));
            _questionnaireLineSnapshotRepository = questionnaireLineSnapshotRepository ?? throw new ArgumentNullException(nameof(questionnaireLineSnapshotRepository));
            _tracingService = tracingService;
        }

        public string GenerateAndStoreStudyXml(Guid studyId)
        {
            var studyXmlData = CollectStudyXmlData(studyId);

            if (studyXmlData == null)
            {
                return string.Empty;
            }

            var generatedXml = XmlGenerationHelper.GenerateStudyXml(studyXmlData);

            _studyRepository.UpdateStudyXml(studyId, generatedXml);

            return generatedXml;
        }

        private StudyXmlData CollectStudyXmlData(Guid studyId)
        {
            var data = new StudyXmlData();

            _tracingService?.Trace($"[StudyXMLService] Fetching study data for studyId: {studyId}");
            var study = _studyRepository.Get(studyId);
            _tracingService?.Trace($"[StudyXMLService] Study data retrieved: {(data.Study != null ? "Success" : "NULL")}");

            if (study.StatusCode == KT_Study_StatusCode.Draft)
            {
                _tracingService?.Trace($"Study {studyId} is in DRAFT, can't generate XML yet.");
                return null;
            }

            data.Study = study;

            _tracingService?.Trace($"[StudyXMLService] Fetching project data for projectId: {data.Study.KT_Project.Id}");
            data.Project = _projectRepository.Get(data.Study.KT_Project.Id);
            _tracingService?.Trace($"[StudyXMLService] Project data retrieved: {(data.Project != null ? "Success" : "NULL")}");

            _tracingService?.Trace($"[StudyXMLService] Fetching languages for studyId: {studyId}");
            data.Languages = _languageRepository.GetStudyLanguages(studyId);
            var languageCount = data.Languages?.Count() ?? 0;
            _tracingService?.Trace($"[StudyXMLService] Languages retrieved: {languageCount} items");

            _tracingService?.Trace($"[StudyXMLService] Fetching questionnaire line SNAPSHOTS for studyId: {studyId}");
            data.QuestionnaireLinesSnapshot =
                _questionnaireLineSnapshotRepository.GetStudyQuestionnaireLineSnapshots(studyId);

            var questionnaireLineSnapshotIds =
                data.QuestionnaireLinesSnapshot?.Select(q => q.Id).ToList() ?? new List<Guid>();

            _tracingService?.Trace($"[StudyXMLService] Fetching ANSWER SNAPSHOTS for {questionnaireLineSnapshotIds.Count} questionnaire line snapshots");
            data.QuestionnaireLineAnswersSnapshot =
                _answerSnapshotRepository.GetAnswersBySnapshotIds(questionnaireLineSnapshotIds);

            _tracingService?.Trace($"[StudyXMLService] Fetching managed lists with entities for studyId: {studyId}");
            data.ManagedListEntitiesGrouped = _managedListRepository.GetStudyManagedListsWithEntities(studyId);
            var managedListEntityGroupCount = data.ManagedListEntitiesGrouped?.Count ?? 0;
            _tracingService?.Trace($"[StudyXMLService] Managed list entity groups retrieved: {managedListEntityGroupCount} groups");

            if (data.ManagedListEntitiesGrouped?.Any() == true)
            {
                var managedListIds = data.ManagedListEntitiesGrouped.Keys.ToList();
                data.ManagedLists = _managedListRepository.GetManagedListsByIds(managedListIds);
            }
            else
            {
                data.ManagedLists = new List<KTR_ManagedList>();
            }
            var managedListCount = data.ManagedLists?.Count() ?? 0;
            _tracingService?.Trace($"[StudyXMLService] Managed lists derived: {managedListCount} unique items");

            _tracingService?.Trace($"[StudyXMLService] Fetching study subsets for studyId: {studyId}");
            data.Subsets = _subsetDefinitionSnapshotRepository.GetStudySubsetSnapshots(studyId);
            var subsetIds = data.Subsets?
                .Select(x => x.KTR_SubsetDefinition2.Id)
                .Distinct()
                .ToList() ?? new List<Guid>();
            _tracingService?.Trace($"[StudyXMLService] Study subsets retrieved: {subsetIds.Count} items");

            _tracingService?.Trace($"[StudyXMLService] Fetching Subset Entities for {subsetIds.Count} Subset definitions");
            var subsetSnapshotIds = data.Subsets?.Select(s => s.Id).ToList() ?? new List<Guid>();
            data.SubsetEntities = _subsetEntitiesSnapshotRepository.GetSubsetEntitiesSnapshots(subsetSnapshotIds);
            _tracingService?.Trace($"[StudyXMLService] Subset entities retrieved: {data.SubsetEntities.Count()}");

            return data;
        }
    }
}
