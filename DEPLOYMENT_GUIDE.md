# RBFSS Deployment Guide

## Prerequisites

1. **SQL Server**
   - SQL Server 2019+ or SQL Server Express
   - SQL Server Management Studio (optional)

2. **IIS (For Windows Server Deployment)**
   - IIS 10.0+
   - ASP.NET Core Runtime 8.0
   - ASP.NET Core Hosting Bundle

3. **Development Environment**
   - Visual Studio 2022
   - .NET 8.0 SDK

## Local Development Setup

### 1. Database Setup

```powershell
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Navigate to project directory
cd RBFSS

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 2. Run Application

```powershell
# Restore packages
dotnet restore

# Build application
dotnet build

# Run in development mode
dotnet run --environment Development
```

### 3. Access Application

- Navigate to: `https://localhost:7001`
- Use default accounts:
  - Admin: admin@rbfss.com / Admin@123
  - Manager: manager@rbfss.com / Manager@123
  - User: user@rbfss.com / User@123

## Production Deployment

### 1. Database Configuration

Update `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=RBFSS_Production;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  },
  "FileSettings": {
    "UploadPath": "C:\\inetpub\\wwwroot\\RBFSS\\uploads",
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".txt", ".jpg", ".png", ".gif"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Publish Application

```powershell
# Publish for production
dotnet publish -c Release -o ./publish

# Or for specific runtime
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### 3. IIS Setup

1. **Install ASP.NET Core Hosting Bundle**
2. **Create Application Pool**
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated
3. **Create Website**
   - Point to published folder
   - Set appropriate bindings

### 4. Create Database in Production

```sql
-- Connect to SQL Server and create database
CREATE DATABASE RBFSS_Production;

-- Run EF migrations in production
dotnet ef database update --environment Production
```

### 5. Set Permissions

```powershell
# Grant IIS_IUSRS permissions to uploads folder
icacls "C:\inetpub\wwwroot\RBFSS\uploads" /grant "IIS_IUSRS:(OI)(CI)F"

# Grant permissions to application folder
icacls "C:\inetpub\wwwroot\RBFSS" /grant "IIS_IUSRS:(OI)(CI)RX"
```

## Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RBFSS.csproj", "."]
RUN dotnet restore "RBFSS.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RBFSS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RBFSS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RBFSS.dll"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  rbfss:
    build: .
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=RBFSS_Production;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - sqlserver
    volumes:
      - ./uploads:/app/wwwroot/uploads

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

## Monitoring & Maintenance

### 1. Logging

Configure structured logging in `Program.cs`:

```csharp
builder.Logging.AddConsole();
builder.Logging.AddFile("logs/rbfss-{Date}.txt");
```

### 2. Health Checks

Add health checks for database and file system:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddDiskStorageHealthCheck(options =>
        options.AddDrive("C:\\", 1024)); // 1GB minimum
```

### 3. Backup Strategy

```sql
-- Database backup script
BACKUP DATABASE RBFSS_Production
TO DISK = 'C:\Backups\RBFSS_Production.bak'
WITH FORMAT, INIT,
     MEDIANAME = 'RBFSS_Backup',
     NAME = 'Full Backup of RBFSS_Production';
```

### 4. File System Backup

- Regular backup of uploads folder
- Monitor disk space usage
- Implement file cleanup policies

## Security Considerations

### 1. SSL/TLS Configuration

```csharp
// Configure HTTPS in production
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

### 2. Security Headers

```csharp
app.Use((context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    return next.Invoke();
});
```

### 3. File Upload Security

- Scan uploaded files for malware
- Implement file type validation
- Store files outside web root
- Use virus scanning integration

## Performance Optimization

### 1. Database Optimization

```csharp
// Configure connection pooling
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure();
    });
});
```

### 2. Caching

```csharp
// Add memory caching
builder.Services.AddMemoryCache();

// Add distributed caching for scale-out scenarios
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

### 3. File Serving

```csharp
// Optimize static file serving
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
```

## Troubleshooting Common Issues

### Database Connection Issues

```powershell
# Test connection
sqlcmd -S YOUR_SERVER -U YOUR_USERNAME -P YOUR_PASSWORD -Q "SELECT 1"

# Check EF migrations
dotnet ef migrations list
dotnet ef database update --verbose
```

### File Upload Issues

```powershell
# Check folder permissions
icacls "path\to\uploads" /verify

# Monitor disk space
Get-WmiObject -Class Win32_LogicalDisk | Select-Object Size,FreeSpace
```

### Performance Issues

- Monitor application performance counters
- Use Application Insights for Azure deployments
- Implement database query profiling
- Monitor memory usage and garbage collection

## Scaling Considerations

### 1. Load Balancing

- Configure multiple IIS instances
- Use sticky sessions for authentication
- Implement distributed caching

### 2. Database Scaling

- Implement read replicas
- Consider database partitioning
- Optimize indexes and queries

### 3. File Storage

- Move to cloud storage (Azure Blob, AWS S3)
- Implement CDN for file delivery
- Consider file deduplication