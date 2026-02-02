# Admin Portal - Implementation Documentation

This document describes the implementation of the Admin Portal for Core Reference & Metadata Management (Issue #10).

## Overview

The Admin Portal provides a comprehensive interface for managing all reference data and survey metadata required by the study design platform. It includes both backend REST APIs and a frontend React application.

## Architecture

### Backend (API)
- **Framework**: ASP.NET Core 10.0 with Minimal APIs
- **Database**: PostgreSQL with Entity Framework Core 10
- **ORM**: Entity Framework Core with migrations
- **Architecture Pattern**: Repository pattern via DbContext
- **Authentication**: Placeholder (system user) - ready for integration

### Frontend (Admin)
- **Framework**: React 19 with TypeScript 5.9
- **Build Tool**: Vite 7
- **Routing**: React Router DOM 7
- **HTTP Client**: Axios
- **Styling**: Custom CSS with modern design

## Database Schema

### Core Entities

#### 1. Question Bank (`Questions`)
- Centralized repository of survey questions
- Fields: Variable name, title, text, type, methodology, versioning
- Supports parent-child relationships
- Includes admin attributes (scale, display type, restrictions, facets)
- Links to answer lists and tags

#### 2. Question Answers (`QuestionAnswers`)
- Answer options for questions
- Fields: Text, code, location (row/column), display order
- Flags: Fixed, exclusive, open, translatable
- Versioned with parent question

#### 3. Modules (`Modules`)
- Reusable containers of questions
- Fields: Variable name, label, description, instructions
- Supports parent-child module hierarchy
- Many-to-many relationship with questions via `ModuleQuestions`

#### 4. Products (`Products`)
- Survey product templates
- Fields: Name, description, rules
- Links to product templates and configuration questions
- Versioned entity

#### 5. Configuration Questions (`ConfigurationQuestions`)
- Product configuration questions
- Fields: Question text, rule type (single/multi coded), AI prompt
- Links to configuration answers
- Many-to-many relationship with products

#### 6. Reference Data
- **Clients**: Simple reference entity for client information
- **Commissioning Markets**: ISO-coded market list
- **Fieldwork Markets**: ISO-coded market list  
- **Tags**: Categorization tags for questions

### Base Classes

- **BaseEntity**: Provides audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, IsActive)
- **VersionedEntity**: Extends BaseEntity with Version and Status fields

## API Endpoints

All endpoints follow RESTful conventions and support JSON payloads.

### Question Bank
- `GET /api/questions` - List all questions (supports search, type, status filters)
- `GET /api/questions/{id}` - Get question by ID
- `POST /api/questions` - Create new question
- `PUT /api/questions/{id}` - Update question
- `DELETE /api/questions/{id}` - Soft delete question
- `GET /api/questions/{id}/answers` - Get question answers
- `POST /api/questions/{id}/answers` - Add answer to question

### Modules
- `GET /api/modules` - List all modules (supports search, status filters)
- `GET /api/modules/{id}` - Get module by ID
- `POST /api/modules` - Create new module
- `PUT /api/modules/{id}` - Update module
- `DELETE /api/modules/{id}` - Soft delete module
- `GET /api/modules/{id}/questions` - Get module questions
- `POST /api/modules/{id}/questions/{questionId}` - Add question to module
- `DELETE /api/modules/{id}/questions/{questionId}` - Remove question from module

### Products
- `GET /api/products` - List all products (supports search, status filters)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Soft delete product

### Configuration Questions
- `GET /api/configuration-questions` - List all configuration questions
- `GET /api/configuration-questions/{id}` - Get configuration question by ID
- `POST /api/configuration-questions` - Create new configuration question
- `PUT /api/configuration-questions/{id}` - Update configuration question
- `DELETE /api/configuration-questions/{id}` - Soft delete configuration question
- `GET /api/configuration-questions/{id}/answers` - Get configuration question answers
- `POST /api/configuration-questions/{id}/answers` - Add answer to configuration question

### Reference Data
- `GET /api/clients` - List all clients
- `GET /api/clients/{id}` - Get client by ID
- `POST /api/clients` - Create new client
- `PUT /api/clients/{id}` - Update client
- `DELETE /api/clients/{id}` - Soft delete client

- `GET /api/tags` - List all tags
- `GET /api/tags/{id}` - Get tag by ID
- `POST /api/tags` - Create new tag
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Soft delete tag

- `GET /api/commissioning-markets` - List all commissioning markets
- `GET /api/fieldwork-markets` - List all fieldwork markets
- (Similar CRUD operations available for markets)

## Frontend Application

### Structure

