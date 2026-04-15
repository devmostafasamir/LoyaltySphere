-- ============================================
-- LoyaltySphere - PostgreSQL Row-Level Security Setup
-- ============================================
-- This script sets up Row-Level Security (RLS) policies for multi-tenant data isolation.
-- RLS ensures that each tenant can only access their own data at the database level.
-- This provides defense-in-depth security beyond application-level filtering.

-- Enable RLS on all tenant-scoped tables
-- ============================================

-- Rewards table
ALTER TABLE rewards ENABLE ROW LEVEL SECURITY;

-- Customers table
ALTER TABLE customers ENABLE ROW LEVEL SECURITY;

-- Reward Rules table
ALTER TABLE reward_rules ENABLE ROW LEVEL SECURITY;

-- Campaigns table
ALTER TABLE campaigns ENABLE ROW LEVEL SECURITY;

-- Transactions table (if in same database)
ALTER TABLE transactions ENABLE ROW LEVEL SECURITY;

-- Create RLS Policies
-- ============================================
-- Policies use the PostgreSQL session variable 'app.current_tenant'
-- which is set by the application before each query.

-- Policy for Rewards table
CREATE POLICY tenant_isolation_policy ON rewards
    USING (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_insert_policy ON rewards
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_update_policy ON rewards
    FOR UPDATE
    USING (tenant_id = current_setting('app.current_tenant', true))
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_delete_policy ON rewards
    FOR DELETE
    USING (tenant_id = current_setting('app.current_tenant', true));

-- Policy for Customers table
CREATE POLICY tenant_isolation_policy ON customers
    USING (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_insert_policy ON customers
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_update_policy ON customers
    FOR UPDATE
    USING (tenant_id = current_setting('app.current_tenant', true))
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

-- Policy for Reward Rules table
CREATE POLICY tenant_isolation_policy ON reward_rules
    USING (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_insert_policy ON reward_rules
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

-- Policy for Campaigns table
CREATE POLICY tenant_isolation_policy ON campaigns
    USING (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_insert_policy ON campaigns
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

-- Policy for Transactions table
CREATE POLICY tenant_isolation_policy ON transactions
    USING (tenant_id = current_setting('app.current_tenant', true));

CREATE POLICY tenant_insert_policy ON transactions
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant', true));

-- Create indexes for performance
-- ============================================
-- Tenant-scoped queries will be much faster with these indexes

CREATE INDEX IF NOT EXISTS idx_rewards_tenant_id ON rewards(tenant_id);
CREATE INDEX IF NOT EXISTS idx_customers_tenant_id ON customers(tenant_id);
CREATE INDEX IF NOT EXISTS idx_reward_rules_tenant_id ON reward_rules(tenant_id);
CREATE INDEX IF NOT EXISTS idx_campaigns_tenant_id ON campaigns(tenant_id);
CREATE INDEX IF NOT EXISTS idx_transactions_tenant_id ON transactions(tenant_id);

-- Composite indexes for common queries
CREATE INDEX IF NOT EXISTS idx_rewards_tenant_customer ON rewards(tenant_id, customer_id);
CREATE INDEX IF NOT EXISTS idx_rewards_tenant_created ON rewards(tenant_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_transactions_tenant_customer ON transactions(tenant_id, customer_id);
CREATE INDEX IF NOT EXISTS idx_transactions_tenant_created ON transactions(tenant_id, created_at DESC);

-- Create function to validate tenant context
-- ============================================
-- This function ensures that app.current_tenant is always set before queries

CREATE OR REPLACE FUNCTION validate_tenant_context()
RETURNS TRIGGER AS $$
BEGIN
    IF current_setting('app.current_tenant', true) IS NULL THEN
        RAISE EXCEPTION 'Tenant context not set. Please set app.current_tenant before executing queries.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply validation trigger to all tenant-scoped tables
CREATE TRIGGER validate_tenant_rewards
    BEFORE INSERT OR UPDATE ON rewards
    FOR EACH ROW
    EXECUTE FUNCTION validate_tenant_context();

CREATE TRIGGER validate_tenant_customers
    BEFORE INSERT OR UPDATE ON customers
    FOR EACH ROW
    EXECUTE FUNCTION validate_tenant_context();

CREATE TRIGGER validate_tenant_reward_rules
    BEFORE INSERT OR UPDATE ON reward_rules
    FOR EACH ROW
    EXECUTE FUNCTION validate_tenant_context();

CREATE TRIGGER validate_tenant_campaigns
    BEFORE INSERT OR UPDATE ON campaigns
    FOR EACH ROW
    EXECUTE FUNCTION validate_tenant_context();

CREATE TRIGGER validate_tenant_transactions
    BEFORE INSERT OR UPDATE ON transactions
    FOR EACH ROW
    EXECUTE FUNCTION validate_tenant_context();

-- Grant permissions
-- ============================================
-- Ensure application user has necessary permissions

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO loyalty_app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO loyalty_app_user;

-- Verification queries
-- ============================================
-- Run these to verify RLS is working correctly

-- Check RLS is enabled
SELECT schemaname, tablename, rowsecurity
FROM pg_tables
WHERE schemaname = 'public'
  AND rowsecurity = true;

-- Check policies exist
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual
FROM pg_policies
WHERE schemaname = 'public';

-- Test tenant isolation (should return 0 rows without tenant context)
-- SELECT * FROM rewards; -- This should fail or return 0 rows

-- Set tenant context and test
-- SET app.current_tenant = 'national-bank';
-- SELECT * FROM rewards; -- This should return only national-bank rewards

-- ============================================
-- IMPORTANT NOTES:
-- ============================================
-- 1. Always set app.current_tenant before executing queries
-- 2. RLS policies are enforced at the database level, providing defense-in-depth
-- 3. Application-level query filters provide additional protection
-- 4. Indexes on tenant_id are critical for performance
-- 5. Test tenant isolation thoroughly before production deployment
-- 6. Monitor query performance and adjust indexes as needed
-- 7. Consider partitioning tables by tenant_id for very large datasets
-- ============================================

COMMENT ON TABLE rewards IS 'Rewards table with Row-Level Security for multi-tenant isolation';
COMMENT ON TABLE customers IS 'Customers table with Row-Level Security for multi-tenant isolation';
COMMENT ON TABLE reward_rules IS 'Reward rules table with Row-Level Security for multi-tenant isolation';
COMMENT ON TABLE campaigns IS 'Campaigns table with Row-Level Security for multi-tenant isolation';
COMMENT ON TABLE transactions IS 'Transactions table with Row-Level Security for multi-tenant isolation';
