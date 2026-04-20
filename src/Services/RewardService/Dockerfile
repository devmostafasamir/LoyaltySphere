# ============================================
# LoyaltySphere Reward Service - Dockerfile
# Multi-stage build for optimized production image
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["LoyaltySphere.sln", "./"]
COPY ["src/Services/RewardService/LoyaltySphere.RewardService.csproj", "src/Services/RewardService/"]
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]

# Restore dependencies
RUN dotnet restore "src/Services/RewardService/LoyaltySphere.RewardService.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/Services/RewardService"
RUN dotnet build "LoyaltySphere.RewardService.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "LoyaltySphere.RewardService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appuser && \
    adduser --system --uid 1001 --ingroup appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "LoyaltySphere.RewardService.dll"]
