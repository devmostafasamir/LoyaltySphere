# Docker Build & Push Guide

Complete guide for building and pushing LoyaltySphere Docker images to Docker Hub.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Build & Push Methods](#build--push-methods)
4. [Docker Hub Images](#docker-hub-images)
5. [Production Deployment](#production-deployment)
6. [CI/CD Integration](#cicd-integration)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Tools

```bash
# Docker (version 24.0+)
docker --version

# Docker Compose (version 2.20+)
docker compose version

# Make (optional, for convenience)
make --version
```

### Docker Hub Account

1. Create account at https://hub.docker.com
2. Create access token:
   - Go to Account Settings → Security → New Access Token
   - Name: `loyalty-sphere-ci`
   - Permissions: Read, Write, Delete
   - Save token securely

3. Login to Docker Hub:
```bash
docker login
# Username: msaid356
# Password: <your-access-token>
```

---

## Quick Start

### Method 1: Using Shell Script (Recommended)

```bash
# Make script executable
chmod +x build-and-push.sh

# Build and push with version tag
./build-and-push.sh v1.0.0

# Build and push as latest
./build-and-push.sh
```

### Method 2: Using Makefile

```bash
# Build and push with version
make push VERSION=v1.0.0

# Build and push as latest
make push
```

### Method 3: Manual Commands

```bash
# Build Reward Service
docker build -t msaid356/loyalty-sphere:reward-service-v1.0.0 \
  -t msaid356/loyalty-sphere:reward-service-latest \
  -f src/Services/RewardService/Dockerfile .

# Push Reward Service
docker push msaid356/loyalty-sphere:reward-service-v1.0.0
docker push msaid356/loyalty-sphere:reward-service-latest

# Build Frontend
docker build -t msaid356/loyalty-sphere:frontend-v1.0.0 \
  -t msaid356/loyalty-sphere:frontend-latest \
  -f src/Web/loyalty-sphere-ui/Dockerfile \
  src/Web/loyalty-sphere-ui

# Push Frontend
docker push msaid356/loyalty-sphere:frontend-v1.0.0
docker push msaid356/loyalty-sphere:frontend-latest
```

---

## Build & Push Methods

### Shell Script Features

The `build-and-push.sh` script provides:

✅ **Automated Build & Push**
- Builds all services sequentially
- Tags with version and latest
- Pushes to Docker Hub automatically

✅ **Error Handling**
- Checks Docker is running
- Validates Docker Hub login
- Exits on build failures

✅ **Colored Output**
- Info (Blue)
- Success (Green)
- Warning (Yellow)
- Error (Red)

✅ **Multi-tagging**
- Version tag: `reward-service-v1.0.0`
- Latest tag: `reward-service-latest`

### Script Usage

```bash
# Basic usage
./build-and-push.sh

# With version
./build-and-push.sh v1.0.0

# With semantic versioning
./build-and-push.sh v1.2.3

# With date-based versioning
./build-and-push.sh $(date +%Y%m%d)
```

---

## Docker Hub Images

### Repository Structure

```
msaid356/loyalty-sphere
├── reward-service-latest
├── reward-service-v1.0.0
├── reward-service-v1.0.1
├── frontend-latest
├── frontend-v1.0.0
└── frontend-v1.0.1
```

### Image Tags

| Tag Pattern | Description | Example |
|------------|-------------|---------|
| `{service}-latest` | Latest stable build | `reward-service-latest` |
| `{service}-v{version}` | Semantic version | `reward-service-v1.2.3` |
| `{service}-{date}` | Date-based version | `reward-service-20260416` |
| `{service}-{commit}` | Git commit SHA | `reward-service-abc1234` |

### Pulling Images

```bash
# Pull latest
docker pull msaid356/loyalty-sphere:reward-service-latest
docker pull msaid356/loyalty-sphere:frontend-latest

# Pull specific version
docker pull msaid356/loyalty-sphere:reward-service-v1.0.0
docker pull msaid356/loyalty-sphere:frontend-v1.0.0

# Pull all images
docker compose -f docker-compose.prod.yml pull
```

---

## Production Deployment

### Using Pre-built Images

```bash
# 1. Pull latest images
docker compose -f docker-compose.prod.yml pull

# 2. Start services
docker compose -f docker-compose.prod.yml up -d

# 3. Verify health
make health
```

### Environment Configuration

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Update with production values
nano .env

# 3. Start with environment file
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

### Production Checklist

- [ ] Update `.env` with secure passwords
- [ ] Enable SSL/TLS certificates
- [ ] Configure firewall rules
- [ ] Set up monitoring (Prometheus/Grafana)
- [ ] Configure log aggregation
- [ ] Set up automated backups
- [ ] Enable health checks
- [ ] Configure resource limits
- [ ] Set up auto-restart policies
- [ ] Test disaster recovery

---

## CI/CD Integration

### GitHub Actions

The project includes automated CI/CD in `.github/workflows/ci-cd.yml`:

**Triggers:**
- Push to `main` branch
- Pull requests to `main`
- Manual workflow dispatch

**Jobs:**
1. **Build & Test** - Compile and test .NET code
2. **Build Docker Images** - Build all Docker images
3. **Push to Docker Hub** - Push images on main branch
4. **Deploy** - Deploy to production (manual approval)

**Secrets Required:**
```yaml
DOCKER_USERNAME: msaid356
DOCKER_PASSWORD: <docker-hub-access-token>
```

### Setting Up GitHub Secrets

```bash
# 1. Go to GitHub repository
# 2. Settings → Secrets and variables → Actions
# 3. Add secrets:
#    - DOCKER_USERNAME: msaid356
#    - DOCKER_PASSWORD: <your-docker-hub-token>
```

### Manual Workflow Trigger

```bash
# Via GitHub UI
# Actions → Docker Build & Push → Run workflow

# Via GitHub CLI
gh workflow run ci-cd.yml
```

---

## Troubleshooting

### Common Issues

#### 1. Docker Login Failed

```bash
# Error: unauthorized: incorrect username or password
# Solution: Use access token instead of password
docker login -u msaid356
# Password: <paste-access-token>
```

#### 2. Build Context Too Large

```bash
# Error: Sending build context to Docker daemon  2.5GB
# Solution: Add to .dockerignore
echo "node_modules/" >> .dockerignore
echo "bin/" >> .dockerignore
echo "obj/" >> .dockerignore
```

#### 3. Multi-platform Build

```bash
# Build for multiple architectures
docker buildx create --use
docker buildx build --platform linux/amd64,linux/arm64 \
  -t msaid356/loyalty-sphere:reward-service-latest \
  --push \
  -f src/Services/RewardService/Dockerfile .
```

#### 4. Image Size Too Large

```bash
# Check image size
docker images msaid356/loyalty-sphere

# Optimize Dockerfile:
# - Use multi-stage builds ✅ (already implemented)
# - Use alpine base images ✅ (already implemented)
# - Remove unnecessary files
# - Combine RUN commands
```

#### 5. Push Rate Limit

```bash
# Error: toomanyrequests: You have reached your pull rate limit
# Solution: Login to Docker Hub (increases rate limit)
docker login
```

### Verification Commands

```bash
# Check Docker daemon
docker info

# Check images
docker images msaid356/loyalty-sphere

# Check running containers
docker ps

# Check logs
docker compose logs -f reward-service

# Test image locally
docker run -p 5001:80 msaid356/loyalty-sphere:reward-service-latest
```

### Cleanup Commands

```bash
# Remove unused images
docker image prune -a

# Remove all stopped containers
docker container prune

# Remove unused volumes
docker volume prune

# Full cleanup
docker system prune -a --volumes
```

---

## Best Practices

### Versioning Strategy

```bash
# Semantic Versioning (Recommended)
v1.0.0  # Major.Minor.Patch
v1.0.1  # Bug fix
v1.1.0  # New feature
v2.0.0  # Breaking change

# Date-based Versioning
20260416  # YYYYMMDD

# Git-based Versioning
git-abc1234  # Git commit SHA
```

### Security Best Practices

1. **Use Access Tokens** - Never use passwords
2. **Scan Images** - Use `docker scan` or Trivy
3. **Non-root User** - Run containers as non-root ✅
4. **Minimal Base Images** - Use alpine variants ✅
5. **Multi-stage Builds** - Reduce attack surface ✅
6. **Health Checks** - Monitor container health ✅
7. **Resource Limits** - Set CPU/memory limits ✅
8. **Secrets Management** - Use Docker secrets or env files

### Performance Optimization

1. **Layer Caching** - Order Dockerfile commands efficiently ✅
2. **Multi-stage Builds** - Separate build and runtime ✅
3. **Parallel Builds** - Use BuildKit
4. **Image Compression** - Use `--compress` flag
5. **Registry Mirror** - Use Docker Hub mirror

---

## Additional Resources

- [Docker Hub Repository](https://hub.docker.com/r/msaid356/loyalty-sphere)
- [Docker Documentation](https://docs.docker.com)
- [Docker Compose Documentation](https://docs.docker.com/compose)
- [Dockerfile Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices)
- [Multi-stage Builds](https://docs.docker.com/build/building/multi-stage)

---

## Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section
2. Review Docker logs: `docker compose logs -f`
3. Check GitHub Issues
4. Contact: msaid356@dockerhub

---

**Last Updated:** April 16, 2026
**Version:** 1.0.0
