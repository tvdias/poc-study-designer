# 🟧 **USER STORY 5 — Auto‑Association of Managed List Entities into Draft Studies**

**(Automatic propagation of Managed List & Entity changes into Draft Study Questions)**

**As a** Client Service user  
**I want** any changes to a Managed List (adding new entities, activating/deactivating entities, or assigning the list to new questions) to **automatically update all Draft Studies**  
**So that** I don’t have to manually re‑select or re‑tick values and I always work with the correct, up‑to‑date list content.

***

# 1) Business Value

*   Ensures that **Draft Studies stay automatically aligned** with the latest Managed List data.
*   Eliminates manual “ticking/unticking” across multiple questions and studies.
*   Prevents human error and list inconsistency when new brands or items are added.
*   Ensures Subset logic (US3) and Subset Synchronisation (US4) operate on fresh data.
*   Supports accurate Study creation, versioning, and downstream scripting.

***

# 2) Why This User Story Exists

Managed Lists may change **after** a Study is created.  
Users expect Draft studies to always reflect the **latest** list and entity set.

Examples:

*   Someone adds new brands.
*   Someone assigns a Managed List to a question that already exists in a Study.
*   Someone deactivates an entity that should no longer appear.

This story defines **the automatic behaviour** that keeps Draft studies correct.

***

# 3) Scope

### In scope

*   Auto‑adding newly created MLEs to all impacted questions in **Draft** Studies.
*   Auto‑deactivating MLEs in Draft Studies (so they disappear from selection/HTML).
*   Auto‑associating lists to questions when a ML is newly assigned.
*   Triggering Subset recalculation (via US3).
*   Triggering Study display refresh (via US4).

### Out of scope

*   Subset creation or reuse logic (covered in US3).
*   Subset/HTML/summary recalculation (covered in US4).
*   Study Version creation (US6).

***

# 4) Preconditions / Dependencies

*   Managed Lists and Entities exist (US1–US2).
*   Subset Engine implemented (US3).
*   Subset Synchronisation implemented (US4).
*   Study has a status: **Draft** or **Not Draft**.

***

# 5) Key Concepts

### **5.1 Draft‑Only Propagation**

Only **Draft** Studies should react to ML/MLE changes.

### **5.2 Event‑Driven**

Updates must occur automatically on events (create/update/deactivate/assign), not on page reload.

### **5.3 Study Context Awareness**

The Study might have Study‑level filtering or Subset Definitions already in place — association logic must respect that.

***

# 6) Functional Requirements (Authoritative Specification)

## **6.1 Auto‑Association When a New MLE Is Added**

When a user adds a new Managed List Entity:

1.  Find all **Draft Studies** that reference this Managed List.
2.  For each such Study, find all **questions** in the Study that reference this Managed List.
3.  Automatically add the new MLE as **selected/available** for each question.
4.  Trigger Subset recalculation (if the question uses Subsets).
5.  Trigger UI refresh (covered in US4).

Rules:

*   No duplicate associations.
*   Maintain MLE Sort Order.
*   Apply subtractive logic: if the Study or question previously used a Subset, the new MLE is **added to the eligible pool** but not necessarily to the Subset selection (US3 governs that).

***

## **6.2 Auto‑Association When a Managed List Is Assigned to a Question**

When a user newly assigns a Managed List to a question:

1.  Identify all **Draft Studies** containing that question.
2.  For each such Study, auto‑associate **all active MLEs** to that question.
3.  Trigger Subset recalculation for this question.
4.  Trigger Study UI refresh.

Rules:

*   Full list is applied initially; users may later narrow it via Subset selection.
*   Inactive MLEs NEVER auto‑associate.

***

## **6.3 Handling MLE Deactivation**

When an MLE is **deactivated**:

*   If Study = **Draft**:
    *   Remove MLE from all question associations in that Study.
    *   Update Subset membership for any Subset that contained the entity.
    *   Trigger Study UI refresh.

*   If Study ≠ Draft:
    *   Do **not** update the Study.
    *   Deactivated MLE remains visible exactly as in the locked version.

***

## **6.4 Handling MLE Reactivation**

When an MLE is activated:

*   If Study = Draft:
    *   Auto‑associate it to the Study‑level template and question‑level selections per rules.
    *   Rebuild Subset selections using subtractive logic (user may remove, but system may not “force add”).
    *   Trigger UI refresh.

*   If Study ≠ Draft:
    *   No change; new Study version picks it up.

***

## **6.5 Handling MLE Updates (Name, Sort Order, Metadata)**

Any non‑structural change (renaming, metadata changes):

*   Must update the Study’s display and Summary Panel automatically.
*   Must not create or remove associations.
*   Must reflect instantly in HTML previews.

***

## **6.6 No Propagation to Non‑Draft Studies**

If a Study is **not Draft**, auto‑association must be entirely suppressed:

*   No adding MLEs
*   No removing MLEs
*   No Subset recalculations
*   No cascade refresh

Changes become visible **only** when new Study Version is created (US6).

***

## **6.7 Idempotency**

*   The system must never create duplicate links if the event fires multiple times.
*   Repeated triggers must not change the final state if nothing changed.

***

## **6.8 Bulk Operations**

If many MLEs are added or deactivated at once:

