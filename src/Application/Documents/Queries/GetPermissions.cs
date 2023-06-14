using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetPermissions
{
    public record Query : IRequest<PermissionDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid DocumentId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PermissionDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PermissionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var performingUser =
                await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);

            if (performingUser is null)
            {
                throw new UnauthorizedAccessException();
            }
            
            var document = await _context.Documents
                .Include(x => x.Folder)
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            if (document.Importer!.Id == request.PerformingUserId)
            {
                var result = new PermissionDto()
                {
                    DocumentId = document.Id,
                    EmployeeId = performingUser.Id,
                    CanRead = true,
                    CanBorrow = true,
                };

                return result;
            }

            var permission = await _context.Permissions.FirstOrDefaultAsync(
                x => x.DocumentId == request.DocumentId && x.EmployeeId == request.PerformingUserId,
                cancellationToken);

            if (permission is null)
            {
                return new PermissionDto()
                {
                    DocumentId = document.Id,
                    EmployeeId = performingUser.Id,
                    CanRead = false,
                    CanBorrow = false,
                };
            }

            return _mapper.Map<PermissionDto>(permission);
        }
    }
}