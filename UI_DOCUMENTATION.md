# Module Management UI - Component Structure

## Module List View
```
┌─────────────────────────────────────────────────────────────┐
│ Module Management                           [+ New Module]  │
├─────────────────────────────────────────────────────────────┤
│ [Search modules...                          ] [Search]      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐│
│  │ AGE - V1       │  │ MODULE_2       │  │ MODULE_3       ││
│  │ [Active]       │  │ [Draft]        │  │ [Active]       ││
│  ├────────────────┤  ├────────────────┤  ├────────────────┤│
│  │ AGE            │  │ Demographics   │  │ Medical Info   ││
│  │                │  │                │  │                ││
│  │ AGE Module     │  │ Basic demo...  │  │ Health...      ││
│  │                │  │                │  │                ││
│  │ Version: 1     │  │ Version: 2     │  │ Version: 1     ││
│  │ Questions: 3   │  │ Questions: 5   │  │ Questions: 8   ││
│  └────────────────┘  └────────────────┘  └────────────────┘│
│                                                              │
│  [Click any card to edit]                                   │
└─────────────────────────────────────────────────────────────┘
```

## Module Form View - General Tab
```
┌─────────────────────────────────────────────────────────────┐
│ Edit Module                                           [×]    │
├─────────────────────────────────────────────────────────────┤
│ [General]  [Related]                                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Module Variable Name *        Module Label *               │
│  [AGE - V1                ]    [AGE                     ]   │
│                                                              │
│  Module Description                                          │
│  [AGE Module                                            ]   │
│                                                              │
│  Module Version Number         Parent Module                │
│  [1 (readonly)            ]    [                        ]   │
│                                                              │
│  Module Instructions                                         │
│  [                                                      ]   │
│  [                                                      ]   │
│                                                              │
│  Status *                      Status Reason                │
│  [Active ▼                ]    [                        ]   │
│                                                              │
│                                      [Cancel]  [Save Module]│
└─────────────────────────────────────────────────────────────┘
```

## Module Form View - Related Tab (Questions)
```
┌─────────────────────────────────────────────────────────────┐
│ Edit Module                                           [×]    │
├─────────────────────────────────────────────────────────────┤
│ [General]  [Related]                                         │
├─────────────────────────────────────────────────────────────┤
│ Questions in Module                                          │
│                                                              │
│ [Select a question...                         ▼] [+ Add]    │
│                                                              │
│ ┌──────────────────────────────────────────────────────────┐│
│ │ Variable │ Type         │ Text        │ Source │ Actions ││
│ ├──────────────────────────────────────────────────────────┤│
│ │ EXACT_AGE│ Numeric input│ Type age    │ Std    │ ↑↓ ×   ││
│ │ AGEBANDS │ Single choice│ Age group   │ Std    │ ↑↓ ×   ││
│ │ EXP_AGE  │ Multiple...  │ Which age...│ Custom │ ↑↓ ×   ││
│ └──────────────────────────────────────────────────────────┘│
│                                                              │
│ • Use ↑↓ arrows to reorder questions                        │
│ • Use × to remove questions from module                     │
│ • Select from dropdown to add new questions                 │
└─────────────────────────────────────────────────────────────┘
```

## API Endpoint Structure

### Module Endpoints
```
POST   /api/modules                              Create module
GET    /api/modules                              List modules (with search)
GET    /api/modules/{id}                         Get module details
PUT    /api/modules/{id}                         Update module
DELETE /api/modules/{id}                         Delete module
GET    /api/modules/{id}/versions                Get version history
```

### Module Question Endpoints
```
POST   /api/modules/{id}/questions               Add question to module
DELETE /api/modules/{id}/questions/{qId}         Remove question
PUT    /api/modules/{id}/questions/reorder       Reorder questions
```

### Question Endpoints
```
POST   /api/questions                            Create question
GET    /api/questions                            List questions (with search)
GET    /api/questions/{id}                       Get question details
```

## Database Schema

```
┌──────────────────┐
│ Modules          │
├──────────────────┤
│ Id               │◄─────────┐
│ VariableName (U) │          │
│ Label            │          │
│ Description      │          │
│ VersionNumber    │          │ Parent/Child
│ ParentModuleId   │──────────┘
│ Instructions     │
│ Status           │
│ StatusReason     │
│ IsActive         │
│ CreatedBy        │
│ CreatedOn        │
│ ModifiedBy       │
│ ModifiedOn       │
└──────────────────┘
         │
         │ 1:N
         ▼
┌──────────────────┐
│ ModuleVersions   │
├──────────────────┤
│ Id               │
│ ModuleId         │
│ VersionNumber    │
│ ChangeDescription│
│ CreatedOn        │
│ CreatedBy        │
└──────────────────┘

┌──────────────────┐
│ Questions        │
├──────────────────┤
│ Id               │◄────┐
│ VariableName (U) │     │
│ QuestionType     │     │
│ QuestionText     │     │
│ QuestionSource   │     │
│ IsActive         │     │
│ CreatedBy        │     │ N:M
│ CreatedOn        │     │ (via ModuleQuestions)
└──────────────────┘     │
                         │
┌──────────────────┐     │
│ ModuleQuestions  │     │
├──────────────────┤     │
│ Id               │     │
│ ModuleId         │─────┘
│ QuestionId       │─────┘
│ DisplayOrder     │
│ CreatedOn        │
└──────────────────┘

(U) = Unique Index
```

## Key Features

1. **Module Management**
   - Create, edit, delete modules
   - Version tracking
   - Parent/child relationships
   - Status management (Active/Inactive/Draft)

2. **Question Bank**
   - Create standard and custom questions
   - Assign to multiple modules
   - Maintain in centralized bank

3. **Question Assignment**
   - Add questions from bank to modules
   - Remove questions from modules
   - Reorder questions within modules
   - Display order preserved

4. **Search & Filter**
   - Search modules by name or label
   - Search questions by variable name or text

5. **User Interface**
   - Responsive grid layout
   - Tabbed interface for complex forms
   - Accessible controls with ARIA labels
   - Visual feedback on hover/focus
