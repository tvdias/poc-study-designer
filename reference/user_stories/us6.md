# 🟧 **USER STORY 6 — Study Creation**

**(Versioning • Questionnaire Setup • Managed Lists & Subsets • Synchronisation • Permissions • Audit)**

**As a** Client Service user  
**I want** to create a **new Study** (and **new Study Versions**) under a Project  
**So that** I can manage market‑specific questionnaires with correct versioning, automated setup, governed answer lists/subsets, consistent state rules, and collaborative permissions — ready for downstream scripting and exports.

***

## 1) Business Value

*   **Speed & correctness**: one action creates a fully‑formed Study or Study Version with the right questions, lists, and subsets.
*   **Traceability & compliance**: built‑in versioning, audit, and snapshot readiness.
*   **Governance & consistency**: enforces state rules, read‑only behaviour, and deterministic data structures.
*   **Collaboration**: automatic sharing with the Project Owner & Access Team.

***

## 2) Scope

**In scope**

*   Create **new Study (Version 1)** and **new Study Version (Version N>1)**.
*   Auto‑copy **questionnaire lines**, **managed lists**, **managed list entities**, **subsets**, and **mappings**.
*   **Project–Study synchronisation** (has studies, count).
*   **State model** (Draft vs Not Draft) and **Draft‑only editing**.
*   **Permissions** (auto‑share) and **audit/snapshot readiness**.
*   **Performance** targets and **idempotency**.

**Out of scope**

*   Managed Lists, MLEs, and Subset creation engines themselves (US‑1..US‑4).
*   Draft auto‑association on subsequent ML/MLE changes (US‑5).
*   Scripting/export jobs (consumers of snapshots).

***

## 3) Preconditions / Dependencies

*   Project exists and is active; user has create permission for Studies.
*   Project Master Questionnaire exists and is in a consistent state.
*   Managed Lists, Entities, and Subset Engine (US‑1..US‑4) are available.
*   Auto‑association logic (US‑5) is available to keep **Draft** Studies aligned after creation.

***

## 4) Functional Requirements (Authoritative)

### 4.1 Versioning & Lineage

1.  **Version 1 (new Study)**
    *   When creating a brand‑new Study under a Project → set **Version = 1**.
    *   Set **Status = Draft** by default, with full editability.

2.  **Version N>1 (new Study Version)**
    *   When creating from an existing Study → set **Version = max(existing versions in lineage) + 1**.
    *   Copy data from **parent version** (latest selectable) as base.
    *   Only **one Draft** version is allowed in a lineage at any time (block second Draft with a clear message).

3.  **Uniqueness & Audit**
    *   Version numbers must be unique within a lineage.
    *   Persist **lineage links** (ParentStudyId or MasterStudyId), timestamp, user, and reason/comments.

***

### 4.2 Project–Study Synchronisation

4.  On create/update/delete, automatically update the Project:
    *   **HasStudies** (boolean)
    *   **StudyCount** (integer)
    *   **LastStudyModifiedOn** (timestamp)

***

### 4.3 Questionnaire Setup (Copy/Preserve Rules)

5.  **Source for Version 1**: copy all **active** questions from the **Project Master Questionnaire**.
6.  **Source for Version N**: copy questions from the **parent Study version**, respecting its active/inactive state.
7.  **Preserve question ordering** exactly as per source (stable order keys).
8.  **Active/Inactive propagation**:
    *   If a question is inactive in source → create it inactive in new Study/Version.
    *   If a question was removed in parent Study version → keep it removed in the new version (do not re‑add).
9.  **Question metadata**: copy text, codes/ids, display settings, field properties, routing hooks (as data), scripter notes, rationale, and other attributes exactly.

***

### 4.4 Managed Lists & Entities

10. For each question copied, also copy **Managed List assignments** from the source.
11. **Version 1**: include all **active** MLEs valid for the Study context.
12. **Version N**: include **exactly** the MLEs active in the **parent Study version** for that question (do not expand beyond parent unless governed by Draft auto‑association rules).
13. **Ordering**: preserve Sort Order for MLEs; if missing, default alphabetical by Name.

***

### 4.5 Subsets (Copy/Recompute/Reuse)

14. If the parent used a **Subset** for a given question, the new version must **reference the same Subset signature** (recompute or reuse deterministically).
15. If the parent used **full list**, the new version must start with full list.
16. If a Subset membership is now invalid due to deactivated MLEs, recompute by subtracting invalid entities and keep signature stable; if empty, prompt to use full list (or block with a clear message if empty not allowed).
17. Subset naming: `{LIST}_SUB{n}`; never reuse numbers. Maintain signature determinism.

***

### 4.6 State & Editing Governance

18. **Draft**:

*   Questionnaire lines, list selections, and subset selections are editable.
*   US‑5 applies: new ML/MLE changes propagate automatically while it’s Draft.

19. **Not Draft (e.g., Ready for Scripting, Approved)**:

