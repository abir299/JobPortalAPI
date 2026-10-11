# JobPortalAPI

A Job Portal REST API built with ASP.NET Core Web API, Entity Framework Core, and SQL Server.

## Features

- User registration and login
- JWT authentication
- Role-based authorization (JobSeeker, Employer, Admin)
- Job creation, viewing, updating, and deletion
- Job search, location filtering, category filtering, and pagination
- Job application submission
- View personal job applications
- Employer application management
- Application status updates
- Global error handling
- SQL Server database integration

## Technologies Used

- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- Microsoft SQL Server
- JWT Bearer Authentication
- Postman

## Database Setup

1. Create or configure a SQL Server database connection.
2. Update the `DefaultConnection` value in `appsettings.json`.
3. Open Package Manager Console in Visual Studio.
4. Run:

```powershell
Update-Database
```

## Run the Project

1. Open `JobPortalAPI.sln` in Visual Studio.
2. Configure the database connection and JWT settings.
3. Build and run the project.
4. Use Postman to test the API endpoints.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Account/register` | Register a user |
| POST | `/api/Account/login` | Login and obtain JWT |
| GET | `/api/Jobs` | Get jobs with search and filters |
| GET | `/api/Jobs/{id}` | Get job details |
| POST | `/api/Jobs` | Create a job (Employer/Admin) |
| PUT | `/api/Jobs/{id}` | Update a job |
| DELETE | `/api/Jobs/{id}` | Delete a job |
| POST | `/api/JobApplications/{jobId}` | Apply for a job |
| GET | `/api/JobApplications/my-applications` | View personal applications |
| PUT | `/api/JobApplications/{applicationId}/status` | Update application status |
| GET | `/api/JobApplications/employer-applications` | View applications for employer's jobs |

## Authentication

For protected endpoints, log in through `/api/Account/login` and copy the returned JWT token.

In Postman, select **Authorization → Bearer Token** and paste the token.

Newly registered users receive the `JobSeeker` role. Employer access must be assigned securely by an administrator.

## Application Statuses

Supported status values:

- Reviewed
- Shortlisted
- Rejected
- Accepted

New applications start with `Pending`.

## Project Purpose

This project was developed to practice REST API development, database integration, authentication, authorization, and job application workflows using ASP.NET Core.
