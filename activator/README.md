# Graphene Trace — Sensore Live Brief (Prototype)

This repository contains a prototype ASP.NET Core Web API (C#) that ingests Sensore pressure-map CSV frames (32x32), stores them in SQL Server, computes metrics and alerts, and exposes endpoints for users, clinicians and admins.

Features implemented:
- DB schema (EF Core) for Users, PressureFrames, Metrics, Alerts, Comments
- Pressure analysis service computing Peak Pressure Index and Contact Area %
- Alerts for high-pressure regions (connected components >= 10 pixels and high pressure)
- API endpoints for uploading frames, querying metrics over time ranges, posting comments

Assumptions:
- Pressure frames are 32x32 values (CSV with 32 columns per row, 32 rows per frame).
- Values range 1-255; 1 indicates no pressure.
- Authentication/authorization is stubbed (role property on user). Replace with real auth in production.

Run (Windows PowerShell):
```powershell
# Configure SQL Server connection in appsettings.json (DefaultConnection)
dotnet restore
dotnet ef database update
dotnet run --project Activator.Api
```

Next steps / TODO:
- Add JWT authentication and role-based authorization
- Add frontend dashboard for heatmap visualization
- Add more metrics and nicer visualizations
