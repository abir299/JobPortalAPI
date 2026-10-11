# JobPortalAPI — Job Portal REST API

A RESTful Job Portal API built with ASP.NET Core Web API, Entity Framework Core, and SQL Server. The application supports job posting, job applications, JWT authentication, role-based authorization, and employer-specific access control.

## Features

- **JWT Authentication** — secure registration and login with hashed passwords
- **Role-Based Authorization** — JobSeeker, Employer, and Admin roles
- **Job Management** — create, view, update, and delete job postings
- **Job Search & Filtering** — search by keyword, location, and category
- **Pagination** — paginated job listings with total record count
- **Job Applications** — apply for jobs and track application status
- **Application Management** — employers can review applications and update their status
- **DTOs & Validation** — request validation and controlled API responses
- **Service Layer Architecture** — business logic separated from controllers
- **Entity Framework Core** — database access using SQL Server
- **Global Exception Handling** — consistent responses for unexpected server errors
- **Ownership-Based Authorization** — employers can only update or delete their own jobs

## Tech Stack

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- Microsoft SQL Server
- JWT Bearer Authentication
- Data Annotation Validation
- Postman
- Git & GitHub

## Project Structure

```text
JobPortalAPI/
├── Controllers/
│   ├── AccountController.cs
│   ├── JobsController.cs
│   └── JobApplicationsController.cs
├── Data/
│   └── JobPortalDbContext.cs
├── DTOs/
│   ├── Account/
│   ├── Applications/
│   └── Jobs/
├── Interfaces/
│   └── IJobService.cs
├── Models/
├── Services/
│   └── JobService.cs
├── Program.cs
├── appsettings.json
└── JobPortalAPI.csproj
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Microsoft SQL Server
- Visual Studio 2022 or another compatible .NET IDE
- Postman

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/abir299/JobPortalAPI.git
   ```

2. Open the project directory:

   ```bash
   cd JobPortalAPI/JobPortalAPI
   ```

3. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

4. Configure the database connection string in `appsettings.json`.

5. Ensure the `JobPortalDb` database and required tables exist. Apply the project's EF Core migrations if migrations are configured.

6. Run the application:

   ```bash
   dotnet run
   ```

Use the HTTPS URL displayed in the terminal as the API base URL.

## Configuration

Configure the following settings for your local environment:

- `ConnectionStrings:DefaultConnection` — SQL Server connection string
- `Jwt:Key` — a strong, private signing key
- `Jwt:Issuer` and `Jwt:Audience` — JWT configuration values, if required by your setup
- `Jwt:ExpiryMinutes` — token lifetime setting

**Security:** Never commit real passwords, production JWT signing keys, or other private credentials. Use .NET User Secrets or environment variables for sensitive configuration.

## API Endpoints

Replace `{baseUrl}` with your running application's base URL.

### Authentication

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/Account/register` | Public |
| POST | `/api/Account/login` | Public |

Registration example:

```json
{
  "fullName": "Job Seeker",
  "email": "jobseeker@example.com",
  "password": "Test@12345"
}
```

Login example:

```json
{
  "email": "jobseeker@example.com",
  "password": "Test@12345"
}
```

Use the returned JWT token for protected endpoints:

```text
Authorization: Bearer <your-token>
```

### Jobs

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/Jobs` | Public |
| GET | `/api/Jobs/{id}` | Public |
| POST | `/api/Jobs` | Employer, Admin |
| PUT | `/api/Jobs/{id}` | Employer, Admin — owner or Admin |
| DELETE | `/api/Jobs/{id}` | Employer, Admin — owner or Admin |

Search and pagination example:

```text
GET /api/Jobs?search=Developer&location=Dhaka&page=1&pageSize=10
```

Job creation example:

```json
{
  "title": "Junior .NET Developer",
  "description": "Develop and maintain ASP.NET Core Web APIs",
  "companyName": "ABC Technologies",
  "location": "Dhaka, Bangladesh",
  "salary": 40000,
  "category": "Software Engineering"
}
```

### Job Applications

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/JobApplications/{jobId}` | JobSeeker |
| GET | `/api/JobApplications/my-applications` | JobSeeker |
| GET | `/api/JobApplications/employer-applications` | Employer, Admin |
| PUT | `/api/JobApplications/{applicationId}/status` | Employer, Admin — ownership enforced |

Supported application status values:

- `Reviewed`
- `Shortlisted`
- `Rejected`
- `Accepted`

Status update example:

```json
{
  "status": "Shortlisted"
}
```

## Testing

The API can be tested with Postman.

Tested scenarios include:

- User registration and login
- JWT-protected endpoints
- Job creation and updates
- Keyword and location filtering
- Pagination
- DTO validation for invalid input
- Job application submission and status updates
- Forbidden access when another employer attempts to modify a job they do not own

## Future Improvements

- Swagger / OpenAPI documentation
- Automated unit and integration tests
- Logging and structured error reporting
- Advanced filtering and sorting
- CI/CD and cloud deployment

## Author

**Abir Ahmed**

GitHub: [abir299](https://github.com/abir299)
