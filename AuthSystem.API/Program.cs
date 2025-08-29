using AuthSystem.API.Middleware;
using AuthSystem.Application.Interfaces;
using AuthSystem.Application.Services;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Services;
using AuthSystem.Infrastructure.Data;
using AuthSystem.Infrastructure.Repositories;
using AuthSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthSystem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});


// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Domain Services
builder.Services.AddScoped<UserDomainService>();

// Infrastructure Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Application Services
builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{

    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthSystem API V1");
    c.RoutePrefix = ""; // this makes Swagger load at root "/"
});


app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();




//using AuthSystem.API.Middleware;
//using AuthSystem.Application.Interfaces;
//using AuthSystem.Application.Services;
//using AuthSystem.Domain.Interfaces.Repositories;
//using AuthSystem.Domain.Interfaces.Services;
//using AuthSystem.Domain.Services;
//using AuthSystem.Infrastructure.Data;
//using AuthSystem.Infrastructure.Repositories;
//using AuthSystem.Infrastructure.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//// Configure Swagger with JWT support
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthSystem API", Version = "v1" });

//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme.",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});

//// ------------------ DATABASE ------------------
//// Use MySQL instead of SQL Server
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//                       ?? Environment.GetEnvironmentVariable("DefaultConnection")
//                       ?? throw new InvalidOperationException("Database connection string not configured");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
//);

//// ------------------ REPOSITORIES & SERVICES ------------------
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<UserDomainService>();
//builder.Services.AddScoped<IPasswordService, PasswordService>();
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();

//// ------------------ JWT Authentication ------------------
//var jwtSecret = builder.Configuration["Jwt:Secret"]
//                ?? Environment.GetEnvironmentVariable("Jwt__Secret")
//                ?? throw new InvalidOperationException("JWT secret not configured");
//var key = Encoding.ASCII.GetBytes(jwtSecret);

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false;
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(key),
//        ValidateIssuer = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidateAudience = true,
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        ValidateLifetime = true,
//        ClockSkew = TimeSpan.Zero
//    };
//});

//builder.Services.AddAuthorization();

//// ------------------ CORS ------------------
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//var app = builder.Build();

//// ------------------ HTTP PIPELINE ------------------
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthSystem API V1");
//    c.RoutePrefix = "";
//});

//app.UseHttpsRedirection();
//app.UseCors("AllowAll");
//app.UseMiddleware<ExceptionMiddleware>();
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//// ------------------ ENSURE DATABASE ------------------
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    await context.Database.EnsureCreatedAsync();
//}

//app.Run();
