using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AppFleetNexus.Security.Tenancy;

public class RlsConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public RlsConnectionInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        base.ConnectionOpened(connection, eventData);
        SetSessionVariables(connection);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        await SetSessionVariablesAsync(connection, cancellationToken);
    }

    private void SetSessionVariables(DbConnection connection)
    {
        if (_tenantContext.TenantId == Guid.Empty) return;

        using var command = connection.CreateCommand();
        command.CommandText = $"SET LOCAL app.current_tenant_id = '{_tenantContext.TenantId}';";
        if (_tenantContext.IsSuperAdmin)
        {
            command.CommandText += " SET LOCAL app.is_super_admin = 'true';";
        }
        command.ExecuteNonQuery();
    }

    private async Task SetSessionVariablesAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId == Guid.Empty) return;

        using var command = connection.CreateCommand();
        command.CommandText = $"SET LOCAL app.current_tenant_id = '{_tenantContext.TenantId}';";
        if (_tenantContext.IsSuperAdmin)
        {
            command.CommandText += " SET LOCAL app.is_super_admin = 'true';";
        }
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
