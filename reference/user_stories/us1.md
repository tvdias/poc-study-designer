## 🟧 USER STORY 1 — Project‑Level **Managed List** (Create & Govern)

**Title**: Create and manage Project‑level Managed Lists  
**As a** Client Service (CS) user  
**I want** to create and manage **Managed Lists** (e.g., Brands, Categories) at **Project** scope  
**So that** questions can reference consistent, reusable answer sets across all Studies in the Project.

### Business Value

*   Reuse and governance of answer options
*   Lower data maintenance overhead
*   Fewer scripting errors due to inconsistent lists

### In Scope

*   CRUD for Managed Lists under a Project
*   Assign/unassign Managed Lists to questions
*   Active/Inactive behavior and audit trail
*   Search, filter, paging for large lists

### Out of Scope

*   Study propagation (covered in US 3)
*   Subset creation (covered in US 4/5)

### Assumptions

*   Project exists and user has permission to edit it
*   Managed List name must be unique within a Project

### Functional Rules

1.  **Create/Update/Delete/Deactivate** a Managed List under a Project.
2.  **Unique Name per Project**: block duplicates at creation/update.
3.  **Active/Inactive**: inactive lists are hidden from new assignments but **do not** break existing references.
4.  Questions may be linked to Managed Lists from either the **Managed Lists** screen or the **Questionnaire** screen.
5.  **Audit** every change (who/when/what).

### Acceptance Criteria

*   **AC‑ML‑01**: Create a Managed List with a unique name; duplicate name in same Project is blocked with a clear error.
*   **AC‑ML‑02**: Deactivating a Managed List hides it from new assignments but preserves existing question/study references.
*   **AC‑ML‑03**: Users can assign/unassign a Managed List to questions from both the Managed Lists screen and Questionnaire screen.
*   **AC‑ML‑04**: Audit entries exist for create/update/deactivate actions.
*   **AC‑ML‑05**: List view supports search, filter, and paging with sensible defaults.

### Test Scenarios

*   **TS‑ML‑01**: Create “BRANDS\_2026”, assign to two questions, verify relationships.
*   **TS‑ML‑02**: Attempt duplicate name; expect blocking validation.
*   **TS‑ML‑03**: Deactivate the list; verify it’s not selectable for new assignments, existing references remain intact.

### Non‑Functional

*   Pagination default 50 rows; server‑side filtering.
*   All writes idempotent; concurrency conflicts return a friendly retry message.
*   Telemetry: create/update/deactivate counts; assignment events.

### Implementation Notes (Pseudo‑logic)

```pseudo
onManagedListCreate(projectId, name):
  assertUnique(projectId, name)
  create ML { projectId, name, status=Active, audit }

onManagedListDeactivate(mlId):
  set ML.status = Inactive
  // do not touch existing references

assignManagedListToQuestion(mlId, questionId):
  create Question_ManagedList(mlId, questionId)
```
