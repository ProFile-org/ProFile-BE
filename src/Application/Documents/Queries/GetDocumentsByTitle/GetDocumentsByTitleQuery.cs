using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentsByTitle;

public record GetDocumentsByTitleQuery : IRequest<PaginatedList<DocumentDto>>
{
    public Guid UserId { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
}

public class GetDocumentsByTitleQueryHandler : IRequestHandler<GetDocumentsByTitleQuery, PaginatedList<DocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDocumentsByTitleQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DocumentDto>> Handle(GetDocumentsByTitleQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);

        if (currentUser is null)
        {
            throw new KeyNotFoundException("User does not exist");
        }

        if (currentUser.Department is null)
        {
            throw new KeyNotFoundException("Department does not exist");
        }

        var currentUserDepartmentId = currentUser.Department.Id;

        var documents = await _context.Documents
            .Include(x => x.Department)
            .Where(x => (currentUser.Role.Equals("Admin") 
                         || (x.Department != null && x.Department.Id.Equals(currentUserDepartmentId))) 
                        && !string.IsNullOrEmpty(request.SearchTerm)
                        && x.Title.ToLower().Contains(request.SearchTerm.ToLower()))
            .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.Title)
            .PaginatedListAsync(request.Page, request.Size);
        return documents;
    }
}