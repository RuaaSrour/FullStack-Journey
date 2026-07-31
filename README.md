# Student Course Management API

A RESTful ASP.NET Core Web API for managing students, teachers, and courses.

This project was built to practice working with ASP.NET Core Web API, Entity Framework Core, JWT
Authentication, and role-based authorization. It includes CRUD operations, course enrollment,
prerequisites, authentication, and search with pagination.

---

## Features

- Student, Teacher, and Course CRUD operations
- JWT Authentication
- Role-based Authorization (Admin, Teacher, Student)
- Student ownership authorization
- Student-Course enrollment
- Teacher-Course assignment
- Course prerequisites
- Course status and pass status
- Search and pagination
- Swagger API documentation

---

## Technologies

- ASP.NET Core 10
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI

---

## Project Structure

```
Controllers/
DTOs/
Models/
Data/
Services/
Migrations/
Program.cs
appsettings.json
```

---

## Prerequisites

Install these before you start:

1. **.NET 10 SDK** — `dotnet --version` should print `10.x`.
2. **SQL Server LocalDB** — the default connection string points at `(localdb)\MSSQLLocalDB`.
   - Windows: comes with the Visual Studio "ASP.NET and web development" workload, or install
     standalone via the "SQL Server Express LocalDB" installer.
   - macOS/Linux, or if you don't want LocalDB: LocalDB itself is Windows-only. Run SQL Server in
     Docker instead (`mcr.microsoft.com/mssql/server`) and change
     `ConnectionStrings:DefaultConnection` in `Assig1/appsettings.json` to point at it, e.g.
     `Server=localhost,1433;Database=Assig1Db;User Id=sa;Password=...;TrustServerCertificate=True`.
3. Optional: Visual Studio 2022+ (for the Package Manager Console migration workflow), or just the
   `dotnet` CLI — both are covered below.

---

## Getting Started

### 1. Clone and restore

```bash
git clone <this-repo-url>
cd Assig1/Assig1
dotnet restore
```

All commands below assume you're in `Assig1/Assig1` (the folder containing `Assig1.csproj`),
unless stated otherwise.

### 2. Configure the JWT signing key (required)

The JWT signing key is **not** stored in `appsettings.json` and is **not** committed to git — it
lives in .NET User Secrets. The app throws `InvalidOperationException: JWT key is missing` on
startup until you set one. On any new machine, run:

```bash
dotnet user-secrets set "Jwt:Key" "a-long-random-string-at-least-32-characters-long"
```

