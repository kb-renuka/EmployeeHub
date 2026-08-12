# EmployeeHub — Employee Management REST API

A role-based Employee Management REST API built with **C#, ASP.NET Core 8 Web API, Entity Framework Core, and SQL Server**, featuring JWT authentication, CRUD operations, relational data access, validation, and Swagger/OpenAPI documentation.

## Tech Stack

- C# / ASP.NET Core 8 Web API
- Entity Framework Core 8 (Code-First, migrations)
- SQL Server
- ASP.NET Core Identity (users, roles, password hashing)
- JWT Bearer authentication
- Swashbuckle (Swagger / OpenAPI)

## Features

- **Auth**: register, login, JWT issuance, Admin/User roles (via ASP.NET Core Identity — no hand-rolled password hashing)
- **Employees**: full CRUD, search by name/email/job title, filter by department, pagination
- **Departments**: full CRUD (Admin-only writes), list employees per department
- **Leave Requests**: employee submits a request, Admin approves/rejects, full leave history per employee, pending queue for managers

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, a local SQL Server install, or SQL Server in Docker)

## Setup & Run

```bash
cd EmployeeHub.Api
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet run
```

Update the connection string in `appsettings.json` and replace `Jwt:Key` with your own secret before running anywhere but localhost. Open `/swagger` once running to test every endpoint interactively.

## Endpoint Reference

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | none | Register a new user (Admin or User role) |
| POST | `/api/auth/login` | none | Log in, returns JWT |
| GET | `/api/departments` | any authenticated user | List all departments |
| GET | `/api/departments/{id}` | any authenticated user | Get one department |
| GET | `/api/departments/{id}/employees` | any authenticated user | List employees in a department |
| POST | `/api/departments` | Admin | Create a department |
| PUT | `/api/departments/{id}` | Admin | Update a department |
| DELETE | `/api/departments/{id}` | Admin | Delete a department (blocked if it still has employees) |
| GET | `/api/employees?search=&departmentId=&page=&pageSize=` | any authenticated user | List/search/filter employees, paginated |
| GET | `/api/employees/{id}` | any authenticated user | Get one employee |
| POST | `/api/employees` | Admin | Add an employee |
| PUT | `/api/employees/{id}` | Admin | Update an employee |
| DELETE | `/api/employees/{id}` | Admin | Remove an employee |
| GET | `/api/leaverequests/employee/{employeeId}` | any authenticated user | Leave history for one employee |
| GET | `/api/leaverequests/pending` | Admin | Pending leave requests queue |
| POST | `/api/leaverequests/employee/{employeeId}` | any authenticated user | Submit a leave request |
| PUT | `/api/leaverequests/{id}/action` | Admin | Approve or reject a pending request |
