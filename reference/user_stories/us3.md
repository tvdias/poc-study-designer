# 🟧 USER STORY 3 — **Subset Definitions** (Auto‑Detect • Auto‑Create • Auto‑Reuse)

**Title**: Automatically detect and govern **Subsets** of Managed List Entities at **Study + Question** level  
**As a** Client Service user  
**I want** the system to **automatically create or reuse** a **Subset Definition** whenever a question in a Study uses **only part** of a Managed List  
**So that** I can reuse consistent, governed sublists without duplicating managed lists or maintaining per‑question lists manually.

***

## 1) Business Value

*   Eliminates duplicate lists; enforces consistency and reuse.
*   Reduces manual effort and data errors during Study setup.
*   Guarantees deterministic behaviour for scripting, exports, snapshots, and versioning.

***

## 2) Core Concepts (Definitions)

*   **Managed List (ML)**: A Project‑scoped collection of entities (e.g., brands).
*   **Managed List Entity (MLE)**: One item in a ML (e.g., “Coca‑Cola”).
*   **Subset Definition (Subset)**: A **Study + Managed List** scoped record that represents a **partial selection** of ML entities, usable by one or more questions in the same Study.
*   **Membership Signature**: Deterministic hash of the **sorted unique** MLE IDs that form the Subset. Ensures that identical sets produce the same signature.
*   **Template vs Question refinement**: Study‑level template restricts the universe of eligible entities; Question‑level refinement applies an exact subset to a specific question.

***

## 3) Scope

**In scope**

*   Detect **full** vs **partial** selections.
*   Create or reuse Subset based on **Membership Signature**.
*   Deterministic **naming** and UI **governance** (no manual edits).
*   **Draft‑only** editability; read‑only when Study is not Draft.
*   Ready for downstream **snapshot/export** usage.

**Out of scope**

*   Creating/updating Managed Lists or MLEs (covered in US‑1/US‑2).
*   Draft auto‑association logic (US‑5).
*   Study Creation workflow and versioning (US‑6).

***

## 4) Preconditions / Dependencies

*   US‑1 (Managed Lists) and US‑2 (MLEs) implemented and available.
*   The question is already linked to a Managed List at Project/Study scope.
*   Study has a state: **Draft** or **Not Draft** (e.g., Ready for Scripting, Approved, etc.).

***

## 5) Functional Requirements (Authoritative)

### 5.1 Subset Detection (Full vs Partial)

*   On saving a selection for a **question** in a **Study**:
    *   If the selection equals the **entire active** set for that Study + ML → **no Subset** required; clear any prior Subset link for that question.
    *   If the selection is a **proper subset** → compute the **Membership Signature** and proceed to create or reuse.

### 5.2 Membership Signature (Deterministic)

*   Build signature by sorting and de‑duplicating the selected MLE IDs and hashing the sequence.
*   Signature scope is **Study + ML**; identical sets in different Studies produce different Subsets.

### 5.3 Create vs Reuse Behaviour

*   **Reuse**: If a Subset already exists for the **same Study + ML** with the **same signature**, link the question to that Subset (do not create a new one).
*   **Create**: If not found, create a new Subset with the computed signature, then link the question to it.

### 5.4 Naming and Governance

*   Persisted name must follow **`{PARENT_LIST_NAME}_SUB{n}`**, where `n` increases **sequentially per Study + ML**.
*   **No gap reuse**: once a number is used, never reassign it (even if earlier Subsets are deleted).
*   **No manual edits**: Subset name and signature are server‑controlled; fields locked on UI forms.
*   Optional UI convenience aliases (e.g., “SUB1/SUB2”) may be displayed but are derived, not stored.

### 5.5 Editability by Study State

*   **Draft**: Matrices/forms are editable; users can **remove** entities (subtractive‑only) to adjust the selection, triggering Subset recalculation.
*   **Not Draft**: Matrices/forms are **read‑only**; changes require creating a **new Study version**.

### 5.6 One Subset per Question+ML

*   A question can reference **at most one** Subset per Managed List in the same Study.

