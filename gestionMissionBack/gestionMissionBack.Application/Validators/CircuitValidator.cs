using FluentValidation;
using gestionMissionBack.Application.DTOs.Circuit;

namespace gestionMissionBack.Application.Validators
{
    public class CircuitValidator : AbstractValidator<CircuitDto>
    {
        public CircuitValidator()
        {
            RuleFor(c => c.MissionId)
                .NotEmpty()
                .WithMessage("Mission ID is required");

            RuleFor(c => c.DepartureDate)
                .NotEmpty()
                .WithMessage("Departure date is required");

            RuleFor(c => c.DepartureSiteId)
                .NotEmpty()
                .WithMessage("Departure site is required");

            RuleFor(c => c.ArrivalSiteId)
                .NotEmpty()
                .WithMessage("Arrival site is required")
                .Must((circuit, arrivalSiteId) => arrivalSiteId != circuit.DepartureSiteId)
                .WithMessage("Arrival site must be different from departure site");
        }
    }
}