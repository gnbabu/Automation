
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.TestRunner;
using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.Workers;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ------------------------------
// Logging
// ------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ------------------------------
// Configuration
// ------------------------------
var configuration = builder.Configuration;

// ------------------------------
// Services
// ------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Repositories
builder.Services.AddScoped<SqlDataAccessHelper>();
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAutomationRepository, AutomationRepository>();
builder.Services.AddScoped<ITestSuitesRepository, TestSuitesRepository>();
builder.Services.AddScoped<ITestScreenshotRepository, TestScreenshotRepository>();
builder.Services.AddScoped<ITestCaseAssignmentRepository, TestCaseAssignmentRepository>();
builder.Services.AddScoped<ITestCaseExecutionQueueRepository, TestCaseExecutionQueueRepository>();
builder.Services.AddScoped<ITestCaseExecutionLogRepository, TestCaseExecutionLogRepository>();
builder.Services.AddScoped<IReleaseRepository, ReleaseRepository>();
builder.Services.AddScoped<IReleaseFileService, ReleaseFileService>();
builder.Services.AddScoped<IReleaseReadinessService, ReleaseReadinessService>();
builder.Services.AddScoped<IReleaseNotificationService, ReleaseNotificationService>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();

// Email providers - all plain SMTP, so every one of them (Brevo, Mailgun, Amazon SES's
// SMTP interface, and any future SMTP-based vendor - Office365, Zoho, Postmark, etc.)
// shares the same generic SmtpEmailService/SmtpProviderSettings pair; adding a new
// provider only ever needs another name added to this list + its own Email:<Name> config
// block, never a new class. Named options let multiple distinct SmtpProviderSettings
// instances (one per provider name) coexist from a single settings type. SendGrid support
// has been removed.
var smtpProviderNames = new[] { "Brevo", "Office365", "AmazonSES" };
foreach (var providerName in smtpProviderNames)
{
    builder.Services.Configure<SmtpProviderSettings>(providerName, builder.Configuration.GetSection($"Email:{providerName}"));

    builder.Services.AddKeyedScoped<IEmailService>(providerName, (sp, key) =>
        new SmtpEmailService(
            sp.GetRequiredService<IOptionsMonitor<SmtpProviderSettings>>().Get((string)key!),
            sp.GetRequiredService<ILogger<SmtpEmailService>>(),
            (string)key!));
}

// The only place provider selection happens - everything else in the app just injects
// plain IEmailService and never knows which provider is active. Defaults to Brevo unless
// Email:Provider is explicitly set to something else (e.g. a future SMTP provider).
builder.Services.AddScoped<IEmailService>(sp =>
{
    var providerName = builder.Configuration["Email:Provider"];
    providerName = string.IsNullOrWhiteSpace(providerName) ? "Brevo" : providerName;
    return sp.GetRequiredKeyedService<IEmailService>(providerName);
});


builder.Services.AddScoped<ITestRunner, NUnitEngineTestRunner>();

builder.Services.AddHostedService<TestQueueWorker>();
builder.Services.AddHostedService<ReleaseDllsReadyNotificationWorker>();


// Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = configuration["JWTKey:ValidIssuer"],
            ValidAudience = configuration["JWTKey:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWTKey:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured"))
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

// Swagger (with JWT auth support)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Automation API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token (e.g., 'Bearer eyJhbGci...')"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
// ------------------------------
// Build the app
// ------------------------------
var app = builder.Build();

// ------------------------------
// Middleware
// ------------------------------
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowAll");

app.UseAuthentication();  // <-- Required before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
