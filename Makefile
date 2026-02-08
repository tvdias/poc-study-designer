.PHONY: help build up down logs clean restart validate

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

build: ## Build all Docker images
	docker compose build

up: ## Start all services
	docker compose up -d

down: ## Stop all services
	docker compose down

logs: ## View logs from all services
	docker compose logs -f

clean: ## Remove all containers, volumes, and images
	docker compose down -v --rmi all

restart: ## Restart all services
	docker compose restart

validate: ## Validate Docker configurations
	@echo "Validating docker-compose.yml..."
	@docker compose config --quiet
	@echo "✓ docker-compose.yml is valid"
	@echo "Validating .devcontainer/docker-compose.yml..."
	@docker compose -f .devcontainer/docker-compose.yml config --quiet
	@echo "✓ .devcontainer/docker-compose.yml is valid"
	@echo "All configurations are valid!"

dev: ## Run with Aspire for development
	aspire run

install: ## Install development dependencies
	dotnet restore
	cd src/Designer && npm install
	cd src/Admin && npm install

test: ## Run tests
	dotnet test

api-logs: ## View API logs
	docker compose logs -f api

designer-logs: ## View Designer logs
	docker compose logs -f designer

admin-logs: ## View Admin logs
	docker compose logs -f admin

db-logs: ## View PostgreSQL logs
	docker compose logs -f postgres

shell-api: ## Open shell in API container
	docker compose exec api bash

shell-designer: ## Open shell in Designer container
	docker compose exec designer sh

shell-admin: ## Open shell in Admin container
	docker compose exec admin sh

psql: ## Connect to PostgreSQL
	docker compose exec postgres psql -U postgres -d studydb

redis-cli: ## Connect to Redis CLI
	docker compose exec redis redis-cli