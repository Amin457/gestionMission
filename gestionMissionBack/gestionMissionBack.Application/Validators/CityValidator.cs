using FluentValidation;
using gestionMissionBack.Application.DTOs.City;

namespace gestionMissionBack.Application.Validators
{
    public class CityValidator : AbstractValidator<CityDto>
    {
        public CityValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("City name is required.")
                .MaximumLength(100).WithMessage("City name must not exceed 100 characters.");

            RuleFor(c => c.Region)
                .MaximumLength(100).WithMessage("Region must not exceed 100 characters.");

            RuleFor(c => c.PostalCode)
                .MaximumLength(10).WithMessage("Postal code must not exceed 10 characters.");
        }
    }
}
