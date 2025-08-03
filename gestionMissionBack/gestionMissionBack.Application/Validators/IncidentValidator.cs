using FluentValidation;
using gestionMissionBack.Application.DTOs.Incident;

namespace gestionMissionBack.Application.Validators
{
    public class IncidentValidator : AbstractValidator<IncidentDto>
    {
        private static readonly string[] ValidTypes = { "Delay", "Breakdown", "LogisticsIssue" };
        private static readonly string[] ValidStatuses = { "Reported", "Resolved", "InProgress" };

        public IncidentValidator()
        {
            RuleFor(i => i.MissionId)
                .GreaterThan(0).WithMessage("Mission ID is required and must be greater than 0");

            RuleFor(i => i.Type)
                .IsInEnum().WithMessage("Type must be a valid IncidentType enum value");

            RuleFor(i => i.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(i => i.ReportDate)
                .NotEmpty().WithMessage("Report date is required");

            RuleFor(i => i.Status)
                .IsInEnum().WithMessage("Status must be a valid IncidentStatus enum value");
        }
    }
}