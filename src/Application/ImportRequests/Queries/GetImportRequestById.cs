using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.ImportDocument;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ImportRequests.Queries;

public class GetImportRequestById {
    public record Query : IRequest<ImportRequestDto>
    {
        public Guid CurrentUserId { get; init; }
        public string CurrentUserRole { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
        public Guid RequestId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, ImportRequestDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ImportRequestDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var importRequest = await _context.ImportRequests
                .Include(x => x.Document)
                .ThenInclude(x => x.Importer)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RequestId), cancellationToken);
            
            if (importRequest is null)
            {
                throw new KeyNotFoundException("Import request does not exist.");
            }
            
            if (request.CurrentUserRole.IsStaff()
                && (request.CurrentStaffRoomId is null || importRequest.Room.Id != request.CurrentStaffRoomId))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }
            
            if (request.CurrentUserRole.IsEmployee()
                && importRequest.Document.ImporterId != request.CurrentUserId)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }
            
            return _mapper.Map<ImportRequestDto>(importRequest);
        }
    }
}