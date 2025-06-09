using System.Reflection;
using System.Text;
using CloudAppServer.Application.Authentication.Interfaces;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.ConfigModels;
using CloudAppServer.Domain.Interfaces;
using CloudAppServer.Infrastructure.BackgroundServices;
using CloudAppServer.Infrastructure.ConfigModels;
using CloudAppServer.Infrastructure.Notifications;
using CloudAppServer.Infrastructure.Persistence;
using CloudAppServer.Infrastructure.Persistence.Repositories;
using CloudAppServer.Infrastructure.Services;
using CloudAppServer.Infrastructure.SignalR;
using CloudAppServer.Middleware;
using CloudAppServer.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 20L * 1024 * 1024 * 1024;  // 20 гигабайт
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20L * 1024 * 1024 * 1024; // 20 гигабайт
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbContextConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CloudAppDbContext>(options => options.UseNpgsql(dbContextConnectionString));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(path: "Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.Configure<S3Config>(builder.Configuration.GetSection("S3"));
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

var apiAssembly = Assembly.Load("CloudAppServer.Api");
var applicationAssembly = Assembly.Load("CloudAppServer.Application");
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(apiAssembly, applicationAssembly));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserLoginRequestRepository, UserLoginRequestRepository>();
builder.Services.AddScoped<ICloudFileRepository, CloudFileRepository>();

var telegramBotToken = builder.Configuration.GetValue<string>("TelegramBotToken");
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramBotToken!));

builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddSingleton<IS3Service, S3Service>();
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IAuthenticationSessionStore, AuthenticationSessionStore>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IFileUploadNotifier, SignalRFileNotifier>();

var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
if (jwtConfig is null)
    throw new NullReferenceException("JwtConfig is null");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
        
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://84.201.180.242"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Total-Pages", "X-Page-Size", "X-Page-Number", "Content-Disposition");
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}

using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;
var context = services.GetRequiredService<CloudAppDbContext>();

await context.Database.MigrateAsync();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHub<FileUploadHub>("/hubs/currentFileProgress");

app.Run();

Log.CloseAndFlush();