using FluentValidation;

namespace Application.Lockers.Commands.DisableLocker;

public class DisableLockerCommandValidator : AbstractValidator<DisableLockerCommand>
{
    public DisableLockerCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.LockerId)
            .NotEmpty().WithMessage("Locker Id is required.");
    }
}