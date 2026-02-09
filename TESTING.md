# Testing Guide

This document provides comprehensive guidance on testing in the POC Study Designer project.

## Overview

The project uses different testing approaches for different components:

- **Frontend (React)**: Component testing with Vitest and React Testing Library
- **Backend (.NET)**: Unit and integration testing with xUnit
- **End-to-End**: Not currently implemented

## Frontend Testing

### Admin Application

The Admin application (`src/Admin/`) has component tests using Vitest and React Testing Library.

#### Running Tests

```bash
cd src/Admin
npm install  # Install dependencies if not already done
npm test     # Run tests in watch mode
npm test -- --run  # Run tests once (CI mode)
```

#### Test Configuration

Tests are configured in `vite.config.ts`:

```typescript
test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
}
```

The setup file (`src/setupTests.ts`) imports jest-dom matchers for enhanced assertions.

#### Existing Component Tests

1. **SidePanel Component** (`src/components/ui/SidePanel.test.tsx`)
   - Tests rendering behavior (open/closed states)
   - Tests close interactions (button, overlay, Escape key)
   - 6 tests, all passing

2. **TagsPage Component** (`src/pages/TagsPage.test.tsx`)
   - Tests loading states
   - Tests CRUD operations (create, view, delete)
   - Tests validation error handling
   - Tests user interactions
   - 11 tests (note: 1 test may fail due to behavioral expectations)

#### Test Coverage Status

Current component test coverage:
- **SidePanel**: ✅ Tested (6 tests)
- **TagsPage**: ✅ Tested (11 tests)
- **ClientsPage**: ❌ Not tested
- **CommissioningMarketsPage**: ❌ Not tested
- **ConfigurationQuestionsPage**: ❌ Not tested
- **FieldworkMarketsPage**: ❌ Not tested
- **ModulesPage**: ❌ Not tested
- **ProductTemplatesPage**: ❌ Not tested
- **ProductsPage**: ❌ Not tested
- **QuestionBankPage**: ❌ Not tested

**Recommendation**: The existing tests for TagsPage provide a good template that can be adapted for other pages. Consider adding tests for pages with complex interactions or critical business logic. Pages like ClientsPage, FieldworkMarketsPage, and CommissioningMarketsPage follow similar patterns and would benefit from similar test coverage.

#### Writing Component Tests

Follow these best practices when writing component tests:

1. **Use React Testing Library queries by priority**:
   - `getByRole` (preferred) - most accessible
   - `getByLabelText` - for form fields
   - `getByText` - for text content
   - `getByTestId` - last resort

2. **Mock API calls**:
   ```typescript
   vi.mock('../services/api', () => ({
       myApi: {
           getAll: vi.fn(),
           create: vi.fn(),
           // ... other methods
       },
   }));
   ```

3. **Test user interactions, not implementation**:
   ```typescript
   // ✅ Good - tests user behavior
   fireEvent.click(screen.getByRole('button', { name: /new/i }));
   await waitFor(() => {
       expect(screen.getByRole('heading', { name: /New Tag/i })).toBeInTheDocument();
   });

   // ❌ Bad - tests implementation details
   expect(component.state.isOpen).toBe(true);
   ```

4. **Handle async operations**:
   ```typescript
   await waitFor(() => {
       expect(screen.getByText('Expected text')).toBeInTheDocument();
   });
   ```

5. **Mock icons and heavy components**:
   ```typescript
   vi.mock('../components/ui/Icons', () => ({
       PlusIcon: () => <span data-testid="icon-plus">Plus</span>,
   }));
   ```

#### Example Test Structure

```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MyComponent } from './MyComponent';
import { myApi } from '../services/api';

vi.mock('../services/api');

describe('MyComponent', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders loading state', () => {
        (myApi.getAll as any).mockImplementation(() => new Promise(() => {}));
        render(<MyComponent />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('handles user interaction', async () => {
        (myApi.getAll as any).mockResolvedValue([]);
        render(<MyComponent />);
        
        await waitFor(() => {
            expect(screen.queryByText('Loading...')).not.toBeInTheDocument();
        });

        fireEvent.click(screen.getByRole('button', { name: /submit/i }));

        await waitFor(() => {
            expect(myApi.create).toHaveBeenCalled();
        });
    });
});
```

