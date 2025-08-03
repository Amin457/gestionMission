using FluentValidation;
using gestionMissionBack.Application.DTOs.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Validators
{
    public class RoleValidator : AbstractValidator<RoleDto>
    {
        public RoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The role name is required.")
                .MaximumLength(15).WithMessage("The role name cannot exceed 15 characters.\r\n"); 
            
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("The role code is required.")
                .MaximumLength(10).WithMessage("The role code cannot exceed 15 characters.\r\n");  
            
            RuleFor(x => x.Libelle)
                .NotEmpty().WithMessage("The role libelle is required.")
                .MaximumLength(15).WithMessage("The role libelle cannot exceed 15 characters.\r\n");
        }
    }
}
