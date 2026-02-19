🟧 USER STORY 2 — Managed List Entities (Expanded & Self‑Contained)
Title: Manage Entities inside a Project‑Level Managed List
As a Client Service user
I want to create and maintain the individual Entities (values) inside a Managed List
So that questionnaire questions that reference the list can display and use accurate, structured, and governed answer options.

🎯 Business Value

Ensures that answer options (brands, products, categories, etc.) are consistent across a Project
Reduces scripting errors caused by manual or duplicate entries
Supports automated Study behavior (Draft auto‑association, subset generation, versioning consistency)
Enables large lists to be curated efficiently and safely


📌 Core Concepts
Managed List Entity (MLE)
A Managed List Entity is one item in a list (e.g., “Coca‑Cola” inside a Brand List). Each entity contains:

































FieldDescriptionNameHuman readable label (“Coca‑Cola”)CodeUnique technical key within this Managed List (“COCA_COLA”)Sort OrderOptional explicit ordering overrideStatusActive / InactiveMetadata (optional)Alias, tags, numeric value, language‑specific text, etc.Audit DataCreated by, Modified by, Timestamps, Change summary
MLEs belong to a single Managed List, which belongs to a Project.

🧩 Functional Requirements (Fully Detailed)
1. Create & Update Managed List Entities

Users can add entities individually (UI form) or in bulk.
Each entity must have a unique Code within the same Managed List (case‑insensitive).
Updating an entity must allow changing:

Name
Sort Order
Metadata
Status (Active/Inactive)
but not allow changing the Managed List it belongs to.


Updates must be atomic and validated (no partial save).


2. Activation & Deactivation Rules

Active Entities appear in:

Project master questionnaire selections
Draft studies
Study version creation
Subset detection


Inactive Entities:

Must not appear in new Study versions
Must not appear in Draft auto‑association
Must remain in existing locked/non‑Draft studies unchanged


Deactivation must not retroactively remove the entity from:

Existing Study versions
Subsets belonging to locked versions
Snapshots


Re‑activating an entity makes it available again for new Draft studies and future versions only.


3. Bulk Add / Bulk Update
Bulk add/update must support:

Paste from clipboard (rows of Code, Name, optional Sort Order, optional metadata).
CSV or grid‑based add.
Bulk operations must provide a validation summary listing:

Inserted rows
Updated rows
Skipped rows
Rejected rows (with reasons)


Errors must not block the entire import; instead, partial success is allowed.

Bulk validation rules:

Duplicate Codes within the import batch must be rejected.
Codes conflicting with existing MLEs must update (if allowed) or reject (if user deselects “allow updates”).
Names may duplicate but Codes cannot.


4. Ordering & Display Logic

When Sort Order is provided → use Sort Order.
When Sort Order is empty → order alphabetically by Name.
When both existing and new entities appear together → merge ordering according to rule above.
Ordering must remain stable when MLEs are updated.


5. Relationship Rules (Critical for downstream Study logic)

Every MLE belongs to exactly one Managed List.
When an MLE is created, Draft Studies referencing that list must auto‑associate the new entity (defined in User Story 5).
Changing MLE Status triggers downstream actions (subset refresh, auto‑association logic, etc.).
MLE Name updates must be reflected everywhere it is displayed (questionnaire lines, subset displays, drill‑downs).


6. Validation (Detailed)
The following must be validated on create/update:





































ValidationDescriptionUnique CodeCase‑insensitive uniqueness within the same Managed ListRequired FieldsName and Code are requiredCode FormatAlphanumeric + underscore, starts with letterLength ConstraintsName ≤ 200 chars, Code ≤ 100 chars (configurable)Sort OrderInteger; if invalid, reject rowMetadataMust match schema rules; unknown fields rejectedStatusOnly “Active” or “Inactive” allowed
If any row fails validation during bulk import, user must see detailed feedback.

7. Audit Requirements
Every create/update/deactivate action must include:

