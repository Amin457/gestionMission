using FluentValidation;
using gestionMissionBack.Application.DTOs.User;
using gestionMissionBack.Infrastructure.Interfaces;

namespace gestionMissionBack.Application.Validators
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
    {
        private readonly HashSet<string> _validRoleNames;

        public UserUpdateValidator(IRoleRepository roleRepository)
        {
            _validRoleNames = new HashSet<string>(
                roleRepository.GetAllAsync().Result.Select(r => r.Name)
            );

            RuleFor(user => user.UserId)
                .GreaterThan(0).WithMessage("User ID is required.");

            RuleFor(user => user.FirstName)
                .NotEmpty().When(user => user.FirstName != null).WithMessage("The first name is required if provided.");

            RuleFor(user => user.LastName)
                .NotEmpty().When(user => user.LastName != null).WithMessage("The last name is required if provided.");

            RuleFor(user => user.Email)
                .NotEmpty().When(user => user.Email != null).WithMessage("The email is required if provided.")
                .EmailAddress().When(user => user.Email != null).WithMessage("Invalid email format.");

            RuleFor(user => user.Password)
                .MinimumLength(6).When(user => user.Password != null).WithMessage("The password must be at least 6 characters long if provided.");

            RuleFor(user => user.Role)
                    .NotEmpty().WithMessage("Role is required.")
                    .Must(roleName => _validRoleNames.Contains(roleName))
                    .When(user => user.Role != null)
                    .WithMessage("Role does not exist.");
        }
    }
}