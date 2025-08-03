using Microsoft.AspNetCore.Authorization;
using gestionMissionBack.Domain.Entities;

namespace gestionMissionBack.Api.StartupExtensions
{
    public static class AuthorizationExtensions
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ChauffeurOnly", policy => policy.RequireRole("Chauffeur"));
            });
        }
    }
}
