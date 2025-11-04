using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Activator.Api.Data;
using Activator.Api.Services;
using Activator.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure EF Core: prefer real SQL Server if configured, otherwise use InMemory for local/dev
// For local development use an in-memory database unless a valid SQL Server connection
// string is explicitly provided in configuration. This avoids accidental attempts
// to connect to a misconfigured SQL Server during local runs and CI-free demos.
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(conn))
{
    // If a connection string is provided, use SQL Server. This allows developers
    // to attach a real database (for example with SSMS) by setting the
    // `ConnectionStrings:DefaultConnection` value in appsettings.Development.json
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(conn));
}
else
{
    // Default to in-memory for Development when no SQL connection is configured
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("activator_inmemory")
           .EnableSensitiveDataLogging());
}

builder.Services.AddScoped<IPressureAnalysisService, PressureAnalysisService>();

var app = builder.Build();

// Always enable Swagger for this demo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Activator API V1");
});

app.UseHttpsRedirection();

// Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Enable CORS
app.UseCors();

// Simple header-based auth middleware for demo / local development
app.UseMiddleware<SimpleAuthMiddleware>();

app.UseAuthorization();

app.MapControllers();

// If using a real SQL Server connection (DefaultConnection present), ensure the
// database/schema exists on startup. For simple demos we call EnsureCreated; in
// production you should use EF Migrations instead.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("Database ensured/created.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WARNING: Could not EnsureCreated database: {ex.Message}");
    }
}

app.Run();
