-- =========================================================
-- PostgreSQL schema + seed for ProfileDbContext
-- =========================================================
BEGIN;

-- Utility trigger func to maintain updated_at
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ------------------------------
-- TABLE: profiles
-- ------------------------------
CREATE TABLE IF NOT EXISTS profiles (
  id            BIGSERIAL PRIMARY KEY,
  first_name    VARCHAR(100) NOT NULL,
  last_name     VARCHAR(100),
  email         VARCHAR(256) NOT NULL,
  phone         VARCHAR(32),
  date_of_birth DATE,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- case-insensitive unique email
CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_email_lower ON profiles ((LOWER(email)));

DROP TRIGGER IF EXISTS trg_profiles_updated_at ON profiles;
CREATE TRIGGER trg_profiles_updated_at
BEFORE UPDATE ON profiles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ------------------------------
-- TABLE: addresses
-- ------------------------------
CREATE TABLE IF NOT EXISTS addresses (
  id           BIGSERIAL PRIMARY KEY,
  profile_id   BIGINT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  line1        VARCHAR(200) NOT NULL,
  line2        VARCHAR(200),
  city         VARCHAR(100) NOT NULL,
  state        VARCHAR(100),
  postal_code  VARCHAR(20),
  country      VARCHAR(100) NOT NULL,
  is_primary   BOOLEAN NOT NULL DEFAULT FALSE,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_addresses_profile_id ON addresses(profile_id);

-- one primary address per profile
CREATE UNIQUE INDEX IF NOT EXISTS ux_addresses_one_primary_per_profile
  ON addresses(profile_id) WHERE is_primary = TRUE;

DROP TRIGGER IF EXISTS trg_addresses_updated_at ON addresses;
CREATE TRIGGER trg_addresses_updated_at
BEFORE UPDATE ON addresses
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ------------------------------
-- TABLE: verification_tokens
-- ------------------------------
CREATE TABLE IF NOT EXISTS verification_tokens (
  id          BIGSERIAL PRIMARY KEY,
  profile_id  BIGINT NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  token       VARCHAR(128) NOT NULL,
  token_type  VARCHAR(50) NOT NULL,   -- 'email_verification', 'password_reset', etc.
  issued_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  expires_at  TIMESTAMPTZ NOT NULL,
  consumed_at TIMESTAMPTZ,
  is_revoked  BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS ix_verification_tokens_profile_id ON verification_tokens(profile_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_verification_tokens_token ON verification_tokens(token);

-- ------------------------------
-- TABLE: audit_records
-- ------------------------------
CREATE TABLE IF NOT EXISTS audit_records (
  id            BIGSERIAL PRIMARY KEY,
  entity_name   VARCHAR(128) NOT NULL,
  entity_id     VARCHAR(64)  NOT NULL,
  action        VARCHAR(32)  NOT NULL,
  changed_by    VARCHAR(128),
  changed_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  changes_json  JSONB,
  correlation_id UUID
);

CREATE INDEX IF NOT EXISTS ix_audit_entity ON audit_records(entity_name, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_action_time ON audit_records(action, changed_at);

-- =========================================================
-- Seed data (idempotent, safe to re-run)
-- =========================================================

-- Profiles (insert only if email not present, case-insensitive)
WITH rows AS (
  SELECT 'swapnil.bawankar@example.com'::text AS email, 'Swapnil'::text AS first_name, 'Bawankar'::text AS last_name, '+91-9876543210'::text AS phone, '1990-05-12'::date AS dob UNION ALL
  SELECT 'isha.kulkarni@example.com', 'Isha', 'Kulkarni', '+91-9822012345', '1995-10-23'::date UNION ALL
  SELECT 'rahul.kulkarni@example.com', 'Rahul', 'Kulkarni', '+91-9811122233', '1988-02-03'::date
)
INSERT INTO profiles (first_name, last_name, email, phone, date_of_birth, is_active)
SELECT r.first_name, r.last_name, r.email, r.phone, r.dob, TRUE
FROM rows r
LEFT JOIN profiles p ON LOWER(p.email) = LOWER(r.email)
WHERE p.id IS NULL;

-- Addresses
-- Swapnil primary (Pune)
INSERT INTO addresses (profile_id, line1, line2, city, state, postal_code, country, is_primary)
SELECT p.id, 'Flat 502, Sky Heights', 'Baner', 'Pune', 'Maharashtra', '411045', 'India', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.profile_id = p.id AND a.line1 = 'Flat 502, Sky Heights' AND a.city = 'Pune'
  );

-- Swapnil secondary
INSERT INTO addresses (profile_id, line1, line2, city, state, postal_code, country, is_primary)
SELECT p.id, 'Plot 10, Central Park', 'Hinjawadi Phase 2', 'Pune', 'Maharashtra', '411057', 'India', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.profile_id = p.id AND a.line1 = 'Plot 10, Central Park' AND a.city = 'Pune'
  );

-- Isha primary (Pune)
INSERT INTO addresses (profile_id, line1, line2, city, state, postal_code, country, is_primary)
SELECT p.id, 'Nyati Emporius, Kharadi', NULL, 'Pune', 'Maharashtra', '411014', 'India', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.profile_id = p.id AND a.line1 = 'Nyati Emporius, Kharadi' AND a.city = 'Pune'
  );

-- Rahul primary (Bengaluru)
INSERT INTO addresses (profile_id, line1, line2, city, state, postal_code, country, is_primary)
SELECT p.id, 'Brigade Meadows, Kanakapura Road', NULL, 'Bengaluru', 'Karnataka', '560062', 'India', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('rahul.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.profile_id = p.id AND a.line1 = 'Brigade Meadows, Kanakapura Road' AND a.city = 'Bengaluru'
  );

-- Verification tokens
INSERT INTO verification_tokens (profile_id, token, token_type, expires_at, is_revoked)
SELECT p.id, 'verify-swapnil-001', 'email_verification', NOW() + INTERVAL '24 hours', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (SELECT 1 FROM verification_tokens t WHERE t.token = 'verify-swapnil-001');

INSERT INTO verification_tokens (profile_id, token, token_type, expires_at, is_revoked)
SELECT p.id, 'verify-isha-001', 'email_verification', NOW() + INTERVAL '24 hours', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (SELECT 1 FROM verification_tokens t WHERE t.token = 'verify-isha-001');

INSERT INTO verification_tokens (profile_id, token, token_type, expires_at, is_revoked)
SELECT p.id, 'reset-rahul-001', 'password_reset', NOW() + INTERVAL '2 hours', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('rahul.kulkarni@example.com')
  AND NOT EXISTS (SELECT 1 FROM verification_tokens t WHERE t.token = 'reset-rahul-001');

-- Audit records (one 'INSERT' per profile)
INSERT INTO audit_records (entity_name, entity_id, action, changed_by, changes_json, correlation_id)
SELECT 'Profile', p.id::text, 'INSERT', 'system',
       jsonb_build_object('first_name', p.first_name, 'last_name', p.last_name, 'email', p.email),
       gen_random_uuid()
FROM profiles p
WHERE NOT EXISTS (
    SELECT 1 FROM audit_records a
    WHERE a.entity_name = 'Profile' AND a.entity_id = p.id::text AND a.action = 'INSERT'
);

COMMIT;