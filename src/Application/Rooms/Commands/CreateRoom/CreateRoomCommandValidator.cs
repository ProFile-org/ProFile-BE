using Application.Common.Interfaces;
using FluentValidation;

namespace Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    private readonly IApplicationDbContext _context;
    public CreateRoomCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Room's capacity cannot be less than 1");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.")
            .Must(BeUnique).WithMessage("Room name already exists.");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");
    }

    private bool BeUnique(string name)
    {
        return _context.Rooms.FirstOrDefault(x => x.Name.Equals(name)) is null;
    }
}