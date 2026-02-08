# POC Study Designer

A comprehensive study design platform built with .NET Aspire, featuring a distributed microservices architecture with React-based frontends, serverless processing functions, and Azure Service Bus integration.

## Project Overview

- **Frontend Applications**: React-based UIs for designing and administering studies
- **Backend Services**: .NET-based APIs and serverless functions
- **Message Processing**: Azure Service Bus-based event-driven architecture

## Project Structure

### Frontend Applications

- **Admin** (`src/Admin/`) - Administration portal
  - React TypeScript application

- **Designer** (`src/Designer/`) - Study design interface
  - React TypeScript application

### Backend Services

- **Api** (`src/Api/`) - Main REST API
  - C# .NET project
  - Core business logic and data management
  - Study-related CRUD operations and validations

- **AppHost** (`src/AppHost/`) - .NET Aspire orchestrator
  - Local development environment management

### Processing & Integration

- **CluedinProcessor** (`src/CluedinProcessor/`) - CluedIn integration processor
  - Azure Function
  - Processes data from CluedIn platform
  - Azure Service Bus event consumer

- **ProjectsProcessor** (`src/ProjectsProcessor/`) - Project processing function
  - Azure Function
  - Handles project creation
  - Azure Service Bus event consumer

- **ServiceBusPublisher** (`src/ServiceBusPublisher/`) - Event publisher
  - C# utility project
  - Publishes events to Azure Service Bus

## Prerequisites

- **.NET 10.0**
- **Node.js 18.x** or later (for frontend development)
- **.NET Aspire 13**
- **Docker Desktop** or equivalent (such as Podman)
- **Azure CLI** and a valid subscription (required by aspire)
- **Azure Functions Core Tools** (for local function development and testing)

## Development Options

### Option 1: Development Container (Recommended)

The project includes a complete development container configuration that provides all necessary dependencies and tools.

#### Using VS Code Dev Containers

1. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
2. Open the project in VS Code
3. When prompted, click "Reopen in Container" or run `Dev Containers: Reopen in Container` from the command palette
4. Wait for the container to build and start (first time takes longer)
5. Once ready, run the application using Aspire:
   ```bash
   aspire run
   ```

The devcontainer includes:
- .NET 10.0 SDK
- Node.js 18
- Docker-in-Docker support
- Azure CLI and Azure Functions Core Tools
- PostgreSQL and Redis services
- All required VS Code extensions

### Option 2: Running with Aspire (Local)

The easiest way to run the entire project locally is using Aspire:

```bash
aspire run
```

This will:
- Start the Aspire Dashboard
- Automatically start all configured services
- Manage container orchestration
- Provide logs and health monitoring

### Option 3: Running with Docker Compose

You can run the entire application stack using Docker Compose for production-like deployments:

```bash
# Copy the example environment file
cp .env.example .env

# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

Services will be available at:
- **Designer App**: http://localhost:3000
- **Admin App**: http://localhost:3001
- **API**: http://localhost:5000
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379
