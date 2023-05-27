using Application.Identity;
using FluentValidation;

namespace Application.Users.Commands.Add;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username cannot exceed 64 characters.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Require valid email.")
            .MaximumLength(320).WithMessage("Email length too long.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .MaximumLength(64).WithMessage("Role cannot exceed 64 characters.")
            .Must(BeNotAdmin).WithMessage("Cannot add a user as Administrator.");
        
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Position)
            .MaximumLength(64).WithMessage("Position cannot exceed 64 characters.");
    }

    private static bool BeNotAdmin(string role)
    {
        return !role.Equals(IdentityData.Roles.Admin);
    }
}