```
src/Admin/src/
├── components/        # Reusable components
│   ├── Layout.tsx    # Main layout with sidebar
│   └── Layout.css
├── pages/            # Page components
│   ├── Dashboard.tsx
│   ├── Questions/    # Question Bank pages
│   ├── Modules/      # Module pages
│   ├── Clients/      # Client pages
│   ├── Tags/         # Tags pages
│   ├── Markets/      # Markets pages
│   ├── Products/     # Products pages
│   └── ConfigurationQuestions/
├── services/         # API client services
│   ├── api.ts       # Axios configuration
│   └── adminApi.ts  # API endpoint wrappers
├── types/           # TypeScript type definitions
│   └── entities.ts
├── App.tsx          # Main application component
└── main.tsx         # Application entry point
```

### Features

1. **Dashboard**: Overview with navigation cards to all sections
2. **Question Bank**: 
   - List view with search and filters (type, status)
   - Displays variable name, title, type, status, version
   - Actions: View, Delete
3. **Modules**: List view with search, displays variable name, label, status, version
4. **Clients**: Simple list of clients
5. **Markets**: Dual view showing commissioning and fieldwork markets
6. **Tags**: List of all tags with descriptions
7. **Products**: List view with status and version
8. **Configuration Questions**: List view with question text and rule type

### Styling

- Modern, clean design with professional appearance
- Sidebar navigation with icons
- Color-coded status badges
- Responsive tables with hover effects
- Consistent spacing and typography

## Configuration

### Backend Configuration (appsettings.json)

```json
{
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:5174"
  ]
}
```

### Frontend Configuration (.env)

```
VITE_API_URL=http://localhost:5433/api
```

## Running the Application

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- PostgreSQL database (via Aspire)
- Docker Desktop (for Aspire)

### Using Aspire (Recommended)

From the repository root:
```bash
aspire run
```

This will start:
- PostgreSQL database
- API on http://localhost:5433
- Admin portal on http://localhost:5173

### Manual Start

**API:**
```bash
cd src/api
dotnet run
```

**Admin Portal:**
```bash
cd src/Admin
npm install
npm run dev
```

### Database Migrations

The initial migration is already created. When running with Aspire, the database will be automatically created and migrated.

To manually apply migrations:
```bash
cd src/api
dotnet ef database update
```

## Security Considerations

1. **CORS**: Configured to allow only specific origins (localhost development ports)
2. **HTTPS**: Production builds default to HTTPS
3. **Soft Deletes**: Data is never physically deleted, only marked inactive
4. **Audit Trail**: All entities track creation and modification timestamps and users
5. **Input Validation**: Ready for Data Annotations or FluentValidation integration
6. **Authentication**: System user placeholder ready for integration with authentication provider

## Known Limitations & Future Enhancements

1. **Authentication**: Currently uses "system" as the user. Needs integration with authentication provider.
2. **Input Validation**: Basic validation present. Consider adding FluentValidation for comprehensive rules.
3. **DTOs**: Direct entity usage in APIs. Consider adding DTOs for better separation of concerns.
4. **Detail Pages**: Placeholder implementations. Need full CRUD forms.
5. **Pagination**: Not implemented. Should be added for large datasets.
6. **Error Handling**: Basic error handling. Consider adding global error boundary and toast notifications.
7. **Testing**: No automated tests yet. Should add unit and integration tests.
8. **Accessibility**: Basic HTML structure. Should add ARIA attributes and keyboard navigation.

## Database Migration Path

The initial migration creates all tables with proper relationships:
- Questions with self-referencing parent relationship
- Modules with self-referencing parent relationship  
- Many-to-many relationships via junction tables (QuestionTags, ModuleQuestions, ProductConfigurationQuestions)
- Unique constraints on ISO codes for markets
- Proper foreign key constraints with cascade rules

## API Design Principles

1. **RESTful**: Follows REST conventions for endpoints and HTTP verbs
2. **Minimal APIs**: Uses .NET 10 minimal API approach for simplicity
3. **Consistent Responses**: All endpoints return consistent JSON structures
4. **Error Handling**: Uses ProblemDetails for error responses
5. **Filtering**: Query parameters for search and filtering
6. **Soft Deletes**: DELETE operations mark records inactive rather than removing them

## Contributing

When extending this system:

1. **New Entities**: 
   - Inherit from BaseEntity or VersionedEntity
   - Add DbSet to AdminDbContext
   - Configure relationships in OnModelCreating
   - Create migration
   - Add API endpoints
   - Add frontend types and services

2. **New Endpoints**:
   - Create new endpoint class in src/api/Endpoints
   - Register in Program.cs
   - Follow existing patterns for consistency

3. **New Pages**:
   - Create page component in src/Admin/src/pages
   - Add route in App.tsx
   - Add navigation link in Layout.tsx
   - Follow existing styling patterns

## Support

For issues or questions, please refer to the GitHub repository issue tracker.
