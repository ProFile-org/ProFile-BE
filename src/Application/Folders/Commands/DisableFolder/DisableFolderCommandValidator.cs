using FluentValidation;

namespace Application.Folders.Commands.DisableFolder;

public class DisableFolderCommandValidator : AbstractValidator<DisableFolderCommand>
{
    public DisableFolderCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(f => f.FolderId)
            .NotEmpty().WithMessage("FolderId is required");
    }
}