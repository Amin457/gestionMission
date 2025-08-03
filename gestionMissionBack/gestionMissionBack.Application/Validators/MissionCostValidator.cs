using FluentValidation;
using gestionMissionBack.Application.DTOs.MissionCost;

namespace gestionMissionBack.Application.Validators
{
    public class MissionCostValidator : AbstractValidator<MissionCostDto>
    {
        private static readonly string[] ValidTypes = { "Fuel", "Toll", "Maintenance" };

        public MissionCostValidator()
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
        }
    }
}