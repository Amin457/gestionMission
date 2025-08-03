using FluentValidation;
using gestionMissionBack.Application.DTOs.Site;

namespace gestionMissionBack.Application.Validators
{
    public class SiteValidator : AbstractValidator<SiteDto>
    {
        public SiteValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Site name is required.")
                .MaximumLength(100).WithMessage("Site name must not exceed 100 characters.");

            RuleFor(s => s.Type)
                .NotEmpty().WithMessage("Site type is required.")
                .MaximumLength(50).WithMessage("Site type must not exceed 50 characters.");

            RuleFor(s => s.Address)
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

            RuleFor(s => s.Phone)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

            RuleFor(s => s.CityId)
                .NotEmpty().WithMessage("City ID is required.");

            RuleFor(s => s.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(s => s.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");
        }
    }
}
