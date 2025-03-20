using Microsoft.OpenApi.Models;
using BusinessLayer.Interface;
using NLog;
using NLog.Web;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Services;
using BusinessLayer.Service;
using Middleware.JwtHelper;
using RepositoryLayer.Service;
using StackExchange.Redis;
using Middleware.RabbitMQClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("Starting application...");

var builder = WebApplication.CreateBuilder(args);

// ✅ Setup NLog for logging
builder.Logging.ClearProviders();
builder.Host.UseNLog();


// ✅ Retrieve database connection string
var connectionString = builder.Configuration.GetConnectionString("AddressAppDB");
logger.Info("Database connection string loaded successfully.");

// ✅ Redis Configuration with Fallback to In-Memory Cache
var redisConnection = builder.Configuration.GetSection("Redis:Connection").Value;
IConnectionMultiplexer? redisMultiplexer = null;
try
{
    redisMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
    builder.Services.AddSingleton(redisMultiplexer);

    // ✅ Register Redis as a distributed cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = builder.Configuration.GetSection("Redis:InstanceName").Value;
    });

    // User session
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // 30-minute session timeout
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    logger.Info("Redis connected successfully.");
}
catch (Exception ex)
{
    logger.Warn($"Redis connection failed: {ex.Message}. Continuing without Redis.");

    // ✅ Fallback: Use in-memory cache if Redis is unavailable
    builder.Services.AddDistributedMemoryCache();
    logger.Info("Using in-memory cache as a fallback.");
}


// ✅ Register services
builder.Services.AddControllers();

// ✅ Register DbContext
builder.Services.AddDbContext<AddressBookContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<Middleware.Email.SMTP>();

// ✅ Register Business & Repository Layer
builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();

// ✅ Register JWT Token Helper as a Singleton
builder.Services.AddSingleton<IJwtTokenHelper, JwtTokenHelper>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($" Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"🔹 Received Token: {token}");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine(" Token is missing in the request.");
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                Console.WriteLine("JWT Challenge: Unauthorized - Token missing or invalid.");

                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Unauthorized: Invalid or missing token",
                    data = (string)null
                });
            }
        };
    });


// ✅ Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        // Enable JWT Authentication in Swagger UI
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Enter 'Bearer YOUR_TOKEN' in the field below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement{
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
        }    });
    });

var app = builder.Build();

// ✅ Enable Swagger UI in Development
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Middleware Configuration
app.UseHttpsRedirection();

// ✅ Ensure proper Authentication & Authorization order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

logger.Info("Application started successfully.");
app.Run();