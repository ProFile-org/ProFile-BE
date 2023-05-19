using FluentValidation;

namespace Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username cannot exceed 64 characters.");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Require valid email.")
            .MaximumLength(320).WithMessage("Email length too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Your password cannot be empty");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role cannot be empty.")
            .MaximumLength(64).WithMessage("Role cannot exceed 64 characters.")
            .Must(BeNotAdmin).WithMessage("Cannot add a user as Administrator.");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position cannot be empty.")
            .MaximumLength(64).WithMessage("Position cannot exceed 64 characters.");
    }

    private static bool BeNotAdmin(string role)
    {
        return !role.Equals("Administrator");
    }
}