### 5.7 Study‑Level & Question‑Level Views

*   **Study template view**: defines the eligible entities for the Study across multiple questions.
*   **Question refinement view**: derives or reuses Subsets for the exact selection a question uses.

### 5.8 Triggers & Refresh

*   On **save** of a selection: detect → create/reuse → link → **refresh** question HTML preview and Study subset summaries (idempotent; no duplicate renders).
*   On **MLE add/deactivate**:
    *   If Study is Draft, reconcile selections according to US‑5 (auto‑association) and then recompute Subset(s).
    *   If Study is not Draft, no change until a new Study version is created.

### 5.9 Snapshot Readiness

*   Subset Definitions and memberships must be persisted in a way that snapshots and exports can include the exact Subset used by each question.

***

## 6) Validation Rules

*   **Entity validity**: All selected MLE IDs must exist and belong to the linked Managed List; any invalid IDs block the save with a clear message.
*   **Active context**: Selected MLEs must be valid for the Study’s context; deactivated MLEs cannot enter new Subsets (handled at save).
*   **No empty subsets**: If the selection becomes empty, the user must choose the full list or select at least one MLE; do not persist empty Subsets.
*   **Single Subset link**: A question cannot link to more than one Subset for the same ML in the same Study.
*   **Study state rule**: Reject writes if Study is not Draft; show a clear read‑only message.

***

## 7) Acceptance Criteria (AC)

*   **AC‑SUB‑01 (Full vs Partial)**  
    Saving a **full** selection clears any Subset link; saving a **partial** selection creates or reuses a Subset for that Study + ML.

*   **AC‑SUB‑02 (Signature Reuse)**  
    Two questions in the **same Study** referencing the **same ML** and selecting the **same MLE set** both link to the **same Subset** (reuse).

*   **AC‑SUB‑03 (Sequential Naming, No Gaps)**  
    Newly created Subsets for the same Study + ML follow the **`{LIST}_SUB{n}`** pattern with strictly increasing `n` and **no gap reuse**; Subset name is not user‑editable.

*   **AC‑SUB‑04 (Draft‑Only Editing)**  
    If Study ≠ Draft, attempts to change selection are blocked and matrices are read‑only; a new Study version is required to change subsets.

*   **AC‑SUB‑05 (Refresh UI & Summaries)**  
    After Subset create/reuse, the **question HTML** and **Study subset summaries** update immediately and consistently.

*   **AC‑SUB‑06 (Snapshot Persistency)**  
    Subset name and membership used by each question is persisted and included in downstream snapshot/export processes.

***

## 8) Test Scenarios (Gherkin‑style)

**TS‑01 — Create First Subset**

    Given Study S is Draft and question Q uses ML "BRANDS"
    When the user selects 8 of 20 entities and saves
    Then system creates "BRANDS_SUB1" with that signature
    And links Q → BRANDS_SUB1
    And refreshes the question HTML and Study summaries

**TS‑02 — Reuse Existing Subset for Another Question**

    Given "BRANDS_SUB1" (8 entities) exists in Study S
    When another question Q2 selects the same 8 entities and saves
    Then Q2 is linked to "BRANDS_SUB1" (no new subset created)

**TS‑03 — Sequential Naming Without Gap Reuse**

    Given "BRANDS_SUB1" and "BRANDS_SUB3" exist for Study S + ML BRANDS
    When a new unique partial selection is saved
    Then a new subset "BRANDS_SUB4" is created (not SUB2)

**TS‑04 — Full Selection Clears Subset**

    Given question Q is currently linked to "BRANDS_SUB1"
    When the user changes selection to all 20 entities and saves
    Then the Subset link is cleared (Q uses full list)

**TS‑05 — Read‑Only When Not Draft**

    Given Study S is Ready for Scripting
    When the user attempts to change subset membership
    Then the operation is blocked with a read‑only message

**TS‑06 — Snapshot Persistency**

    Given question Q uses "BRANDS_SUB2"
    When a Study snapshot is generated
    Then the snapshot includes the Subset name and the exact membership

***

## 9) Non‑Functional Requirements

