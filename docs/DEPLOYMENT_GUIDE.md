# 🚀 LoyaltySphere - Complete Deployment Guide

## 📋 Table of Contents

1. [Local Development Setup](#local-development-setup)
2. [Docker Deployment](#docker-deployment)
3. [Kubernetes Deployment](#kubernetes-deployment)
4. [Production Checklist](#production-checklist)
5. [Monitoring & Observability](#monitoring--observability)
6. [Troubleshooting](#troubleshooting)

---

## 🏠 Local Development Setup

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 15+
- Redis 7+
- RabbitMQ 3+
- Docker & Docker Compose (optional)

### Quick Start (Docker Compose)

```bash
# Clone repository
git clone https://github.com/Mostafa-SAID7/national-bank.git
cd national-bank

# Start all services
docker compose up -d

# Wait for services to be healthy (30-60 seconds)
docker compose ps

# Access application
# Frontend: http://localhost:4200
# Backend API: http://localhost:5000
# RabbitMQ UI: http://localhost:15672 (guest/guest)
```

### Manual Setup

#### 1. Database Setup

```bash
# Start PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE loyaltysphere;

# Run migrations (from project root)
cd src/Services/RewardService
dotnet ef database update

# Apply RLS policies
psql -U postgres -d loyaltysphere -f deployment/scripts/setup-rls.sql
```

#### 2. Backend Setup

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Start Reward Service
cd src/Services/RewardService
dotnet run
```

#### 3. Frontend Setup

```bash
# Navigate to frontend
cd src/Web/loyalty-sphere-ui

# Install dependencies
npm install --legacy-peer-deps

# Start development server
npm start

# Build for production
npm run build
```

---

## 🐳 Docker Deployment

### Build Docker Images

```bash
# Build backend image
docker build -t loyaltysphere/reward-service:latest \
  -f src/Services/RewardService/Dockerfile .

# Build frontend image
docker build -t loyaltysphere/frontend:latest \
  -f src/Web/loyalty-sphere-ui/Dockerfile \
  src/Web/loyalty-sphere-ui
```

### Push to Registry

```bash
# Tag images
docker tag loyaltysphere/reward-service:latest \
  ghcr.io/mostafa-said7/national-bank/reward-service:latest

docker tag loyaltysphere/frontend:latest \
  ghcr.io/mostafa-said7/national-bank/frontend:latest

# Login to GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Push images
docker push ghcr.io/mostafa-said7/national-bank/reward-service:latest
docker push ghcr.io/mostafa-said7/national-bank/frontend:latest
```

### Docker Compose Production

```bash
# Use production compose file
docker compose -f docker-compose.prod.yml up -d

# View logs
docker compose logs -f

# Scale services
docker compose up -d --scale reward-service=3

# Stop services
docker compose down
```

---

## ☸️ Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (1.25+)
- kubectl configured
- Helm 3+ (optional)
- Ingress controller (nginx)
- Cert-manager for TLS

### Step 1: Create Namespace

```bash
kubectl apply -f deployment/k8s/namespace.yaml
```

### Step 2: Configure Secrets

```bash
# Create PostgreSQL password
kubectl create secret generic loyalty-sphere-secrets \
  --from-literal=postgres-password=YOUR_SECURE_PASSWORD \
  --from-literal=rabbitmq-password=YOUR_SECURE_PASSWORD \
  -n loyalty-sphere

# Or apply from file (after updating values)
kubectl apply -f deployment/k8s/secrets.yaml
```

### Step 3: Apply ConfigMap

```bash
kubectl apply -f deployment/k8s/configmap.yaml
```

### Step 4: Deploy Infrastructure

```bash
# PostgreSQL
kubectl apply -f deployment/k8s/postgresql/

# Redis
kubectl apply -f deployment/k8s/redis/

# RabbitMQ
kubectl apply -f deployment/k8s/rabbitmq/

# Wait for infrastructure to be ready
kubectl wait --for=condition=ready pod -l app=postgresql -n loyalty-sphere --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n loyalty-sphere --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n loyalty-sphere --timeout=300s
```

### Step 5: Deploy Application

```bash
# Reward Service
kubectl apply -f deployment/k8s/reward-service/

# Frontend
kubectl apply -f deployment/k8s/frontend/

# Wait for application to be ready
kubectl wait --for=condition=ready pod -l app=reward-service -n loyalty-sphere --timeout=300s
kubectl wait --for=condition=ready pod -l app=frontend -n loyalty-sphere --timeout=300s
```

### Step 6: Verify Deployment

```bash
# Check all pods
kubectl get pods -n loyalty-sphere

# Check services
kubectl get svc -n loyalty-sphere

# Check ingress
kubectl get ingress -n loyalty-sphere

# View logs
kubectl logs -f deployment/reward-service -n loyalty-sphere
```

### Step 7: Configure DNS

```bash
# Get ingress IP
kubectl get ingress loyalty-sphere-ingress -n loyalty-sphere

# Add DNS records
# loyaltysphere.example.com -> INGRESS_IP
# api.loyaltysphere.example.com -> INGRESS_IP
```

### Step 8: Setup TLS (Optional)

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create ClusterIssuer
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

---

## ✅ Production Checklist

### Security

- [ ] Update all default passwords
- [ ] Configure OAuth2/OIDC provider
- [ ] Enable HTTPS/TLS
- [ ] Configure network policies
- [ ] Enable pod security policies
- [ ] Set up secrets management (Vault/Sealed Secrets)
- [ ] Configure RBAC
- [ ] Enable audit logging

### Performance

- [ ] Configure horizontal pod autoscaling
- [ ] Set resource limits and requests
- [ ] Enable caching (Redis)
- [ ] Configure CDN for static assets
- [ ] Optimize database indexes
- [ ] Enable connection pooling
- [ ] Configure rate limiting

### Reliability

- [ ] Set up health checks
- [ ] Configure liveness/readiness probes
- [ ] Enable automatic restarts
- [ ] Set up backup strategy
- [ ] Configure disaster recovery
- [ ] Test failover scenarios
- [ ] Document runbooks

### Monitoring

- [ ] Set up Prometheus metrics
- [ ] Configure Grafana dashboards
- [ ] Enable distributed tracing (Jaeger)
- [ ] Set up log aggregation (ELK/Loki)
- [ ] Configure alerting (PagerDuty/Slack)
- [ ] Monitor SLIs/SLOs
- [ ] Set up uptime monitoring

### Compliance

- [ ] Enable audit logging
- [ ] Configure data retention policies
- [ ] Set up backup encryption
- [ ] Document security controls
- [ ] Perform security scanning
- [ ] Review access controls
- [ ] Compliance documentation

---

## 📊 Monitoring & Observability

### Prometheus Metrics

```bash
# Install Prometheus
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring

# Access Prometheus
kubectl port-forward -n monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090
```

### Grafana Dashboards

```bash
# Access Grafana
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80

# Default credentials: admin/prom-operator
```

### Application Insights

The application exports OpenTelemetry metrics:

- Request count and duration
- Database query performance
- Cache hit/miss rates
- SignalR connection metrics
- Custom business metrics

### Log Aggregation

```bash
# Install Loki
helm repo add grafana https://grafana.github.io/helm-charts
helm install loki grafana/loki-stack -n monitoring

# View logs in Grafana
# Add Loki as data source
# Query: {namespace="loyalty-sphere"}
```

---

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Failed

```bash
# Check PostgreSQL pod
kubectl get pods -n loyalty-sphere -l app=postgresql

# View logs
kubectl logs -n loyalty-sphere deployment/postgresql

# Test connection
kubectl exec -it deployment/postgresql -n loyalty-sphere -- psql -U postgres -d loyaltysphere
```

#### 2. SignalR Not Connecting

```bash
# Check CORS settings in appsettings.json
# Verify WebSocket support in ingress
# Check firewall rules

# Test WebSocket connection
wscat -c ws://api.loyaltysphere.example.com/hubs/rewards
```

#### 3. High Memory Usage

```bash
# Check resource usage
kubectl top pods -n loyalty-sphere

# Increase memory limits
kubectl set resources deployment/reward-service \
  --limits=memory=1Gi \
  -n loyalty-sphere
```

#### 4. Slow API Responses

```bash
# Check database performance
kubectl exec -it deployment/postgresql -n loyalty-sphere -- \
  psql -U postgres -d loyaltysphere -c "SELECT * FROM pg_stat_activity;"

# Check Redis cache
kubectl exec -it deployment/redis -n loyalty-sphere -- redis-cli INFO stats

# Enable query logging
# Update appsettings.json: Serilog__MinimumLevel__Override__Microsoft.EntityFrameworkCore: "Information"
```

### Debug Commands

```bash
# Get pod details
kubectl describe pod POD_NAME -n loyalty-sphere

# View events
kubectl get events -n loyalty-sphere --sort-by='.lastTimestamp'

# Execute commands in pod
kubectl exec -it deployment/reward-service -n loyalty-sphere -- /bin/sh

# Port forward for debugging
kubectl port-forward deployment/reward-service 5000:5000 -n loyalty-sphere

# View resource usage
kubectl top nodes
kubectl top pods -n loyalty-sphere
```

### Health Check Endpoints

```bash
# Backend health
curl http://localhost:5000/health

# Backend readiness
curl http://localhost:5000/health/ready

# Frontend health
curl http://localhost:4200/health
```

---

## 🔄 Rollback Procedures

### Kubernetes Rollback

```bash
# View deployment history
kubectl rollout history deployment/reward-service -n loyalty-sphere

# Rollback to previous version
kubectl rollout undo deployment/reward-service -n loyalty-sphere

# Rollback to specific revision
kubectl rollout undo deployment/reward-service --to-revision=2 -n loyalty-sphere

# Check rollout status
kubectl rollout status deployment/reward-service -n loyalty-sphere
```

### Database Rollback

```bash
# List migrations
dotnet ef migrations list

# Rollback to specific migration
dotnet ef database update MIGRATION_NAME

# Generate rollback script
dotnet ef migrations script CURRENT_MIGRATION TARGET_MIGRATION
```

---

## 📞 Support

For issues and questions:

- GitHub Issues: https://github.com/Mostafa-SAID7/national-bank/issues
- Documentation: See README.md
- Architecture: See ADMIN_ARCHITECTURE.md

---

## 📝 Additional Resources

- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
- [.NET Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Angular Deployment](https://angular.io/guide/deployment)
- [PostgreSQL High Availability](https://www.postgresql.org/docs/current/high-availability.html)
- [RabbitMQ Clustering](https://www.rabbitmq.com/clustering.html)

---

**Last Updated**: April 16, 2026  
**Version**: 1.0.0  
**Status**: Production Ready ✅