*   Questionnaire and subset editing is **read‑only**.
*   No propagation from ML/MLE changes (US‑5 suppressed).
*   To change content, user must **create a new Study version**.

***

### 4.7 Permissions (Auto‑Share)

20. On **Study creation** (any version), **auto‑share** the Study and all child records with:

*   **Project Owner**
*   **Project Access Team**

21. Permission set: **Read, Write, Associate, Append** (Delete optional/off by policy).
22. Inherit sharing to all child entities (questionnaire lines, ML assignments, subsets, subset entities, snapshots).

***

### 4.8 Snapshots & Export Readiness

23. Ensure Study and child entities are **snapshot‑ready**: a snapshot must be able to record the exact set of questions, lists, subsets, and memberships at a point in time.
24. Guarantee **referential integrity** and stable identifiers to support scripting/export consumers.

***

### 4.9 Idempotency, Transactions & Error Handling

25. **Idempotent creation**: re‑trying the same “create version” operation must not duplicate lines or associations.
26. **Transactional**: all copied data must be committed atomically; partial writes must be rolled back on failure.
27. **Clear blocking messages**:

*   Another Draft exists → “Only one Draft version is allowed in this Study; finish or abandon the existing Draft first.”
*   Empty subsets after propagation → “The selected subset yielded zero entities; choose full list or adjust selection.”
*   Inconsistent source state → “Cannot create a new version due to inconsistent parent data; please fix the parent version.”

***

## 5) Acceptance Criteria (AC)

*   **AC‑STUDY‑01 — Versioning Rules**  
    Creating a new Study sets **Version = 1**; creating from an existing Study sets **Version = max + 1** with unique numbering and correct lineage tracking.

*   **AC‑STUDY‑02 — One Draft Only**  
    Attempting to create a second Draft version in the same lineage is blocked with a clear message.

*   **AC‑STUDY‑03 — Questionnaire Copy**  
    Version 1 uses the Project Master Questionnaire; Version N uses the parent Study version; both preserve question order, active/inactive status, and metadata.

*   **AC‑STUDY‑04 — Managed Lists & Entities**  
    Questions in Version 1 include active MLEs; Version N reflects the parent’s effective MLE set with correct order and no extraneous additions.

*   **AC‑STUDY‑05 — Subset Consistency**  
    Subsets are reused/recomputed by signature; naming follows `{LIST}_SUB{n}`; invalid members are subtracted; empty subsets are blocked with instructions.

*   **AC‑STUDY‑06 — State Governance**  
    Draft is editable; Not Draft is read‑only; Draft auto‑association applies; Not Draft suppresses propagation.

*   **AC‑STUDY‑07 — Project Sync**  
    Project fields (`HasStudies`, `StudyCount`, `LastStudyModifiedOn`) update correctly on create/update/delete.

*   **AC‑STUDY‑08 — Permissions**  
    Study and child records are auto‑shared with Project Owner & Access Team with Read/Write/Associate/Append, inherited to all child entities.

*   **AC‑STUDY‑09 — Snapshot Readiness**  
    Snapshot of a Study captures the exact questions, lists, subsets, and memberships used, with stable identifiers.

*   **AC‑STUDY‑10 — Idempotent & Transactional**  
    Re‑tries do not duplicate content; failures roll back; user sees clear error messages.

***

## 6) Test Scenarios (Gherkin‑style)

**TS‑01 — Create New Study (V1)**

    Given a Project with a valid Master Questionnaire
    When the user creates a new Study
    Then the Study is created as Version = 1, Status = Draft
    And all active Master questions are copied with preserved order and metadata
    And Project.HasStudies = true and Project.StudyCount increments by 1
    And the Study and its children are shared with Project Owner & Access Team

**TS‑02 — Create New Study Version (V2)**

    Given a Study Version 1 exists and is Not Draft
    When the user creates Version 2
    Then Version = 2 is created with parent lineage
    And all V1 questions and subset references are copied/reused deterministically
    And the Study is editable (Draft)

**TS‑03 — Prevent Second Draft**

    Given a Draft version already exists for a Study
    When the user attempts to create another Draft version
    Then the system blocks with a clear "one draft only" message

**TS‑04 — Subset Reuse & Invalid Members**

    Given V1 question uses a subset that included entities now deactivated
    When V2 is created
    Then V2 reuses the subset signature but subtracts invalid entities
    And if empty, the user is instructed to choose full list or adjust

**TS‑05 — ML/MLE Propagation in Draft**

    Given Version 2 is in Draft
    When a new MLE is added to a linked Managed List (outside Study creation)
    Then the MLE appears in the Draft Study questions automatically (US5)
    And Study displays refresh (US4)

**TS‑06 — Read‑Only in Not Draft**

    Given a Study is Ready for Scripting
    When the user tries to modify questions or subsets
    Then the operation is blocked with a read‑only message

**TS‑07 — Snapshot Readiness**

    Given a Study Version is complete
    When a snapshot is generated
    Then the snapshot includes the exact questions, lists, subsets, and memberships used

