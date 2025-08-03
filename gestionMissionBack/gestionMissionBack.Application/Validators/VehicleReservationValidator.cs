using FluentValidation;
using gestionMissionBack.Application.DTOs.VehicleReservation;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.Validators
{
    public class VehicleReservationValidator : AbstractValidator<VehicleReservationDto>
    {
        public VehicleReservationValidator()
        {
            RuleFor(vr => vr.RequesterId)
                .GreaterThan(0).WithMessage("Requester ID must be greater than 0");

            RuleFor(vr => vr.VehicleId)
                .GreaterThan(0).WithMessage("Vehicle ID must be greater than 0");

            RuleFor(vr => vr.Departure)
                .NotEmpty().WithMessage("Departure is required")
                .MaximumLength(100).WithMessage("Departure cannot exceed 100 characters");

            RuleFor(vr => vr.Destination)
                .NotEmpty().WithMessage("Destination is required")
                .MaximumLength(100).WithMessage("Destination cannot exceed 100 characters")
                .Must((vr, destination) => destination != vr.Departure)
                .WithMessage("Destination must be different from Departure");

            RuleFor(vr => vr.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .LessThanOrEqualTo(vr => vr.EndDate).WithMessage("Start date must be before or equal to end date");

            RuleFor(vr => vr.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(vr => vr.StartDate).WithMessage("End date must be after or equal to start date");

            RuleFor(vr => vr.Status)
                .IsInEnum().WithMessage("Status must be a valid VehicleReservationStatus value");
        }
    }
}