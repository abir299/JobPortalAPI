# JobPortalAPI — Job Portal REST API

A RESTful Job Portal API built with ASP.NET Core Web API, Entity Framework Core, and Microsoft SQL Server. It provides secure authentication, role-based authorization, job posting, advanced job search, application tracking, employer dashboards, and ownership-based access control.

## Features

- **JWT Authentication** — registration, login, password hashing, and token-based access.
- **Role-Based Authorization** — JobSeeker, Employer, and Admin roles.
- **Job Management** — create, view, update, and delete job postings.
- **Advanced Search** — search by job title, description, and company.
- **Job Filtering** — filter by location, category, job type, experience level, and salary range.
- **Sorting & Pagination** — sort by salary or posting date and retrieve paginated results.
- **Application Deadlines** — validate deadlines and prevent applications after a job's deadline.
- **Job Expiration Tracking** — expose `isExpired` in job details and employer dashboard responses.
- **Employer Dashboard** — view total, active, and expired jobs, application totals, and status statistics.
- **Job Applications** — apply for jobs, view personal applications, and prevent duplicate applications.
- **Application Management** — retrieve employer applications, filter by job or status, and update application statuses.
- **DTO Validation** — validate job data, salary ranges, pagination, and application status values.
- **Service Layer Architecture** — separate business logic from controllers.
- **EF Core & SQL Server** — database relationships and migrations.
- **Global Exception Handling** — consistent error responses.
- **Ownership-Based Authorization** — restrict employers to managing their own jobs and related applications; Admin has broader access.

## Tech Stack

- C#
- ASP.NET Core Web API
- .NET — see `JobPortalAPI.csproj` for the target framework
- Entity Framework Core
- Microsoft SQL Server
- JWT Bearer Authentication
- Data Annotations
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
├── Migrations/
├── Services/
│   └── JobService.cs
├── Program.cs
├── appsettings.json
└── JobPortalAPI.csproj
```

## Getting Started

### Prerequisites

- .NET SDK compatible with the project target framework
- Microsoft SQL Server
- Visual Studio or another compatible .NET IDE
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

3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Configure the SQL Server connection string using your local configuration.

5. Apply the existing EF Core migrations if needed, using Visual Studio Package Manager Console:

   ```powershell
   Update-Database
   ```

   Alternatively, use `dotnet ef database update` if EF Core CLI tools are installed and configured.

6. Run the API:

   ```bash
   dotnet run
   ```

Use the HTTPS URL displayed in the terminal as the API base URL.

## Configuration

Configure the settings expected by the application:

- `ConnectionStrings:DefaultConnection` — SQL Server connection string.
- `Jwt:Key` — private signing key.
- `Jwt:Issuer` — token issuer, if configured.
- `Jwt:Audience` — token audience, if configured.
- `Jwt:ExpiryMinutes` — token lifetime, if configured.

**Security:** Never commit real passwords, production signing keys, or other secrets. Use .NET User Secrets or environment variables for sensitive configuration.

## API Endpoints

Replace `{baseUrl}` with the HTTPS URL of your running application. Protected endpoints require a JWT Bearer token.

### Authentication

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/Account/register` | Public |
| POST | `/api/Account/login` | Public |

Example registration body (use an email that is not already registered):

```json
{
  "fullName": "Job Seeker",
  "email": "jobseeker@example.com",
  "password": "Test@12345"
}
```

Example login body:

```json
{
  "email": "jobseeker@example.com",
  "password": "Test@12345"
}
```

For protected endpoints, set Postman's Authorization type to **Bearer Token** and provide the token returned by login.

### Jobs

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/Jobs` | Public |
| GET | `/api/Jobs/{id}` | Public |
| POST | `/api/Jobs` | Employer, Admin |
| PUT | `/api/Jobs/{id}` | Job owner, Admin |
| DELETE | `/api/Jobs/{id}` | Job owner, Admin |
| GET | `/api/Jobs/dashboard` | Employer, Admin |

#### Search, Filter, Sort & Pagination

Supported query parameters:

| Parameter | Purpose |
|---|---|
| `search` | Search job title, description, or company |
| `location` | Filter by location |
| `category` | Filter by category |
| `jobType` | Filter by job type |
| `experienceLevel` | Filter by experience level |
| `minSalary` | Minimum salary |
| `maxSalary` | Maximum salary |
| `sortBy` | `salary_asc`, `salary_desc`, or `oldest`; default is newest |
| `page` | Page number; default `1` |
| `pageSize` | Results per page; default `10`, maximum `100` |

Example:

```text
GET /api/Jobs?search=Developer&location=Dhaka&jobType=Full-time&minSalary=30000&sortBy=salary_desc&page=1&pageSize=10
```

Public job search excludes jobs whose application deadline has passed. Jobs without a deadline remain available.

#### Create a Job

`POST /api/Jobs`

```json
{
  "title": "Junior .NET Developer",
  "description": "Develop and maintain ASP.NET Core Web APIs",
  "companyName": "ABC Technologies",
  "location": "Dhaka, Bangladesh",
  "salary": 40000,
  "category": "Software Engineering",
  "jobType": "Full-time",
  "experienceLevel": "Entry-level",
  "applicationDeadline": "2027-01-31T23:59:00Z"
}
```

Allowed `jobType` values:

- `Full-time`
- `Part-time`
- `Contract`
- `Internship`

Allowed `experienceLevel` values:

- `Entry-level`
- `Mid-level`
- `Senior`

`applicationDeadline` is optional. If supplied, it must be in the future.

#### Employer Dashboard

`GET /api/Jobs/dashboard`

The dashboard provides:

- Total jobs
- Active jobs
- Expired jobs
- Total applications
- Pending, Reviewed, Shortlisted, Accepted, and Rejected application counts
- Per-job application counts and expiration information

Employers see their own job data. Admin can access the broader dashboard.

### Job Applications

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/JobApplications/{jobId}` | JobSeeker |
| GET | `/api/JobApplications/my-applications` | JobSeeker |
| GET | `/api/JobApplications/employer-applications` | Employer, Admin |
| PUT | `/api/JobApplications/{applicationId}/status` | Job owner, Admin |

The employer applications endpoint supports optional `jobId` and `status` query parameters.

Supported application statuses:

- `Pending`
- `Reviewed`
- `Shortlisted`
- `Accepted`
- `Rejected`

Example status update body:

```json
{
  "status": "Shortlisted"
}
```

The API prevents duplicate applications by the same job seeker for the same job and blocks applications after the job's application deadline.

## Error Handling & Validation

The API validates request fields, pagination values, salary ranges, job IDs, and application status values. It returns appropriate client errors for invalid requests and enforces authorization when users attempt to manage jobs or applications outside their permissions.

## Testing

Use Postman to verify:

- Registration and login
- JWT-protected endpoints and role restrictions
- Job creation, details, updates, and deletion
- Search, filters, sorting, and pagination
- Invalid salary ranges and invalid request data
- Expired job filtering and deadline enforcement
- Employer dashboard metrics
- Application submission and duplicate prevention
- Application status updates and query filters
- Authorization for jobs and applications

## Future Improvements

- Automated unit and integration tests
- Swagger / OpenAPI documentation
- Structured logging and monitoring
- CI/CD pipeline
- Cloud deployment

## Author

**Abir Ahmed**

GitHub: https://github.com/abir299

Repository: https://github.com/abir299/JobPortalAPI
