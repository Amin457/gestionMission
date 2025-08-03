using FluentValidation;
using gestionMissionBack.Application.DTOs.MissionCost;
using Microsoft.AspNetCore.Http;

namespace gestionMissionBack.Application.Validators
{
    public class MissionCostCreateValidator : AbstractValidator<MissionCostCreateDto>
    {
        public MissionCostCreateValidator()
        {
            RuleFor(mc => mc.MissionId)
                .GreaterThan(0).WithMessage("Mission ID is required and must be greater than 0");

            RuleFor(mc => mc.Type)
                .IsInEnum().WithMessage("Type must be a valid MissionCostType enum value");

            RuleFor(mc => mc.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0");

            RuleFor(mc => mc.Date)
                .NotEmpty().WithMessage("Date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Date cannot be in the future");

            RuleFor(mc => mc.ReceiptPhotos)
                .Must(photos => photos == null || photos.Count <= 10)
                .WithMessage("Maximum 10 receipt photos allowed");

            When(mc => mc.ReceiptPhotos != null, () =>
            {
                RuleForEach(mc => mc.ReceiptPhotos)
                    .Must(photo => photo != null && photo.Length > 0)
                    .WithMessage("Photo file cannot be empty");

                RuleForEach(mc => mc.ReceiptPhotos)
                    .Must(photo => photo != null && photo.Length <= 10 * 1024 * 1024) // 10MB
                    .WithMessage("Photo file size must be less than 10MB");

                RuleForEach(mc => mc.ReceiptPhotos)
                    .Must(photo => photo != null && IsValidImageType(photo))
                    .WithMessage("Photo must be a valid image file (JPEG, PNG, GIF)");
            });
        }

        private bool IsValidImageType(IFormFile file)
        {
            if (file == null) return false;
            
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }
    }
} 