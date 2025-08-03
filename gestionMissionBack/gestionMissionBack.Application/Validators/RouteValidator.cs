using FluentValidation;
using gestionMissionBack.Application.DTOs.Route;

namespace gestionMissionBack.Application.Validators
{
    public class RouteValidator : AbstractValidator<RouteDto>
    {
        public RouteValidator()
        {
            RuleFor(r => r.CircuitId)
                .NotEmpty()
                .WithMessage("Circuit ID is required");

            RuleFor(r => r.DepartureSiteId)
                .NotEmpty()
                .WithMessage("Departure site is required");

            RuleFor(r => r.ArrivalSiteId)
                .NotEmpty()
                .WithMessage("Arrival site is required");

            RuleFor(r => r.DistanceKm)
                .GreaterThanOrEqualTo(0)
                .When(r => r.DistanceKm.HasValue)
                .WithMessage("Distance must be greater than or equal to 0");
        }
    }
}