# PCF Matrix Control

A (PCF) control for displaying and managing matrix-style data in Power Apps.

## Prerequisites

Before getting started, ensure you have the following installed:

- **Node.js** (LTS version recommended)
- **Power Platform CLI**

  npm install -g @microsoft/powerapps-cli

- **.NET SDK** (for solution building)
- **Developer Command Prompt** or Visual Studio (optional, for msbuild)


## Setup & Development

### 1. Create PCF Control

# Initialize the PCF control
pac pcf init --namespace Kantar.Controls --name MatrixControl --template dataset

# Navigate to project directory
cd MatrixControl

# Install dependencies
npm install

# Build the project
npm run build

# Start development server with watch mode
npm start watch


### 2. Environment Authentication

# Authenticate with your Dynamics 365 environment
pac auth create --url https://uc1mspoc.crm4.dynamics.com

# Note: You can also use your development environment URL


# Build the PCF control
npm run build


### Solution Management

# Navigate to solution directory
cd ..\MatrixControlSolution

# Update solution version (replace [Buildnumber] with actual build number)
pac solution version --buildversion [Buildnumber]

# Build the solution
dotnet build
# Alternative: Use Developer Command Prompt with msbuild

## Deployment

### Import and Publish Solution

# Import solution to environment with force overwrite and auto-publish
pac solution import --path "bin\Debug\MatrixControlSolution.zip" --publish-changes --force-overwrite


## Unit Testing

### Test Dependencies (if needed)

npm install --save-dev react@^18.2.0 react-dom@^18.2.0 @types/react@^18.0.0 @types/react-dom@^18.0.0 @testing-library/react@^13.4.0 @testing-library/jest-dom@^6.0.0 @testing-library/user-event@^14.0.0


## Unit Testing

### Test Setup

Unit testing has been pre-configured for this project. The testing framework includes:

- **Jest** - Testing framework
- **React Testing Library** - Component testing utilities
- **TypeScript** support

### Test Structure

Reference the existing test files for examples:

- **Service Tests**: `_tests_/services/DataService.test.ts`
- **Component Tests**: `_tests_/components/DataService.test.tsx`

### Installing Additional Test Dependencies

If `npm install` doesn't install all required testing libraries, run:

npm install --save-dev react@^18.2.0 react-dom@^18.2.0 @types/react@^18.0.0 @types/react-dom@^18.0.0 @testing-library/react@^13.4.0 @testing-library/jest-dom@^6.0.0 @testing-library/user-event@^14.0.0

### Running Tests

# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Generate coverage report
npm run test:coverage

### Writing New Tests

When adding new unit tests:

1. Follow the existing patterns in the `_tests_` directory
2. Use the established naming convention: `[serviceName].test.ts` or `[ComponentName].test.tsx`
3. Reference existing test files for structure and best practices
4. Ensure proper mocking of PCF context and dependencies

## Project Structure

MatrixControl/
├── _tests_/                 # Unit tests
│   ├── services/           # Service layer tests
│   └── components/         # Component tests
├── MatrixControl/          # Main PCF control source
├── node_modules/           # Dependencies
├── out/                    # Build output

Outside the Project Directory
MatrixControlSolution/  # Solution package

## Troubleshooting

### Common Issues

1. **Build Failures**: Ensure all dependencies are installed with `npm install`
2. **Authentication Issues**: Verify your environment URL and permissions
3. **Solution Import Errors**: Check that the solution path is correct and the environment is accessible

# PCF Reusability
### Example: Study Managed List Entity
Below is an example configuration for using the **Matrix Control** with the **Study Managed List (ktr_studymanagedlistentity)**.
When placed on Managed List form. Rows: Managed List Entity, Columns: Study.

- entityId: Bind to table column - Managed List (Text)
- entityName: ktr_managedlist  

Details of Row - Managed List Entity
- rowParentField: ktr_managedlist
- rowEntityName: ktr_managedlistentity  
- rowIdField: ktr_managedlistentityid  
- rowDisplayField: ktr_answertextvalue

Details of Column - Study
- columnParentField: kt_project 
- columnEntityName: kt_study 
- columnIdField: kt_studyid 
- columnDisplayField: kt_name 
- columnParentAttrField: ktr_parentstudy 
- columnVersionField: ktr_versionnumber  

Details of Junction record
- junctionEntityName: ktr_studymanagedlistentity 
- junctionIdField: ktr_studymanagedlistentityid 
- junctionRowField: ktr_managedlistentity
- junctionColumnField: ktr_study 

Details of Parent Entity of Row
- parentEntityId: ktr_managedlistid  
- parentEntityName: ktr_managedlist  

#### Notes
- The configuration can be updated dynamically based on the parent entity. 
- Usage of parent entity: 
  To fetch studies indirectly: Managed List Entity (Row) -> Managed List (Parent Entity) -> Get Project lookup -> Get Studies.
