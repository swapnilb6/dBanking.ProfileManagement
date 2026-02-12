-- =====================================================================
-- Profile Management DB Reset & Seed (Manual Run)
-- Aligned with v1.0 Handover Pack (CustomerId UUID, verification-first)
-- =====================================================================

BEGIN;

-- -------------------------------------------------
-- 0) Drop old/new triggers (if they exist)
-- -------------------------------------------------
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_profiles_updated_at') THEN
    EXECUTE 'DROP TRIGGER IF EXISTS trg_profiles_updated_at ON profiles;';
  END IF;

  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_addresses_updated_at') THEN
    EXECUTE 'DROP TRIGGER IF EXISTS trg_addresses_updated_at ON addresses;';
  END IF;
END $$;

-- -------------------------------------------------
-- 1) Drop indexes (legacy + current names, if they exist)
-- -------------------------------------------------
-- Legacy indexes from earlier script
DROP INDEX IF EXISTS ux_profiles_email_lower;
DROP INDEX IF EXISTS ix_addresses_profile_id;
DROP INDEX IF EXISTS ux_addresses_one_primary_per_profile;
DROP INDEX IF EXISTS ix_verification_tokens_profile_id;
DROP INDEX IF EXISTS ux_verification_tokens_token;
DROP INDEX IF EXISTS ix_audit_entity;
DROP INDEX IF EXISTS ix_audit_action_time;

-- Current corrected schema indexes
DROP INDEX IF EXISTS ux_profiles_email_verified_lower;
DROP INDEX IF EXISTS ux_profiles_pending_email_lower;
DROP INDEX IF EXISTS ux_profiles_phone_verified;
DROP INDEX IF EXISTS ux_profiles_pending_phone;
DROP INDEX IF EXISTS ix_addresses_customer_id;
DROP INDEX IF EXISTS ux_addresses_one_primary_per_customer;
DROP INDEX IF EXISTS ix_verification_tokens_lookup;
DROP INDEX IF EXISTS ux_verification_tokens_pending_unique;
DROP INDEX IF EXISTS ix_audit_customer_time;
DROP INDEX IF EXISTS ix_audit_entity;

-- -------------------------------------------------
-- 2) Drop tables (children first to honor FKs)
-- -------------------------------------------------
DROP TABLE IF EXISTS verification_tokens;
DROP TABLE IF EXISTS addresses;
DROP TABLE IF EXISTS audit_records;
DROP TABLE IF EXISTS profiles;

-- -------------------------------------------------
-- 3) Drop utility function (if exists)
-- -------------------------------------------------
DROP FUNCTION IF EXISTS set_updated_at;

COMMIT;

-- =========================================================
-- Recreate schema
-- =========================================================

BEGIN;

-- -------------------------------------------------
-- 4) Ensure required extensions
-- -------------------------------------------------
CREATE EXTENSION IF NOT EXISTS pgcrypto; -- gen_random_uuid(), digest()

