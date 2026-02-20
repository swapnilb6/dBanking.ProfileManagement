-- =====================================================================
-- Profile Management DB Reset & Seed (Aligned with corrected entities/DTOs/mapping)
-- =====================================================================

BEGIN;

-- Drop triggers first if present
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_profiles_updated_at') THEN
    EXECUTE 'DROP TRIGGER IF EXISTS trg_profiles_updated_at ON profiles;';
  END IF;
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_addresses_updated_at') THEN
    EXECUTE 'DROP TRIGGER IF EXISTS trg_addresses_updated_at ON addresses;';
  END IF;
END $$;

-- Drop indexes (legacy + current)
DROP INDEX IF EXISTS ux_profiles_email_lower;
DROP INDEX IF EXISTS ux_profiles_email_verified_lower;
DROP INDEX IF EXISTS ux_profiles_pending_email_lower;
DROP INDEX IF EXISTS ux_profiles_phone_verified;
DROP INDEX IF EXISTS ux_profiles_pending_phone;

DROP INDEX IF EXISTS ix_addresses_profile_id;
DROP INDEX IF EXISTS ix_addresses_customer_id;
DROP INDEX IF EXISTS ux_addresses_one_primary_per_profile;
DROP INDEX IF EXISTS ux_addresses_one_primary_per_customer;

DROP INDEX IF EXISTS ix_verification_tokens_profile_id;
DROP INDEX IF EXISTS ix_verification_tokens_lookup;
DROP INDEX IF EXISTS ux_verification_tokens_token;
DROP INDEX IF EXISTS ux_verification_tokens_pending_unique;

DROP INDEX IF EXISTS ix_audit_entity;
DROP INDEX IF EXISTS ix_audit_action_time;
DROP INDEX IF EXISTS ix_audit_customer_time;

-- Drop tables (children first)
DROP TABLE IF EXISTS verification_tokens;
DROP TABLE IF EXISTS addresses;
DROP TABLE IF EXISTS audit_records;
DROP TABLE IF EXISTS profiles;

-- Drop helper funcs
DROP FUNCTION IF EXISTS set_updated_at;

COMMIT;

BEGIN;

