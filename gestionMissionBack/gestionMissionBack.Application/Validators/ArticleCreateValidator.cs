using FluentValidation;
using gestionMissionBack.Application.DTOs.Article;

namespace gestionMissionBack.Application.Validators
{
    public class ArticleCreateValidator : AbstractValidator<ArticleCreateDto>
    {
        public ArticleCreateValidator()
        {
            RuleFor(a => a.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

            RuleFor(a => a.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(a => a.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");

            RuleFor(a => a.Weight)
                .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater than or equal to 0");

            RuleFor(a => a.Volume)
                .GreaterThanOrEqualTo(0).WithMessage("Volume must be greater than or equal to 0");

            RuleFor(a => a.Photos)
                .Must(photos => photos == null || photos.Count <= 10)
                .WithMessage("Maximum 10 photos allowed");
        }
    }
} 