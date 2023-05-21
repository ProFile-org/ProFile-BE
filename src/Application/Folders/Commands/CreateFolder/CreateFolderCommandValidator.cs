using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Commands.CreateFolder;

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(f => f.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.");

        RuleFor(f => f.Description)
            .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");

        RuleFor(f => f.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Folder's capacity cannot be less than 1");

        RuleFor(f => f.LockerId)
            .NotEmpty().WithMessage("LockerId is required.");
    }
    
}