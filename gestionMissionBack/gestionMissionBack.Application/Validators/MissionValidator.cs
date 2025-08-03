using FluentValidation;
using gestionMissionBack.Application.DTOs.Mission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Validators
{

    public class MissionValidator : AbstractValidator<MissionDto>
    {
        public MissionValidator()
        {
            RuleFor(m => m.Type)
                .IsInEnum().WithMessage("Invalid mission type");

            RuleFor(m => m.Status)
                .IsInEnum().WithMessage("Invalid mission status");

            //RuleFor(m => m.Quantity)
            //    .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(m => m.Service)
                .NotEmpty().WithMessage("Service is required.")
                .MaximumLength(50).WithMessage("Service must not exceed 50 characters.");

            //RuleFor(m => m.SourceId)
            //    .GreaterThan(0).WithMessage("Source is required.");

            //RuleFor(m => m.DestinationId)
            //    .GreaterThan(0).WithMessage("Destination is required.");


            RuleFor(m => m.Receiver)
                .NotEmpty().WithMessage("Receiver is required.")
                .MaximumLength(50).WithMessage("Receiver must not exceed 50 characters.");

            RuleFor(m => m.SystemDate)
                .NotEmpty().When(m => m.SystemDate != null).WithMessage("SystemDate is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("SystemDate cannot be in the future.");

            RuleFor(m => m.DesiredDate)
                .GreaterThanOrEqualTo(DateTime.Today).When(m => m.DesiredDate.HasValue)
                .WithMessage("DesiredDate cannot be in the past.");
        }

    }
}