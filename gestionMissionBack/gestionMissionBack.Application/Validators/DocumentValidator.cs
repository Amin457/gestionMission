using FluentValidation;
using gestionMissionBack.Application.DTOs.Document;

namespace gestionMissionBack.Application.Validators

{
    public class DocumentValidator : AbstractValidator<DocumentDto>
    {
        private static readonly string[] ValidTypes = { "Image", "Pdf", "Other" };
        public DocumentValidator()
        {

            RuleFor(d => d.TaskId)
                .GreaterThan(0).WithMessage("Task ID is required and must be greater than 0");

            RuleFor(d => d.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(d => d.Type)
                .IsInEnum().WithMessage("Type must be a valid DocumentType enum value");

            RuleFor(d => d.StoragePath)
                .NotEmpty().WithMessage("Storage path is required")
                .MaximumLength(500).WithMessage("Storage path cannot exceed 500 characters");

            RuleFor(d => d.AddedDate)
                .NotEmpty().WithMessage("Added date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Added date cannot be in the future");
        }
    }

    public class DocumentCreateValidator : AbstractValidator<DocumentCreateDto>
    {
        public DocumentCreateValidator()
        {
            RuleFor(d => d.TaskId)
                .GreaterThan(0).WithMessage("Task ID is required and must be greater than 0");

            RuleFor(d => d.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(d => d.Type)
                .IsInEnum().WithMessage("Type is required");

            RuleFor(d => d.File)
                .NotNull().WithMessage("File is required");
        }
    }

    public class DocumentUpdateValidator : AbstractValidator<DocumentDto>
    {
        public DocumentUpdateValidator()
        {
            RuleFor(d => d.TaskId)
                .GreaterThan(0).WithMessage("Task ID is required and must be greater than 0");

            RuleFor(d => d.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(d => d.Type)
                .NotEmpty().WithMessage("Type is required");

            RuleFor(d => d)
                .Must(d => !string.IsNullOrEmpty(d.KeptFile) || d.NewFile != null)
                .WithMessage("Either KeptFile or NewFile must be provided");
        }
    }
}