Timestamp
User
Operation type
Old value → New value diff
Source (UI form, bulk import, system process)
Correlation ID for bulk operations


8. Non‑Functional Requirements

Bulk operations must handle at least 5,000 rows in one request.
Validation must be streaming, not loaded entire into memory.
Read queries must be optimized (paging, server‑side filtering).
All operations must be idempotent (re-running bulk does not create duplicates).
System must support concurrent modification with optimistic concurrency checks.
Response time:

Single create/update < 300 ms target
Bulk import < 3 seconds for 5k rows


Logging must include failure reason summaries.


🧪 Acceptance Criteria (Expanded and Precise)
AC‑MLE‑01 — Unique Code Enforcement
When creating or updating an MLE,
If the Code already exists in the same Managed List (case‑insensitive),
Then the save must fail with a clear blocking error.
AC‑MLE‑02 — Create MLE
Given a valid Managed List,
When the user provides Code, Name, optional fields,
Then the entity is created, ordered correctly, and appears in the list UI.
AC‑MLE‑03 — Update MLE
When updating Name, Sort Order, metadata, or Status,
Then the entity updates atomically and all display surfaces refresh.
AC‑MLE‑04 — Deactivation Rules
When user deactivates an MLE,
Then:

It disappears from all Draft studies and new version generation.
Existing non‑Draft studies retain the entity.
Subset recalculation is triggered for Draft studies only.

AC‑MLE‑05 — Bulk Add/Update
When performing a bulk import,
Then the system must:

Validate each row independently
Insert valid new MLEs
Update valid existing MLEs
Return a structured summary
Reject invalid rows with detailed reasons

AC‑MLE‑06 — Sorting
When Sort Order is provided,
Then lists reflect Sort Order ascending;
Otherwise alphabetical by Name.
AC‑MLE‑07 — Audit
All create, update, deactivate actions must be logged with required audit attributes.

🧪 Test Scenarios (Exhaustive)
TS‑01 — Create Entity
Input: Code = “COCA_COLA”, Name = “Coca‑Cola”, Status = Active
Expected: Appears in list; ordering respected.
TS‑02 — Duplicate Code Blocked
Input: Code = “Coca_Cola” and existing code “COCA_COLA”
Expected: Duplicate error (case‑insensitive).
TS‑03 — Bulk Add Mixed Valid/Invalid Rows
Input: 10 rows, 2 invalid
Expected:

8 rows created
2 errors listed with explanations
Bulk operation succeeds with partial import

TS‑04 — Deactivation Behavior
Deactivate an entity used in:

Draft Study → disappears and subsets adjust
Locked Study → preserved
Expected: Correct selective propagation.

TS‑05 — Update Sort Order
Change Sort Order of one MLE
Expected: Ordering updates accordingly.
TS‑06 — Metadata Update
Input: Add alias = “Coke”
Expected: Metadata stored and available for downstream processing.
TS‑07 — Concurrency
Two users update same MLE
Expected: Optimistic concurrency error on second write.
TS‑08 — Re‑Activation
Inactive → Active
Expected: Entity reappears for new Draft studies and future versions.

🔧 Implementation Notes (For Developers)
Data Model

Table: ManagedListEntity
Fields: Id, ManagedListId, Name, Code, SortOrder, Status, Metadata (JSON), Audit fields.

Triggers (Event Handlers)


On Create MLE

Validate
Save
Trigger auto‑association for Draft Studies
Trigger subset sync



On Update MLE

If Status changed → trigger downstream logic
If Name changed → refresh display surfaces
If Sort Order changed → reorder list view



On Bulk Import

Stream rows
Validate row‑by‑row
Insert/update
Collect summary
Emit audit log per row



Ordering Algorithm
if SortOrder exists:
    sort by SortOrder ASC
else:
    sort by Name (A→Z)

Idempotency
Ensure that calling bulk import multiple times with the same rows does not create duplicate entries.