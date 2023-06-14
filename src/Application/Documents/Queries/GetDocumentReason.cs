using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentReason
{
    public record Query : IRequest<ReasonDto>
    {
        public User User { get; set; }
        public Guid DocumentId { get; init; }
        public RequestType Type { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, ReasonDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public QueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ReasonDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.RequestLogs
                .Include(x => x.Object)
                .ThenInclude(y => y.Department)
                .Include(x => x.Object)
                .ThenInclude(y => y.Importer)
                .FirstOrDefaultAsync(x => x.Object!.Id == request.DocumentId
                                          && x.Type == request.Type, cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Document does not have a request.");
            }

            await EnforceRoleConstraintsAsync(_currentUserService.GetCurrentUser(), log);
            
            return _mapper.Map<ReasonDto>(log);
        }

        private async Task EnforceRoleConstraintsAsync(User user, RequestLog log)
        {
            var role = user.Role;

            switch (role)
            {
                case IdentityData.Roles.Staff when log.Object!.Department!.Id != user.Department!.Id:
                    throw new ConflictException("Staff cannot access this request.");
                case IdentityData.Roles.Employee when log.Type == RequestType.Import:
                {
                    if (log.Object!.Importer!.Id != user.Id)
                    {
                        throw new ConflictException("User cannot access this request.");
                    }

                    break;
                }
                case IdentityData.Roles.Employee:
                {
                    var borrow = await _context.Borrows.FirstOrDefaultAsync(x =>
                        x.Borrower.Id == user.Id && x.Document.Id == log.Object!.Id);

                    if (borrow is null)
                    {
                        throw new ConflictException("User cannot access this request.");
                    }

                    break;
                }
            }
        }
    }
}