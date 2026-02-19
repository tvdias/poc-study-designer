# Entity Definitions and Required Fields

This document lists all entities involved in Project Creation, Study Creation, Versioning, Questionnaire Management, Managed Lists, MLEs, Subsets, and Auto‑Association.

---

# 1. Project

| Field | Type | Description |
|-------|------|-------------|
| **ProjectId** | GUID (PK) | Unique ID |
| Name | String | Project name |
| HasStudies | Boolean | Indicates if any Study exists |
| StudyCount | Integer | Number of Studies |
| LastStudyModifiedOn | Datetime | Derived from Study updates |
| CreatedOn / CreatedBy | System | Audit |

---

# 2. Study

| Field | Type | Description |
|-------|------|-------------|
| **StudyId** | GUID (PK) |
| ProjectId | FK → Project |
| VersionNumber | Integer |
| Status | OptionSet (Draft / ReadyForScripting / Approved / Locked) |
| ParentStudyId | FK → Study (previous version) |
| MasterStudyId | FK → root version |
| CreatedOn / CreatedBy | System |

**Rules:**  
- Only one Draft per lineage.  
- Version numbering must be sequential.

---

# 3. Questionnaire Line (QL)

| Field | Type | Description |
|-------|------|-------------|
| **QLId** | GUID (PK) |
| StudyId | FK → Study |
| QuestionCode | String |
| SortOrder | Integer |
| IsActive | Boolean |
| QuestionText | String |
| QuestionMetadata | JSON |
| CreatedOn | Datetime |

---

# 4. Managed List

| Field | Type | Description |
|-------|------|-------------|
| **ManagedListId** | GUID (PK) |
| ProjectId | FK |
| Name | String (Unique per project) |
| IsActive | Boolean |
| CreatedOn | Datetime |

---

# 5. Managed List Entity (MLE)

| Field | Type | Description |
|-------|------|-------------|
| **MLEId** | GUID (PK) |
| ManagedListId | FK |
| Code | String (Unique per ML) |
| Name | String |
| SortOrder | Integer |
| IsActive | Boolean |
| Metadata | JSON |

**Rules:**  
- Unique Code per ML.  
- Deactivation affects Draft Studies only.

---

# 6. Question–Managed List Link (QuestionMLLink)

| Field | Type | Description |
|-------|------|-------------|
| **QuestionMLLinkId** | GUID (PK) |
| QLId | FK |
| ManagedListId | FK |
| LinkMetadata | JSON |

---

# 7. Question‑MLE Link (QL_MLE_Link)

| Field | Type | Description |
|-------|------|-------------|
| **QLMLEId** | GUID (PK) |
| QLId | FK |
| MLEId | FK |
| IsActiveInStudy | Boolean |
| SortOrder | Integer |

**Rules:**  
- Draft only auto-association.  
- Idempotent behaviour required.

---

# 8. Subset Definition

| Field | Type | Description |
|-------|------|-------------|
| **SubsetId** | GUID (PK) |
| StudyId | FK |
| ManagedListId | FK |
| Name | String (`LIST_SUBn`) |
| SignatureHash | String |
| CreatedOn | Datetime |

**Rules:**  
- No manual editing of Name.  
- Sequential naming; no reuse of numbers.

---

# 9. Subset Entity (Subset Membership)

| Field | Type | Description |
|-------|------|-------------|
| **SubsetEntityId** | GUID (PK) |
| SubsetId | FK |
| MLEId | FK |
| SortOrder | Integer |

---

# 10. Snapshot Entities

(*Exact tables implementation-dependent, but must capture:`Study`,`QL`,`QL_MLE`,`SubsetDefinition`,`SubsetEntity`*)

| Required Data | Notes |
|---------------|-------|
| Study version metadata | Immutable snapshot of header |
| QLs | Exact content & order preserved |
| ML assignments | Full mapping |
| MLE selections | Concrete values used |
| Subsets | Exact subset definitions + membership |
| Routing/metadata | All required for scripting |

---

# 11. Permissions / Sharing Records

| Field | Description |
|-------|-------------|
| Principal (User/Team) | Project Owner or Project Access Team |
| Object ID | Study, QLs, Lists, Subsets, etc. |
| Rights | Read, Write, Append, Associate |