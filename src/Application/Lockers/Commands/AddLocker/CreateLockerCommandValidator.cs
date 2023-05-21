using Application.Common.Interfaces;
using FluentValidation;

namespace Application.Lockers.Commands.AddLocker;

public class CreateLockerCommandValidator : AbstractValidator<CreateLockerCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateLockerCommandValidator(IApplicationDbContext context)
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
    }

    private bool BeUnique(string name)
    {
        return _context.Lockers.FirstOrDefault(x => x.Name.Equals(name)) is null;
    }
    
    private bool BeValid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}