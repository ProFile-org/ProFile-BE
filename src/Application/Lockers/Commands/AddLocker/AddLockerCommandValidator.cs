using Application.Common.Interfaces;
using FluentValidation;

namespace Application.Lockers.Commands.AddLocker;

public class AddLockerCommandValidator : AbstractValidator<AddLockerCommand>
{
    private readonly IApplicationDbContext _context;

    public AddLockerCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Locker's capacity cannot be less than 1");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Locker's name is required.")
            .MaximumLength(64).WithMessage("Locker's name cannot exceed 64 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(256).WithMessage("Locker's description cannot exceed 256 characters.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");
    }
}
