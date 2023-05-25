using FluentValidation;

namespace Application.Rooms.Commands.DisableRoom;

public class DisableRoomCommandValidator : AbstractValidator<DisableRoomCommand>
{
    public DisableRoomCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");
    }
}