(`Assig1.csproj` already has a `UserSecretsId`, so `dotnet user-secrets init` isn't needed.)

`Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpiryMinutes` are already set in `appsettings.json` and
don't need changing.

### 3. Database

The connection string in `appsettings.json` defaults to a local `Assig1Db` database on LocalDB —
no changes needed if you're using LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Assig1Db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Apply migrations with **either**:

**Option A — Visual Studio Package Manager Console**

```
Update-Database
```

**Option B — dotnet CLI**

```bash
dotnet tool install --global dotnet-ef   # skip if already installed
dotnet ef database update
```

This creates all tables: `Students`, `Teachers`, `Courses`, `StudentCourses`,
`CoursePrerequisites`, `Users` (accounts).

### 4. Bootstrap a Teacher/Admin account

The only self-service registration endpoint is `POST /api/auth/register/student`, which creates a
**Student**-role account. There is no API endpoint to create Teacher/Admin accounts — by design,
so nobody can grant themselves elevated access. On a fresh database you need to insert the first
Teacher/Admin account directly.

Generate a password hash with a throwaway console app (uses the same `PasswordHasher<T>` the API
uses, so the hash is compatible):

```bash
dotnet new console -o TempHasher
cd TempHasher
dotnet add package Microsoft.Extensions.Identity.Core
```

Replace `Program.cs` with:

```csharp
using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<object>();
Console.WriteLine(hasher.HashPassword(null!, "Admin123!"));
```

```bash
dotnet run
```

Copy the printed hash, then insert the account (`Role`: `0` = Student, `1` = Teacher, `2` = Admin):

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Assig1Db -Q "INSERT INTO Users (Username, PasswordHash, Role, StudentId, TeacherId) VALUES ('admin', '<paste-hash-here>', 2, NULL, NULL);"
```

Repeat with `Role = 1` for a Teacher account. You can delete the `TempHasher` folder afterward.
`StudentId`/`TeacherId` can stay `NULL` — an account only needs a link if you want it to represent
a specific existing `Student`/`Teacher` row.

### 5. Run the app

```bash
dotnet run --launch-profile http
```

- API base URL: `http://localhost:5088`
- Swagger UI: `http://localhost:5088/swagger`
- (`--launch-profile https` also exposes `https://localhost:7186`)

### 6. Authenticate in Swagger

1. `POST /api/auth/login` with `{ "username": "...", "password": "..." }` (or
   `POST /api/auth/register/student` first if you want a fresh student account — it needs an
   existing `Student` row's `Id`, create one via `POST /api/students` as Admin first, or use one
   already in the database).
2. Copy the `token` value from the response.
3. Click the padlock icon (top-right of Swagger UI), paste the token (no `Bearer ` prefix — Swagger
   adds it), click **Authorize**.
4. All subsequent requests in that Swagger session send the token automatically.

---

## API Reference

Enums are serialized/accepted as strings (`CourseStatus`: `NotStarted`, `InProgress`, `Completed`,
`Withdrawn`; `PassStatus`: `Pending`, `Passed`, `Failed`; `UserRole`: `Student`, `Teacher`, `Admin`).

**Role key:** _Any_ = any authenticated user · _Own_ = a Student caller is additionally restricted
to their own linked `studentId` (403 Forbidden otherwise) · Teacher/Admin never restricted.

### Auth — `/api/auth` (all `[AllowAnonymous]`)

| Method & route           | Body                                | Notes                                                                                                                                |
| ------------------------ | ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| POST `/register/student` | `{ username, password, studentId }` | Creates a Student-role account linked to an existing `Student`. 400 if username taken, student not found, or student already linked. |
| POST `/login`            | `{ username, password }`            | Returns `{ token, username, role, expiresAt }`. 401 on bad credentials.                                                              |

### Students — `/api/students`

| Method & route                           | Roles                | Body                                           | Notes                                                                                                              |
| ---------------------------------------- | -------------------- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| GET `/`                                  | Teacher, Admin       | —                                              | All students.                                                                                                      |
| GET `/{id}`                              | Any (Own)            | —                                              | 404 if missing.                                                                                                    |
| POST `/`                                 | Admin                | `{ name }`                                     |                                                                                                                    |
| PUT `/{id}`                              | Admin                | `{ name }`                                     |                                                                                                                    |
| DELETE `/{id}`                           | Admin                | —                                              |                                                                                                                    |
| POST `/{studentId}/courses/{courseId}`   | Student (Own), Admin | —                                              | Enrolls; checks prerequisites — 400 with `missingPrerequisites` list if unmet; 400 if already enrolled.            |
| DELETE `/{studentId}/courses/{courseId}` | Student (Own), Admin | —                                              | Drops the enrollment.                                                                                              |
| PUT `/{studentId}/courses/{courseId}`    | Teacher, Admin       | `{ courseStatus, passStatus, completionDate }` | Grades/status. **Deliberately excludes Student** — can't self-mark as Passed. 400 if `Passed` without `Completed`. |
| GET `/{studentId}/courses`               | Any (Own)            | —                                              | Student's enrolled courses.                                                                                        |

### Courses — `/api/courses`

| Method & route                                            | Roles          | Body                    | Notes                               |
| --------------------------------------------------------- | -------------- | ----------------------- | ----------------------------------- |
| GET `/`                                                   | Any            | —                       |                                     |
| GET `/{id}`                                               | Any            | —                       |                                     |
| POST `/`                                                  | Teacher, Admin | `{ name, hours (1-4) }` |                                     |
| PUT `/{id}`                                               | Teacher, Admin | `{ name, hours }`       |                                     |
| DELETE `/{id}`                                            | Teacher, Admin | —                       |                                     |
| GET `/{courseId}/students`                                | Any            | —                       | Enrolled students.                  |
| GET `/{courseId}/teachers`                                | Any            | —                       | Assigned teachers.                  |
| GET `/{courseId}/prerequisites`                           | Any            | —                       |                                     |
| POST `/{courseId}/prerequisites/{prerequisiteCourseId}`   | Teacher, Admin | —                       | 400 if self-reference or duplicate. |
| DELETE `/{courseId}/prerequisites/{prerequisiteCourseId}` | Teacher, Admin | —                       |                                     |

### Teachers — `/api/teachers`

| Method & route                           | Roles | Body       | Notes                                                   |
| ---------------------------------------- | ----- | ---------- | ------------------------------------------------------- |
| GET `/`                                  | Any   | —          |                                                         |
| GET `/{id}`                              | Any   | —          |                                                         |
| POST `/`                                 | Admin | `{ name }` |                                                         |
| PUT `/{id}`                              | Admin | `{ name }` |                                                         |
| DELETE `/{id}`                           | Admin | —          |                                                         |
| POST `/{teacherId}/courses/{courseId}`   | Admin | —          | Assigns a course to a teacher. 400 if already assigned. |
| DELETE `/{teacherId}/courses/{courseId}` | Admin | —          |                                                         |
| GET `/{teacherId}/courses`               | Any   | —          |                                                         |

### Student-course search — `/api/student-courses` (class-level `Teacher,Admin` only)

| Method & route | Roles          | Query params                                                 | Notes                                                                                                                                                              |
| -------------- | -------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| GET `/search`  | Teacher, Admin | `studentName?`, `courseName?`, `pageNumber=1`, `pageSize=10` | AND-combined `Contains()` filters, sorted by student name then course name, returns `PagedResult` (`items`, `totalCount`, `totalPages`, `pageNumber`, `pageSize`). |

---

## Testing Walkthrough

Run these in order (Swagger, curl, or Postman). Replace `$TOKEN` with the token from the matching
login.

### 1. Bootstrap data as Admin

Log in as your seeded admin account, then:

- `POST /api/students` a couple of students, `POST /api/teachers` a teacher, `POST /api/courses`
  two courses (e.g. "Intro to C#", "Advanced C#").
- `POST /api/courses/{advancedId}/prerequisites/{introId}` to require Intro before Advanced.

### 2. Register + log in as a Student

- `POST /api/auth/register/student` with one of the student IDs from step 1.
- `POST /api/auth/login` to get that student's token.
- Confirm ownership checks: `GET /api/students/{ownId}` → `200`; `GET /api/students/{otherId}` →
  `403`; same for `GET /api/students/{id}/courses`.
- `POST /api/students/{ownId}/courses/{advancedId}` → `400` (missing prerequisite: Intro).
- `POST /api/students/{ownId}/courses/{introId}` → `200` (no prerequisite required).
- `GET /api/students` (list all) → `403` (Student isn't allowed).

### 3. Log in as Teacher

- `PUT /api/students/{ownId}/courses/{introId}` with
  `{ "courseStatus": "Completed", "passStatus": "Passed", "completionDate": "2026-01-01" }` →
  `200`.
- `GET /api/student-courses/search?studentName=...` → `200`, paged results.

### 4. Back to the Student token

- `POST /api/students/{ownId}/courses/{advancedId}` → now `200` (prerequisite satisfied).
- `PUT /api/students/{ownId}/courses/{introId}` → `403` (Students can't self-grade).

### 5. Log in as Admin

Verify that Admin can access all endpoints, including:

- Students
- Teachers
- Courses
- Update
- Delete

If every step above returns the expected status code, auth, roles, ownership checks, prerequisite
validation, and search/pagination are all verified working.

---

## Known Limitations

- LocalDB is Windows only; cross-platform use requires pointing the connection string at a real
  SQL Server instance (see Prerequisites).
- Teacher and Admin accounts are created manually — no self-registration endpoint exists for those
  roles, by design.
- SQL Server must be installed locally (or reachable over the network/Docker).
- `dotnet-ef` CLI is optional; migrations can be created/applied from Visual Studio's Package
  Manager Console instead.

---

## Future Improvements

- Refresh Tokens
- Repository Pattern
- Unit Testing
- Docker Support
- Logging
- AutoMapper
- Global Exception Handling

---

## Author

Ruaa Srour

Computer Engineering Student
