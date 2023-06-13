using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentReason
{
    public record Query : IRequest<ReasonDto>
    {
        public Guid DocumentId { get; init; }
        public RequestType Type { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, ReasonDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ReasonDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.RequestLogs
                .FirstOrDefaultAsync(x => x.Object!.Id == request.DocumentId
                    && x.Type == request.Type, cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Document does not have a request.");
            }
            
            return _mapper.Map<ReasonDto>(log);
        }
    }
}