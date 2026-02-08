# Quick Start Guide

## For Developers (Using Dev Containers)

### Prerequisites
- Docker Desktop
- Visual Studio Code
- Dev Containers extension

### Steps
1. Open project in VS Code
2. Press `F1` → Select "Dev Containers: Reopen in Container"
3. Wait for container to build (first time only)
4. Run: `aspire run`
5. Access apps:
   - Designer: http://localhost:5173
   - Admin: http://localhost:5174

## For Production Deployment (Using Docker Compose)

### Prerequisites
- Docker and Docker Compose

### Steps
```bash
# 1. Clone repository
git clone https://github.com/tvdias/poc-study-designer.git
cd poc-study-designer

# 2. Configure environment
cp .env.example .env
# Edit .env with your settings

# 3. Start all services
docker compose up -d

# 4. Check status
docker compose ps

# 5. Access apps
# - Designer: http://localhost:3000
# - Admin: http://localhost:3001
# - API: http://localhost:5000
```

## For Local Development (Using Aspire)

### Prerequisites
- .NET 10.0 SDK
- Node.js 18+
- .NET Aspire 13
- Docker Desktop

### Steps
```bash
# 1. Install Aspire workload
dotnet workload install aspire

# 2. Restore dependencies
dotnet restore

# 3. Run with Aspire
aspire run

# 4. Access Aspire dashboard
# Opens automatically in browser
```

## Troubleshooting

### Dev Container Issues
- Ensure Docker is running
- Try: `Dev Containers: Rebuild Container`

### Docker Compose Issues
```bash
# View logs
docker compose logs -f

# Reset everything
docker compose down -v
docker compose up -d --build
```

### Aspire Issues
```bash
# Update Aspire
dotnet workload update

# Clean and rebuild
dotnet clean
dotnet build
```

## Next Steps

- Read [CONTAINERS.md](CONTAINERS.md) for detailed documentation
- Check [README.md](README.md) for full project information
- Review the code structure in `src/` directory