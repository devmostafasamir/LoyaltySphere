# 🚀 Render.com Deployment Guide

Complete guide for deploying LoyaltySphere to Render.com.

**Instance:** loyalty-sphere (capybara)  
**Region:** AWS AP-NorthEast-1 (Tokyo) / Singapore  
**Plan:** Loyal Lemming (Starter)

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Deploy](#quick-deploy)
3. [Manual Setup](#manual-setup)
4. [Configuration](#configuration)
5. [Environment Variables](#environment-variables)
6. [Custom Domain](#custom-domain)
7. [Monitoring](#monitoring)
8. [Troubleshooting](#troubleshooting)

---

## 🎯 Prerequisites

### Required

- ✅ GitHub account with repository
- ✅ Render.com account (free tier available)
- ✅ Docker Hub account (for pre-built images)

### Optional

- Custom domain name
- Render CLI for local deployment

---

## 🚀 Quick Deploy

### Method 1: Blueprint (Recommended)

1. **Connect GitHub Repository**
   ```
   https://dashboard.render.com/select-repo
   ```

2. **Create Blueprint**
   - Click "New" → "Blueprint"
   - Select your repository
   - Render will detect `render.yaml`
   - Click "Apply"

3. **Wait for Deployment**
   - Render creates all services automatically
   - PostgreSQL database
   - Redis cache
   - Reward Service (Backend)
   - Frontend (Angular)

4. **Access Application**
   ```
   Frontend: https://loyalty-sphere-ui.onrender.com
   Backend:  https://loyalty-reward-service.onrender.com
   ```

### Method 2: Docker Hub Images

1. **Pull from Docker Hub**
   ```bash
   docker pull msaid356/loyalty-sphere:reward-service-latest
   docker pull msaid356/loyalty-sphere:frontend-latest
   ```

2. **Create Web Service**
   - New → Web Service
   - Docker → External Image
   - Image URL: `msaid356/loyalty-sphere:reward-service-latest`
   - Region: Singapore
   - Plan: Starter

3. **Configure Environment**
   - Add environment variables (see below)
   - Set health check path: `/health`
   - Enable auto-deploy

---

## 🛠️ Manual Setup

### Step 1: Create PostgreSQL Database

```bash
# Via Render Dashboard
1. New → PostgreSQL
2. Name: loyalty-postgres
3. Database: loyalty_sphere
4. User: postgres
5. Region: Singapore
6. Plan: Starter (Free)
7. Create Database
```

**Connection String:**
```
postgres://postgres:PASSWORD@HOST:5432/loyalty_sphere
```

### Step 2: Create Redis Cache

```bash
# Via Render Dashboard
1. New → Redis
2. Name: loyalty-redis
3. Region: Singapore
4. Plan: Starter (Free)
5. Maxmemory Policy: allkeys-lru
6. Create Redis
```

**Connection String:**
```
redis://red-XXXXX:6379
```

### Step 3: Deploy Backend (Reward Service)

```bash
# Via Render Dashboard
1. New → Web Service
2. Connect Repository: loyalty-sphere
3. Name: loyalty-reward-service
4. Environment: Docker
5. Region: Singapore
6. Branch: main
7. Dockerfile Path: src/Services/RewardService/Dockerfile
8. Docker Context: .
9. Plan: Starter ($7/month)
10. Advanced:
    - Health Check Path: /health
    - Auto-Deploy: Yes
```

**Environment Variables:**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=<postgres-connection-string>
Redis__ConnectionString=<redis-connection-string>
CORS__AllowedOrigins=https://loyalty-sphere-ui.onrender.com
```

### Step 4: Deploy Frontend (Angular)

```bash
# Via Render Dashboard
1. New → Web Service
2. Connect Repository: loyalty-sphere
3. Name: loyalty-sphere-ui
4. Environment: Docker
5. Region: Singapore
6. Branch: main
7. Dockerfile Path: src/Web/loyalty-sphere-ui/Dockerfile.render
8. Docker Context: src/Web/loyalty-sphere-ui
9. Plan: Starter ($7/month)
10. Advanced:
    - Health Check Path: /
    - Auto-Deploy: Yes
```

**Environment Variables:**
```bash
NODE_ENV=production
API_URL=https://loyalty-reward-service.onrender.com
```

---

## ⚙️ Configuration

### render.yaml Structure

```yaml
services:
  - type: pserv          # PostgreSQL
  - type: redis          # Redis Cache
  - type: web            # Backend API
  - type: web            # Frontend
```

### Service Dependencies

```
Frontend → Backend → PostgreSQL
                  → Redis
```

### Auto-Deploy Triggers

- Push to `main` branch
- Pull request merge
- Manual deploy via dashboard

---

## 🔐 Environment Variables

### Backend (Reward Service)

| Variable | Value | Required |
|----------|-------|----------|
| `ASPNETCORE_ENVIRONMENT` | Production | ✅ |
| `ASPNETCORE_URLS` | http://+:80 | ✅ |
| `ConnectionStrings__DefaultConnection` | From PostgreSQL | ✅ |
| `Redis__ConnectionString` | From Redis | ✅ |
| `CORS__AllowedOrigins` | Frontend URL | ✅ |
| `JWT__Secret` | Generate secure key | ✅ |
| `JWT__Issuer` | loyalty-sphere | ✅ |
| `JWT__Audience` | loyalty-sphere-api | ✅ |

### Frontend (Angular)

| Variable | Value | Required |
|----------|-------|----------|
| `NODE_ENV` | production | ✅ |
| `API_URL` | Backend URL | ✅ |

### Generating Secrets

```bash
# JWT Secret (256-bit)
openssl rand -base64 32

# Or use online generator
https://generate-secret.vercel.app/32
```

---

## 🌐 Custom Domain

### Add Custom Domain

1. **Go to Service Settings**
   ```
   Dashboard → Service → Settings → Custom Domain
   ```

2. **Add Domain**
   ```
   Frontend: app.yourdomain.com
   Backend:  api.yourdomain.com
   ```

3. **Configure DNS**
   ```
   Type: CNAME
   Name: app
   Value: loyalty-sphere-ui.onrender.com
   
   Type: CNAME
   Name: api
   Value: loyalty-reward-service.onrender.com
   ```

4. **Enable HTTPS**
   - Render automatically provisions SSL certificates
   - Force HTTPS redirect enabled by default

### Update CORS

```bash
# Update backend environment variable
CORS__AllowedOrigins=https://app.yourdomain.com
```

---

## 📊 Monitoring

### Render Dashboard

**Metrics Available:**
- CPU usage
- Memory usage
- Request count
- Response time
- Error rate
- Deployment history

**Access Logs:**
```bash
# Via Dashboard
Service → Logs → View Logs

# Via CLI
render logs -s loyalty-reward-service
render logs -s loyalty-sphere-ui
```

### Health Checks

**Backend:**
```bash
curl https://loyalty-reward-service.onrender.com/health
```

**Frontend:**
```bash
curl https://loyalty-sphere-ui.onrender.com/health
```

### Alerts

Configure alerts in Render Dashboard:
- Service down
- High CPU usage
- High memory usage
- Deployment failures

---

## 🐛 Troubleshooting

### Common Issues

#### 1. Service Won't Start

**Symptoms:**
- Service stuck in "Deploying" state
- Build fails
- Container crashes

**Solutions:**
```bash
# Check logs
render logs -s loyalty-reward-service --tail 100

# Verify Dockerfile
docker build -t test -f src/Services/RewardService/Dockerfile .

# Check environment variables
render env list -s loyalty-reward-service
```

#### 2. Database Connection Failed

**Symptoms:**
- Backend returns 500 errors
- Logs show "Connection refused"

**Solutions:**
```bash
# Verify connection string
echo $ConnectionStrings__DefaultConnection

# Test connection
psql $DATABASE_URL

# Check PostgreSQL service status
render ps -s loyalty-postgres
```

#### 3. Frontend Can't Connect to Backend

**Symptoms:**
- API calls fail with CORS errors
- Network errors in browser console

**Solutions:**
```bash
# Check API_URL environment variable
render env get API_URL -s loyalty-sphere-ui

# Verify CORS configuration
curl -H "Origin: https://loyalty-sphere-ui.onrender.com" \
  https://loyalty-reward-service.onrender.com/health

# Update CORS settings
render env set CORS__AllowedOrigins=https://loyalty-sphere-ui.onrender.com \
  -s loyalty-reward-service
```

#### 4. Build Timeout

**Symptoms:**
- Build exceeds 15 minutes
- "Build timed out" error

**Solutions:**
```bash
# Optimize Dockerfile
# - Use multi-stage builds ✅
# - Cache dependencies ✅
# - Minimize layers

# Use pre-built images
# Instead of building on Render, use Docker Hub images
docker pull msaid356/loyalty-sphere:reward-service-latest
```

#### 5. Out of Memory

**Symptoms:**
- Service crashes randomly
- "OOMKilled" in logs

**Solutions:**
```bash
# Upgrade plan
# Starter: 512 MB RAM
# Standard: 2 GB RAM
# Pro: 4 GB RAM

# Optimize application
# - Reduce memory usage
# - Enable garbage collection
# - Use caching wisely
```

---

## 💰 Pricing

### Free Tier

- ✅ 750 hours/month of free services
- ✅ PostgreSQL: 90 days free, then $7/month
- ✅ Redis: 90 days free, then $10/month
- ✅ Static sites: Free forever

### Paid Plans

| Plan | Price | RAM | CPU | Bandwidth |
|------|-------|-----|-----|-----------|
| Starter | $7/month | 512 MB | 0.5 CPU | 100 GB |
| Standard | $25/month | 2 GB | 1 CPU | 400 GB |
| Pro | $85/month | 4 GB | 2 CPU | 1000 GB |

**Total Monthly Cost (Starter):**
- Backend: $7
- Frontend: $7
- PostgreSQL: $7
- Redis: $10
- **Total: $31/month**

---

## 🚀 Performance Optimization

### Backend Optimization

```csharp
// Enable response compression
builder.Services.AddResponseCompression();

// Configure caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Enable HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        https.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});
```

### Frontend Optimization

```typescript
// Enable production mode
enableProdMode();

// Lazy load modules
const routes: Routes = [
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.module')
      .then(m => m.DashboardModule)
  }
];

// Use OnPush change detection
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

### Database Optimization

```sql
-- Add indexes
CREATE INDEX idx_customers_tenant_id ON customers(tenant_id);
CREATE INDEX idx_rewards_customer_id ON rewards(customer_id);
CREATE INDEX idx_campaigns_active ON campaigns(is_active, start_date, end_date);

-- Enable connection pooling
-- Already configured in Entity Framework Core
```

---

## 📚 Additional Resources

### Render Documentation

- [Render Docs](https://render.com/docs)
- [Blueprint Spec](https://render.com/docs/blueprint-spec)
- [Docker Deployment](https://render.com/docs/docker)
- [Environment Variables](https://render.com/docs/environment-variables)

### LoyaltySphere Documentation

- [Docker Guide](DOCKER_GUIDE.md)
- [Deployment Guide](DEPLOYMENT_GUIDE.md)
- [Quick Start](QUICK_START.md)

---

## ✅ Deployment Checklist

Before going live:

- [ ] All services deployed successfully
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Health checks passing
- [ ] CORS configured correctly
- [ ] Custom domain configured (optional)
- [ ] SSL certificates active
- [ ] Monitoring alerts set up
- [ ] Backup strategy in place
- [ ] Load testing completed
- [ ] Security audit passed

---

## 🎉 Success!

Your LoyaltySphere application is now live on Render!

**Access URLs:**
- Frontend: https://loyalty-sphere-ui.onrender.com
- Backend: https://loyalty-reward-service.onrender.com
- Swagger: https://loyalty-reward-service.onrender.com/swagger

**Next Steps:**
1. Configure custom domain
2. Set up monitoring alerts
3. Enable auto-scaling (Pro plan)
4. Configure backups
5. Run load tests

---

**Instance ID:** d41d1e79-b85a-4e54-b33f-bf5b1279901c  
**Created:** April 16, 2026 02:59:02 UTC  
**Region:** AWS AP-NorthEast-1 (Tokyo)

---

*Deployment guide last updated: April 16, 2026*
