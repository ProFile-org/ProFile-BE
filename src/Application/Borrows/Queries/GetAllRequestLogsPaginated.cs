using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Users.Queries;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllRequestLogsPaginated
{
    public record Query : IRequest<PaginatedList<LogDto>>
    {
        public Guid? BorrowRequestId { get; set; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<LogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<LogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.Logs
                .Where(x => x.ObjectType!.Equals("BorrowRequest"))
                .AsQueryable();

            if (request.BorrowRequestId is not null)
            {
                logs = logs.Where(x => x.ObjectId! == request.BorrowRequestId);
            }

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x =>
                    x.Message!.ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }

            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 10 : request.Size;

            var count = await logs.CountAsync(cancellationToken);
            var list  = await logs
                .OrderByDescending(x => x.Time!)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);

            var result = list.Select(x => new LogDto()
                {
                    Id = x.Id,
                    Event = x.Event,
                    Template = x.Template,
                    Message = x.Message,
                    Level = x.Level,
                    ObjectType = x.ObjectType,
                    User = _mapper.Map<UserDto>(_context.Users.FirstOrDefault(y => y.Id == x.UserId!)),
                    ObjectId = x.ObjectId!,
                    Time = x.Time?.ToDateTimeUtc(),
                })
                .ToList();

                return new PaginatedList<LogDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}
