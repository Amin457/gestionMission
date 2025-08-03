using FluentValidation;
using gestionMissionBack.Application.DTOs.Article;

namespace gestionMissionBack.Application.Validators
{
    public class ArticleUpdateValidator : AbstractValidator<ArticleUpdateDto>
    {
        public ArticleUpdateValidator()
        {
            RuleFor(a => a.ArticleId)
                .GreaterThan(0).WithMessage("Article ID must be greater than 0");

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

            RuleFor(a => a.NewPhotos)
                .Must(photos => photos == null || photos.Count <= 10)
                .WithMessage("Maximum 10 new photos allowed");
        }
    }
} 