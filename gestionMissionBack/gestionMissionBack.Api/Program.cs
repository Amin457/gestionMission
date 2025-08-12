using gestionMissionBack.Application.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Api.Middleware;
using FluentValidation.AspNetCore;
using gestionMissionBack.Application;
using gestionMissionBack.Infrastructure;
using Microsoft.OpenApi.Models;
using gestionMissionBack.Api.StartupExtensions;
using System.Net.Http.Headers;
using gestionMissionBack.Api.Hubs;
using gestionMissionBack.Api.Services;
using gestionMissionBack.Application.Interfaces;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200","http://localhost:3000", "http://192.168.77.52:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(ProfileMapping));

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Swagger Configuration
builder.Services.AddSwaggerConfiguration();

// Add Authorization Policies
builder.Services.AddAuthorizationPolicies();

// Add Fluent Validation Services   
builder.Services.AddFluentValidationServices();

// Add SignalR
builder.Services.AddSignalR();

// Configure SignalR to use JWT authentication
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Register SignalR Service
builder.Services.AddScoped<ISignalRService, SignalRService>();

// Register Metrics Service
builder.Services.AddScoped<IMetricsService, MetricsService>();

// Add Prometheus Metrics
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("OpenRouteService", client =>
{
    client.BaseAddress = new Uri("https://api.openrouteservice.org/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAngularApp");

// Add Prometheus Metrics Middleware
app.UseMetricServer();
app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// Map Prometheus Metrics Endpoint
app.MapMetrics();

// Map SignalR hub with the path that frontend expects
app.MapHub<NotificationHub>("/api/notificationHub");

app.Run();
