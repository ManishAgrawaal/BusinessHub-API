using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using MTS_API.Data;
using MTS_API.Models;
using MTS_API.Services;

using Resend;

namespace MTS_API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =========================================================
        // Controllers
        // =========================================================

        builder.Services.AddControllers();


        // =========================================================
        // Resend Email Service
        // =========================================================

        builder.Services
            .AddOptions<ResendClientOptions>()
            .Configure(options =>
            {
                options.ApiToken =
                    builder.Configuration["Resend:ApiKey"];
            });

        builder.Services.AddHttpClient<ResendClient>();

        builder.Services.AddTransient<IResend, ResendClient>();

        builder.Services.AddScoped<IEmailService, EmailService>();


        // =========================================================
        // CORS
        // =========================================================

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MtsReactPolicy", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "https://manishtechnologysolution.com",
                        "https://www.manishtechnologysolution.com"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });


        // =========================================================
        // OpenAPI / Swagger
        // =========================================================

        builder.Services.AddOpenApi();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT token"
                });

            options.AddSecurityRequirement(document =>
                new Microsoft.OpenApi.OpenApiSecurityRequirement
                {
                    [
                        new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                            "Bearer",
                            document)
                    ] = []
                });
        });


        // =========================================================
        // Database
        // =========================================================

        builder.Services.AddDbContext<MtsDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration
                    .GetConnectionString("MtsConnection")));


        // =========================================================
        // Application Services
        // =========================================================

        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        builder.Services.AddScoped<AuthService>();


        // =========================================================
        // JWT Authentication
        // =========================================================

        var jwtKey = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "JWT Key is not configured.");
        }

        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            builder.Configuration["Jwt:Issuer"],

                        ValidAudience =
                            builder.Configuration["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtKey))
                    };
            });


        // =========================================================
        // Build Application
        // =========================================================

        var app = builder.Build();


        // =========================================================
        // OpenAPI / Swagger
        // =========================================================

        app.MapOpenApi();

        app.UseSwagger();

        app.UseSwaggerUI();


        // =========================================================
        // HTTPS
        // =========================================================

        app.UseHttpsRedirection();


        // =========================================================
        // CORS
        // =========================================================

        app.UseCors("MtsReactPolicy");


        // =========================================================
        // Authentication & Authorization
        // =========================================================

        app.UseAuthentication();

        app.UseAuthorization();


        // =========================================================
        // Controllers
        // =========================================================

        app.MapControllers();


        // =========================================================
        // Run Application
        // =========================================================

        app.Run();
    }
}