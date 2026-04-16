# ============================================
# PostgreSQL with RLS - Render.com
# ============================================

FROM postgres:16-alpine

# Copy initialization scripts
COPY scripts/setup-rls.sql /docker-entrypoint-initdb.d/01-setup-rls.sql

# Set permissions
RUN chmod 644 /docker-entrypoint-initdb.d/*.sql

# Expose PostgreSQL port
EXPOSE 5432

# Health check
HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
  CMD pg_isready -U postgres || exit 1
