using FluentValidation;

namespace Application.Rooms.Commands.Remove;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");
    }   
}