using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFleetNexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostgresRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Enable RLS
                ALTER TABLE public.contacts ENABLE ROW LEVEL SECURITY;
                ALTER TABLE public.vehicles ENABLE ROW LEVEL SECURITY;

                -- Force RLS on the table owner
                ALTER TABLE public.contacts FORCE ROW LEVEL SECURITY;
                ALTER TABLE public.vehicles FORCE ROW LEVEL SECURITY;

                -- Policies for contacts
                DROP POLICY IF EXISTS tenant_isolation_select ON public.contacts;
                CREATE POLICY tenant_isolation_select ON public.contacts
                    FOR SELECT USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_insert ON public.contacts;
                CREATE POLICY tenant_isolation_insert ON public.contacts
                    FOR INSERT WITH CHECK (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_update ON public.contacts;
                CREATE POLICY tenant_isolation_update ON public.contacts
                    FOR UPDATE USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_delete ON public.contacts;
                CREATE POLICY tenant_isolation_delete ON public.contacts
                    FOR DELETE USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                -- Policies for vehicles
                DROP POLICY IF EXISTS tenant_isolation_select ON public.vehicles;
                CREATE POLICY tenant_isolation_select ON public.vehicles
                    FOR SELECT USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_insert ON public.vehicles;
                CREATE POLICY tenant_isolation_insert ON public.vehicles
                    FOR INSERT WITH CHECK (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_update ON public.vehicles;
                CREATE POLICY tenant_isolation_update ON public.vehicles
                    FOR UPDATE USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );

                DROP POLICY IF EXISTS tenant_isolation_delete ON public.vehicles;
                CREATE POLICY tenant_isolation_delete ON public.vehicles
                    FOR DELETE USING (
                        ""TenantId""::text = current_setting('app.current_tenant_id', true)
                        OR current_setting('app.is_super_admin', true) = 'true'
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Disable RLS
                ALTER TABLE public.contacts NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE public.vehicles NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE public.contacts DISABLE ROW LEVEL SECURITY;
                ALTER TABLE public.vehicles DISABLE ROW LEVEL SECURITY;

                -- Drop policies
                DROP POLICY IF EXISTS tenant_isolation_select ON public.contacts;
                DROP POLICY IF EXISTS tenant_isolation_insert ON public.contacts;
                DROP POLICY IF EXISTS tenant_isolation_update ON public.contacts;
                DROP POLICY IF EXISTS tenant_isolation_delete ON public.contacts;

                DROP POLICY IF EXISTS tenant_isolation_select ON public.vehicles;
                DROP POLICY IF EXISTS tenant_isolation_insert ON public.vehicles;
                DROP POLICY IF EXISTS tenant_isolation_update ON public.vehicles;
                DROP POLICY IF EXISTS tenant_isolation_delete ON public.vehicles;
            ");
        }
    }
}
