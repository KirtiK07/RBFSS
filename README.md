# Role-Based File Sharing System (RBFSS)

A secure, full-stack file management web application built with **ASP.NET Core 10 MVC**, featuring granular role-based access control, folder hierarchy, audit logging, and file sharing.

---

## Project Overview

RBFSS is a web-based file sharing system that implements role-based access control (RBAC) to ensure secure file management. The system supports three distinct user roles with different permission levels, a hierarchical folder structure, full audit logging, and a custom RBAC middleware layer.

---

## Architecture

The system follows a **Three-Tier Architecture**:

- **Presentation Layer** — ASP.NET Core MVC Controllers + Razor Views with Bootstrap 5.3
- **Business Logic Layer** — Service classes with role-based permission checks
- **Data Access Layer** — Entity Framework Core with Repository pattern

Security is enforced at **three independent layers**:
1. **RbacMiddleware** — blocks requests by URL prefix before the controller runs
2. **[Authorize] attributes** — controller/action level role checks
3. **Service layer** — ownership checks inside business logic (FileService, FolderService)

---

## User Roles & Permissions

| Action | Admin | Manager | User |
|--------|-------|---------|------|
| Upload files | ✅ | ✅ (manager/ folder) | ✅ (user/ folder) |
| Download any file | ✅ | ✅ | ❌ |
| Download own / shared file | ✅ | ✅ | ✅ |
| Edit own file | ✅ | ✅ | ✅ |
| Edit any file | ✅ | ❌ | ❌ |
| Delete own file | ✅ | ✅ | ✅ |
| Delete any file | ✅ | ❌ | ❌ |
| Share with all users | ✅ | ✅ | ❌ |
| Create / delete folders | ✅ | ✅ (own) | ❌ |
| Browse folder tree | ✅ | ✅ | ✅ |
| View all audit logs | ✅ | ❌ | ❌ |
| Manage users & roles | ✅ | ❌ | ❌ |

---

## Technology Stack

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 10 MVC (C#) | Web framework — routing, controllers, Razor views |
| Entity Framework Core 10 | ORM — code-first migrations, LINQ queries |
| ASP.NET Core Identity | Authentication, password hashing, role management |
| SQL Server LocalDB | Local development database |
| Bootstrap 5.3 | Responsive UI components |
| Custom RBAC Middleware | Route-level access enforcement |
| Repository + Service Pattern | Layered architecture for separation of concerns |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio) or SQL Server

---

## Setup Instructions

**1. Clone the repository**
```bash
git clone https://github.com/KirtiK07/Role-Based-File-Sharing-System.git
cd Role-Based-File-Sharing-System
```

**2. Apply database migrations**
```bash
dotnet ef database update
```

**3. Run the application**
```bash
dotnet run
```

**4. Open in browser**
```
http://localhost:5000
```

---

## Default Accounts (seeded on first run)

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@rbfss.com | Admin@123 |
| Manager | manager@rbfss.com | Manager@123 |
| User | user@rbfss.com | User@123 |

---

## Project Structure

```
RBFSS/
├── Controllers/        # HTTP handlers (Admin, Manager, User, File, Folder, Account)
├── Data/               # EF Core DbContext
├── Middleware/         # Custom RBAC middleware
├── Migrations/         # EF Core database migrations
├── Models/             # Domain models + DTOs
├── Repositories/       # Data access layer (interfaces + implementations)
├── Services/           # Business logic layer (interfaces + implementations)
├── Views/              # Razor views per controller
├── wwwroot/            # Static files, CSS, uploaded files
├── docs/               # Technical documentation (architecture + flowcharts)
├── Program.cs          # App bootstrap, DI registration, middleware pipeline
└── appsettings.json    # Connection string, file settings
```

---

## Key Features

- **File upload / download / edit / soft-delete** — files are never permanently erased
- **Hierarchical folder browser** — self-referencing parent → child folder tree with breadcrumb navigation
- **File sharing** — managers can share a file with all users in one click via FilePermission records
- **Role-based upload paths** — manager files go to `wwwroot/uploads/manager/`, user files to `wwwroot/uploads/user/`
- **Audit logging** — every upload, download, delete, and RBAC denial is recorded with user ID, timestamp, and IP address
- **White / blue / black UI theme** — clean navbar with dark-gradient header, blue-accented cards and tables

---


## Submitted By
- Kirti Kolare

**Submitted to**: School of Information Technology, Indira University
**Academic Year**: 2025-2026
