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
- **Docker Desktop** or "equivalent" (as Podman)
- **Azure CLI** and a valid subscription (required by aspire)
- **Azure Functions Core Tools** (for local function development and testing)

### Running with Aspire

The easiest way to run the entire project locally is using Aspire:

```bash
aspire run
```

This will:
- Start the Aspire Dashboard
- Automatically start all configured services
- Manage container orchestration
- Provide logs and health monitoring
