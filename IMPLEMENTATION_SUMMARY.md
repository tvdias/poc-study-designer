# Module Management Feature - Implementation Summary

## Overview
This implementation adds comprehensive Module Management functionality to the POC Study Designer, allowing users to create, manage, and organize modules with questions from a Question Bank.

## Backend Implementation

### Entities Created

1. **Module** (`src/Api/Features/Modules/Module.cs`)
   - VariableName: Unique identifier for the module
   - Label: Display name
   - Description: Module description
   - VersionNumber: Version tracking
   - ParentModuleId: Support for hierarchical modules (parent/child relationships)
   - Instructions: Module-specific instructions
   - Status: Module status (Active, Inactive, Draft)
   - StatusReason: Additional context for status
   - IsActive: Boolean flag
   - Related collections: ChildModules, ModuleQuestions, Versions

2. **ModuleVersion** (`src/Api/Features/Modules/ModuleVersion.cs`)
   - Tracks version history for modules
   - Stores version number, change description, and audit info

3. **Question** (`src/Api/Features/Questions/Question.cs`)
   - VariableName: Unique question identifier
   - QuestionType: Type of question (e.g., "Numeric input", "Single choice")
   - QuestionText: The actual question text
   - QuestionSource: "Standard" or "Custom"
   - IsActive: Boolean flag

4. **ModuleQuestion** (`src/Api/Features/Modules/ModuleQuestion.cs`)
   - Join table linking modules and questions
   - DisplayOrder: Controls question ordering within modules
   - Allows reordering and management of questions in modules

### API Endpoints

#### Module Management
- **POST /api/modules** - Create a new module
- **GET /api/modules** - List all modules (with optional search query)
- **GET /api/modules/{id}** - Get module details with questions
- **PUT /api/modules/{id}** - Update module
- **DELETE /api/modules/{id}** - Delete module (validates no child modules exist)

#### Module Versions
- **GET /api/modules/{moduleId}/versions** - Get version history

#### Module Questions
- **POST /api/modules/{moduleId}/questions** - Add question to module
- **DELETE /api/modules/{moduleId}/questions/{questionId}** - Remove question from module
- **PUT /api/modules/{moduleId}/questions/reorder** - Reorder questions in module

#### Question Bank
- **POST /api/questions** - Create new question
- **GET /api/questions** - List all questions (with optional search query)
- **GET /api/questions/{id}** - Get question details

### Validation

All requests are validated using FluentValidation:
- `CreateModuleValidator` - Validates module creation requests
- `UpdateModuleValidator` - Validates module update requests
- `AddQuestionToModuleValidator` - Validates question assignment
- `ReorderModuleQuestionsValidator` - Validates reorder operations
- `CreateQuestionValidator` - Validates question creation

### Testing

62 unit tests added in `src/Api.Tests`:
- `ModuleUnitTests.cs` - Tests for module validators and models
- `QuestionUnitTests.cs` - Tests for question validators and models

All tests passing ✓

### Database Migration

Migration file created: `20260205144934_AddModulesAndQuestions.cs`
- Creates tables: Modules, ModuleVersions, Questions, ModuleQuestions
- Establishes relationships and constraints
- Adds unique indexes on variable names

## Frontend Implementation

### Components Created

1. **ModuleList** (`src/Designer/src/components/ModuleList.tsx`)
   - Displays all modules in a grid layout
   - Search functionality
   - Status badges (Active/Inactive)
   - Shows version number and question count
   - Click to edit functionality
   - "New Module" button

2. **ModuleForm** (`src/Designer/src/components/ModuleForm.tsx`)
   - Two tabs: "General" and "Related"
   - **General Tab:**
     - Variable Name, Label, Description fields
     - Version Number (read-only, auto-incremented)
     - Parent Module selection
     - Instructions (large text area)
     - Status dropdown (Active, Inactive, Draft)
     - Status Reason field
   - **Related Tab:**
     - Question assignment from Question Bank
     - Question list table showing:
       - Variable Name
       - Question Type
       - Question Text
       - Standard or Custom
       - Created By
     - Reordering controls (up/down arrows)
     - Remove question functionality

3. **Type Definitions** (`src/Designer/src/types/module.ts`)
   - TypeScript interfaces for Module, Question, ModuleQuestion, ModuleVersion

### Styling

Clean, professional UI with:
- Card-based layout for modules
- Form with clear sections and validation
- Responsive grid layout
- Accessible components with ARIA labels
- Hover effects and visual feedback

## Features Implemented

✅ Create modules with all required fields
✅ Manage module versions (backend ready, endpoint available)
✅ Assign questions from Question Bank to modules
✅ Reorder questions within modules (up/down controls)
✅ Associate modules with parent modules (hierarchical structure)
✅ Track module history (ModuleVersion entity and endpoint)
✅ Search and filter modules
✅ Search and filter questions
✅ Full CRUD operations for modules and questions
✅ Comprehensive validation
✅ Unit tests for all validators

## Architecture Notes

The implementation follows the existing codebase patterns:
- **Vertical Slice Architecture**: Each feature (Modules, Questions) in its own folder
- **Minimal APIs**: Clean endpoint definitions with typed results
- **FluentValidation**: Request validation
- **Entity Framework Core**: Database operations with PostgreSQL
- **React Hooks**: Functional components with TypeScript
- **Clean separation**: Backend API, Frontend UI

## Running the Application

### Prerequisites
- .NET 10.0 SDK
- Node.js 18+
- PostgreSQL database
- .NET Aspire workload

### Steps
1. Start with Aspire:
   ```bash
   cd src/AppHost
   dotnet run
   ```
   This will start all services including the API and Designer UI.

2. Access the Designer UI through the Aspire dashboard
3. The Module Management feature is the main interface

### Creating Sample Data

Example API calls to create test data:

```bash
# Create a question
POST /api/questions
{
  "variableName": "EXACT_AGE",
  "questionType": "Numeric input",
  "questionText": "Please type in your age",
  "questionSource": "Standard"
}

# Create a module
POST /api/modules
{
  "variableName": "AGE - V1",
  "label": "AGE",
  "description": "AGE Module",
  "status": "Active"
}

# Add question to module
POST /api/modules/{moduleId}/questions
{
  "questionId": "{questionId}",
  "displayOrder": 1
}
```

## Future Enhancements

Potential additions:
- Module version comparison UI
- Bulk question import
- Question template library
- Module cloning/duplication
- Advanced search with filters
- Export module definitions
- Audit log viewing in UI
- Module publishing workflow
