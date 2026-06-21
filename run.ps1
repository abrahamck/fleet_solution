$ErrorActionPreference = 'Continue'
dotnet ef migrations add MultiTenantSecurityModel --project c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-data --startup-project c:\Learn\fleet_solution\app-fleet-nexus-net\api\appfleet-nexus-api > c:\Learn\fleet_solution\ef-log.txt 2>&1
echo "Finished EF" >> c:\Learn\fleet_solution\ef-log.txt
