using FluentValidation;

namespace Application.Folders.Commands.Disable;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(f => f.FolderId)
            .NotEmpty().WithMessage("FolderId is required.");
    }
}