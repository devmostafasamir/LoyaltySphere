# LoyaltySphere Angular Frontend

Premium loyalty and rewards platform frontend built with Angular 18, Tailwind CSS v4, and SignalR for real-time updates.

## 🎨 Features

- **Cinematic Red Theme**: Deep red (#9F1239) with gold accents for premium loyalty feel
- **Real-Time Updates**: SignalR integration for live points balance and notifications
- **Reactive State**: Angular signals for optimal performance with OnPush change detection
- **Responsive Design**: Mobile-first with Tailwind CSS v4
- **Smooth Animations**: Reward pop effects, fade-ins, and celebration animations
- **Multi-Tenant**: Automatic tenant header injection via HTTP interceptor

## 🚀 Quick Start

### Prerequisites
- Node.js 20+
- npm or yarn

### Installation
```bash
npm install
```

### Development Server
```bash
npm start
```

Navigate to `http://localhost:4200`. The application will automatically reload if you change any source files.

### Build
```bash
npm run build
```

Build artifacts will be stored in the `dist/` directory.

### Production Build
```bash
npm run build -- --configuration production
```

## 📁 Project Structure

```
src/
├── app/
│   ├── core/
│   │   ├── interceptors/       # HTTP interceptors (tenant, auth, error)
│   │   └── services/           # Core services (SignalR, Reward, Toast)
│   ├── features/
│   │   └── dashboard/          # Dashboard component with real-time updates
│   ├── shared/
│   │   └── components/
│   │       └── toast/          # Toast notification system
│   ├── app.component.ts        # Root component
│   ├── app.config.ts           # Application configuration
│   └── app.routes.ts           # Route definitions
├── environments/               # Environment configurations
├── styles.css                  # Global Tailwind v4 theme
├── index.html                  # Main HTML file
└── main.ts                     # Application bootstrap
```

## 🎯 Key Components

### Dashboard Component
- Real-time points balance display
- Live transaction feed
- Tier progress indicator
- Reward celebration animations
- SignalR connection status

### SignalR Service
- WebSocket connection management
- Automatic reconnection with exponential backoff
- Reactive signals for notifications
- Customer subscription management

### Reward Service
- API integration for balance and history
- Reactive state management with signals
- Points calculation and redemption

### Toast Service
- Global notification system
- Multiple toast types (success, error, info, reward, tier upgrade)
- Auto-dismiss with configurable duration
- Cinematic animations

## 🔧 Configuration

### Environment Variables
Edit `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  tenantId: 'national-bank',
  mockAuthToken: 'mock-jwt-token'
};
```

### Tenant Configuration
The tenant is automatically added to all API requests via the `tenantInterceptor`. In production, this would come from:
- Subdomain (tenant.loyaltysphere.com)
- JWT token claims
- User session

## 🎨 Theming

The application uses a centralized Tailwind v4 theme defined in `src/styles.css` with the `@theme` directive.

### Color Palette
- **Crimson Red** (#9F1239): Primary brand color
- **Gold** (#F59E0B): Reward accents
- **Dark Slate** (#0F172A): Background

### Custom Animations
- `animate-fadeIn`: Fade in effect
- `animate-fadeInUp`: Fade in from bottom
- `animate-scaleIn`: Scale in effect
- `animate-rewardPop`: Reward celebration animation
- `animate-sparkle`: Sparkle effect for rewards

## 🔌 API Integration

### Base URL
```
http://localhost:5000/api/v1
```

### Endpoints
- `GET /rewards/balance/{customerId}` - Get points balance
- `GET /rewards/history/{customerId}` - Get transaction history
- `POST /rewards/calculate` - Calculate and award points
- `POST /rewards/redeem` - Redeem points

### SignalR Hub
```
ws://localhost:5000/hubs/rewards
```

**Events:**
- `PointsAwarded` - Points awarded notification
- `PointsRedeemed` - Points redeemed notification
- `TierUpgraded` - Tier upgrade notification

## 🧪 Testing

### Unit Tests
```bash
npm test
```

### E2E Tests
```bash
npm run e2e
```

## 📦 Docker

### Build Docker Image
```bash
docker build -t loyalty-sphere-ui .
```

### Run Container
```bash
docker run -p 4200:80 loyalty-sphere-ui
```

## 🚀 Deployment

### Production Checklist
- [ ] Update environment variables in `environment.prod.ts`
- [ ] Configure real authentication (replace mock token)
- [ ] Set up HTTPS for SignalR WebSocket
- [ ] Configure CORS on backend
- [ ] Enable production mode
- [ ] Optimize bundle size
- [ ] Set up CDN for static assets

### Build for Production
```bash
npm run build -- --configuration production
```

### Deploy to Azure Static Web Apps
```bash
az staticwebapp create \
  --name loyalty-sphere-ui \
  --resource-group loyalty-sphere-rg \
  --source ./dist/loyalty-sphere-ui \
  --location "East US" \
  --branch main
```

## 🎤 Interview Talking Points

### 1. Angular Signals
"I used Angular signals for reactive state management. Signals provide fine-grained reactivity with automatic dependency tracking. Combined with OnPush change detection, this gives optimal performance by only re-rendering components when their signal dependencies change."

### 2. SignalR Integration
"The SignalR service manages WebSocket connections with automatic reconnection using exponential backoff. When the connection drops, it retries at 0s, 2s, 10s, then 30s intervals. Notifications are exposed as signals, making them reactive throughout the component tree."

### 3. HTTP Interceptors
"I implemented three interceptors: tenant (adds X-Tenant-Id header), auth (adds JWT token), and error (global error handling with toast notifications). This centralizes cross-cutting concerns and keeps components clean."

### 4. Tailwind v4 Theme
"I used Tailwind v4's @theme directive for centralized design system configuration. All colors, spacing, animations, and utilities are defined once in styles.css. This ensures consistency and makes theme changes trivial."

### 5. Performance Optimization
"Multiple strategies: OnPush change detection, signals for reactive state, lazy loading routes, bundle optimization, and avoiding unnecessary re-renders. The dashboard only updates when SignalR notifications arrive or user interactions occur."

## 📝 License

MIT License - Free for portfolio and learning purposes

## 👨‍💻 Author

Built as a portfolio project demonstrating modern Angular architecture and real-time web applications.

---

**Ready to impress in your next interview!** 🚀
