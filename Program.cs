using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RequisitionSystem.Data;
using RequisitionSystem.Policies;
using Serilog;
using Serilog.Events;

/*****************************************************************************
 * APPLICATION INITIALIZATION
 ****************************************************************************/
var builder = WebApplication.CreateBuilder(args);

/*****************************************************************************
 * CONFIGURATION SETUP
 ****************************************************************************/
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

/*****************************************************************************
 * LOGGING CONFIGURATION
 ****************************************************************************/
var logFolder = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
if (!Directory.Exists(logFolder))
{
    Directory.CreateDirectory(logFolder);
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logFolder, "system-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

/*****************************************************************************
 * SERVICE CONFIGURATION
 ****************************************************************************/

/*****************************************************************************
 * 1. CONTROLLER SERVICE CONFIGURATION
 *    Enables MVC controllers with features like model binding, 
 *    validation, and JSON formatting
 ****************************************************************************/
builder.Services.AddControllers();

/*****************************************************************************
 * SECURITY CONFIGURATION
 * Enables detailed identity model error messages for debugging
 ****************************************************************************/
IdentityModelEventSource.ShowPII = true;

/*****************************************************************************
 * 2. API ENDPOINT EXPLORER SERVICE
 *    Enables discovery of API endpoints for documentation tools
 ****************************************************************************/
builder.Services.AddEndpointsApiExplorer();

/*****************************************************************************
 * 3. SWAGGER DOCUMENTATION SERVICE
 *    Configures API documentation generation with JWT support
 ****************************************************************************/
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Requisition API End Points", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

/*****************************************************************************
 * 4. DATABASE CONTEXT CONFIGURATION
 *    Registers the application's DbContext with SQL Server provider
 *    Includes retry logic for transient failures
 ****************************************************************************/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

/*****************************************************************************
 * 5. AUTHENTICATION SERVICE CONFIGURATION
 *    Sets up JWT Bearer token authentication with validation parameters
 ****************************************************************************/
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

/*****************************************************************************
 * 6. AUTHORIZATION POLICIES CONFIGURATION
 *    Defines custom authorization policies and their handlers
 ****************************************************************************/
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminPolicy()))
    .AddPolicy("EmployeeAccess", policy => policy.Requirements.Add(new EmployeePolicy()));

builder.Services.AddScoped<IAuthorizationHandler, AdminPolicyHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EmployeePolicyHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

/*****************************************************************************
 * 7. CORS CONFIGURATION
 *    Defines cross-origin resource sharing policies
 ****************************************************************************/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
    });

    /*
     * EXAMPLE OF RESTRICTED CORS POLICY
     * Uncomment to use specific trusted domains only
     */
    // options.AddPolicy("Restricted", policy =>
    // {
    //     policy.WithOrigins("https://yourfrontend.com", "https://admin.yoursite.com")
    //           .AllowAnyHeader()
    //           .AllowAnyMethod();
    // });
});

/*****************************************************************************
 * APPLICATION BUILDING
 ****************************************************************************/
var app = builder.Build();

/*****************************************************************************
 * MIDDLEWARE PIPELINE CONFIGURATION
 ****************************************************************************/

/*****************************************************************************
 * DEVELOPMENT ENVIRONMENT CONFIGURATION
 *    Enables developer exception page and Swagger UI
 ****************************************************************************/
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
}

/*****************************************************************************
 * PRODUCTION MIDDLEWARE
 *    HTTPS redirection and security headers
 ****************************************************************************/
app.UseHttpsRedirection();

/*****************************************************************************
 * ROUTING AND SECURITY MIDDLEWARE
 *    Configures request pipeline processing order
 ****************************************************************************/
app.UseRouting();
app.UseCors("AllowAll"); // (Change to "Restricted" for production environments with restricted CORS policy)
app.UseAuthentication();
app.UseAuthorization();

/*****************************************************************************
 * ENDPOINT CONFIGURATION
 *    Maps controller routes
 ****************************************************************************/
app.MapControllers();

/*****************************************************************************
 * APPLICATION EXECUTION
 ****************************************************************************/
app.Run();