*   Logic must run in **batch mode**.
*   A single batched update must occur per Study and per question.
*   Refresh operations consolidated into a single UI update invocation.

***

# 7) Validation Rules

*   Cannot auto‑associate inactive entities.
*   Cannot auto‑associate entities to questions not referencing the ML.
*   Cannot update Studies not in Draft.
*   Must ensure entity type compatibility if MLs have types/tags.
*   Must ensure Subset Definitions remain valid post‑reconciliation.
*   Must preserve the question’s existing Subset selection where possible.

***

# 8) Acceptance Criteria (AC)

### **AC‑AUTO‑01 — New MLE Auto‑Propagation (Draft Only)**

When a new MLE is created, all Draft Studies referencing the ML must automatically include that entity for all relevant questions.

### **AC‑AUTO‑02 — ML‑to‑Question Linking Auto‑Propagation**

When a Managed List is assigned to a question, Draft Studies must receive the entire set of active entities for that list.

### **AC‑AUTO‑03 — Deactivation Behaviour**

If an MLE is deactivated, Draft Studies must immediately remove it from all questions and subsets; locked Studies must remain unchanged.

### **AC‑AUTO‑04 — Reactivation Behaviour**

If an MLE is reactivated, Draft Studies must make it available again for selection and propagate it to templates and new Subsets where appropriate.

### **AC‑AUTO‑05 — No Cross‑Study Pollution**

Changes must only apply to Draft Studies referencing the affected ML/entity — no accidental updates to other Studies.

### **AC‑AUTO‑06 — No Duplication**

After auto‑association, each question must contain exactly one link to each valid MLE — no duplicates.

### **AC‑AUTO‑07 — Trigger Downstream Logic**

Auto‑association must trigger:

*   Subset recalculation and re‑signature
*   Subset synchronisation
*   Full display refresh  
    … with **no manual action** required.

### **AC‑AUTO‑08 — State‑Respecting Behaviour**

If Study ≠ Draft, entity assignment, removal, activation, or deactivation must produce no changes.

***

# 9) Test Scenarios (Exhaustive)

### **TS‑01 — New MLE Added**

Draft Study → new brand added → immediately appears in that Study’s relevant questions.

### **TS‑02 — New ML Assignment to Question**

Link ML to question → Draft Studies all inherit the full entity list.

### **TS‑03 — Deactivated MLE in Draft**

Deactivate MLE → removed from question displays + removed from Subsets.

### **TS‑04 — Deactivated MLE in Locked Study**

Deactivate MLE → locked Study remains unchanged.

### **TS‑05 — Reactivated MLE**

Entity set inactive → active → reappears in Draft Study displays.

### **TS‑06 — Bulk Add**

Add 100 entities → all appear across Draft Studies in batches, with a single UI refresh.

### **TS‑07 — Idempotency**

Add same MLE again or re‑fire event → no duplicates.

### **TS‑08 — Multi‑Question Studies**

Only questions referencing the same ML update; others remain untouched.

***

# 10) Non‑Functional Requirements

*   **Performance**:
    *   ≤ 2 seconds for 100 new MLEs across 10 Draft Studies.
    *   Batch updates required.

*   **Concurrency**:
    *   Must handle simultaneous updates to ML/MLE without corrupting Study state.

*   **Atomicity**:
    *   Either all Draft Studies update correctly or none do.

*   **Auditability**:
    *   All auto‑updates logged with before/after state.

***

# 11) Data Model Notes

### **Study Managed List Entities**

Stores the association of MLEs to Study questions.

### **Subset Definitions / Entities**

Used to recalc subsets after auto‑association.

### **QuestionnaireLine links**

Each question has a link to its MLEs; we update these.

***

# 12) Pseudo‑Code (Implementation‑Ready)

```text
onMLECreate(mlId, mleId):
    studies = findDraftStudiesUsingManagedList(mlId)
    for study in studies:
        questions = findQuestionsUsingManagedList(study, mlId)
        bulkAssociate(mleId, questions)
        recalcSubsetsForStudy(study, mlId)
        refreshStudyDisplays(study)

onMLEDeactivate(mlId, mleId):
    studies = findDraftStudiesUsingManagedList(mlId)
    for study in studies:
        questions = findQuestionsUsingManagedList(study, mlId)
        bulkRemove(mleId, questions)
        recalcSubsetsForStudy(study, mlId)
        refreshStudyDisplays(study)

onManagedListQuestionLink(questionId, mlId):
    studies = findDraftStudiesContainingQuestion(questionId)
    for s in studies:
        mles = findAllActiveMLEs(mlId)
        bulkAssociate(mles, [questionId])
        recalcSubsetsForStudy(s, mlId)
        refreshStudyDisplays(s)
```

***

# 13) Security & Permissions

*   Only users who can edit Studies can trigger these changes (directly or indirectly).
*   Read‑only users cannot trigger Subset or association changes.
*   Audit every automatic update with a consistent correlation ID.

***

# 14) Definition of Done (DoD)

*   All acceptance criteria and test cases pass.
*   No Draft Study becomes stale after ML/MLE changes.
*   No locked Study is modified.
*   No duplicate associations exist.
*   Subset and display updates occur automatically.
*   All logs, telemetry, and audit entries validated.
*   Performance validated with realistic datasets.
