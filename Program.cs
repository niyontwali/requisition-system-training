using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RequisitionSystem.Data;

// creation of a builder object
var builder = WebApplication.CreateBuilder(args);

// Get your variable keys from app settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

/**********************************************
Configuring services
************************************************/
/**********************************************
1. Adds a controller service for mvc
This will allow us to use controller action, 
model binding, validation using data annotations 
specified in models, json formating for request and response bodies
************************************************/
builder.Services.AddControllers();

// TO make sure that error stacks related to Identity is shown
IdentityModelEventSource.ShowPII = true;


/**********************************************
 2. Adds an endpoint api explorer that will allow 
 the app to discover API Endpoints. This is used
 by swagger to list available routes
************************************************/
builder.Services.AddEndpointsApiExplorer();

/**********************************************
3. Adds a service to the builder to enable automatic 
generation of API documentation using swagger and 
provides a UI to test your endpoints
************************************************/
// builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Requisition API End Points", Version = "v1" });

    // Add JWT Bearer definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    // Make sure all endpoints require authorization by default
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
/************************************************************************
    4. AddDbContext
    Register the application's DbContext with SQL Server as the database provider.
    This sets up Entity Framework Core to connect to a SQL Server database using the
    connection string defined in appsettings.json under "DefaultConnection".
    
    The 'EnableRetryOnFailure()' method adds resiliency by automatically retrying 
    failed database operations (e.g., due to transient network issues).

    => check on lambda functions (Assignment)
**********************************************************************/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

/*********************************************************************
5. Modification of the default Authentication Service
************************************************************************/

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

        // Keep the same event handlers
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    ok = false,
                    message = "Unauthorized: Please provide a token"
                }));
            }
        };
    });

/*********************************************************************
5. Handle cors. This adds CORA (Cross-Orgin Resource Sharing)
************************************************************************/

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
    });

    /*
     Allow specific origin, the name of this policy is called Restricted. Uncomment the codes for restricted cors policy
     options.AddPolicy("Restricted", policy =>
     {
         policy.WithOrigins("https://yourfrontend.com", "https://admin.yoursite.com") // Trusted domains
               .AllowAnyHeader()
               .AllowAnyMethod();
     });
 */
});

/***************************
Build the application after all
the services are added.
******************************/
var app = builder.Build();

// if the app is running in the devlopment environment use swagger other dont
if (app.Environment.IsDevelopment())
{
    // Add development exception error handling to display detailed errors for debugging purpose
    app.UseDeveloperExceptionPage();

    // Enabale or use swagger setups
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // For production handle exceptions by redirecting to a custom error handling page
    app.UseExceptionHandler("/error");
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();


// enable routing
app.UseRouting();

// use or enable our set cors
app.UseCors();

// enable authentication middleware
app.UseAuthentication();

// enable authorization
app.UseAuthorization();

// map the atrribute-routed controllers to the app
app.MapControllers();

app.Run();