# 🟧 USER STORY 4 — **Subset Synchronisation & Refresh**

**(Automatic regeneration of displays, summaries, and mappings when Subsets or Managed List Entities change)**

**As a** Client Service user  
**I want** the system to automatically **refresh, recompute, and update** all Study‑level and Question‑level displays whenever Subset Definitions or Subset Entities change  
**So that** the questionnaire always shows the correct entities and subsets without manual intervention.

***

# 1) Business Value

*   Ensures that Study displays never become stale or incorrect.
*   Eliminates manual refresh or recalculation steps for users.
*   Guarantees that downstream processes (scripting, snapshots, exports) reflect the true, current state.
*   Reduces user confusion in high‑volume projects and large Managed Lists.

***

# 2) Why This User Story Exists

User Story 3 creates Subsets; User Story 5 auto‑propagates MLE additions/removals during Draft.  
But neither of those alone maintains the **UI correctness** nor **Study summaries**.

This story ensures:

*   Question matrices always show the correct (subset-based) list of entities.
*   Study summarisation panels always show correct counts, names, and statuses.
*   Snapshots reference the correct membership.
*   Every single Subset change is visible everywhere instantly.

***

# 3) Scope

### **In scope**

*   Refreshing question‑level HTML displays.
*   Refreshing Study‑level subset summaries.
*   Rebuilding internal summary tables for Subset Entities.
*   Reacting to:
    *   Subset creation
    *   Subset updates
    *   Subset deletion
    *   MLE activation
    *   MLE deactivation
    *   MLE additions (Draft only)
    *   Study version creation
*   Ensuring all views and summaries remain consistent.

### **Out of scope**

*   Creating or updating Subsets (User Story 3).
*   Draft auto‑association logic for new MLEs (User Story 5).
*   Study Creation logic (User Story 6).

***

# 4) Preconditions / Dependencies

*   Managed Lists and MLEs exist (US1–US2).
*   Subset Engine (US3) is implemented — Subset Definitions and membership signatures exist.
*   Study exists with known state (Draft or Not Draft).

***

# 5) Key Concepts

### **5.1 Subset‑Aware Question Display**

A question that references a Subset should show **exactly those MLEs** in UI components:

*   HTML preview
*   Matrix grids
*   Question properties panel

### **5.2 Study Summary**

A Study must maintain a summary of all Subset Definitions, including:

*   The number of subsets per Managed List
*   The number of entities in each subset
*   The exact list of included entities
*   Which questions use which Subset

### **5.3 Synchronisation Event**

A synchronisation occurs when:

1.  A Subset is created
2.  A Subset is reused
3.  A Subset is deleted
4.  A Subset’s membership changes as the result of user action
5.  A new MLE is added (Draft only)
6.  An MLE is deactivated
7.  A new Study version is created (and inherits Subsets)

***

# 6) Functional Requirements (Authoritative)

## **6.1 Automatic Refresh of Question‑Level Displays**

When a Subset changes in any way, for any question referencing it, the system must instantly refresh:

*   The question’s HTML preview
*   The list of visible entities
*   Any per‑entity visual indicators (active/inactive, default, exclusive types, etc.)
*   Any metadata that comes from the MLE or Subset

This refresh must:

*   Be **event‑driven**, not dependent on UI reload
*   Be **idempotent** (multiple triggers produce same final state)

***

## **6.2 Automatic Refresh of Study‑Level Subset Summary**

When a Subset changes or MLEs change, the Study summary must automatically update to show:

*   All subsets for the Study
*   Their names
*   Their membership counts
*   The exact MLE labels
*   The questions that reference each subset
*   Whether the subset is full or partial relative to the Managed List

***

## **6.3 Rebuild Internal Subset Entity Summaries**

The system must maintain internal records that power summary panels.  
Whenever Subset membership changes, the system must recompute:

*   The list of MLE IDs belonging to the Subset
*   The display order of those MLEs
*   Counts (e.g., total entities, active entities)

This must occur automatically on any Subset or MLE change.

***

## **6.4 Respect Study State (Draft vs Not Draft)**

*   If the Study is **Draft**, all Subset and MLE changes may trigger refresh + recalculation.
*   If the Study is **Not Draft**:
    *   Subset/membership cannot be modified.
    *   Synchronisation must ensure **no changes propagate** from MLE updates.
    *   The summary must remain accurate to the current locked version.

***

## **6.5 Synchronisation with Study Versioning**

When a new Study version is created:

*   All Subset Definitions must be copied or inherited appropriately (depending on your architecture).
*   The Subset Summary must be regenerated for the new version.
*   Subset entity displays must refresh to match the versioned dataset.

***

## **6.6 Deletion Behaviour**

If a Subset is deleted:

*   All questions referencing it must revert to **full list** or **new subset** logic (as implemented in US3).
*   All displays must remove references to the deleted Subset.
*   The Study Summary must be updated accordingly.

***

## **6.7 HTML Rendering Rules**

The HTML renderer must support:

*   Displaying only the MLEs in the subset
*   Showing inactive or overridden entities with correct markers
*   Showing ordering based on MLE sort order
*   Showing subset name in question metadata areas

***

# 7) Validation Rules

