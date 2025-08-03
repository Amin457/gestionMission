using FluentValidation;
using gestionMissionBack.Application.DTOs.Vehicle;

namespace gestionMissionBack.Application.Validators
{
    public class VehicleCreateValidator : AbstractValidator<VehicleCreateDto>
    {
        public VehicleCreateValidator()
        {
            RuleFor(v => v.Type)
                .IsInEnum().WithMessage("Invalid vehicle type");

            RuleFor(v => v.LicensePlate)
                .NotEmpty().WithMessage("License plate is required")
                .MaximumLength(50).WithMessage("License plate cannot exceed 50 characters");

            RuleFor(v => v.MaxCapacity)
                .GreaterThan(0).WithMessage("Max capacity must be greater than 0");

            RuleFor(v => v.Photos)
                .Must(photos => photos == null || photos.Count <= 10)
                .WithMessage("Maximum 10 photos allowed");
        }
    }
} 