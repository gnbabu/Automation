
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AutomationAPI.Repositories;
using AutomationAPI.Repositories.Helpers;
using AutomationAPI.Repositories.Interfaces;
using AutomationAPI.Repositories.TestRunner;

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
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IAutomationRepository, AutomationRepository>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
builder.Services.AddScoped<ITestSuitesRepository, TestSuitesRepository>();

builder.Services.AddScoped<ITestRunner, ReflectionTestRunner>();

builder.Services.AddHostedService<TestQueueWorker>();


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
