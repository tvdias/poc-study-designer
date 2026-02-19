# Test Cases — Project Creation & Management  
This document provides detailed, self-contained test cases covering all business requirements for Projects, Studies, Questionnaire Lines, Managed Lists, MLEs, Subsets, Auto-Association, Permissions, and Snapshots.

---

# 1. PROJECT

## TC-PROJ-01 — Create Project
**Precondition:** User has permission to create projects  
**Steps:**  
1. Open Project creation UI  
2. Enter required metadata & save  
**Expected Results:**  
- Project is created with correct metadata  
- HasStudies = false  
- StudyCount = 0  

## TC-PROJ-02 — Update Project Counters When Study Added
**Steps:**  
1. Create Project  
2. Create first Study under it  
**Expected Results:**  
- HasStudies = true  
- StudyCount = 1  
- LastStudyModifiedOn updated  

## TC-PROJ-03 — Update Counters When Study Deleted
**Steps:**  
1. Delete a Study  
2. Reload Project  
**Expected:**  
- StudyCount decremented  
- HasStudies recalculated  

---

# 2. STUDY CREATION (V1)

## TC-STUDY-V1-01 — Create Study Version 1
**Steps:**  
1. Choose “Create Study”  
2. Select Project → confirm  
**Expected:**  
- Study created with VersionNumber = 1  
- Status = Draft  
- Questionnaire Lines copied from Master Questionnaire  
- Managed Lists, MLEs, Subsets copied correctly  
- Auto-share applied  

## TC-STUDY-V1-02 — QL Ordering Preserved
**Expected:**  
- QLs in Study maintain exact ordering from Master  

## TC-STUDY-V1-03 — Inactive Questions Not Included
**Expected:**  
- Only active questions appear in V1  

## TC-STUDY-V1-04 — Auto-Share on Creation
**Expected:**  
- Study shared with Project Owner & Access Team  
- Permissions: Read, Write, Append, Associate  

---

# 3. STUDY VERSIONING (Vn)

## TC-STUDY-VN-01 — Create Version n
**Steps:**  
1. Open V1  
2. Select “Create Version”  
**Expected:**  
- VersionNumber = previous + 1  
- ParentStudyId set  
- Status = Draft  

## TC-STUDY-VN-02 — Copy QLs From Parent Version
**Expected:**  
- All QLs appear exactly as parent  
- Activation states preserved  

## TC-STUDY-VN-03 — Copy ML/MLE Assignments From Parent
**Expected:**  
- Same MLs linked  
- Same filtered MLEs appear  

## TC-STUDY-VN-04 — Enforce “Only One Draft” Rule
**Steps:**  
1. Ensure a Draft exists  
2. Attempt to create another Draft  
**Expected:**  
- Error: “Only one Draft version allowed per Study”  

## TC-STUDY-VN-05 — Removed QLs Stay Removed
**Expected:**  
- QLs removed in the parent must not reappear in new version  

---

# 4. STUDY STATUS MODEL

## TC-STUDY-STATUS-01 — Draft Editable
**Expected:**  
- User can edit QLs, ML/MLE selections, Subsets  

## TC-STUDY-STATUS-02 — Ready for Scripting is Read-Only
**Expected:**  
- All editing disabled  
- Inline UI disabled  
- Auto-association not triggered  

## TC-STUDY-STATUS-03 — Locked/Approved Immutable
**Expected:**  
- No structural changes allowed  
- No MLE propagation  

---

# 5. QUESTIONNAIRE LINES

## TC-QL-01 — QL Created for Each Question
**Expected:**  
- QL created for every active question from source  

## TC-QL-02 — QL Metadata Copied
**Expected:**  
- All text, codes, settings copied identically  

## TC-QL-03 — QL Editing Only in Draft
**Expected:**  
- Editing allowed only when StudyStatus = Draft  

---

# 6. MANAGED LISTS (PROJECT LEVEL)

## TC-ML-01 — Create Managed List
**Expected:**  
- List saved with unique name per project  

## TC-ML-02 — Deactivate Managed List
**Expected:**  
-Cannot be assigned to new questions  
- Does not impact locked Studies  

## TC-ML-03 — Assign ML to Question
**Expected:**  
- Link created  
- All active MLEs auto-added to Draft Studies  

---

# 7. MANAGED LIST ENTITIES (MLEs)

## TC-MLE-01 — Create MLE
**Expected:**  
- Code must be unique within ML  
- Appears in selection for Draft Studies  

## TC-MLE-02 — Bulk Import
**Expected:**  
- Valid rows inserted/updated  
- Invalid rows rejected with summary  

## TC-MLE-03 — MLE Deactivation
**Expected:**  
- Removed from Draft Studies  
- Locked Studies retain old associations  

## TC-MLE-04 — MLE Reactivation
**Expected:**  
- Becomes available to Draft Studies  

---

# 8. AUTO‑ASSOCIATION (DRAFT ONLY)

## TC-AA-01 — New MLE Added → Auto-Propagate
**Steps:**  
1. Add new MLE  
**Expected:**  
- Draft Studies automatically receive it  
- No duplicates  

## TC-AA-02 — ML Assigned to QL → Auto-Propagate
**Expected:**  
- All active MLEs added to QL  

## TC-AA-03 — MLE Deactivated → Auto-Removal
**Expected:**  
- MLE removed from Draft Studies  
- Subsets recalculated  

## TC-AA-04 — No Propagation to Non-Draft
**Expected:**  
- Locked Studies unchanged  

---

# 9. SUBSET DEFINITIONS

## TC-SUB-01 — Create Subset When Partial Selection
**Expected:**  
- SubsetDefinition created  
- Name = LIST_SUB1  

## TC-SUB-02 — Reuse Subset When Signature Matches
**Expected:**  
- Identical selection uses existing Subset  

## TC-SUB-03 — Sequential Naming No Gap Reuse
**Expected:**  
- LIST_SUB1, LIST_SUB2, LIST_SUB4 (if SUB3 deleted – no reuse)  

## TC-SUB-04 — Draft Only Editing
**Expected:**  
- Cannot modify in non‑Draft  

## TC-SUB-05 — Prevent Empty Subset
**Expected:**  
- Error shown or fallback to full list  

---

# 10. SUBSET SYNCHRONISATION

## TC-SYNC-01 — Subset Change Refreshes QL Display
**Expected:**  
- HTML preview updated instantly  

## TC-SYNC-02 — Update Study-Level Summary
**Expected:**  
- Subset counts & membership updated  

## TC-SYNC-03 — MLE Deactivation Updates Subsets
**Expected:**  
- MLE removed from subsets in Draft Studies  

## TC-SYNC-04 — No Refresh for Locked Studies
**Expected:**  
- Locked study remains unchanged  

---

# 11. PERMISSIONS & SHARING

## TC-PERM-01 — Auto-Share on Study Creation
**Expected:**  
- Project Owner & Access Team have access  

## TC-PERM-02 — Child Entities Inherit Sharing
**Expected:**  
- QLs, Subsets, MLE links are shared identically  

---

# 12. SNAPSHOT & EXPORT READINESS

## TC-SNAP-01 — Snapshot Captures Exact Structure
**Expected:**  
- All QLs, MLs, MLEs, Subsets included  

## TC-SNAP-02 — Locked Study Snapshot Always Consistent
**Expected:**  
- Snapshot matches final locked version exactly  

## TC-SNAP-03 — Snapshot Stable Across Re‑Exports
**Expected:**  
- Idempotent snapshots  
