using Application.Common.Interfaces;
using Application.Common.Models.Dtos.DashBoard;
using Domain.Statuses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Dashboards.Queries;

public class GetImportDocuments
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date can not be empty.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date can not be empty.");
        }
    }
    public record Query : IRequest<List<MetricResultDto>>
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

    }

    public class QueryHandler : IRequestHandler<Query, List<MetricResultDto>>
    {
        private readonly IApplicationDbContext _context;

        public QueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MetricResultDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = new List<MetricResultDto>();

            var currentDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;

            while (currentDate <= endDate)
            {
                var nextDate = currentDate.AddDays(1);
                var label = $"{currentDate:dd-MM} to {nextDate:dd-MM}";
                if (nextDate <= endDate)
                {
                    var date = currentDate;
                    var importedDocumentsCount = await _context.Documents
                        .Where(x => x.Created >= LocalDateTime.FromDateTime(date)
                                    && x.Created <= LocalDateTime.FromDateTime(nextDate)
                                    && x.Status != DocumentStatus.Issued)
                        .CountAsync(cancellationToken);
                
                    result.Add(new MetricResultDto()
                    {
                        Label = label,
                        Value = importedDocumentsCount
                    });
                }
                currentDate = nextDate;
            }

            return result;
        }
    }
}