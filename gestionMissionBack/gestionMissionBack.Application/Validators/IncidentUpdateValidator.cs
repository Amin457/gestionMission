using FluentValidation;
using gestionMissionBack.Application.DTOs.Incident;
using Microsoft.AspNetCore.Http;

namespace gestionMissionBack.Application.Validators
{
    public class IncidentUpdateValidator : AbstractValidator<IncidentUpdateDto>
    {
        public IncidentUpdateValidator()
        {
            RuleFor(i => i.MissionId)
                .GreaterThan(0).WithMessage("Mission ID is required and must be greater than 0");

            RuleFor(i => i.Type)
                .IsInEnum().WithMessage("Type must be a valid IncidentType enum value");

            RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(i => i.ReportDate)
                .NotEmpty().WithMessage("Report date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Report date cannot be in the future");

            RuleFor(i => i.Status)
                .IsInEnum().WithMessage("Status must be a valid IncidentStatus enum value");

            RuleFor(i => i.IncidentDocs)
                .Must(docs => docs == null || docs.Count <= 10)
                .WithMessage("Maximum 10 incident documents allowed");

            When(i => i.IncidentDocs != null, () =>
            {
                RuleForEach(i => i.IncidentDocs)
                    .Must(doc => doc != null && doc.Length > 0)
                    .WithMessage("Document file cannot be empty");

                RuleForEach(i => i.IncidentDocs)
                    .Must(doc => doc != null && doc.Length <= 10 * 1024 * 1024) // 10MB
                    .WithMessage("Document file size must be less than 10MB");

                RuleForEach(i => i.IncidentDocs)
                    .Must(doc => doc != null && IsValidDocumentType(doc))
                    .WithMessage("Document must be a valid file type (JPEG, PNG, GIF, PDF, DOC, DOCX)");
            });
        }

        private bool IsValidDocumentType(IFormFile file)
        {
            if (file == null) return false;
            
            var allowedTypes = new[] { 
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }
    }
} 