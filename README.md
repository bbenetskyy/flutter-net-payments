# flutter-net-payments
A full-stack payment platform with a Flutter frontend and .NET backend, enabling secure user management, authentication, virtual bank card creation, IBAN account linking, and peer-to-peer transfers.

## Architecture note: Unified MicroAppDb

MicroApp now uses a single EF Core DbContext: MicroAppDb, which contains all domain tables:
- Users and Roles (identity/authorization)
- Cards (cards domain)

Rationale:
- Simpler local development and deployment with one schema and one connection string.
- Consistent seeding and startup initialization in a single place.

Configuration (MicroApp/appsettings.json):
- Program.cs reads ConnectionStrings:DefaultConnection for MicroAppDb.
- Legacy UsersConnection and CardsConnection keys are no longer used by MicroApp and may be removed later.

Example:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=MicroApp;..."
}

Database creation/migrations:
- On startup, the app ensures the database exists (EnsureCreated) and seeds default roles and an optional admin user.
- If you prefer migrations for MicroAppDb, add and apply migrations targeting MicroAppDb in the future.

## Changelog

| Description                                                                                                                                                                                                                                                                                                                                                                      | Consumed Time |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------|
| Mapping out project structure                                                                                                                                                                                                                                                                                                                                                    | 1 hour        |
| Created MSSQL Docker Image with AuthService. Connect to DB and make Tables for Users and Roles. Add Validation and predefine roles and admin users.                                                                                                                                                                                                                              | 3 hours       |
| Created UserService. Separated Users and Roles Table. Create Connection between two microservices and makes JWT authentication works between them.                                                                                                                                                                                                                               | 3 hours       |
| Created CardsService. Create Table for Cards and validation. Face problem with simultaneous working of 3 microservice instances with own DB files.                                                                                                                                                                                                                               | 4 hours       |
| Replace microservices DB files to single MSSQL Docker Image. Make all microservices work with one DB - will be a solution, but migrations to DB in that case will cause a lot of new problem I would like to avoid. So solution in our case where microservices are not so important and could be divided easely later (in case we will need it) - replace with one API Service. | 0.5 hours     |
| Created PaymentService. Create Table for Payments and validation. Ensure Permissions are worked at Postman and new relations between tables are correct.                                                                                                                                                                                                                         | 3 hours       |