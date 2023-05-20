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
            .GreaterThan(0).WithMessage("Locker's capacity must be greater than 0");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Locker's name is required.")
            .MaximumLength(64).WithMessage("Locker's name cannot exceed 64 characters.")
            .Must(BeUnique).WithMessage("Locker's name already exists.");

        RuleFor(x => x.Description)
            .MaximumLength(256).WithMessage("Locker's description must not exceed 256 characters.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required.")
            .Must(BeValid).WithMessage("Invalid Room ID format");
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