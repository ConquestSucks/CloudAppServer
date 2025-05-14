using System.Reflection;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using CloudAppServer.Infrastructure.BackgroundServices;
using CloudAppServer.Infrastructure.Persistence;
using CloudAppServer.Infrastructure.Persistence.Repositories;
using CloudAppServer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiDocument(); 

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

var apiAssembly = Assembly.Load("CloudAppServer.Api");
var applicationAssembly = Assembly.Load("CloudAppServer.Application");
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies([apiAssembly, applicationAssembly]));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

var telegramBotToken = builder.Configuration.GetValue<string>("TelegramBotToken");
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramBotToken!));

builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddScoped<ITelegramService, TelegramService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
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