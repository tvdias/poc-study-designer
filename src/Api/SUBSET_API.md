# Subset Management API

## Overview

The Subset Management feature automatically detects and governs partial selections of Managed List Entities at the Study + Question level. It provides deterministic reuse of identical subsets across multiple questions while enforcing draft-only editing rules.

## Key Concepts

- **Managed List (ML)**: A Project-scoped collection of entities (e.g., brands, categories)
- **Managed List Entity (MLE)**: One item in a Managed List
- **Subset Definition**: A partial selection of ML entities, uniquely identified by a signature hash
- **Membership Signature**: Deterministic SHA-256 hash of sorted unique MLE IDs
- **Sequential Naming**: Subsets follow the pattern `{MANAGED_LIST_NAME}_SUB{n}` where n increases sequentially with no gap reuse

## API Endpoints

### Save Question Selection

**Endpoint:** `POST /api/subsets/save-selection`

Saves a question's selection of managed list items. Automatically detects full vs partial selections and creates or reuses subset definitions.

**Request Body:**
```json
{
  "projectId": "guid",
  "questionnaireLineId": "guid",
  "managedListId": "guid",
  "selectedManagedListItemIds": ["guid", "guid", ...]
}
```

**Response (200 OK):**
```json
{
  "questionnaireLineId": "guid",
  "managedListId": "guid",
  "isFullSelection": false,
  "subsetDefinitionId": "guid",
  "subsetName": "BRANDS_SUB1"
}
```

**Behavior:**
- **Full Selection**: If all active items are selected, clears subset link (subsetDefinitionId = null)
- **Partial Selection**: Creates new subset or reuses existing subset with same signature
- **Reuse**: If another question in the same project uses identical items, returns same subset ID
- **Sequential Naming**: New subsets get next available number (e.g., SUB1, SUB2, SUB3...)

**Validation:**
- Project must exist and be in Draft status
- Questionnaire line must exist
- Managed list must exist and belong to project
- All selected items must be valid and belong to managed list
- Selection cannot be empty

**Error Responses:**
- `400 Bad Request`: Validation errors or non-Draft project
- `404 Not Found`: Project, question, or managed list not found

---

### Get Subset Details

**Endpoint:** `GET /api/subsets/{id}`

Retrieves detailed information about a specific subset definition including all its members.

**Response (200 OK):**
```json
{
  "id": "guid",
  "projectId": "guid",
  "managedListId": "guid",
  "managedListName": "BRANDS",
  "name": "BRANDS_SUB1",
  "signatureHash": "a1b2c3...",
  "status": "Active",
  "members": [
    {
      "managedListItemId": "guid",
      "value": "COCA_COLA",
      "label": "Coca-Cola",
      "sortOrder": 1
    },
    ...
  ],
  "createdOn": "2026-02-18T23:00:00Z",
  "createdBy": "user@example.com"
}
```

**Error Responses:**
- `404 Not Found`: Subset not found

---

### Get Subsets for Project

**Endpoint:** `GET /api/subsets/project/{projectId}`

Lists all subset definitions for a specific project.

**Response (200 OK):**
```json
{
  "subsets": [
    {
      "id": "guid",
      "managedListId": "guid",
      "managedListName": "BRANDS",
      "name": "BRANDS_SUB1",
      "memberCount": 5,
      "createdOn": "2026-02-18T23:00:00Z"
    },
    ...
  ]
}
```

## Business Rules

### Draft-Only Editing
Subset operations can only be performed on projects with status `Draft`. Attempting to modify subsets on non-Draft projects returns:
```
"This project is read-only. Create a new version to edit subsets. Current status: {status}"
```

### Full vs Partial Detection
- **Full Selection**: When all active items from the managed list are selected, no subset is created. The question links directly to the managed list.
- **Partial Selection**: When fewer than all items are selected, a subset definition is created or reused.

### Signature-Based Reuse
Two questions selecting the same items (regardless of order) will share the same subset:
- Question 1 selects items [A, B, C] → Creates BRANDS_SUB1
- Question 2 selects items [C, A, B] → Reuses BRANDS_SUB1 (same signature)
- Question 3 selects items [A, B] → Creates BRANDS_SUB2 (different signature)

### Sequential Naming
- First subset for a managed list: `{LIST}_SUB1`
- Second subset: `{LIST}_SUB2`
- If SUB2 is deleted and a new subset is created: `{LIST}_SUB3` (no gap reuse)
- Naming is per (Project, Managed List) pair - different projects can have SUB1

## Database Schema

### SubsetDefinitions
- Stores subset metadata and signature hash
- Unique constraint on (ProjectId, ManagedListId, SignatureHash)
- Indexed on (ProjectId, ManagedListId) for efficient lookup

### SubsetMemberships
- Links subset definitions to their member items
- Unique constraint on (SubsetDefinitionId, ManagedListItemId)

### QuestionSubsetLinks
- Links questions to subset definitions
- Nullable SubsetDefinitionId (null = full selection)
- Unique constraint on (QuestionnaireLineId, ManagedListId)

## Testing

### Unit Tests
- Signature computation (deterministic, order-independent)
- Validation rules
- All unit tests pass: 260/260 ✓

### Integration Tests
- Partial selection creates subset
- Full selection clears subset
- Same selection reuses subset
- Sequential naming without gaps
- Get subset details
- Get all subsets for project
- Draft-only enforcement

## Examples

### Example 1: First Partial Selection
```
POST /api/subsets/save-selection
{
  "projectId": "project-123",
  "questionnaireLineId": "question-456",
  "managedListId": "brands-789",
  "selectedManagedListItemIds": ["item-1", "item-2", "item-3"]
}

Response:
{
  "isFullSelection": false,
  "subsetDefinitionId": "subset-abc",
  "subsetName": "BRANDS_SUB1"
}
```

### Example 2: Reusing Existing Subset
```
POST /api/subsets/save-selection
{
  "projectId": "project-123",
  "questionnaireLineId": "question-999",  // Different question
  "managedListId": "brands-789",
  "selectedManagedListItemIds": ["item-3", "item-1", "item-2"]  // Same items, different order
}

Response:
{
  "isFullSelection": false,
  "subsetDefinitionId": "subset-abc",  // Same subset ID
  "subsetName": "BRANDS_SUB1"  // Same subset name
}
```

### Example 3: Full Selection
```
POST /api/subsets/save-selection
{
  "projectId": "project-123",
  "questionnaireLineId": "question-111",
  "managedListId": "brands-789",
  "selectedManagedListItemIds": ["item-1", "item-2", ..., "item-20"]  // All 20 items
}

Response:
{
  "isFullSelection": true,
  "subsetDefinitionId": null,  // No subset needed
  "subsetName": null
}
```

## Notes

- Subsets are project-scoped: different projects can have subsets with same name
- Signature hash ensures deterministic reuse across application restarts
- Sequential naming prevents confusion and maintains audit trail
- Draft-only editing enforces proper versioning workflow
- Ready for snapshot/export functionality (US-6)
