using FluentValidation;

namespace Application.Lockers.Commands.Disable;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(x => x.LockerId)
            .NotEmpty().WithMessage("LockerId is required.");
    }
}