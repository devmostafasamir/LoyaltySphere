# LoyaltySphere - Testing Guide 🧪

Complete guide for testing the loyalty platform end-to-end.

## Quick Test Scenarios

### Scenario 1: New Customer Journey (5 minutes)

#### Step 1: Start Services
```bash
docker compose up -d
```

#### Step 2: Open Frontend
Navigate to http://localhost:4200

You should see:
- ✅ Dashboard with 0 points
- ✅ Bronze tier badge
- ✅ Connection status (green = connected)
- ✅ Empty transaction feed

#### Step 3: Enroll Customer (Swagger)
1. Open http://localhost:5000/swagger
2. Find `POST /api/v1/customers/enroll`
3. Click "Try it out"
4. Use this payload:
```json
{
  "customerId": "cust-demo-001",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "email": "ahmed.hassan@example.com"
}
```
5. Add header: `X-Tenant-Id: national-bank`
6. Click "Execute"
7. Verify response: `201 Created`

#### Step 4: Award Points (First Transaction)
1. Find `POST /api/v1/rewards/calculate`
2. Click "Try it out"
3. Use this payload:
```json
{
  "customerId": "cust-demo-001",
  "transactionAmount": 500.00,
  "merchantId": "merchant-001",
  "merchantCategory": "Retail"
}
```
4. Add header: `X-Tenant-Id: national-bank`
5. Click "Execute"

#### Step 5: Watch Real-Time Magic! ✨
Switch to the dashboard (http://localhost:4200):
- ✅ Points balance updates instantly (50 points)
- ✅ Reward animation plays (sparkle effect)
- ✅ Toast notification appears
- ✅ Transaction appears in feed
- ✅ Progress bar updates

#### Step 6: Award More Points (Tier Upgrade)
Repeat Step 4 with larger amounts to trigger tier upgrade:
```json
{
  "customerId": "cust-demo-001",
  "transactionAmount": 10000.00,
  "merchantId": "merchant-001"
}
```

Watch for:
- ✅ Tier upgrade celebration (🏆)
- ✅ Tier badge changes (Bronze → Silver)
- ✅ Special toast notification

### Scenario 2: Multi-Tenant Isolation (3 minutes)

#### Test Data Isolation
1. Create customer in Tenant A (national-bank):
```bash
curl -X POST http://localhost:5000/api/v1/customers/enroll \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-tenant-a",
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "email": "ahmed@tenanta.com"
  }'
```

2. Try to access from Tenant B (suez-bank):
```bash
curl http://localhost:5000/api/v1/rewards/balance/cust-tenant-a \
  -H "X-Tenant-Id: suez-bank"
```

Expected: `404 Not Found` (data isolation working!)

3. Access from correct tenant:
```bash
curl http://localhost:5000/api/v1/rewards/balance/cust-tenant-a \
  -H "X-Tenant-Id: national-bank"
```

Expected: `200 OK` with balance data

### Scenario 3: Points Redemption (2 minutes)

#### Step 1: Award Points
```bash
curl -X POST http://localhost:5000/api/v1/rewards/calculate \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-demo-001",
    "transactionAmount": 1000.00
  }'
```

#### Step 2: Redeem Points
```bash
curl -X POST http://localhost:5000/api/v1/rewards/redeem \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-demo-001",
    "pointsToRedeem": 50,
    "redemptionType": "Cashback",
    "redemptionDetails": "Bank account credit"
  }'
```

Watch dashboard:
- ✅ Balance decreases
- ✅ Redemption appears in feed
- ✅ Toast notification shows

## API Testing with cURL

### Customer Management

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

#### Get Customer Details
```bash
curl http://localhost:5000/api/v1/customers/cust-123 \
  -H "X-Tenant-Id: national-bank"
```

#### Update Customer
```bash
curl -X PUT http://localhost:5000/api/v1/customers/cust-123 \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "email": "ahmed.new@example.com",
    "phoneNumber": "+20123456789"
  }'
```

#### Deactivate Customer
```bash
curl -X POST http://localhost:5000/api/v1/customers/cust-123/deactivate \
  -H "X-Tenant-Id: national-bank"
```

#### Reactivate Customer
```bash
curl -X POST http://localhost:5000/api/v1/customers/cust-123/reactivate \
  -H "X-Tenant-Id: national-bank"
```

#### List Customers (Paginated)
```bash
curl "http://localhost:5000/api/v1/customers?pageNumber=1&pageSize=10" \
  -H "X-Tenant-Id: national-bank"
```

### Reward Operations

#### Calculate Reward
```bash
curl -X POST http://localhost:5000/api/v1/rewards/calculate \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-123",
    "transactionAmount": 1000.00,
    "currency": "EGP",
    "merchantId": "merchant-456",
    "merchantCategory": "Retail"
  }'
```

#### Redeem Points
```bash
curl -X POST http://localhost:5000/api/v1/rewards/redeem \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-123",
    "pointsToRedeem": 100,
    "redemptionType": "Cashback"
  }'
```

#### Get Balance
```bash
curl http://localhost:5000/api/v1/rewards/balance/cust-123 \
  -H "X-Tenant-Id: national-bank"
```

#### Get History
```bash
curl "http://localhost:5000/api/v1/rewards/history/cust-123?pageNumber=1&pageSize=20" \
  -H "X-Tenant-Id: national-bank"
```

## SignalR Testing

### Test with Browser Console

1. Open http://localhost:4200
2. Open browser console (F12)
3. Run this code:

```javascript
// SignalR is already connected by the dashboard
// You can test by awarding points via API and watching console logs

// The dashboard automatically:
// 1. Connects to SignalR hub
// 2. Subscribes to customer updates
// 3. Listens for events
// 4. Updates UI reactively
```

### Test Connection Manually

```javascript
// Create new connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/hubs/rewards")
  .build();

// Start connection
await connection.start();
console.log("Connected to SignalR");

// Subscribe to customer
await connection.invoke("SubscribeToCustomer", "cust-123");
console.log("Subscribed to customer updates");

// Listen for events
connection.on("PointsAwarded", (notification) => {
  console.log("Points awarded:", notification);
});

connection.on("TierUpgraded", (notification) => {
  console.log("Tier upgraded:", notification);
});

// Award points via API and watch events arrive!
```

## Load Testing

### Simple Load Test with cURL

```bash
# Award points 100 times
for i in {1..100}; do
  curl -X POST http://localhost:5000/api/v1/rewards/calculate \
    -H "Content-Type: application/json" \
    -H "X-Tenant-Id: national-bank" \
    -d "{
      \"customerId\": \"cust-$i\",
      \"transactionAmount\": 1000.00
    }" &
done
wait
```

### Load Test with k6 (Advanced)

Create `load-test.js`:
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10, // 10 virtual users
  duration: '30s',
};

