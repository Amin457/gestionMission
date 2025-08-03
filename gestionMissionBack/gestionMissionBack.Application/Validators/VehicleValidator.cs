using FluentValidation;
using gestionMissionBack.Application.DTOs.Vehicle;

namespace gestionMissionBack.Application.Validators
{
    public class VehicleValidator : AbstractValidator<VehicleDto>
    {
        public VehicleValidator()
        {
            RuleFor(v => v.Type)
                .IsInEnum().WithMessage("Invalid vehicle type");

            RuleFor(v => v.LicensePlate)
                .NotEmpty().WithMessage("License plate is required")
                .MaximumLength(20).WithMessage("License plate cannot exceed 20 characters");

            RuleFor(v => v.MaxCapacity)
                .GreaterThan(0).WithMessage("Max capacity must be greater than 0");
        }
    }
}