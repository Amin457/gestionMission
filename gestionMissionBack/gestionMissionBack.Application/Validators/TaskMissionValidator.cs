using FluentValidation;
using gestionMissionBack.Application.DTOs.TaskMission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Validators
{
    public class TaskMissionValidator : AbstractValidator<TaskMissionDto>
    {
        public TaskMissionValidator()
        {

            RuleFor(t => t.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            //RuleFor(t => t.AssignmentDate)
            //    .NotEmpty().WithMessage("Assignment date is required")
            //    .LessThanOrEqualTo(DateTime.Now).WithMessage("Assignment date cannot be in the future");

            //RuleFor(t => t.CompletionDate)
            //    .GreaterThanOrEqualTo(DateTime.Now.Date)
            //    .When(t => t.CompletionDate.HasValue)
            //    .WithMessage("Completion date must be on or after today");

            RuleFor(t => t.Status)
                 .IsInEnum().WithMessage("Status must be a valid value");

            RuleFor(t => t.MissionId)
                .GreaterThan(0)
                .WithMessage("Mission ID must be greater than 0")
                .NotEmpty()
                .WithMessage("Mission ID is required");  
            
            RuleFor(t => t.SiteId)
                .GreaterThan(0)
                .WithMessage("Site ID must be greater than 0")
                .NotEmpty()
                .WithMessage("Site ID is required");

            RuleFor(t => t.IsFirstTask)
                .NotNull()
                .WithMessage("IsFirstTask must be specified");
        }

    }
}