# 🐳 Docker Quick Start Guide

Quick reference for building and pushing LoyaltySphere Docker images.

---

## 🚀 Quick Commands

### Build & Push to Docker Hub

```bash
# Method 1: Shell Script (Recommended)
./build-and-push.sh v1.0.0

# Method 2: Makefile
make push VERSION=v1.0.0

# Method 3: Manual
docker build -t msaid356/loyalty-sphere:reward-service-v1.0.0 -f src/Services/RewardService/Dockerfile .
docker push msaid356/loyalty-sphere:reward-service-v1.0.0
```

### Run Locally (Development)

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

### Run from Docker Hub (Production)

```bash
# Pull and start
docker compose -f docker-compose.prod.yml up -d

# View logs
docker compose -f docker-compose.prod.yml logs -f

# Stop services
docker compose -f docker-compose.prod.yml down
```

---

## 📦 Your Docker Hub Images

**Repository:** `msaid356/loyalty-sphere`

**Available Images:**
- `msaid356/loyalty-sphere:reward-service-latest`
- `msaid356/loyalty-sphere:reward-service-v1.0.0`
- `msaid356/loyalty-sphere:frontend-latest`
- `msaid356/loyalty-sphere:frontend-v1.0.0`

---

## 🔑 First Time Setup

### 1. Login to Docker Hub

```bash
docker login
# Username: msaid356
# Password: <your-docker-hub-access-token>
```

### 2. Make Script Executable

```bash
chmod +x build-and-push.sh
```

### 3. Build and Push

```bash
./build-and-push.sh v1.0.0
```

---

## 📋 Common Tasks

### Build Specific Service

```bash
# Reward Service only
docker build -t msaid356/loyalty-sphere:reward-service-latest \
  -f src/Services/RewardService/Dockerfile .

# Frontend only
docker build -t msaid356/loyalty-sphere:frontend-latest \
  -f src/Web/loyalty-sphere-ui/Dockerfile \
  src/Web/loyalty-sphere-ui
```

### Push Specific Service

```bash
# Reward Service
docker push msaid356/loyalty-sphere:reward-service-latest

# Frontend
docker push msaid356/loyalty-sphere:frontend-latest
```

### Pull Images

```bash
# Pull all
docker compose -f docker-compose.prod.yml pull

# Pull specific
docker pull msaid356/loyalty-sphere:reward-service-latest
```

### Check Images

```bash
# List local images
docker images msaid356/loyalty-sphere

# Check image size
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
```

---

## 🎯 Makefile Commands

```bash
make help          # Show all commands
make dev           # Start development
make prod          # Start production
make build         # Build all images
make push          # Build and push to Docker Hub
make logs          # View all logs
make clean         # Stop and remove containers
make health        # Check service health
```

---

## 🔍 Troubleshooting

### Docker Login Issues

```bash
# Use access token, not password
docker login -u msaid356
# Password: <paste-your-access-token>
```

### Build Fails

```bash
# Check Docker is running
docker info

# Clean build cache
docker builder prune -a

# Rebuild without cache
docker compose build --no-cache
```

### Push Fails

```bash
# Check you're logged in
docker login

# Check image exists
docker images msaid356/loyalty-sphere

# Try pushing again
docker push msaid356/loyalty-sphere:reward-service-latest
```

---

## 📚 Full Documentation

For detailed documentation, see:
- **[DOCKER_GUIDE.md](docs/DOCKER_GUIDE.md)** - Complete Docker guide
- **[DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md)** - Deployment instructions
- **[QUICK_START.md](docs/QUICK_START.md)** - Project quick start

---

## 🌐 Access Services

After starting with `docker compose up -d`:

| Service | URL | Credentials |
|---------|-----|-------------|
| Frontend | http://localhost:4200 | - |
| Reward API | http://localhost:5001 | - |
| RabbitMQ | http://localhost:15672 | guest/guest |
| Jaeger | http://localhost:16686 | - |
| Prometheus | http://localhost:9090 | - |
| Grafana | http://localhost:3000 | admin/admin |

---

## ✅ Verification

```bash
# Check all services are running
docker compose ps

# Check service health
make health

# Test API
curl http://localhost:5001/health

# Test Frontend
curl http://localhost:4200
```

---

**Quick Help:** Run `make help` for all available commands
