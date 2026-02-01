# Study Designer - POC

This is a proof-of-concept for a Study Designer application built with .NET Aspire, featuring microservices architecture with Service Bus messaging.

## Architecture Overview

The solution consists of:

### Frontend Applications
- **Study Designer Frontend** (`src/frontend`) - For creating and managing clinical studies
- **Study Reviewer Frontend** (`src/frontend-reviewer`) - For reviewing and approving clinical studies

Both frontends are React + TypeScript applications built with Vite.

### Backend Services
- **StudyDesigner.API** (`src/StudyDesigner.API`) - Main API server providing REST endpoints
  - Built with ASP.NET Core
  - Uses Redis for output caching
  - Provides OpenAPI/Swagger documentation
  - Health check endpoint at `/health`

### Azure Functions
- **ServiceBusConsumer** (`src/ServiceBusConsumer`) - Consumes study messages from Service Bus
  - Listens to `sampletopic` with `designer-subscription`
- **StudyProcessor** (`src/StudyProcessor`) - Processes study data from Service Bus
  - Listens to `sampletopic` with `processor-subscription`

### Console Applications
- **MessagePublisher** (`src/MessagePublisher`) - Console app for publishing messages to Service Bus
  - Interactive CLI for sending test messages and study creation messages
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
   - Study Reviewer Frontend
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

Follow the interactive prompts to:
1. Send test messages
2. Send study creation messages

## Project Structure

```
src/
├── frontend/                      # Study Designer Frontend (React)
├── frontend-reviewer/             # Study Reviewer Frontend (React)
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

# Study Reviewer Frontend
cd src/frontend-reviewer
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
- Weather Forecast: `GET http://localhost:<port>/api/weatherforecast`
- Health Check: `GET http://localhost:<port>/health`
- OpenAPI/Swagger: Available in development mode

### Message Flow
1. Run MessagePublisher to send a study creation message
2. ServiceBusConsumer receives and logs the message
3. StudyProcessor processes the study data

## Technology Stack

- **Backend**: .NET 10.0, ASP.NET Core, Azure Functions
- **Frontend**: React 19, TypeScript, Vite
- **Messaging**: Azure Service Bus
- **Caching**: Redis (via Aspire)
- **Orchestration**: .NET Aspire
- **Observability**: OpenTelemetry

## Notes

- This is a proof-of-concept implementation
- Azure Functions require proper Service Bus configuration to run
- Redis is automatically managed by Aspire
- All services support OpenTelemetry for distributed tracing
