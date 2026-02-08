# Container Development and Deployment Guide

This guide provides detailed instructions for developing and deploying the POC Study Designer using containers.

## Development with VS Code Dev Containers

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Podman](https://podman.io/)
- [VS Code](https://code.visualstudio.com/)
- [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### Getting Started

1. Open the project in VS Code
2. Press `F1` and select `Dev Containers: Reopen in Container`
3. Wait for the container to build (first time only)
4. Once ready, the terminal will be inside the container

### What's Included

The devcontainer provides:
- **.NET 10.0 SDK** - For backend development
- **Node.js 18** - For frontend development
- **Docker-in-Docker** - Run Docker commands inside the container
- **Azure CLI** - Azure integration
- **Azure Functions Core Tools** - Function development
- **PostgreSQL 17** - Database service
- **Redis 7** - Cache service
- **Pre-configured VS Code extensions** - C#, ESLint, Prettier, etc.

### Working with the Devcontainer

Inside the devcontainer, you can use all the standard development commands:

```bash
# Restore .NET dependencies
dotnet restore

# Run the application with Aspire
aspire run

# Build frontend apps
cd src/Designer
npm install
npm run dev

# Run tests
dotnet test
```

### Port Forwarding

The following ports are automatically forwarded:
- **5000** - API (HTTP)
- **5001** - API (HTTPS)
- **5173** - Designer App
- **5174** - Admin App
- **5432** - PostgreSQL
- **6379** - Redis

## Production Deployment with Docker Compose

### Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/tvdias/poc-study-designer.git
cd poc-study-designer

# 2. Create environment file
cp .env.example .env
# Edit .env to set your passwords and configuration

# 3. Build and start all services
docker compose up -d

# 4. Check service status
docker compose ps

# 5. View logs
docker compose logs -f
```

### Services

The docker-compose setup includes:

| Service | Description | Port | Health Check |
|---------|-------------|------|--------------|
| postgres | PostgreSQL 17 database | 5432 | ✓ |
| redis | Redis 7 cache | 6379 | ✓ |
| api | .NET API service | 5000 | ✓ |
| designer | Designer frontend (nginx) | 3000 | - |
| admin | Admin frontend (nginx) | 3001 | - |

### Accessing the Application

After starting with `docker compose up -d`:

- **Designer App**: http://localhost:3000
- **Admin App**: http://localhost:3001
- **API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/scalar/v1

### Managing Services

```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# View logs for all services
docker compose logs -f

# View logs for specific service
docker compose logs -f api

# Restart a service
docker compose restart api

# Rebuild and restart services
docker compose up -d --build

# Remove all containers and volumes
docker compose down -v
```

### Environment Variables

Create a `.env` file based on `.env.example`:

```bash
# PostgreSQL Configuration
POSTGRES_PASSWORD=your-secure-password

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
```

### Database Migrations

Apply database migrations after first startup:

```bash
# Access the API container
docker compose exec api bash

# Run migrations
dotnet ef database update

# Exit the container
exit
```

### Scaling Services

You can scale services as needed:

```bash
# Scale API to 3 instances
docker compose up -d --scale api=3
```

## Building Individual Images

### Build Frontend Images

```bash
# Designer
cd src/Designer
docker build -t poc-study-designer-designer:latest .

# Admin
cd src/Admin
docker build -t poc-study-designer-admin:latest .
```

### Build API Image

```bash
cd src
docker build -f Api/Dockerfile -t poc-study-designer-api:latest .
```

## Troubleshooting

### Container Won't Start

Check logs:
```bash
docker compose logs [service-name]
```

### Database Connection Issues

Ensure PostgreSQL is ready:
```bash
docker compose exec postgres pg_isready -U postgres
```

### Port Already in Use

Change the port mapping in `docker-compose.yml`:
```yaml
ports:
  - "3000:80"  # Change 3000 to another port
```

### Clear Everything and Start Fresh

```bash
# Stop and remove all containers, networks, and volumes
docker compose down -v

# Remove all images
docker compose down --rmi all

# Start fresh
docker compose up -d --build
```

## Production Considerations

### Security

1. **Use strong passwords** - Update `POSTGRES_PASSWORD` in `.env`
2. **Use HTTPS** - Configure SSL certificates for production
3. **Use secrets management** - Consider Docker secrets or Azure Key Vault
4. **Update base images regularly** - Keep dependencies up to date

### Performance

1. **Resource limits** - Set memory and CPU limits in docker-compose.yml
2. **Use production-optimized images** - Multi-stage builds are already configured
3. **Enable caching** - Redis is configured for output caching

### Monitoring

1. **Health checks** - Already configured for database and API
2. **Logging** - Use `docker compose logs` or a centralized logging solution
3. **Metrics** - Consider integrating with Prometheus/Grafana

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Push Docker Images

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Build images
      run: docker compose build
    
    - name: Push to registry
      run: |
        docker compose push
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [VS Code Dev Containers](https://code.visualstudio.com/docs/devcontainers/containers)
- [.NET Docker Documentation](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
