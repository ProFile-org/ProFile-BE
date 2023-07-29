using Application.Common.Interfaces;
using Application.Common.Models.Dtos.DashBoard;
using FluentValidation;
using MediatR;

namespace Application.Dashboards.Queries;

public class GetTheTopDriveUser
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date can not be empty.");
        }
    }
    public record Query : IRequest<MetricResultDto>
    {
        public DateTime Date { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, MetricResultDto>
    {
        private readonly IApplicationDbContext _context;

        public QueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<MetricResultDto> Handle(Query request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}