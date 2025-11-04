# Attach / Use the Activator database with SSMS

This project defaults to an in-memory database for quick local runs. To attach and use a real SQL Server database (so you can view it in SQL Server Management Studio / SSMS), follow these steps.

1) Choose a SQL Server instance

- Recommended for local development: `(localdb)\MSSQLLocalDB` (installed with Visual Studio).
- Or use a local SQL Server Express instance: `.\SQLEXPRESS` or a remote server.

2) Create the database (two options)

Option A — Let the app create the schema (recommended for quick local setup):
- Update `Activator.Api\appsettings.Development.json` (already includes a sample `DefaultConnection`).
- Ensure the connection is correct for your SQL Server instance.
- Start the API (it will call `EnsureCreated()` on startup and create the database + tables):

```powershell
cd "c:\Users\Lenovo\Desktop\Graphene\activator\Activator.Api"
dotnet run
```

- After startup, open SSMS and connect to your instance. Locate the `ActivatorDb` database and explore tables.

Option B — Create DB first in SSMS and let app connect:
- Open SSMS and connect to your SQL Server instance.
- Right-click `Databases` → `New Database...` → name it `ActivatorDb` (or another name).
- Update `Activator.Api\appsettings.Development.json` -> `ConnectionStrings:DefaultConnection` with that server and DB name.
- Start the API; it will create the schema automatically.

3) (Optional) Use EF Migrations instead of EnsureCreated

- Install the EF tools if not present:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

- From the solution root, add an initial migration and apply it:

```powershell
cd "c:\Users\Lenovo\Desktop\Graphene\activator"
cd Activator.Api
dotnet ef migrations add InitialCreate -p . -s ..\Activator.Api\Activator.Api.csproj
dotnet ef database update -p . -s ..\Activator.Api\Activator.Api.csproj
```

4) Verify in SSMS

- After the app runs and the schema is created, open SSMS → connect to your instance → expand `Databases` → `ActivatorDb` → `Tables` and you should see tables like `Users`, `PressureFrames`, `PressureMetrics`, `Alerts`, `Comments`, `ClinicianAssignments`.

5) Troubleshooting

- If the API fails to start due to SQL Server login issues, check the connection string. For Windows integrated auth use `Trusted_Connection=True`. For SQL auth provide `User Id=...;Password=...`.
- If using LocalDB and you cannot see it in SSMS, ensure LocalDB is installed and visible (connect to `(localdb)\\MSSQLLocalDB`).
- If the schema doesn't appear, either run EF Migrations or confirm the API logs showed `Database ensured/created.` on startup.

6) After attaching

- Once the DB is present and the API is configured to use it, the `test-api.ps1` script (or the Web UI) will exercise the real DB and you can inspect the inserted rows in SSMS.

If you want I can:
- Attempt to connect to your LocalDB from this environment and create the database automatically (I can run the commands if you allow me to run commands on your machine). 
- Or, I can add a helper PowerShell script that creates the DB and runs EF migrations automatically. Which option do you prefer?