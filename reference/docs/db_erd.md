```mermaid
erDiagram

    PROJECT ||--|{ STUDY : "has many"
    PROJECT ||--|{ MANAGED_LIST : "defines"
    PROJECT {
        string ProjectId PK
        string Name
        bool HasStudies
        int StudyCount
        datetime LastStudyModifiedOn
    }

    STUDY ||--|{ QUESTIONNAIRE_LINE : "contains"
    STUDY ||--|{ SUBSET_DEFINITION : "has"
    STUDY ||--|| STUDY : "parent version (lineage)"
    STUDY {
        string StudyId PK
        string ProjectId FK
        int VersionNumber
        string Status  // Draft, ReadyForScripting, Approved
        string ParentStudyId FK
        datetime CreatedOn
    }

    QUESTIONNAIRE_LINE ||--|{ QUESTION_ML_LINK : "uses ML"
    QUESTIONNAIRE_LINE {
        string QLId PK
        string StudyId FK
        string QuestionCode
        int SortOrder
        bool IsActive
    }

    MANAGED_LIST ||--|{ MLE : "has entities"
    MANAGED_LIST ||--|{ QUESTION_ML_LINK : "assigned to Qs"
    MANAGED_LIST {
        string ManagedListId PK
        string ProjectId FK
        string Name
        bool IsActive
    }

    MLE {
        string MLEId PK
        string ManagedListId FK
        string Code
        string Name
        int SortOrder
        bool IsActive
    }

    QUESTION_ML_LINK ||--|{ QL_MLE_LINK : "Draft associations"
    QUESTION_ML_LINK ||--|| SUBSET_DEFINITION : "may use subset"
    QUESTION_ML_LINK {
        string QuestionMLLinkId PK
        string QLId FK
        string ManagedListId FK
    }

    QL_MLE_LINK {
        string QLMLEId PK
        string QLId FK
        string MLEId FK
        bool IsActiveInStudy
    }

    SUBSET_DEFINITION ||--|{ SUBSET_ENTITY : "membership"
    SUBSET_DEFINITION {
        string SubsetId PK
        string StudyId FK
        string ManagedListId FK
        string Name // LIST_SUBn
        string SignatureHash
    }

    SUBSET_ENTITY {
        string SubsetEntityId PK
        string SubsetId FK
        string MLEId FK
    }
```