using Microsoft.Extensions.DependencyInjection;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using FluentValidation;
using System.Reflection;

namespace gestionMissionBack.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register Application Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService,RoleService>();
            services.AddScoped<IMissionService, MissionService>();
            services.AddScoped<ITaskMissionService, TaskMissionService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IIncidentService, IncidentService>();
            services.AddScoped<IMissionCostService, MissionCostService>();
            services.AddScoped<ICircuitService, CircuitService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleReservationService, VehicleReservationService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, NotificationService>();
            // Register FluentValidation Validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