-- -------------------------------------------------
-- 5) Utility trigger function for updated_at
-- -------------------------------------------------
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- -------------------------------------------------
-- 6) TABLE: profiles (business key = customer_id)
-- -------------------------------------------------
CREATE TABLE IF NOT EXISTS profiles (
  id                     BIGSERIAL   PRIMARY KEY,                -- surrogate
  customer_id            UUID        NOT NULL UNIQUE,            -- business key used by API

  -- Basic
  first_name             VARCHAR(100) NOT NULL,
  last_name              VARCHAR(100),

  -- Contact (verification-first model)
  email                  VARCHAR(256),
  email_status           VARCHAR(32)  NOT NULL DEFAULT 'Unverified', -- Unverified|Verified|PendingVerification
  pending_email          VARCHAR(256),

  phone_e164             VARCHAR(20),
  phone_status           VARCHAR(32)  NOT NULL DEFAULT 'Unverified', -- Unverified|Verified|PendingVerification
  pending_phone_e164     VARCHAR(20),

  -- Profile info
  date_of_birth          DATE,
  is_active              BOOLEAN      NOT NULL DEFAULT TRUE,

  -- Preferences (flattened; owned-type alternative)
  sms_enabled            BOOLEAN,
  email_enabled          BOOLEAN,
  push_enabled           BOOLEAN,
  regulatory_consent     BOOLEAN,
  language               VARCHAR(16),
  time_zone              VARCHAR(64),
  preferences_updated_at TIMESTAMPTZ,

  -- Timestamps
  created_at             TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Expression-based unique indexes (case-insensitive) for email
CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_email_verified_lower
  ON profiles ((LOWER(email)))
  WHERE email IS NOT NULL AND email_status = 'Verified';

CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_pending_email_lower
  ON profiles ((LOWER(pending_email)))
  WHERE pending_email IS NOT NULL;

-- Phone uniqueness (partial)
CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_phone_verified
  ON profiles (phone_e164)
  WHERE phone_e164 IS NOT NULL AND phone_status = 'Verified';

CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_pending_phone
  ON profiles (pending_phone_e164)
  WHERE pending_phone_e164 IS NOT NULL;

-- updated_at trigger
DROP TRIGGER IF EXISTS trg_profiles_updated_at ON profiles;
CREATE TRIGGER trg_profiles_updated_at
BEFORE UPDATE ON profiles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- -------------------------------------------------
-- 7) TABLE: addresses
-- -------------------------------------------------
CREATE TABLE IF NOT EXISTS addresses (
  id              BIGSERIAL     PRIMARY KEY,                -- surrogate
  address_id      UUID          NOT NULL UNIQUE,            -- public id for API
  customer_id     UUID          NOT NULL REFERENCES profiles(customer_id) ON DELETE CASCADE,

  type            VARCHAR(32)   NOT NULL DEFAULT 'Residential', -- Residential|Mailing|Work

  line1           VARCHAR(200)  NOT NULL,
  line2           VARCHAR(200),
  city            VARCHAR(100)  NOT NULL,
  state_province  VARCHAR(100),
  postal_code     VARCHAR(20),
  country_code    CHAR(2)       NOT NULL,                   -- ISO-3166-1 alpha-2

  is_primary      BOOLEAN       NOT NULL DEFAULT FALSE,

  effective_from  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  effective_to    TIMESTAMPTZ,

  created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_addresses_customer_id ON addresses(customer_id);

CREATE UNIQUE INDEX IF NOT EXISTS ux_addresses_one_primary_per_customer
  ON addresses(customer_id) WHERE is_primary = TRUE;

DROP TRIGGER IF EXISTS trg_addresses_updated_at ON addresses;
CREATE TRIGGER trg_addresses_updated_at
BEFORE UPDATE ON addresses
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- -------------------------------------------------
-- 8) TABLE: verification_tokens
-- -------------------------------------------------
CREATE TABLE IF NOT EXISTS verification_tokens (
  id             BIGSERIAL     PRIMARY KEY,
  customer_id    UUID          NOT NULL REFERENCES profiles(customer_id) ON DELETE CASCADE,

  type           VARCHAR(16)   NOT NULL,      -- 'EmailLink'|'SmsOtp'
  channel_value  VARCHAR(256)  NOT NULL,      -- normalized email or phone
  token_hash     CHAR(64)      NOT NULL,      -- SHA-256 hex

  issued_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  expires_at     TIMESTAMPTZ   NOT NULL,

  attempt_count  INT           NOT NULL DEFAULT 0,
  max_attempts   INT           NOT NULL DEFAULT 5,
  status         VARCHAR(24)   NOT NULL DEFAULT 'Pending', -- Pending|Verified|Expired|Revoked|AttemptsExceeded
  verified_at    TIMESTAMPTZ,

  correlation_id UUID
);

-- Lookup & uniqueness indexes
CREATE INDEX IF NOT EXISTS ix_verification_tokens_lookup
  ON verification_tokens (customer_id, type, status, channel_value, expires_at DESC);

CREATE UNIQUE INDEX IF NOT EXISTS ux_verification_tokens_pending_unique
  ON verification_tokens (customer_id, type, channel_value)
  WHERE status = 'Pending';

-- -------------------------------------------------
-- 9) TABLE: audit_records (append-only)
-- -------------------------------------------------
CREATE TABLE IF NOT EXISTS audit_records (
  id                 BIGSERIAL   PRIMARY KEY,
  customer_id        UUID        NOT NULL,
  entity_name        VARCHAR(128) NOT NULL,    -- 'Profile'|'Address'|'Preferences'|'Contact'
  entity_id          VARCHAR(64)  NOT NULL,    -- e.g., 'profile' or address_id::text
  action             VARCHAR(32)  NOT NULL,    -- 'INSERT'|'UPDATE'|'UPSERT'|'SET_PRIMARY'...
  actor              VARCHAR(128),
  channel            VARCHAR(64),              -- 'API'|'System'|'Batch'|'Admin'
  changed_at         TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  old_json           JSONB,
  new_json           JSONB,
  changed_fields_csv TEXT,
  correlation_id     UUID
);

CREATE INDEX IF NOT EXISTS ix_audit_customer_time
  ON audit_records (customer_id, changed_at DESC);

