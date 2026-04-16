#!/bin/bash

# ============================================
# LoyaltySphere - Render.com Deployment Script
# ============================================
# Deploys LoyaltySphere to Render.com
# Instance: loyalty-sphere (capybara)
# Region: AWS AP-NorthEast-1 (Tokyo)
# ============================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

echo ""
echo "============================================"
echo "  LoyaltySphere - Render.com Deployment"
echo "============================================"
echo ""

# Check if render CLI is installed
if ! command -v render &> /dev/null; then
    print_warning "Render CLI not found. Installing..."
    npm install -g render-cli
    print_success "Render CLI installed"
fi

# Check if logged in
print_info "Checking Render authentication..."
if ! render whoami &> /dev/null; then
    print_warning "Not logged in to Render. Please login:"
    render login
fi

print_success "Authenticated with Render"

# Deploy blueprint
print_info "Deploying blueprint to Render..."
echo ""

if render blueprint deploy; then
    echo ""
    print_success "Deployment initiated successfully!"
    echo ""
    echo "============================================"
    echo "  Deployment Status"
    echo "============================================"
    echo ""
    echo "Your services are being deployed to Render."
    echo ""
    echo "Monitor deployment:"
    echo "  https://dashboard.render.com"
    echo ""
    echo "Services:"
    echo "  - PostgreSQL: loyalty-postgres"
    echo "  - Redis: loyalty-redis"
    echo "  - Backend: loyalty-reward-service"
    echo "  - Frontend: loyalty-sphere-ui"
    echo ""
    echo "Once deployed, access your app at:"
    echo "  Frontend: https://loyalty-sphere-ui.onrender.com"
    echo "  Backend:  https://loyalty-reward-service.onrender.com"
    echo "  Swagger:  https://loyalty-reward-service.onrender.com/swagger"
    echo ""
    echo "============================================"
else
    print_error "Deployment failed. Check the logs above."
    exit 1
fi