*   **Subset Existence**: The Subset must exist and be linked to the correct Study + ML.
*   **Membership Validity**: Cannot include inactive or invalid MLEs for the Study’s context.
*   **State Validity**: Reject any attempted change in a non‑Draft Study.
*   **Consistent Summary**: Summary counts must always reflect current membership.
*   **Idempotent Refresh**: Repeated triggers cannot produce duplicate counts or stale HTML.

***

# 8) Acceptance Criteria

### **AC‑SYNC‑01 — Immediate Question‑Level Refresh**

When a Subset is created/updated/deleted, the corresponding question’s HTML and UI displays must update automatically without requiring page reload.

### **AC‑SYNC‑02 — Study Summary Auto‑Rebuild**

All Subset changes update the Study‑level summary panel instantly, showing correct membership counts, MLE names, and question references.

### **AC‑SYNC‑03 — State‑Aware Synchronisation**

If Study ≠ Draft:

*   No Subset membership changes allowed.
*   No propagation of MLE changes into the Study.
*   Displays remain fixed to the Study’s locked state.

### **AC‑SYNC‑04 — MLE Change Reconciliation**

If an MLE is added:

*   Draft → Subset/matrix updated automatically (US5 handles association) and then this US handles display refresh.
*   Not Draft → Displays must remain unchanged.

If an MLE is deactivated:

*   Draft → Subset and question displays must remove or mark the MLE.
*   Not Draft → No change to locked displays.

### **AC‑SYNC‑05 — Delete Subset**

Deleting a Subset must instantly:

*   Remove it from all question displays
*   Remove it from Study summary
*   Remove any question‑subset links
*   Trigger fallback behaviour as defined in US3

### **AC‑SYNC‑06 — No Duplication or Stale Data**

Refresh logic must be idempotent:

*   No double rendering
*   No ghost entities
*   No subset counts mismatches

### **AC‑SYNC‑07 — Version‑Aware Refresh**

When a new Study version is created, all Subset displays and summaries must match the inherited Subset structure.

***

# 9) Test Scenarios (Complete)

### **TS‑01 — Subset Creation Refresh**

Create a Subset for a question → question HTML updates instantly.

### **TS‑02 — Subset Change Refresh**

Remove an entity from a Subset → displays and summary update instantly.

### **TS‑03 — Subset Deletion**

Delete a Subset → all displays must fall back to full list or nearest applicable Subset.

### **TS‑04 — MLE Deactivation in Draft**

Deactivate an MLE used in a Subset → that MLE must disappear from all displays and summaries.

### **TS‑05 — MLE Deactivation in Locked Study**

Deactivate MLE → locked Study shows no change.

### **TS‑06 — MLE Addition in Draft**

Add new MLE to ML → auto‑association happens (US5) → refresh updates HTML & summary.

### **TS‑07 — MLE Addition in Locked Study**

Add MLE → displays remain unchanged until versioning.

### **TS‑08 — Version Creation**

Create new Study version → Subsets and summaries correctly inherited and fully refreshed.

### **TS‑09 — Idempotent Refresh**

Trigger same update twice → displays remain correct, non‑duplicated.

***

# 10) Non‑Functional Requirements

*   **Refresh latency**: < 500ms for typical Studies (≤200 questions).
*   **Batch refresh**: Must handle multiple subsets per question with event deduplication.
*   **Concurrency**: Must support simultaneous updates from multiple users without corruption.
*   **Audit**: All Subset changes logged (create/update/delete).

***

# 11) Data Model Notes

### **SubsetDefinition**

*   Id
*   StudyId
*   ManagedListId
*   Name
*   SignatureHash
*   CreatedOn/By
*   Status

### **SubsetEntities**

*   Id
*   SubsetDefinitionId
*   ManagedListEntityId
*   SortOrder

### **QuestionSubsetLink**

*   QuestionId
*   SubsetDefinitionId
*   StudyId
*   ManagedListId

### **Snapshot Entities**

*   Must persist the SubsetDefinition and its membership exactly at snapshot time.

***

# 12) Implementation Logic (Pseudo‑Code)

```text
onSubsetChanged(subsetId):
    questions = findQuestionsUsingSubset(subsetId)
    for q in questions:
        regenerateQuestionHTML(q)
    regenerateStudySubsetSummary(subset.studyId)

onMLEChanged(mlId, entityId, action):
    studies = findStudiesUsingManagedList(mlId)
    for s in studies:
        if s.status == Draft:
            reconcileSubsetsForStudy(s, mlId)
            refreshAllDisplays(s)
        else:
            continue  // locked, no propagation

onSubsetDeleted(subsetId):
    questions = findQuestionsUsingSubset(subsetId)
    for q in questions:
        clearSubsetLink(q)
        fallbackToFullListOrRecompute(q)
    regenerateStudySubsetSummary(subset.studyId)
```

***

# 13) Security & Permissions

*   Same access rights as Study modify operations.
*   Only users allowed to edit Studies in Draft can trigger Subset changes.
*   Summaries and HTML refresh must respect user read permissions.

***

# 14) Definition of Done (DoD)

*   All ACs and test cases pass.
*   No stale UI or summary inconsistencies found across multiple editing operations.
*   Snapshot/export consistently reflects updated Subset data.
*   Logs and telemetry capture all refresh and recalculation events.
*   Unit, integration, and UI tests implemented.
