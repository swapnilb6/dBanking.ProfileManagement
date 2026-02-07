-- Runs only on the first DB init (empty volume)
CREATE EXTENSION IF NOT EXISTS citext;
-- If you’re using UUID generation in SQL (not needed for EF), uncomment:
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";