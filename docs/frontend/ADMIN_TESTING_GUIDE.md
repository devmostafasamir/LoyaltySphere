# 🧪 Admin Dashboard - Testing Guide

## Quick Testing Checklist

### Prerequisites
```bash
# Terminal 1 - Backend
cd src/Services/RewardService
dotnet run

# Terminal 2 - Frontend
cd src/Web/loyalty-sphere-ui
npm start
```

## 1️⃣ Test Admin Dashboard (Analytics)

### Access
```
http://localhost:4200/admin/dashboard
```

### What to Check
- [ ] Page loads without errors
- [ ] 4 metric cards display:
  - Total Customers
  - Points Awarded
  - Points Redeemed
  - Active Campaigns
- [ ] Tier distribution chart shows with progress bars
- [ ] Recent transactions timeline displays (7 days)
- [ ] Quick action buttons are visible
- [ ] Loading spinner shows during data fetch
- [ ] All numbers format correctly

### Expected Behavior
- Metrics update automatically when data changes
- Tier colors match (Bronze=Orange, Silver=Slate, Gold=Amber, Platinum=Purple)
- Progress bars animate smoothly
- Cards have hover effects

## 2️⃣ Test Campaign Management

### Access
```
http://localhost:4200/admin/campaigns
```

### Test: View Campaigns
- [ ] Page loads without errors
- [ ] Campaign grid displays
- [ ] Filter buttons work (All, Active, Inactive)
- [ ] Each campaign card shows:
  - Campaign name
  - Type badge (Bonus/Multiplier/Cashback)
  - Description
  - Status toggle
  - Duration dates
  - Type-specific value

### Test: Create Bonus Campaign
1. Click "Create Campaign" button
2. Fill in form:
   ```
   Campaign Name: Test Bonus Campaign
   Description: Earn 500 bonus points
   Campaign Type: Bonus
   Bonus Points: 500
   Start Date: [Today]
   End Date: [30 days from now]
   Target Segment: All Customers
   ```
3. Click "Create Campaign"
4. Check:
   - [ ] Success toast appears
   - [ ] Modal closes
   - [ ] New campaign appears in grid
   - [ ] Campaign shows as active

### Test: Create Multiplier Campaign
1. Click "Create Campaign"
2. Fill in form:
   ```
   Campaign Name: 2x Points Weekend
   Description: Double points on all purchases
   Campaign Type: Multiplier
   Points Multiplier: 2.0
   Start Date: [Next Saturday]
   End Date: [Next Sunday]
   ```
3. Click "Create Campaign"
4. Check:
   - [ ] Campaign created successfully
   - [ ] Multiplier value displays correctly (2x)

### Test: Create Cashback Campaign
1. Click "Create Campaign"
2. Fill in form:
   ```
   Campaign Name: 10% Cashback
   Description: Get 10% cashback on fuel
   Campaign Type: Cashback
   Cashback Percentage: 10
   Start Date: [Today]
   End Date: [60 days from now]
   Target Merchant Category: Fuel
   ```
3. Click "Create Campaign"
4. Check:
   - [ ] Campaign created successfully
   - [ ] Cashback percentage displays correctly (10%)
   - [ ] Target category shows in card

### Test: Toggle Campaign Status
1. Find an active campaign
2. Click status toggle button
3. Check:
   - [ ] Button changes from "Active" to "Inactive"
   - [ ] Button color changes (green → gray)
   - [ ] Success toast appears
   - [ ] Campaign updates in grid

4. Click toggle again
5. Check:
   - [ ] Campaign reactivates
   - [ ] Button changes back to "Active"

### Test: Filter Campaigns
1. Click "Active" filter
   - [ ] Only active campaigns show
2. Click "Inactive" filter
   - [ ] Only inactive campaigns show
3. Click "All" filter
   - [ ] All campaigns show

### Test: Empty State
1. Deactivate all campaigns
2. Click "Inactive" filter
3. Check:
   - [ ] Empty state message shows
   - [ ] "Create Your First Campaign" button displays

## 3️⃣ Test Navigation

### Test: Sidebar Navigation
- [ ] Sidebar is fixed on left side
- [ ] Logo displays with gradient
- [ ] Customer section shows Dashboard link
- [ ] Admin section shows 4 links:
  - Analytics
  - Campaigns
  - Customers
  - Reward Rules
- [ ] User profile shows at bottom

### Test: Route Highlighting
1. Navigate to `/dashboard`
   - [ ] Dashboard link highlights
2. Navigate to `/admin/dashboard`
   - [ ] Analytics link highlights
3. Navigate to `/admin/campaigns`
   - [ ] Campaigns link highlights

