using Application.Common.Interfaces;
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
    public class Result
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public record Query : IRequest<List<Result>>
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

    }

    public class QueryHandler : IRequestHandler<Query, List<Result>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public QueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = new List<Result>();

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
                
                    result.Add(new Result()
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