CREATE INDEX IF NOT EXISTS ix_audit_entity
  ON audit_records (entity_name, entity_id);

COMMIT;

-- =========================================================
-- Seed data (idempotent)
-- =========================================================

BEGIN;

-- 10) Seed Profiles (ensure case-insensitive matching on email)
WITH rows AS (
  SELECT LOWER('swapnil.bawankar@example.com')::text AS email_norm, 'Swapnil'::text AS first_name, 'Bawankar'::text AS last_name,
         '+919876543210'::text AS phone_e164, '1990-05-12'::date AS dob UNION ALL
  SELECT LOWER('isha.kulkarni@example.com'), 'Isha', 'Kulkarni', '+919822012345', '1995-10-23'::date UNION ALL
  SELECT LOWER('rahul.kulkarni@example.com'), 'Rahul', 'Kulkarni', '+919811122233', '1988-02-03'::date
)
INSERT INTO profiles (customer_id, first_name, last_name, email, email_status, phone_e164, phone_status, date_of_birth, is_active)
SELECT gen_random_uuid(), r.first_name, r.last_name, r.email_norm, 'Verified', r.phone_e164, 'Verified', r.dob, TRUE
FROM rows r
LEFT JOIN profiles p ON LOWER(p.email) = r.email_norm
WHERE p.id IS NULL;

-- 11) Seed Addresses (per customer)
-- Swapnil primary (Pune)
INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Flat 502, Sky Heights', 'Baner', 'Pune', 'Maharashtra', '411045', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.customer_id = p.customer_id
        AND a.line1 = 'Flat 502, Sky Heights'
        AND a.city = 'Pune'
  );

-- Swapnil secondary
INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Plot 10, Central Park', 'Hinjawadi Phase 2', 'Pune', 'Maharashtra', '411057', 'IN', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.customer_id = p.customer_id
        AND a.line1 = 'Plot 10, Central Park'
        AND a.city = 'Pune'
  );

-- Isha primary (Pune)
INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Nyati Emporius, Kharadi', NULL, 'Pune', 'Maharashtra', '411014', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.customer_id = p.customer_id
        AND a.line1 = 'Nyati Emporius, Kharadi'
        AND a.city = 'Pune'
  );

-- Rahul primary (Bengaluru)
INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Brigade Meadows, Kanakapura Road', NULL, 'Bengaluru', 'Karnataka', '560062', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('rahul.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM addresses a
      WHERE a.customer_id = p.customer_id
        AND a.line1 = 'Brigade Meadows, Kanakapura Road'
        AND a.city = 'Bengaluru'
  );

-- 12) Seed Verification Tokens (email link; hashes only)
-- swapnil
INSERT INTO verification_tokens (customer_id, type, channel_value, token_hash, expires_at, max_attempts, status)
SELECT p.customer_id, 'EmailLink', p.email, encode(digest('verify-swapnil-001','sha256'),'hex'), NOW() + INTERVAL '24 hours', 5, 'Pending'
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM verification_tokens t
      WHERE t.customer_id = p.customer_id
        AND t.type = 'EmailLink'
        AND t.channel_value = p.email
        AND t.status = 'Pending'
  );

-- isha
INSERT INTO verification_tokens (customer_id, type, channel_value, token_hash, expires_at, max_attempts, status)
SELECT p.customer_id, 'EmailLink', p.email, encode(digest('verify-isha-001','sha256'),'hex'), NOW() + INTERVAL '24 hours', 5, 'Pending'
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM verification_tokens t
      WHERE t.customer_id = p.customer_id
        AND t.type = 'EmailLink'
        AND t.channel_value = p.email
        AND t.status = 'Pending'
  );

-- 13) Seed Audit Records (INSERT per profile)
INSERT INTO audit_records (customer_id, entity_name, entity_id, action, actor, channel, old_json, new_json, correlation_id)
SELECT p.customer_id, 'Profile', 'profile', 'INSERT', 'system', 'Seed',
       NULL,
       jsonb_build_object(
         'first_name', p.first_name,
         'last_name', p.last_name,
         'email', p.email,
         'phone_e164', p.phone_e164
       ),
       gen_random_uuid()
FROM profiles p
WHERE NOT EXISTS (
    SELECT 1 FROM audit_records a
    WHERE a.customer_id = p.customer_id
      AND a.entity_name = 'Profile'
      AND a.entity_id = 'profile'
      AND a.action = 'INSERT'
);

COMMIT;

-- =====================================================================
-- End of script
-- =====================================================================