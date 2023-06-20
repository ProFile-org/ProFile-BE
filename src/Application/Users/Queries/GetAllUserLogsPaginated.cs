using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetAllUserLogsPaginated
{
    public record Query : IRequest<PaginatedList<UserLogDto>>
    {
        public Guid? UserId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<UserLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.UserLogs
                .Include(x => x.ObjectId)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .AsQueryable();

            if (request.UserId is not null)
            {
                logs = logs.Where(x => x.ObjectId! == request.UserId);
            }
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x => 
                    x.Action.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }
            
            return await logs
                .LoggingListPaginateAsync<User, UserLog, UserLogDto>(
                    request.Page,
                    request.Size,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}