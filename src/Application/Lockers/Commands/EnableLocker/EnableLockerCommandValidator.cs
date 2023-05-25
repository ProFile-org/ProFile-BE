using FluentValidation;

namespace Application.Lockers.Commands.EnableLocker;

public class EnableLockerCommandValidator : AbstractValidator<EnableLockerCommand>
{
    public EnableLockerCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.LockerId)
            .NotEmpty().WithMessage("Locker Id is required.");
    }
}