using FluentValidation;
using gestionMissionBack.Application.DTOs.Article;

namespace gestionMissionBack.Application.Validators
{
    public class ArticleValidator : AbstractValidator<ArticleDto>
    {
        public ArticleValidator()
        {

            RuleFor(a => a.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(a => a.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(a => a.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be zero or positive");

            RuleFor(a => a.Weight)
                .GreaterThanOrEqualTo(0).WithMessage("Weight must be zero or positive");

            RuleFor(a => a.Volume)
                .GreaterThanOrEqualTo(0).WithMessage("Volume must be zero or positive");
        }
    }
}