### Designer Application

The Designer application (`src/Designer/`) currently does not have component tests. Given its simple structure (single-page application without complex components), component testing may not be necessary at this stage.

**Recommendation**: Consider adding component tests if the Designer application grows in complexity or if specific UI interactions need to be validated.

## Backend Testing

### Unit Tests

Unit tests are located in `src/Api.Tests/` and test individual components in isolation.

#### Running Unit Tests

```bash
dotnet test src/Api.Tests/
```

#### Test Structure

Unit tests follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task ValidTag_ShouldPassValidation()
{
    // Arrange
    var request = new CreateTagRequest("Valid Tag Name");
    var validator = new CreateTagValidator();

    // Act
    var result = await validator.ValidateAsync(request, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}
```

#### Naming Conventions

- Test files: `{Feature}UnitTests.cs`
- Test methods: `{Scenario}_{ExpectedBehavior}`

Examples:
- `ClientUnitTests.cs`
- `TagUnitTests.cs`
- `ValidTag_ShouldPassValidation()`
- `EmptyName_ShouldFailValidation()`

### Integration Tests

Integration tests are located in `src/Api.IntegrationTests/` and test full API workflows.

#### Running Integration Tests

```bash
dotnet test src/Api.IntegrationTests/
```

Integration tests use `WebApplicationFactory` to test the entire API stack including database operations.

## Testing Best Practices

### General Guidelines

1. **Write tests for new features**: All new features should include appropriate tests
2. **Test behavior, not implementation**: Focus on what the code does, not how it does it
3. **Keep tests simple and focused**: One test should verify one behavior
4. **Use meaningful test names**: Test names should clearly describe what they test
5. **Avoid test interdependencies**: Tests should run independently and in any order

### Coverage Goals

While we don't enforce strict coverage percentages, aim for:
- **Critical paths**: 100% coverage (authentication, data validation, business logic)
- **UI components**: Test user interactions and edge cases
- **API endpoints**: Test both success and error scenarios
- **Utilities**: High coverage for shared utilities

### Continuous Integration

Tests run automatically on:
- Pull requests
- Commits to main branch
- Manual workflow dispatch

Ensure all tests pass before merging pull requests.

## Troubleshooting

### Frontend Tests

**Problem**: Tests fail with "vitest: not found"
```bash
cd src/Admin
npm install  # Reinstall dependencies
```

**Problem**: Tests timeout
- Increase the default timeout in test files:
  ```typescript
  await waitFor(() => {
      expect(...).toBeInTheDocument();
  }, { timeout: 5000 });
  ```

**Problem**: Can't find elements
- Use `screen.debug()` to see the rendered output
- Check if you're using the correct query method
- Ensure async operations have completed with `waitFor`

### Backend Tests

**Problem**: Database connection issues
- Ensure PostgreSQL is running (via Docker or Aspire)
- Check connection strings in configuration

**Problem**: Tests fail due to migrations
- Delete the test database and let it recreate
- Ensure migrations are up to date

## Adding Tests to New Components

### Adding Component Tests

When creating a new React component in the Admin app:

1. Create a test file alongside your component: `MyComponent.test.tsx`
2. Set up mocks for any API calls or external dependencies
3. Test rendering in different states (loading, success, error)
4. Test user interactions
5. Test validation and error handling

### Adding API Tests

When adding a new API feature:

1. Create unit tests in `Api.Tests/Features/{FeatureName}/`
2. Test validation with FluentValidation tests
3. Create integration tests in `Api.IntegrationTests/`
4. Test both success and error scenarios
5. Test edge cases and boundary conditions

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [xUnit Documentation](https://xunit.net/)
- [FluentValidation Testing](https://docs.fluentvalidation.net/en/latest/testing.html)
