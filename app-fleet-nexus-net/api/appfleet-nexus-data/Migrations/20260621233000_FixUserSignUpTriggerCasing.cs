using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFleetNexus.Data.Migrations
{
    public partial class FixUserSignUpTriggerCasing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
DECLARE
    v_tenant_id     UUID;
    v_first_name    TEXT;
    v_last_name     TEXT;
BEGIN
    -- Extract metadata from Supabase signup payload
    v_first_name := NEW.raw_user_meta_data ->> 'first_name';
    v_last_name  := NEW.raw_user_meta_data ->> 'last_name';

    -- Fallback: use email prefix if no name provided
    IF v_first_name IS NULL OR v_first_name = '' THEN
        v_first_name := split_part(NEW.email, '@', 1);
    END IF;

    -- 1. Create public.users row (mirrors auth.users) with quoted PascalCase columns
    INSERT INTO public.users (""Id"", ""Email"", ""FirstName"", ""LastName"", ""CreatedDate"", ""IsDeleted"")
    VALUES (NEW.id, NEW.email, v_first_name, v_last_name, NOW(), FALSE);

    -- 2. Create a new tenant with quoted PascalCase columns
    v_tenant_id := gen_random_uuid();
    INSERT INTO public.tenants (""Id"", ""Name"", ""CreatedDate"", ""IsActive"")
    VALUES (v_tenant_id, COALESCE(v_first_name, '') || '''s Organization', NOW(), TRUE);

    -- 3. Link user to tenant as Admin with quoted PascalCase columns
    INSERT INTO public.tenant_users (""UserId"", ""TenantId"", ""Role"", ""IsSuperAdmin"", ""JoinedDate"")
    VALUES (NEW.id, v_tenant_id, 'Admin', FALSE, NOW());

    RETURN NEW;
END;
$$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to the lowercase columns
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
DECLARE
    v_tenant_id     UUID;
    v_first_name    TEXT;
    v_last_name     TEXT;
BEGIN
    v_first_name := NEW.raw_user_meta_data ->> 'first_name';
    v_last_name  := NEW.raw_user_meta_data ->> 'last_name';

    IF v_first_name IS NULL OR v_first_name = '' THEN
        v_first_name := split_part(NEW.email, '@', 1);
    END IF;

    INSERT INTO public.users (id, email, first_name, last_name, created_date, is_deleted)
    VALUES (NEW.id, NEW.email, v_first_name, v_last_name, NOW(), FALSE);

    v_tenant_id := gen_random_uuid();
    INSERT INTO public.tenants (id, name, created_date, is_active)
    VALUES (v_tenant_id, COALESCE(v_first_name, '') || '''s Organization', NOW(), TRUE);

    INSERT INTO public.tenant_users (user_id, tenant_id, role, is_super_admin, joined_date)
    VALUES (NEW.id, v_tenant_id, 'Admin', FALSE, NOW());

    RETURN NEW;
END;
$$;
");
        }
    }
}
