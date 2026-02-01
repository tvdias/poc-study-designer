# Study Designer - POC

This is a proof-of-concept for a Study Designer application built with .NET Aspire, featuring microservices architecture with Service Bus messaging.

## Architecture Overview

The solution consists of simple "hello world" style applications:

### Frontend Applications
- **Study Designer** (`src/frontend`) - Main designer interface with data table UI
- **Study Admin** (`src/frontend-admin`) - Admin portal for managing studies

Both frontends are React + TypeScript applications built with Vite, featuring a clean table-based UI inspired by Power Platform.

### Backend Services
- **StudyDesigner.API** (`src/StudyDesigner.API`) - Simple REST API
  - Built with ASP.NET Core
  - Uses Redis for output caching
  - Provides OpenAPI/Swagger documentation
  - Health check endpoint at `/health`
  - Simple endpoints: `/api/hello` and `/api/studies`

### Azure Functions
- **ServiceBusConsumer** (`src/ServiceBusConsumer`) - Consumes messages from Service Bus
  - Listens to `sampletopic` with `designer-subscription`
  - Simple hello world message processing
- **StudyProcessor** (`src/StudyProcessor`) - Processes study data from Service Bus
  - Listens to `sampletopic` with `processor-subscription`
  - Simple hello world processing

### Console Applications
- **MessagePublisher** (`src/MessagePublisher`) - Console app for publishing messages to Service Bus
  - Interactive CLI for sending hello world messages
  - Configurable via `appsettings.json` or environment variables

### Orchestration
- **StudyDesigner.AppHost** (`src/StudyDesigner.AppHost`) - .NET Aspire AppHost for orchestration
  - Manages all services and their dependencies
  - Provides service discovery
  - Configures Redis cache
  - Orchestrates frontend and backend deployments

## Prerequisites

- .NET 10.0 SDK or later
- Node.js 18+ and npm
- Azure Service Bus (for message publishing/consumption)
- Redis (automatically provided by Aspire)

## Getting Started

1. **Restore dependencies**
   ```bash
   cd src
   dotnet restore StudyDesigner.slnx
   ```

2. **Build the solution**
   ```bash
   dotnet build StudyDesigner.slnx
   ```

3. **Run the application** (via Aspire AppHost)
   ```bash
   cd StudyDesigner.AppHost
   dotnet run
   ```

   This will start:
   - Redis cache
   - StudyDesigner.API
   - Study Designer Frontend
   - Study Admin Frontend
   - ServiceBusConsumer Function
   - StudyProcessor Function

4. **Access the Aspire Dashboard**
   - The Aspire dashboard will be available at the URL shown in the console output
   - View all services, logs, and telemetry

## Configuration

### Service Bus Connection
The Azure Functions and MessagePublisher require a Service Bus connection string.

**Option 1: User Secrets (recommended for development)**
```bash
cd src/StudyDesigner.AppHost
dotnet user-secrets set "ConnectionStrings:sb" "YOUR_SERVICE_BUS_CONNECTION_STRING"
```

**Option 2: Environment Variable**
```bash
export sb="YOUR_SERVICE_BUS_CONNECTION_STRING"
```

**Option 3: appsettings.json (MessagePublisher only)**
Edit `src/MessagePublisher/appsettings.json`:
```json
{
  "ServiceBus": {
    "ConnectionString": "YOUR_SERVICE_BUS_CONNECTION_STRING",
    "TopicName": "sampletopic"
  }
}
```

### Running MessagePublisher
```bash
cd src/MessagePublisher
dotnet run
```

Follow the interactive prompts to send hello world messages.

## Project Structure

```
src/
├── frontend/                      # Study Designer Frontend (React)
├── frontend-admin/                # Study Admin Frontend (React)
├── StudyDesigner.API/            # Main API Server
├── StudyDesigner.AppHost/        # Aspire Orchestration Host
├── ServiceBusConsumer/           # Azure Function - Message Consumer
├── StudyProcessor/               # Azure Function - Study Processor
└── MessagePublisher/             # Console App - Message Publisher
```

## Development

### Building Frontend Apps
```bash
# Study Designer Frontend
cd src/frontend
npm install
npm run dev

# Study Admin Frontend
cd src/frontend-admin
npm install
npm run dev
```

### Running Individual Services

**API Server:**
```bash
cd src/StudyDesigner.API
dotnet run
```

**Azure Functions (local):**
```bash
cd src/ServiceBusConsumer
func start
# or
dotnet run
```

## Testing

### API Endpoints
Once the API is running, you can test endpoints:
- Hello World: `GET http://localhost:<port>/api/hello`
- Studies: `GET http://localhost:<port>/api/studies`
- Health Check: `GET http://localhost:<port>/health`
- OpenAPI/Swagger: Available in development mode

### Message Flow
1. Run MessagePublisher to send a hello world message
2. ServiceBusConsumer receives and logs the message
3. StudyProcessor processes the message

## Technology Stack

- **Backend**: .NET 10.0, ASP.NET Core, Azure Functions
- **Frontend**: React 19, TypeScript, Vite
- **Messaging**: Azure Service Bus
- **Caching**: Redis (via Aspire)
- **Orchestration**: .NET Aspire
- **Observability**: OpenTelemetry

## Notes

- This is a proof-of-concept with simple "hello world" implementations
- Azure Functions require proper Service Bus configuration to run
- Redis is automatically managed by Aspire
- All services support OpenTelemetry for distributed tracing
- Frontend UIs are inspired by Power Platform with clean table-based layouts
