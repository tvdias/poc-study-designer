# Admin Application

Administration portal for POC Study Designer, built with React, TypeScript, and Vite.

## Features

- **Tags Management**: Create, edit, and manage tags
- **Commissioning Markets**: Manage commissioning market data
- **Fieldwork Markets**: Manage fieldwork market data

## Development

### Prerequisites

- Node.js 18.x or later
- npm or yarn

### Getting Started

```bash
# Install dependencies
npm install

# Start development server (requires Aspire to run the API)
npm run dev

# Build for production
npm run build

# Run linter
npm run lint
```

### Running with the Full Stack

This application requires the backend API to be running. The recommended approach is to use .NET Aspire to orchestrate all services:

```bash
# From the project root
cd src/AppHost
dotnet run
```

The Aspire dashboard will show all running services and their URLs.

## Testing

### Unit Tests

Run unit tests with Vitest:

```bash
npm test
```

### E2E Tests

There are two ways to run E2E tests:

#### Option 1: Aspire-Integrated Tests (Recommended)

Run E2E tests that are fully integrated with Aspire orchestration:

```bash
# From project root
dotnet test src/Admin.E2ETests/Admin.E2ETests.csproj
```

**Benefits:**
- ✅ Automatically starts all services (API, databases, Redis, Admin)
- ✅ No manual URL configuration (Aspire dynamically assigns ports)
- ✅ Integrated with .NET test suite
- ✅ Works reliably even when ports change

See [Admin.E2ETests/README.md](../Admin.E2ETests/README.md) for details.

#### Option 2: Standalone Playwright Tests

Run standalone E2E tests from this directory:

```bash
# Requires Aspire running in another terminal
npm run test:e2e

# Or with UI mode
npm run test:e2e:ui

# Or in headed mode
npm run test:e2e:headed
```

For detailed instructions, see [e2e/README.md](./e2e/README.md).

## Project Structure

```
src/
├── assets/          # Static assets
├── components/      # Reusable UI components
├── layouts/         # Layout components
├── pages/           # Page components
│   ├── TagsPage.tsx
│   ├── CommissioningMarketsPage.tsx
│   └── FieldworkMarketsPage.tsx
├── services/        # API services
└── e2e/            # End-to-end tests
```

---

# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) (or [oxc](https://oxc.rs) when used in [rolldown-vite](https://vite.dev/guide/rolldown)) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```
