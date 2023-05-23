using FluentValidation;

namespace Application.Folders.Commands.AddFolder;

public class AddFolderCommandValidator : AbstractValidator<AddFolderCommand>
{
    public AddFolderCommandValidator()
    {

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(f => f.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.");

        RuleFor(f => f.Description)
            .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");

        RuleFor(f => f.Capacity)
            .NotEmpty().WithMessage("Folder is required.")
            .GreaterThanOrEqualTo(1).WithMessage("Folder's capacity cannot be less than 1.");

        RuleFor(f => f.LockerId)
            .NotEmpty().WithMessage("LockerId is required.");
    }
    
}