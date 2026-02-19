flowchart TD

    %% Layers
    subgraph UI[Presentation Layer - Model Driven App / Custom Controls]
        A1[Project UI<br/>- Create Project<br/>- View Studies<br/>- Managed Lists]
        A2[Study UI<br/>- Create V1/Vn<br/>- Status Changes<br/>- Read-Only Rules]
        A3[Questionnaire UI <br/>- QL list & detail<br/>- Matrix ML selection<br/>- Subset selector]
    end

    subgraph APP[Application Layer - Business Logic & APIs]
        B1[Study Service<br/>- Create V1 / Vn<br/>- Versioning Logic<br/>- Parent Lineage]
        B2[Questionnaire Engine<br/>- Copy QLs<br/>- Maintain order & activation<br/>- Metadata sync]
        B3[Managed List Engine<br/>- ML CRUD<br/>- MLE CRUD<br/>- ML-to-QL assignment]
        B4[Subset Engine<br/>- Auto-create subsets<br/>- Signature logic<br/>- Sequential naming]
        B5[Auto-Association Engine<br/>- Draft-only propagation<br/>- Add new MLEs<br/>- Remove deactivated]
        B6[Synchronisation Engine<br/>- Refresh HTML<br/>- Rebuild summaries<br/>- Update counts]
        B7[Status & Governance Service<br/>- Draft rules<br/>- Read-only enforcement]
        B8[Snapshot Service<br/>- Lock version<br/>- Capture QLs/MLs/Subsets]
        B9[Permission Service<br/>- Auto-share Study\n+ child entities]
        B10[Project Sync Service<br/>- HasStudies<br/>- StudyCount<br/>- LastModified]
    end

    subgraph DATA[Data Layer - Dataverse Entities]
        C1["(Project)"]
        C2["(Study)"]
        C3["(Study Version Linkage)"]
        C4["(QuestionnaireLine)"]
        C5["(QuestionnaireLine-ManagedList)"]
        C6["(QuestionnaireLine-MLE)"]
        C7["(ManagedList)"]
        C8["(ManagedListEntity)"]
        C9["(SubsetDefinition)"]
        C10["(SubsetEntity)"]
        C11["(Snapshot Tables)"]
    end

    %% UI to App
    A1 --> B1
    A1 --> B3
    A2 --> B1
    A3 --> B2
    A3 --> B4

    %% App to App
    B1 --> B2
    B1 --> B3
    B1 --> B4
    B1 --> B5
    B1 --> B7
    B1 --> B9
    B1 --> B10

    B5 --> B4
    B5 --> B6
    B3 --> B5

    B4 --> B6
    B2 --> B6

    B7 --> B2
    B7 --> B4
    B7 --> B5

    B8 --> B2
    B8 --> B3
    B8 --> B4

    %% App to Data
    B1 --> C2
    B1 --> C3
    B10 --> C1

    B2 --> C4
    B2 --> C5

    B3 --> C7
    B3 --> C8

    B5 --> C6
    B5 --> C4

    B4 --> C9
    B4 --> C10

    B6 --> C4
    B6 --> C2

    B8 --> C11

    B9 --> C2
    B9 --> C4
    B9 --> C7
    B9 --> C9

    %% Data relationships (simple arrows)
    C1 --> C2
    C2 --> C4
    C4 --> C5
    C5 --> C6
    C7 --> C8
    C9 --> C10