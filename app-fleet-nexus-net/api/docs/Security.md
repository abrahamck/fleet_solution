# Implement Security Features

1. This app caters to multiple teneants.
2. Each teneant has 
    2.1 its own set of users.
    2.2 Its own set of Drivers.
    2.3 Its own set of Trucks.
3. So the Database should be designed to support this multi-tenancy.
    3.1 Each table should have a TeneantId column.
    3.2 Each table should have a CreatedBy, ModifiedBy, CreatedDate, ModifiedDate columns.
    3.3 Soft Delete for all entities.
    3.4 All queries should be tenant aware.
4. Users are mapped to a Tenant and is the one that logs in and uses the app.
   
    
    
    
    
