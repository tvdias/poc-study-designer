```mermaid
flowchart TD

    A[Start: User Initiates Study Creation] --> B{Is this a New Study or New Version?}

    B -->|New Study (V1)| C[Load Project Master Questionnaire]
    B -->|New Version (Vn)| D[Load Parent Study Version]

    C --> E[Copy Questionnaire Lines into Study]
    D --> E

    %% Managed Lists
    E --> F[Copy Managed List Assignments]
    F --> G{V1 or Vn?}

    G -->|V1| H[Include all Active MLEs for each ML]
    G -->|Vn| I[Include only MLEs active in Parent Study]

    %% Subset Logic
    H --> J[Resolve Subset Definitions]
    I --> J

    J --> K{Subset exists in parent?}
    K -->|Yes| L[Reuse Subset via Signature]
    K -->|No| M[Create New SubsetDefinition]

    %% Cleanup invalid MLEs
    L --> N[Subtract invalid MLEs; prevent empty subsets]
    M --> N

    %% Study State Rules
    N --> O[Set Study Status = Draft]
    O --> P{Any other Draft in lineage?}
    P -->|Yes| Q[Block & Show Error: Only One Draft Allowed]
    P -->|No| R[Continue]

    %% Auto-association Logic
    R --> S["Enable Auto-Association for MLE Changes (Draft Only)"]
    S --> T[Enable Draft Editing for QLs & Subsets]

    %% Permissions
    T --> U[Auto-share Study & Child Entities with Project Owner + Access Team]

    %% Project Sync
    U --> V[Update Project: HasStudies, StudyCount, LastModified]

    V --> W[Snapshot-ready structure established]
    W --> X[End]
```