export default function () {
  const payload = JSON.stringify({
    customerId: `cust-${__VU}`,
    transactionAmount: 1000.00,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': 'national-bank',
    },
  };

  const res = http.post('http://localhost:5000/api/v1/rewards/calculate', payload, params);
  
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

Run:
```bash
k6 run load-test.js
```

## Health Checks

### API Health
```bash
curl http://localhost:5000/api/v1/rewards/health
```

Expected:
```json
{
  "status": "healthy",
  "service": "reward-service",
  "timestamp": "2024-01-15T10:30:00Z",
  "tenant": "national-bank"
}
```

### Database Health
```bash
curl http://localhost:5000/health
```

### RabbitMQ Health
Open http://localhost:15672 (guest/guest)
- Check queues are created
- Verify messages are being processed

### Redis Health
```bash
docker exec loyalty-redis redis-cli ping
```

Expected: `PONG`

## Monitoring

### View Logs

#### All Services
```bash
docker compose logs -f
```

#### Specific Service
```bash
docker compose logs -f reward-service
```

#### Last 100 Lines
```bash
docker compose logs --tail=100 reward-service
```

### Jaeger (Distributed Tracing)
1. Open http://localhost:16686
2. Select service: `reward-service`
3. Click "Find Traces"
4. View request flow across services

### Prometheus (Metrics)
1. Open http://localhost:9090
2. Query: `http_requests_total`
3. View metrics graphs

### Grafana (Dashboards)
1. Open http://localhost:3000 (admin/admin)
2. Navigate to dashboards
3. View system metrics

## Troubleshooting Tests

### Test Fails: Connection Refused

**Problem**: API not responding

**Solution**:
```bash
# Check if services are running
docker compose ps

# Restart services
docker compose restart

# Check logs
docker compose logs reward-service
```

### Test Fails: 404 Not Found

**Problem**: Customer doesn't exist

**Solution**:
```bash
# Enroll customer first
curl -X POST http://localhost:5000/api/v1/customers/enroll \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "customerId": "cust-123",
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com"
  }'
```

### Test Fails: SignalR Not Connecting

**Problem**: WebSocket connection failed

**Solution**:
1. Check API is running: http://localhost:5000/health
2. Verify CORS settings in appsettings.json
3. Check browser console for errors
4. Ensure using correct URL: `ws://localhost:5000/hubs/rewards`

### Test Fails: Tenant Isolation

**Problem**: Can access other tenant's data

**Solution**:
1. Check RLS policies are applied:
```bash
docker exec loyalty-postgres psql -U postgres -d loyalty_sphere -c "SELECT * FROM pg_policies WHERE tablename = 'customers';"
```

2. Verify tenant header is being sent
3. Check TenantInterceptor is registered

## Test Data

### Demo Tenants
1. **national-bank** - National Bank of Egypt
2. **suez-bank** - Suez Canal Bank
3. **shell-egypt** - Shell Egypt
4. **kellogg** - Kellogg

### Demo Customers
```json
{
  "customerId": "cust-demo-001",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "email": "ahmed@example.com"
}
```

```json
{
  "customerId": "cust-demo-002",
  "firstName": "Fatima",
  "lastName": "Ali",
  "email": "fatima@example.com"
}
```

### Demo Transactions
```json
{
  "transactionAmount": 500.00,
  "merchantId": "merchant-retail-001",
  "merchantCategory": "Retail"
}
```

```json
{
  "transactionAmount": 2000.00,
  "merchantId": "merchant-fuel-001",
  "merchantCategory": "Fuel"
}
```

## Automated Testing (Future)

### Unit Tests (.NET)
```bash
cd src/Services/RewardService.Tests
dotnet test
```

### Integration Tests (.NET)
```bash
cd tests/Integration
dotnet test
```

### E2E Tests (Playwright)
```bash
cd tests/E2E
npm test
```

### Component Tests (Angular)
```bash
cd src/Web/loyalty-sphere-ui
npm test
```

## Test Checklist

Before considering testing complete:

- [ ] Customer enrollment works
- [ ] Points calculation works
- [ ] Points redemption works
- [ ] Balance retrieval works
- [ ] History retrieval works
- [ ] Real-time updates work
- [ ] Tier upgrades work
- [ ] Multi-tenant isolation works
- [ ] SignalR connection works
- [ ] Error handling works
- [ ] Health checks work
- [ ] All services start successfully
- [ ] Frontend loads correctly
- [ ] API documentation accessible
- [ ] Monitoring tools accessible

---

**Happy Testing!** 🧪