### Test: Navigation Clicks
- [ ] Click "Dashboard" → Goes to customer dashboard
- [ ] Click "Analytics" → Goes to admin dashboard
- [ ] Click "Campaigns" → Goes to campaigns page
- [ ] All transitions are smooth

## 4️⃣ Test API Integration

### Test: Campaign API
```bash
# Get all campaigns
curl http://localhost:5000/api/v1/admin/campaigns \
  -H "X-Tenant-Id: national-bank"

# Create campaign
curl -X POST http://localhost:5000/api/v1/admin/campaigns \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: national-bank" \
  -d '{
    "campaignName": "API Test Campaign",
    "description": "Testing via API",
    "campaignType": "Bonus",
    "bonusPoints": 1000,
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z"
  }'
```

### Test: Analytics API
```bash
# Get dashboard analytics
curl http://localhost:5000/api/v1/admin/analytics/dashboard \
  -H "X-Tenant-Id: national-bank"
```

### Expected Response Structure
```json
{
  "totalCustomers": 150,
  "activeCustomers": 120,
  "totalPointsAwarded": 50000,
  "totalPointsRedeemed": 15000,
  "activeCampaigns": 3,
  "tierDistribution": [
    { "tier": "Bronze", "count": 80, "totalPoints": 10000 },
    { "tier": "Silver", "count": 40, "totalPoints": 15000 },
    { "tier": "Gold", "count": 20, "totalPoints": 20000 },
    { "tier": "Platinum", "count": 10, "totalPoints": 15000 }
  ],
  "recentTransactions": [
    { "date": "2024-01-15", "count": 45, "totalPoints": 2500 }
  ]
}
```

## 5️⃣ Test Error Handling

### Test: Invalid Campaign Creation
1. Try to create campaign with missing required fields
   - [ ] Form validation prevents submission
   - [ ] Error toast shows

2. Try to create campaign with end date before start date
   - [ ] Backend returns 400 error
   - [ ] Error toast shows with message

### Test: Network Errors
1. Stop backend server
2. Try to load admin dashboard
   - [ ] Loading spinner shows
   - [ ] Error toast appears after timeout
3. Try to create campaign
   - [ ] Error toast shows

## 6️⃣ Test Responsive Design

### Test: Desktop (1920x1080)
- [ ] Sidebar is 256px wide
- [ ] Content area uses remaining space
- [ ] Campaign grid shows 3 columns
- [ ] All cards display properly

### Test: Tablet (768x1024)
- [ ] Sidebar remains visible
- [ ] Campaign grid shows 2 columns
- [ ] Metric cards stack properly

### Test: Mobile (375x667)
- [ ] Sidebar should be toggleable (future enhancement)
- [ ] Campaign grid shows 1 column
- [ ] All content is readable
- [ ] Buttons are touch-friendly

## 7️⃣ Test Performance

### Test: Loading Speed
- [ ] Admin dashboard loads in < 2 seconds
- [ ] Campaign list loads in < 1 second
- [ ] No layout shifts during load
- [ ] Smooth animations (60fps)

### Test: State Updates
- [ ] Campaign status toggle is instant
- [ ] New campaigns appear immediately
- [ ] No unnecessary re-renders
- [ ] Signals update efficiently

## 8️⃣ Test Browser Compatibility

### Chrome
- [ ] All features work
- [ ] Animations are smooth
- [ ] No console errors

### Firefox
- [ ] All features work
- [ ] Styling is consistent

### Safari
- [ ] All features work
- [ ] Backdrop blur works

### Edge
- [ ] All features work
- [ ] No compatibility issues

## ✅ Testing Checklist Summary

### Critical Tests (Must Pass)
- [ ] Admin dashboard loads and displays metrics
- [ ] Can create Bonus campaign
- [ ] Can create Multiplier campaign
- [ ] Can create Cashback campaign
- [ ] Can toggle campaign status
- [ ] Navigation works correctly
- [ ] API integration works
- [ ] Error handling works

### Nice to Have Tests
- [ ] Responsive design works
- [ ] Performance is good
- [ ] Browser compatibility
- [ ] Empty states display
- [ ] Loading states show

## 🐛 Common Issues & Solutions

### Issue: "Cannot read property of undefined"
**Solution**: Check that backend is running and returning data

### Issue: Campaign not appearing after creation
**Solution**: Check browser console for errors, verify API response

### Issue: Styling looks broken
**Solution**: Run `npm install` to ensure Tailwind is installed

### Issue: Navigation not highlighting
**Solution**: Check that routes match exactly in `app.routes.ts`

## 🎉 Testing Complete!

If all tests pass, your admin dashboard is **production-ready**! 🚀

### Next Steps:
1. Commit your changes
2. Deploy to staging
3. Show it off in your interview!

