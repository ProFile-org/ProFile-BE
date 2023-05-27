using FluentValidation;

namespace Application.Documents.Queries.GetAllPaginated;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.RoomId)
            .Must((query, roomId) =>
            {
                if (roomId is null)
                {
                    return query.LockerId is null && query.FolderId is null;
                }

                if (query.LockerId is null)
                {
                    return query.FolderId is null;
                }

                return true;
            }).WithMessage("Container orientation is not consistent");
    }
}