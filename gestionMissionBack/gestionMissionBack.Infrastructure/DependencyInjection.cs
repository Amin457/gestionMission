using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using gestionMissionBack.Infrastructure.Persistence;
using gestionMissionBack.Infrastructure.Repositories;
using gestionMissionBack.Infrastructure.Interfaces;


namespace gestionMissionBack.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Database Context
            services.AddDbContext<MissionFleetContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMissionRepository, MissionRepository>();
            services.AddScoped<ITaskMissionRepository, TaskMissionRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<IMissionCostRepository, MissionCostRepository>();
            services.AddScoped<ICircuitRepository, CircuitRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleReservationRepository, VehicleReservationRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserConnectionRepository, UserConnectionRepository>();
            return services;
        }
    }
}
