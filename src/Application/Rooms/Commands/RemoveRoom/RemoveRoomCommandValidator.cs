using FluentValidation;

namespace Application.Rooms.Commands.RemoveRoom;

public class RemoveRoomCommandValidator : AbstractValidator<RemoveRoomCommand>
{
    public RemoveRoomCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId can not be empty");
    }   
}