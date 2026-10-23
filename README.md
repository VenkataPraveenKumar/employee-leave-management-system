# Employee Leave Management System (ELMS)

An enterprise-grade, full-stack leave management solution built with an **ASP.NET Core Web API** backend and a modern **Angular (zoneless, signal-driven)** frontend.

---

## Architecture Overview

```text
employee-leave-management-system/
├── backend/
│   ├── src/
│   │   ├── ELMS.Domain/             # Entities, Enums, Interfaces
│   │   ├── ELMS.Application/        # DTOs, Business Logic, Services
│   │   ├── ELMS.Infrastructure/     # EF Core, DbContext, Migrations, Repositories
│   │   └── ELMS.WebApi/             # Controllers, Middleware, Auth Config
│   └── tests/
│       └── ELMS.UnitTests/          # xUnit & Moq Test Suites
├── frontend/                        # Angular 20+ (Standalone, Signals, Zoneless)
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                # Guards, Interceptors, Models, Services
│   │   │   ├── features/            # Auth, Dashboard, Leaves, Approvals
│   │   │   └── shared/              # Reusable UI Components
│   │   └── environments/
│   └── package.json
├── .github/
│   └── workflows/
│       └── build-and-test.yml       # CI/CD Pipeline
└── README.md




Local Development Setup
Prerequisites
.NET 8.0 SDK
Node.js 22+ & npm
SQL Server (LocalDB, Express, or Developer Edition)
Angular CLI (npm install -g @angular/cli)

1. Backend Setup

# Navigate to the solution directory
cd *****\employee-leave-management-system

# Restore dependencies
dotnet restore EmployeeLeaveManagementSystem.sln

# Apply database migrations and seed data
dotnet ef database update --project backend/src/ELMS.Infrastructure --startup-project backend/src/ELMS.WebApi

# Run the API project
dotnet run --project backend/src/ELMS.WebApi

The Web API launches at https://localhost:44304 (Swagger available at https://localhost:44304/swagger).

2. Frontend Setup

# Navigate to the frontend directory
cd ******\employee-leave-management-system\frontend

# Install dependencies
npm install --legacy-peer-deps

# Start the dev server
npm start

Access the UI at http://localhost:4200.

------------------------------------------------

Running Automated Tests

# Backend Unit Tests (xUnit + Moq)
dotnet test EmployeeLeaveManagementSystem.sln --configuration Release --verbosity normal

# Frontend Unit Tests (Vitest)
cd frontend
npm test -- --no-watch