*   **Performance**
    *   Subset detect/create/reuse path: target < 300 ms per save on typical Study sizes.
    *   Refresh (question HTML + summaries): target < 500 ms for up to 200 questions.

*   **Determinism & Idempotency**
    *   Signature and naming must be stable across environments.
    *   Re‑saving the same selection must not create duplicates or additional refresh renders.

*   **Scalability**
    *   Support > 5,000 MLEs per ML with paged selection UIs.
    *   Efficient signature lookup (indexed by StudyId, ManagedListId, SignatureHash).

*   **Observability**
    *   Emit telemetry for: operation type, studyId, managedListId, subsetId (if any), duration, result (created/reused/cleared).

***

## 10) Data Model Notes

*   **SubsetDefinition**
    *   Fields: Id, StudyId, ManagedListId, Name (`{LIST}_SUB{n}`), SignatureHash, Status, CreatedBy, CreatedOn.
*   **QuestionSubsetLink**
    *   Fields: Id, StudyId, QuestionnaireLineId, ManagedListId, SubsetDefinitionId.
    *   Constraint: One link per (QuestionnaireLineId, ManagedListId).
*   **Snapshot Artefacts**
    *   Must record the SubsetDefinition used by each QuestionnaireLine at time of snapshot, including its membership (entity IDs).

***

## 11) Pseudo‑Logic (Implementation‑Ready)

```text
function buildSignature(selectedEntityIds):
  ids = sort(unique(selectedEntityIds))
  return hash(ids)

function nextSubsetName(studyId, managedListId):
  n = maxSuffixNumberForStudyAndList(studyId, managedListId) + 1
  return parentListName(managedListId) + "_SUB" + n

onSelectionSave(studyId, questionId, managedListId, selectedEntityIds):
  assert isDraft(studyId)                               // block if not Draft

  if equals(selectedEntityIds, fullActiveSet(studyId, managedListId)):
     unlinkQuestionSubset(questionId, managedListId)    // clear subset link
     refreshDisplays(studyId, questionId)
     return

  sig = buildSignature(selectedEntityIds)
  subset = findSubsetBySignature(studyId, managedListId, sig)

  if subset == null:
     name = nextSubsetName(studyId, managedListId)
     subset = createSubset(studyId, managedListId, name, sig)

  linkQuestionToSubset(questionId, managedListId, subset.id)
  refreshDisplays(studyId, questionId)
```

**Notes**

*   `refreshDisplays` updates question HTML and Study summaries (idempotent).
*   `fullActiveSet` respects Study template constraints (entities eligible for that Study).
*   Writes are atomic; failures leave prior state intact.

***

## 12) Error Handling & Messaging

*   **Study Not Editable**: “This study is read‑only. Create a new version to edit subsets.”
*   **Empty Selection**: “Select at least one entity or choose ‘Use full list’.”
*   **Invalid IDs**: “One or more selected items are invalid for this study.”
*   **Conflicting Links**: “This question already references a subset for this list.”

***

## 13) Security & Audit

*   Only users with edit rights on the Study may modify subsets (Draft only).
*   SubsetDefinition and QuestionSubsetLink inherit sharing from Study/Project.
*   Audit: user, timestamp, operation (create/reuse/clear), old→new membership (diff), correlationId.

***

## 14) Migration & Backfill (if applicable)

*   For existing Studies with partial selections, recalculate signatures and insert SubsetDefinitions and QuestionSubsetLinks to reflect current usage.
*   Assign names using the sequential `{LIST}_SUB{n}` rule per Study + ML, preserving effective membership.

***

## 15) Feature Flags / Configuration

*   `subset.signature.algorithm` (default: SHA‑256)
*   `subset.ui.aliases.enabled` (default: true; purely presentational)
*   `subset.readonly.whenNotDraft` (default: true)

***

## 16) Definition of Done (DoD)

*   All ACs pass in DEV/TEST with representative data volumes.
*   Telemetry and audit entries are emitted and observable.
*   Snapshot/export includes Subset name and exact membership.
*   Documentation for users and runbooks for support are updated.
