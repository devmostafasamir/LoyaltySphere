# LoyaltySphere - Quick Start Guide 🚀

Get the complete loyalty platform running in **5 minutes**!

## Prerequisites

- Docker Desktop (required)
- .NET 8 SDK (optional, for local development)
- Node.js 20+ (optional, for local development)

## Option 1: Docker Compose (Recommended) ⚡

### Start Everything
```bash
docker compose up -d
```

This starts:
- ✅ PostgreSQL with RLS policies
- ✅ RabbitMQ for messaging
- ✅ Redis for caching
- ✅ Reward Service API
- ✅ Angular Frontend
- ✅ Jaeger (distributed tracing)
- ✅ Prometheus + Grafana (monitoring)

### Access Points
- **Frontend**: http://localhost:4200
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **RabbitMQ**: http://localhost:15672 (guest/guest)
- **Jaeger**: http://localhost:16686
- **Grafana**: http://localhost:3000 (admin/admin)

### Stop Everything
```bash
docker compose down
```

### Clean Up (Remove Volumes)
```bash
docker compose down -v
```

## Option 2: Local Development 💻

### Backend (.NET 8)

1. **Navigate to Reward Service**
```bash
cd src/Services/RewardService
```

2. **Restore Dependencies**
```bash
dotnet restore
```

3. **Update Database**
```bash
dotnet ef database update
```

4. **Run API**
```bash
dotnet run
```

API will be available at: http://localhost:5000

### Frontend (Angular 18)

1. **Navigate to Frontend**
```bash
cd src/Web/loyalty-sphere-ui
```

2. **Install Dependencies**
```bash
npm install
```

3. **Start Development Server**
```bash
npm start
```

Frontend will be available at: http://localhost:4200

## Testing the Application 🧪

### 1. Open Frontend
Navigate to http://localhost:4200

### 2. View Dashboard
You'll see:
- Points balance (starts at 0)
- Tier badge (Bronze)
- Real-time connection status
- Empty transaction feed

### 3. Test API with Swagger
Open http://localhost:5000/swagger

#### Enroll a Customer
```json
POST /api/v1/customers/enroll
{
  "customerId": "cust-123",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "email": "ahmed@example.com"
}
```

#### Calculate Reward (Award Points)
```json
POST /api/v1/rewards/calculate
{
  "customerId": "cust-123",
  "transactionAmount": 1000.00,
  "merchantId": "merchant-456"
}
```

Watch the dashboard update in real-time! 🎉

### 4. Test with cURL

#### Enroll Customer
```bash
curl -X POST http://localhost:5000/api/v1/customers/enroll \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-123",
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "email": "ahmed@example.com"
  }'
```

#### Award Points
```bash
curl -X POST http://localhost:5000/api/v1/rewards/calculate \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-123",
    "transactionAmount": 1000.00,
    "merchantId": "merchant-456"
  }'
```

#### Get Balance
```bash
curl http://localhost:5000/api/v1/rewards/balance/cust-123 \
  -H "X-Tenant-Id: national-bank"
```

## Multi-Tenant Testing 🏢

Test with different tenants:

### National Bank of Egypt
```bash
-H "X-Tenant-Id: national-bank"
```

### Suez Canal Bank
```bash
-H "X-Tenant-Id: suez-bank"
```

### Shell Egypt
```bash
-H "X-Tenant-Id: shell-egypt"
```

### Kellogg
```bash
-H "X-Tenant-Id: kellogg"
```

Each tenant has isolated data thanks to PostgreSQL RLS!

## Real-Time Features 🔴

### SignalR Connection
The dashboard automatically connects to SignalR hub at:
```
ws://localhost:5000/hubs/rewards
```

### Live Updates
When you award points via API:
1. ✅ Balance updates instantly
2. ✅ Reward animation plays
3. ✅ Transaction appears in feed
4. ✅ Toast notification shows

### Tier Upgrades
Award enough points to trigger tier upgrade:
- Bronze → Silver: 1,000 lifetime points
- Silver → Gold: 5,000 lifetime points
- Gold → Platinum: 10,000 lifetime points

Watch the celebration animation! 🏆

## Troubleshooting 🔧

### Docker Issues

**Ports Already in Use**
```bash
# Check what's using the port
netstat -ano | findstr :5000

# Stop the process or change port in docker-compose.yml
```

**Database Connection Failed**
```bash
# Restart PostgreSQL container
docker compose restart postgres

# Check logs
docker compose logs postgres
```

### Frontend Issues

**SignalR Not Connecting**
- Ensure API is running on port 5000
- Check browser console for errors
- Verify CORS settings in appsettings.json

**Blank Dashboard**
- Check API health: http://localhost:5000/health
- Verify tenant header is being sent
- Check browser network tab for API errors

### Backend Issues

**Database Migration Failed**
```bash
# Drop and recreate database
docker compose down -v
docker compose up -d postgres
cd src/Services/RewardService
dotnet ef database update
```

**RabbitMQ Connection Failed**
```bash
# Restart RabbitMQ
docker compose restart rabbitmq

# Check RabbitMQ management UI
# http://localhost:15672 (guest/guest)
```

## Development Workflow 🛠️

### 1. Make Backend Changes
```bash
cd src/Services/RewardService
# Edit code
dotnet run
```

### 2. Make Frontend Changes
```bash
cd src/Web/loyalty-sphere-ui
# Edit code
# Hot reload automatically updates browser
```

### 3. View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f reward-service
```

### 4. Restart Services
```bash
# Restart specific service
docker compose restart reward-service

# Restart all
docker compose restart
```

## Next Steps 📚

### Explore the Code
- **Backend**: `src/Services/RewardService/`
- **Frontend**: `src/Web/loyalty-sphere-ui/src/`
- **Infrastructure**: `docker-compose.yml`
- **Database**: `deployment/scripts/setup-rls.sql`

### Read Documentation
- **README.md**: Complete project overview
- **PROJECT_STATUS_FINAL.md**: Implementation details
- **API_LAYER_COMPLETE.md**: API documentation

### Interview Preparation
- Review 10 talking points in README.md
- Understand multi-tenancy strategy
- Practice explaining real-time architecture
- Demonstrate Outbox Pattern implementation

## Common Commands 📝

### Docker
```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# View logs
docker compose logs -f

# Rebuild images
docker compose build

# Remove volumes
docker compose down -v
```

### Backend
```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Frontend
```bash
# Install dependencies
npm install

# Start dev server
npm start

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint
```

## Support 💬

If you encounter issues:
1. Check the troubleshooting section above
2. Review logs: `docker compose logs -f`
3. Verify all services are running: `docker compose ps`
4. Check API health: http://localhost:5000/health

---

**You're all set! Enjoy exploring LoyaltySphere!** 🎉

