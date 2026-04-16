#!/bin/bash

# ============================================
# LoyaltySphere - Docker Build & Push Script
# ============================================
# This script builds all Docker images and pushes them to Docker Hub
# Usage: ./build-and-push.sh [version]
# Example: ./build-and-push.sh v1.0.0
# ============================================

set -e  # Exit on error

# Configuration
DOCKER_USERNAME="msaid356"
DOCKER_REPO="loyalty-sphere"
VERSION="${1:-latest}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored messages
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

# Function to check if Docker is running
check_docker() {
    print_info "Checking Docker..."
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    print_success "Docker is running"
}

# Function to login to Docker Hub
docker_login() {
    print_info "Logging in to Docker Hub..."
    if ! docker login; then
        print_error "Docker login failed. Please check your credentials."
        exit 1
    fi
    print_success "Logged in to Docker Hub"
}

# Function to build and push an image
build_and_push() {
    local service_name=$1
    local dockerfile_path=$2
    local context_path=$3
    local image_name="${DOCKER_USERNAME}/${DOCKER_REPO}:${service_name}-${VERSION}"
    local latest_tag="${DOCKER_USERNAME}/${DOCKER_REPO}:${service_name}-latest"

    print_info "Building ${service_name}..."
    
    if docker build -t "${image_name}" -t "${latest_tag}" -f "${dockerfile_path}" "${context_path}"; then
        print_success "Built ${service_name}"
        
        print_info "Pushing ${image_name}..."
        if docker push "${image_name}"; then
            print_success "Pushed ${image_name}"
        else
            print_error "Failed to push ${image_name}"
            return 1
        fi
        
        print_info "Pushing ${latest_tag}..."
        if docker push "${latest_tag}"; then
            print_success "Pushed ${latest_tag}"
        else
            print_error "Failed to push ${latest_tag}"
            return 1
        fi
    else
        print_error "Failed to build ${service_name}"
        return 1
    fi
}

# Main execution
main() {
    echo ""
    echo "============================================"
    echo "  LoyaltySphere - Docker Build & Push"
    echo "============================================"
    echo ""
    echo "Docker Hub: ${DOCKER_USERNAME}/${DOCKER_REPO}"
    echo "Version: ${VERSION}"
    echo ""
    
    # Check prerequisites
    check_docker
    docker_login
    
    echo ""
    print_info "Starting build process..."
    echo ""
    
    # Build and push Reward Service
    print_info "=== Building Reward Service ==="
    build_and_push "reward-service" "src/Services/RewardService/Dockerfile" "."
    echo ""
    
    # Build and push Frontend
    print_info "=== Building Frontend ==="
    build_and_push "frontend" "src/Web/loyalty-sphere-ui/Dockerfile" "src/Web/loyalty-sphere-ui"
    echo ""
    
    # Summary
    echo ""
    echo "============================================"
    print_success "All images built and pushed successfully!"
    echo "============================================"
    echo ""
    echo "Images pushed:"
    echo "  - ${DOCKER_USERNAME}/${DOCKER_REPO}:reward-service-${VERSION}"
    echo "  - ${DOCKER_USERNAME}/${DOCKER_REPO}:reward-service-latest"
    echo "  - ${DOCKER_USERNAME}/${DOCKER_REPO}:frontend-${VERSION}"
    echo "  - ${DOCKER_USERNAME}/${DOCKER_REPO}:frontend-latest"
    echo ""
    echo "To use these images, update docker-compose.yml:"
    echo "  docker compose -f docker-compose.prod.yml up -d"
    echo ""
}

# Run main function
main
