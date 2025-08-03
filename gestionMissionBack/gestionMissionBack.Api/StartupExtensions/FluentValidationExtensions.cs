using FluentValidation.AspNetCore;

namespace gestionMissionBack.Api.StartupExtensions
{
    public static class FluentValidationExtensions
    {
        public static void AddFluentValidationServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddFluentValidation();
        }
    }
}
