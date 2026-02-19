⭐ EPIC: Project Questionnaire Foundations (Managed Lists → Subsets → Study Creation)
This Epic defines all functionality required to build, maintain, and govern questionnaire data structures across Projects and Studies.

🟧 USER STORY 1 — Managed Lists (Project-Level)
As a Client Service user
I want to create and manage Managed Lists within a Project
So that questions and studies can reuse consistent sets of answer options.
Functional Requirements

A Managed List belongs to one Project and must have a unique name within that Project.
CRUD operations: Create, Read, Update, Deactivate.
Active/Inactive status determines whether the list may be assigned to new questions.
A Managed List may be assigned to multiple questions.
Deactivating a list does not remove or invalidate existing links for already-created studies.
Audit log must record create/update/deactivate actions (user, timestamp, change summary).

Acceptance Criteria

AC‑ML‑01: Creating a Managed List with a duplicate name in the same project is blocked.
AC‑ML‑02: Deactivated lists remain accessible to existing questionnaires but are not selectable for new assignments.
AC‑ML‑03: User can assign the list to questions and see all assignments clearly.
AC‑ML‑04: A full audit trail is preserved.


🟧 USER STORY 2 — Managed List Entities (MLEs)
As a Client Service user
I want to add, modify, activate, or deactivate entities in a Managed List
So that questions referencing the list show the correct answer options.
Functional Requirements

Each entity has: Name, Code (unique within list), optional Sort Order, Active/Inactive status.
Bulk add/update must be supported (paste/import).
Deactivation removes the entity from future use but does not alter existing non‑Draft studies.
Audit must capture all modifications.

Acceptance Criteria

AC‑MLE‑01: Entity Code must be unique per Managed List.
AC‑MLE‑02: Bulk import supports both creation and update with a validation summary.
AC‑MLE‑03: Active entities appear in all Draft studies referencing the list; inactive entities do not.
AC‑MLE‑04: Audit logging is complete.


🟧 USER STORY 3 — Subset Definitions (Auto‑Detection, Auto‑Create, Auto‑Reuse)
As a Client Service user
I want the system to automatically create Subset Definitions when partial selections of a Managed List are applied to a question in a Study
So that subsets are consistent, reusable, and governed without manual list duplication.
Functional Requirements

A Subset Definition belongs to a Study + Managed List combination.
A subset is created whenever the selected entities differ from the full list.
Subset membership signature = sorted list of Selected Entity IDs.
If an identical signature already exists for the Study + ML, system reuses that subset.
Subset names follow pattern {LISTNAME}_SUB{n} with strictly increasing n per Study + ML (no reuse of gaps).
UI for selecting entities must be subtractive-only: users may remove entities but not manually add entities outside governed flows.
Subsets are editable only when the Study is in Draft.

Acceptance Criteria

AC‑SUB‑01: Subset is created automatically on partial selection.
AC‑SUB‑02: Repeated identical selections reuse existing subsets.
AC‑SUB‑03: Naming follows sequential numbering with no reuse.
AC‑SUB‑04: UI subset editing works only in Draft; other states are read‑only.


🟧 USER STORY 4 — Subset Synchronisation & Refresh
As a Client Service user
I want subsets and related displays to update automatically when Subset Definitions or Managed List Entities change
So that questionnaires and Study UI elements are always accurate.
Functional Requirements

Any change to a Subset (create/update/delete) must trigger automatic refresh of:

Question HTML rendering
Study‑level subset summary


MLE changes must reconcile subsets if Study is Draft; otherwise no change until new Study version is created.
All refreshes must be event‑driven, repeatable, and idempotent.

Acceptance Criteria

AC‑SYNC‑01: Subset updates refresh Study summary and question displays automatically.
AC‑SYNC‑02: MLE changes propagate only for Draft studies.
AC‑SYNC‑03: No duplicate or inconsistent subset summaries.


🟧 USER STORY 5 — Auto‑Association to Draft Studies
As a system
I want any new Managed List Entities or newly assigned Managed Lists to automatically propagate to Draft Study questions
So that Draft studies remain synchronized with the latest list content.
Functional Requirements

When a new entity is added to a Managed List, and the Study is Draft, the system must automatically associate that entity with the corresponding questions.
When a Managed List is newly assigned to a question, all active entities must be auto‑associated for all Draft studies containing that question.
If the Study is not Draft, no propagation occurs; a new Study version is required.
Auto‑association must be idempotent (no duplicates).
Must respect subset rules when applicable (subtractive logic).

Acceptance Criteria

AC‑AUTO‑01: Draft Studies automatically receive new entities.
AC‑AUTO‑02: Non‑Draft Studies do not update; change requires new version.
AC‑AUTO‑03: Auto‑association does not create duplicates.


🟧 USER STORY 6 — Study Creation (Versioning + Questionnaire Setup + ML & Subset Copying)
As a Client Service user
I want to create new Studies (and new Study Versions) under a Project
So that I can manage market‑specific questionnaires with full automation, consistency, and governance.
Functional Requirements
A. Study Versioning

When creating a brand‑new Study: Version = 1.
When creating a version from an existing Study: Version = max + 1 within the lineage.
Only one Draft version may exist per lineage.
Version numbers must be unique within a lineage.
A version audit trail must be preserved.

B. Project–Study Synchronisation

Project must maintain an accurate HasStudies flag and Study count.
Updates occur whenever a Study is created/updated/deleted.

C. Automated Questionnaire Setup
When creating a Study, the system must:

Copy all questionnaire lines from:

Project Master Questionnaire (Version 1), OR
Parent Study (Version N).


Preserve:

Question ordering
Active/inactive state


Copy Managed List assignments for each question.
Copy only active entities for Version 1 OR entities active in the parent version for Version N.
Create/reuse Subset Definitions based on entity selections.
Map questions → lists → list entities → subsets without manual intervention.

D. Study Status Restrictions

Questionnaire lines and subset UI are editable only when Study status = Draft.
All editing must be blocked for non‑Draft states.

E. Permissions

When a Study is created, automatically share it and all related records with:

Project Owner
Project Access Team


Permissions must include: Read, Write, Associate, Append.
All associated child records inherit these permissions.

Acceptance Criteria

AC‑STUDY‑01: Version numbers assigned correctly and uniquely.
AC‑STUDY‑02: Only one Draft version allowed; additional Draft is blocked.
AC‑STUDY‑03: Questionnaire Lines are auto‑copied with correct order and status.
AC‑STUDY‑04: Managed Lists, Entities, and Subsets are copied/reused correctly.
AC‑STUDY‑05: Non‑Draft Studies cannot be edited.
AC‑STUDY‑06: Project’s Study indicators updated immediately.
AC‑STUDY‑07: Permissions auto‑share with correct access rights.


⭐ FINAL DEVELOPMENT ORDER (MANDATORY)
To avoid circular dependencies and rework:

US‑1 — Managed Lists
US‑2 — Managed List Entities
US‑3 — Subset Definitions (auto‑create/reuse)
US‑4 — Subset Synchronisation
US‑5 — Auto‑Association Logic for Draft Studies
US‑6 — Study Creation (full automation)

This ordering is the only technically correct sequence that supports Study Creation’s automation.