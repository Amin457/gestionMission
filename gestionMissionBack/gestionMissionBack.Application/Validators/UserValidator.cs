using FluentValidation;
using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace gestionMissionBack.Application.Validators
{
    public class UserValidator : AbstractValidator<UserDto>
    {
        private readonly HashSet<string> _validRoleNames;

        public UserValidator(IRoleRepository roleRepository)
        {
            _validRoleNames = new HashSet<string>(
                roleRepository.GetAllAsync().Result.Select(r => r.Name)
            );

            RuleFor(user => user.FirstName)
                .NotEmpty().WithMessage("The first name is required.");

            RuleFor(user => user.LastName)
                .NotEmpty().WithMessage("The last name is required.");

            RuleFor(s => s.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("The email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(user => user.PasswordHash)
                .NotEmpty().WithMessage("The password is required.")
                .MinimumLength(6).WithMessage("The password must be at least 6 characters long.");

            RuleFor(user => user.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(roleName => _validRoleNames.Contains(roleName))
                .WithMessage("Role does not exist.");

            When(u => u.CurrentDriverStatus.HasValue, () =>
            {
                RuleFor(u => u.CurrentDriverStatus)
                    .IsInEnum().WithMessage("Invalid driver status.");
            });
        }
    }
}