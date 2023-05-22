using FluentValidation;

namespace Application.Documents.Queries.GetDocumentsByTitle;

public class GetDocumentsByTitleQueryValidator : AbstractValidator<GetDocumentsByTitleQuery>
{
    public GetDocumentsByTitleQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required");
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number at least greater than or equal to 1");
        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(1).WithMessage("Page size at least greater than or equal to 1");
    }
}