-- Extensions
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Helper trigger for updated_at
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ===========================
-- PROFILES
-- ===========================
CREATE TABLE IF NOT EXISTS profiles (
  id                     BIGSERIAL   PRIMARY KEY,
  customer_id            UUID        NOT NULL UNIQUE,

  first_name             VARCHAR(100) NOT NULL,
  last_name              VARCHAR(100),

  -- contact (verification-first)
  email                  VARCHAR(256),
  email_status           VARCHAR(32)  NOT NULL DEFAULT 'Unverified',
  pending_email          VARCHAR(256),

  phone_e164             VARCHAR(20),
  phone_status           VARCHAR(32)  NOT NULL DEFAULT 'Unverified',
  pending_phone_e164     VARCHAR(20),

  date_of_birth          DATE,
  is_active              BOOLEAN      NOT NULL DEFAULT TRUE,

  -- preferences (flattened)
  sms_enabled            BOOLEAN,
  email_enabled          BOOLEAN,
  push_enabled           BOOLEAN,
  regulatory_consent     BOOLEAN,
  language               VARCHAR(16),
  time_zone              VARCHAR(64),
  preferences_updated_at TIMESTAMPTZ,

  created_at             TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- case-insensitive unique for verified + pending
CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_email_verified_lower
  ON profiles ((LOWER(email)))
  WHERE email IS NOT NULL AND email_status = 'Verified';

CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_pending_email_lower
  ON profiles ((LOWER(pending_email)))
  WHERE pending_email IS NOT NULL;

-- phone uniqueness (verified + pending)
CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_phone_verified
  ON profiles (phone_e164)
  WHERE phone_e164 IS NOT NULL AND phone_status = 'Verified';

CREATE UNIQUE INDEX IF NOT EXISTS ux_profiles_pending_phone
  ON profiles (pending_phone_e164)
  WHERE pending_phone_e164 IS NOT NULL;

-- updated_at trigger
CREATE TRIGGER trg_profiles_updated_at
BEFORE UPDATE ON profiles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ===========================
-- ADDRESSES
-- ===========================
CREATE TABLE IF NOT EXISTS addresses (
  id              BIGSERIAL     PRIMARY KEY,
  address_id      UUID          NOT NULL UNIQUE,
  customer_id     UUID          NOT NULL REFERENCES profiles(customer_id) ON DELETE CASCADE,

  type            VARCHAR(32)   NOT NULL DEFAULT 'Residential', -- maps from entity Type / enum->string
  line1           VARCHAR(200)  NOT NULL,
  line2           VARCHAR(200),
  city            VARCHAR(100)  NOT NULL,
  state_province  VARCHAR(100),
  postal_code     VARCHAR(20),
  country_code    CHAR(2)       NOT NULL,

  is_primary      BOOLEAN       NOT NULL DEFAULT FALSE,

  effective_from  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  effective_to    TIMESTAMPTZ,

  created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  updated_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_addresses_customer_id ON addresses(customer_id);

CREATE UNIQUE INDEX IF NOT EXISTS ux_addresses_one_primary_per_customer
  ON addresses(customer_id) WHERE is_primary = TRUE;

CREATE TRIGGER trg_addresses_updated_at
BEFORE UPDATE ON addresses
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ===========================
-- VERIFICATION TOKENS
-- ===========================
CREATE TABLE IF NOT EXISTS verification_tokens (
  id             BIGSERIAL     PRIMARY KEY,
  customer_id    UUID          NOT NULL REFERENCES profiles(customer_id) ON DELETE CASCADE,

  type           VARCHAR(16)   NOT NULL,     -- 'EmailLink'|'SmsOtp'
  channel_value  VARCHAR(256)  NOT NULL,     -- normalized email/phone
  token_hash     CHAR(64)      NOT NULL,     -- sha-256 hex
  otp_salt       VARCHAR(64),                -- optional salt used for OTP

  issued_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  expires_at     TIMESTAMPTZ   NOT NULL,
  created_at     TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

  attempt_count  INT           NOT NULL DEFAULT 0,
  max_attempts   INT           NOT NULL DEFAULT 5,
  status         VARCHAR(24)   NOT NULL DEFAULT 'Pending', -- 'Pending'|...
  verified_at    TIMESTAMPTZ,

  correlation_id UUID,
  verification_id UUID         NOT NULL DEFAULT gen_random_uuid(), -- external id
  failure_reason TEXT
);

CREATE INDEX IF NOT EXISTS ix_verification_tokens_lookup
  ON verification_tokens (customer_id, type, status, channel_value, expires_at DESC);

CREATE UNIQUE INDEX IF NOT EXISTS ux_verification_tokens_pending_unique
  ON verification_tokens (customer_id, type, channel_value)
  WHERE status = 'Pending';

-- ===========================
-- AUDIT (append-only)
-- ===========================
CREATE TABLE IF NOT EXISTS audit_records (
  id                 BIGSERIAL    PRIMARY KEY,
  customer_id        UUID         NOT NULL,
  entity_name        VARCHAR(128) NOT NULL,
  entity_id          VARCHAR(64)  NOT NULL,
  action             VARCHAR(32)  NOT NULL,
  actor              VARCHAR(128),
  channel            VARCHAR(64),
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

-- ===========================
-- SEED (idempotent)
-- ===========================
BEGIN;

-- Seed profiles
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

-- Addresses for Swapnil, Isha, Rahul
INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Flat 502, Sky Heights', 'Baner', 'Pune', 'Maharashtra', '411045', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.customer_id = p.customer_id AND a.line1 = 'Flat 502, Sky Heights' AND a.city = 'Pune');

INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Plot 10, Central Park', 'Hinjawadi Phase 2', 'Pune', 'Maharashtra', '411057', 'IN', FALSE
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.customer_id = p.customer_id AND a.line1 = 'Plot 10, Central Park' AND a.city = 'Pune');

INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Nyati Emporius, Kharadi', NULL, 'Pune', 'Maharashtra', '411014', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.customer_id = p.customer_id AND a.line1 = 'Nyati Emporius, Kharadi' AND a.city = 'Pune');

INSERT INTO addresses (address_id, customer_id, type, line1, line2, city, state_province, postal_code, country_code, is_primary)
SELECT gen_random_uuid(), p.customer_id, 'Residential', 'Brigade Meadows, Kanakapura Road', NULL, 'Bengaluru', 'Karnataka', '560062', 'IN', TRUE
FROM profiles p
WHERE LOWER(p.email) = LOWER('rahul.kulkarni@example.com')
  AND NOT EXISTS (SELECT 1 FROM addresses a WHERE a.customer_id = p.customer_id AND a.line1 = 'Brigade Meadows, Kanakapura Road' AND a.city = 'Bengaluru');

-- Verification tokens (email)
INSERT INTO verification_tokens (customer_id, type, channel_value, token_hash, otp_salt, expires_at, max_attempts, status)
SELECT p.customer_id, 'EmailLink', p.email, encode(digest('verify-swapnil-001','sha256'),'hex'), NULL, NOW() + INTERVAL '24 hours', 5, 'Pending'
FROM profiles p
WHERE LOWER(p.email) = LOWER('swapnil.bawankar@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM verification_tokens t
      WHERE t.customer_id = p.customer_id AND t.type = 'EmailLink' AND t.channel_value = p.email AND t.status = 'Pending'
  );

INSERT INTO verification_tokens (customer_id, type, channel_value, token_hash, otp_salt, expires_at, max_attempts, status)
SELECT p.customer_id, 'EmailLink', p.email, encode(digest('verify-isha-001','sha256'),'hex'), NULL, NOW() + INTERVAL '24 hours', 5, 'Pending'
FROM profiles p
WHERE LOWER(p.email) = LOWER('isha.kulkarni@example.com')
  AND NOT EXISTS (
      SELECT 1 FROM verification_tokens t
      WHERE t.customer_id = p.customer_id AND t.type = 'EmailLink' AND t.channel_value = p.email AND t.status = 'Pending'
  );

-- Seed one audit per profile
INSERT INTO audit_records (customer_id, entity_name, entity_id, action, actor, channel, old_json, new_json, correlation_id)
SELECT p.customer_id, 'Profile', 'profile', 'INSERT', 'system', 'Seed', NULL,
       jsonb_build_object('first_name', p.first_name, 'last_name', p.last_name, 'email', p.email, 'phone_e164', p.phone_e164),
       gen_random_uuid()
FROM profiles p
WHERE NOT EXISTS (
    SELECT 1 FROM audit_records a WHERE a.customer_id = p.customer_id AND a.entity_name = 'Profile' AND a.entity_id = 'profile' AND a.action = 'INSERT'
);

COMMIT;

-- =====================================================================
-- End of script
-- =====================================================================