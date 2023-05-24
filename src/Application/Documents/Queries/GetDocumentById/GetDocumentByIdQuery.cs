using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery : IRequest<DocumentDto>
{
    public Guid Id { get; init; } 
}

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException("Document does not exist.");
        }
        
        return _mapper.Map<DocumentDto>(document);
    }
}