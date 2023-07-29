using Application.Common.Interfaces;
using Application.Common.Models.Dtos.DashBoard;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Dashboards.Queries;

public class GetLoggedInUser
{
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

        public async Task<MetricResultDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentDate = request.Date.Date;

            var label = $"{currentDate:dd-MM}";
            var loggedinUsersCount = await _context.Logs
                .Where(x => x.ObjectType!.Equals("Login")
                            && x.Time >= Instant.FromDateTimeUtc(currentDate)
                            && x.Time < Instant.FromDateTimeUtc(currentDate.AddDays(1)))
                .CountAsync(cancellationToken);

            return new MetricResultDto()
            {
                Label = label,
                Value = loggedinUsersCount
            };
        }
    }
}