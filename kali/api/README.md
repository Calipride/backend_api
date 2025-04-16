# API Documentation

## Overview
This is a .NET 8.0 Web API project that implements user authentication and management using JWT (JSON Web Tokens) for security. The API uses LiteDB as its database and includes features like user registration, authentication, profile management, and file uploads.

## Project Structure
```
api/
├── Controllers/         # API Controllers
├── Models/             # Data models and DTOs
├── Data/              # Database files
└── wwwroot/           # Static files (including uploaded images)
```

## Key Components

### Authentication
- Uses JWT Bearer token authentication
- Tokens are generated upon successful login
- Protected endpoints require valid JWT tokens
- Custom BearerTokenHandler for flexible token handling

### Database
- Uses LiteDB (embedded NoSQL database)
- Database file location: `Data/CalisPod.db`
- Automatic database creation if not exists

### Models
- `User`: Core user entity with profile information
- `AuthDtos`: Data transfer objects for authentication
- `JwtSettings`: JWT configuration settings
- `TokenService`: JWT token generation and management
- `PasswordHelper`: Password hashing and verification
- `UserRepository`: Database operations for users

## API Endpoints

### Public Endpoints
```
POST /api/user/register
{
    "username": "string",
    "email": "string",
    "password": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string"
}

POST /api/user/login
{
    "email": "string",
    "password": "string"
}
```

### Protected Endpoints (Require Authentication)
```
GET /api/user               # Get all users
GET /api/user/{id}         # Get user by ID
PUT /api/user/{id}         # Update user
DELETE /api/user/{id}      # Delete user
POST /api/user/{id}/upload-profile-picture  # Upload profile picture
```

## Authentication Flow
1. User registers through `/api/user/register`
2. User logs in through `/api/user/login`
3. Server returns JWT token
4. Include token in subsequent requests:
   ```
   Authorization: Bearer <your-token>
   ```

## Setting Up Development Environment

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Configuration
The application uses two main configuration files:
- `appsettings.json`: Production settings
- `appsettings.Development.json`: Development settings

Key configuration sections:
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "calis-pod-api",
    "Audience": "calis-pod-client",
    "ExpirationInMinutes": 60
  },
  "DatabaseSettings": {
    "DatabasePath": "Data/CalisPod.db"
  }
}
```

### Running the Application
1. Clone the repository
2. Navigate to the api directory
3. Run:
   ```
   dotnet restore
   dotnet run
   ```
4. Access Swagger UI at `http://localhost:5174/swagger`

## Security Features
- Password hashing using BCrypt
- JWT token authentication
- Role-based authorization (Admin/User roles)
- File upload security checks
- Input validation using Data Annotations

## Development Guidelines

### Adding New Endpoints
1. Create appropriate DTOs in Models folder
2. Add new controller method
3. Apply proper authorization attributes
4. Implement repository methods if needed
5. Add validation

### Best Practices
- Always use DTOs for input/output
- Implement proper error handling
- Use authentication for sensitive endpoints
- Validate all input data
- Follow REST conventions

### Error Handling
The API uses standard HTTP status codes:
- 200: Success
- 400: Bad Request
- 401: Unauthorized
- 403: Forbidden
- 404: Not Found
- 500: Internal Server Error

## Testing
Use the provided `api.http` file for testing endpoints. It includes examples for:
- User registration
- Login
- Protected endpoint access
- File uploads

## Admin Features
Special admin endpoints are protected with `[Authorize(Roles = "Admin")]`:
- Make other users admin
- Access to all user data
- System management features

The first user with email "test@example.com" is automatically made admin.

## API Response Formats

### Success Responses
```json
// Login Response
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

// User Response
{
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string",
    "profilePicturePath": "string",
    "isAdmin": boolean,
    "bookings": [],
    "reviews": []
}
```

### Error Responses
```json
// 400 Bad Request
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Bad Request",
    "status": 400,
    "detail": "Email already registered"
}

// 401 Unauthorized
{
    "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    "title": "Unauthorized",
    "status": 401,
    "detail": "Invalid email or password"
}

// 403 Forbidden
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
    "title": "Forbidden",
    "status": 403,
    "detail": "Insufficient permissions to perform this action"
}
```

## Common Use Cases

### User Registration and Authentication
1. Register a new user
2. Login to get JWT token
3. Use token for subsequent requests

### Profile Management
1. Get user profile using JWT token
2. Update profile information
3. Upload profile picture
4. Delete account if needed

### Admin Operations
1. Login as admin (test@example.com)
2. List all users
3. Modify user roles
4. Manage system settings

## Troubleshooting

### Common Issues
1. Token Issues
   - Check token expiration
   - Verify correct "Bearer" prefix
   - Ensure token is sent in Authorization header

2. Permission Issues
   - Verify user roles
   - Check if endpoint requires admin access
   - Ensure token belongs to correct user

3. File Upload Issues
   - Check file size limits
   - Verify supported file types
   - Ensure multipart/form-data format

### Debugging Tips
1. Use Swagger UI for API exploration
2. Check HTTP response codes and messages
3. Monitor server logs in development mode
4. Use the provided examples.http for testing