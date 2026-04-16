# ============================================
# LoyaltySphere - Makefile
# ============================================
# Convenient commands for Docker operations
# ============================================

.PHONY: help build push dev prod clean logs test

# Default target
help:
	@echo "============================================"
	@echo "  LoyaltySphere - Available Commands"
	@echo "============================================"
	@echo ""
	@echo "Development:"
	@echo "  make dev          - Start development environment"
	@echo "  make dev-build    - Build and start development environment"
	@echo "  make logs         - View logs from all services"
	@echo "  make logs-api     - View logs from reward service"
	@echo "  make logs-ui      - View logs from frontend"
	@echo ""
	@echo "Production:"
	@echo "  make prod         - Start production environment"
	@echo "  make prod-pull    - Pull latest images and start"
	@echo ""
	@echo "Docker Build & Push:"
	@echo "  make build        - Build all Docker images"
	@echo "  make push         - Build and push to Docker Hub"
	@echo "  make push VERSION=v1.0.0 - Push with specific version"
	@echo ""
	@echo "Cleanup:"
	@echo "  make stop         - Stop all services"
	@echo "  make clean        - Stop and remove containers"
	@echo "  make clean-all    - Stop, remove containers and volumes"
	@echo ""
	@echo "Testing:"
	@echo "  make test         - Run all tests"
	@echo "  make test-api     - Run API tests"
	@echo ""
	@echo "Utilities:"
	@echo "  make ps           - Show running containers"
	@echo "  make shell-api    - Open shell in reward service"
	@echo "  make shell-db     - Open PostgreSQL shell"
	@echo ""

# Development commands
dev:
	docker compose up -d

dev-build:
	docker compose up -d --build

# Production commands
prod:
	docker compose -f docker-compose.prod.yml up -d

prod-pull:
	docker compose -f docker-compose.prod.yml pull
	docker compose -f docker-compose.prod.yml up -d

# Build and push
build:
	docker compose build

push:
	@chmod +x build-and-push.sh
	@./build-and-push.sh $(VERSION)

# Logs
logs:
	docker compose logs -f

logs-api:
	docker compose logs -f reward-service

logs-ui:
	docker compose logs -f frontend

# Cleanup
stop:
	docker compose down
	docker compose -f docker-compose.prod.yml down

clean:
	docker compose down --remove-orphans
	docker compose -f docker-compose.prod.yml down --remove-orphans

clean-all:
	docker compose down -v --remove-orphans
	docker compose -f docker-compose.prod.yml down -v --remove-orphans

# Testing
test:
	dotnet test LoyaltySphere.sln

test-api:
	dotnet test src/Services/RewardService/Tests/

# Utilities
ps:
	docker compose ps

shell-api:
	docker exec -it loyalty-reward-service /bin/bash

shell-db:
	docker exec -it loyalty-postgres psql -U postgres -d loyalty_sphere

# Database operations
db-migrate:
	docker exec -it loyalty-reward-service dotnet ef database update

db-reset:
	docker compose down postgres
	docker volume rm loyalty-postgres-data
	docker compose up -d postgres

# Health checks
health:
	@echo "Checking service health..."
	@curl -f http://localhost:5001/health || echo "Reward Service: DOWN"
	@curl -f http://localhost:4200 || echo "Frontend: DOWN"
	@docker exec loyalty-postgres pg_isready -U postgres || echo "PostgreSQL: DOWN"
	@docker exec loyalty-rabbitmq rabbitmq-diagnostics ping || echo "RabbitMQ: DOWN"
	@docker exec loyalty-redis redis-cli ping || echo "Redis: DOWN"
