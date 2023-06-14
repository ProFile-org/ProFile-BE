using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetPermissions
{
    public record Query : IRequest<PermissionDto>
    {
        public User PerformingUser { get; init; } = null!;
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
            var document = await _context.Documents
                .Include(x => x.Importer)
                .FirstOrDefaultAsync(x => x.Id == request.DocumentId, cancellationToken);
            if (document is null)
            {
                throw new ConflictException("Document does not exist.");
            }

            if (IsOwner(request.PerformingUser.Id, document))
            {
                return new PermissionDto()
                {
                    DocumentId = document.Id,
                    EmployeeId = request.PerformingUser.Id,
                    CanRead = true,
                    CanBorrow = true,
                };
            }

            var permission = await _context.Permissions.FirstOrDefaultAsync(
                x => x.DocumentId == request.DocumentId && x.EmployeeId == request.PerformingUser.Id,
                cancellationToken);

            if (permission is null)
            {
                return new PermissionDto()
                {
                    DocumentId = document.Id,
                    EmployeeId = request.PerformingUser.Id,
                    CanRead = false,
                    CanBorrow = false,
                };
            }

            return _mapper.Map<PermissionDto>(permission);
        }

        private static bool IsOwner(Guid userId, Document document)
        {
            return document.Importer!.Id == userId;
        }
    }
}