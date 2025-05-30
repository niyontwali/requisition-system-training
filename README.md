# Requisition System API

## Overview

This is a comprehensive requisition management system API built with ASP.NET Core. It provides endpoints for managing users, roles, materials, and requisitions with proper authentication and authorization.

## Key Features

- JWT-based authentication
- Role-based authorization (Admin/Employee)
- CRUD operations for:
  - Users
  - Roles
  - Materials
  - Requisitions
- Detailed audit logging
- Swagger/OpenAPI documentation
- Custom authorization policies

## Core Concepts

### JWT Authentication

JSON Web Tokens are used for secure authentication. Tokens contain user claims and are signed with a secret key.

### Role-Based Access Control (RBAC)

The system implements:

- `AdminPolicy`: For admin-only endpoints
- `EmployeePolicy`: For employee access
- Custom authorization middleware for consistent error responses

### Entity Framework Core

Used for database operations with:

- Code-first migrations
- LINQ queries
- Eager loading of related entities

### Repository Pattern

Data access is abstracted through DbContext, providing separation of concerns.

## Getting Started

### Prerequisites

- .NET 6 SDK
- SQL Server (or compatible database)
- Your favorite IDE (VS Code, Visual Studio, Rider)

### Installation

1. Clone the repository
2. Create `appsettings.json` based on `appsettings.example.json`
3. Configure your database connection string and JWT settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Your_SQL_Connection_String"
  },
  "JwtSettings": {
    "SecretKey": "Your_256-bit_Secret_Key",
    "Issuer": "Your_Issuer",
    "Audience": "Your_Audience",
    "ExpiryMinutes": 60
  }
}
```

4. Apply database migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Authenticate and get JWT

### Users (Admin only)

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID

### Roles (Admin only)

- `GET /api/roles` - Get all roles
- `POST /api/roles` - Create new role
- `PUT /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Delete role

### Materials

- `GET /api/materials` - Get all materials
- `POST /api/materials` - Create material
- `PUT /api/materials/{id}` - Update material
- `DELETE /api/materials/{id}` - Delete material

### Requisitions

- `GET /api/requisitions` - Get all requisitions
- `POST /api/requisitions` - Create requisition
- `PUT /api/requisitions/{id}` - Update requisition
- `PATCH /api/requisitions/{id}/status` - Update status
- `GET /api/requisitions/user/{userId}` - Get user's requisitions
- `GET /api/requisitions/status/{status}` - Filter by status

## Testing the API

The API includes Swagger UI for testing:

1. Run the application
2. Navigate to `/swagger` in your browser
3. Use the interactive documentation to test endpoints

## Security Considerations

- Always use HTTPS in production
- Keep your JWT secret key secure
- Regularly rotate secrets
- Follow principle of least privilege for roles
- Validate all inputs

## Logging

The system implements Serilog for structured logging:

- Logs are written to both console and file
- Daily log rotation with 7-day retention
- Different log levels for system vs application logs

## Deployment

For production deployment:

1. Configure proper CORS policies
2. Set up HTTPS certificates
3. Configure production database
4. Set environment to Production
5. Consider using a reverse proxy (Nginx, IIS)

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss proposed changes.

## License

[MIT](https://choosealicense.com/licenses/mit/)
