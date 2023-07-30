using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetSharedEntryById
{
    public record Query : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }

    public class Handler : IRequestHandler<Query, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IMapper mapper, IApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<EntryDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var permission = await _context.EntryPermissions
                .Include(x => x.Entry)
                .ThenInclude(x => x.Uploader)
                .Include(x => x.Entry)
                .ThenInclude(x => x.Owner)
                .Include(x => x.Entry)
                .ThenInclude(x => x.File)
                .FirstOrDefaultAsync(x => x.EntryId == request.EntryId && x.EmployeeId == request.CurrentUser.Id,
                    cancellationToken);

            if (permission is null)
            {
                throw new KeyNotFoundException("Shared entry does not exist.");
            }

            if (!permission.AllowedOperations.Contains(EntryOperation.View.ToString()))
            {
                throw new NotAllowedException("You do not have permission to view this shared entry.");
            }
            
            return _mapper.Map<EntryDto>(permission.Entry);
        }
    }
}