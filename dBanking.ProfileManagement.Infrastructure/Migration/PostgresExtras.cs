using Microsoft.EntityFrameworkCore.Migrations;

public partial class PostgresExtras : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable citext so contacts.email can be case-insensitive
        migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS citext;");

        // If you didn't set HasColumnType("citext") in configuration, you can still do an expression unique index instead:
        // migrationBuilder.Sql(@"
        //   CREATE UNIQUE INDEX IF NOT EXISTS ux_contacts_email_lower
        //   ON contacts ((lower(email)));
        // ");

        // The partial unique index for addresses is emitted by the model with HasFilter.
        // If you prefer to assert it here too:
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relname = 'ux_addresses_one_primary_per_customer' AND n.nspname = current_schema()
                ) THEN
                    CREATE UNIQUE INDEX ux_addresses_one_primary_per_customer
                    ON addresses (customer_id) WHERE is_primary = true;
                END IF;
            END
            $$;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // You typically leave extensions installed. If you must drop:
        // migrationBuilder.Sql(@"DROP EXTENSION IF EXISTS citext;");

        migrationBuilder.Sql(@"DROP INDEX IF EXISTS ux_addresses_one_primary_per_customer;");
        // migrationBuilder.Sql(@"DROP INDEX IF EXISTS ux_contacts_email_lower;");
    }
}