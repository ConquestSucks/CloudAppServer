using CloudApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var dbContextConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CloudAppDbContext>(options => options.UseNpgsql(dbContextConnectionString));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(path: "Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
    
var services = scope.ServiceProvider;
var context = services.GetRequiredService<CloudAppDbContext>();
    
await context.Database.MigrateAsync();

app.Run();

Log.CloseAndFlush();