**TS‑08 — Idempotent Creation**

    Given a transient error during V2 creation
    When the user retries "create V2"
    Then V2 is created once with no duplicate lines/links

***

## 7) Non‑Functional Requirements

*   **Performance**
    *   Create V1 of a typical Study (≤ 200 questions, ≤ 10 lists, ≤ 2,000 MLE links) in ≤ 5 seconds on average.
    *   Create V2 from V1 with same scale in ≤ 4 seconds on average.
    *   Bulk operations and database/Dataverse interactions use batch APIs where supported.

*   **Scalability**
    *   Handle large Projects (≥ 5,000 MLEs across lists) with paged queries and streaming copy.
    *   Avoid O(n×m) loops; use set‑based operations.

*   **Reliability**
    *   Use transactions; roll back on failure; publish clear errors.
    *   Idempotency tokens for “create version” operations to prevent dupes.

*   **Observability**
    *   Emit telemetry: operation, studyId, parentStudyId, versionNo, duration, counts (questions/links/subsets), outcome.
    *   Correlate with snapshot/export logs for auditing.

***

## 8) Data Model (Informational)

*   **Study**: Id, ProjectId, VersionNo, Status/StatusReason, ParentStudyId/MasterStudyId, CreatedBy/On, Comment/Reason.
*   **StudyQuestionnaireLine**: Id, StudyId, QuestionId (or copied QuestionKey), ActiveFlag, Order, Metadata fields.
*   **StudyQuestionnaireLine–ManagedList**: Id, StudyId, SQL/Dataverse link to ML, display properties.
*   **StudyQuestionnaireLine–MLE**: Id, StudyId, MLEId, Order, ActiveFlag.
*   **SubsetDefinition**: Id, StudyId, ManagedListId, Name `{LIST}_SUB{n}`, SignatureHash.
*   **Question–Subset link**: Id, StudyId, QuestionnaireLineId, SubsetDefinitionId.
*   **Snapshot tables**: capture exact state for scripting/export.

***

## 9) Pseudo‑Logic (Implementation‑Ready)

```text
createStudyV1(projectId, studyFields):
  assert projectIsValid(projectId)
  begin transaction
    study = insert Study { projectId, version=1, status=Draft, lineage=null, ... }
    questions = loadActiveMasterQuestions(projectId)
    copyQuestionsToStudy(study.id, questions)
    copyManagedListLinks(study.id, questions)
    includeActiveMLEsForV1(study.id, questions)                  // full list by default
    resolveAndLinkSubsets(study.id, questions)                    // full or defined subsets (if any)
    autoShareWithProjectTeam(study.id)
    updateProjectCounters(projectId, +1)
  commit

createStudyVersion(parentStudyId, comment?):
  parent = loadStudy(parentStudyId)
  assert noOtherDraftInLineage(parent)
  begin transaction
    versionNo = nextVersionNo(parent.lineage or parent.id)
    study = insert Study { projectId=parent.projectId, version=versionNo, status=Draft, lineage=rootOf(parent), parentStudyId=parent.id, comment }
    questions = loadQuestionsFromParent(parent.id)
    copyQuestionsToStudy(study.id, questions)
    copyManagedListLinks(study.id, questions)
    includeMLEsFromParent(study.id, parent.id, questions)         // preserve effective set
    reuseOrRecomputeSubsets(study.id, parent.id, questions)       // deterministic signature
    cleanInvalidSubsetMembers(study.id)                           // subtract deactivated MLEs; handle empty
    autoShareWithProjectTeam(study.id)
    updateProjectCounters(parent.projectId, 0)                    // count unchanged
  commit
```

**Notes**

*   All “copy” functions must be idempotent and use batched operations.
*   “reuseOrRecomputeSubsets” must rebuild signatures deterministically; do not rename existing Subsets.
*   “cleanInvalidSubsetMembers” removes deactivated MLEs and blocks empty Subsets with guidance.
*   Errors must abort and roll back the transaction.

***

## 10) Error Handling & Messages

*   **Second Draft**: “Only one Draft version is allowed for this Study; finish or abandon the existing Draft first.”
*   **Empty Subset**: “The selected subset yields zero entities; choose full list or adjust selection.”
*   **Inconsistent Parent**: “Cannot create new version due to inconsistent parent data; please correct the parent version.”
*   **Permission**: “You do not have permission to create or edit this Study.”

***

## 11) Security & Permissions

*   **Create/Edit**: restricted to users with Study write privileges.
*   **Auto‑share**: Project Owner & Access Team receive Read/Write/Associate/Append on Study and all children.
*   **Read‑only**: Enforced for Non‑Draft statuses in UI and API.

***

## 12) Definition of Done (DoD)

*   All **ACs** pass with representative datasets.
*   Creation is **idempotent**, **transactional**, and meets performance targets.
*   **Snapshots** include exact questions, lists, subsets, and memberships.
*   **Project counters/sync** correct.
*   **Auto‑share** applied and verified.
*   **Telemetry** and **audit** events present and readable in observability tooling.
*   User and support documentation updated.
