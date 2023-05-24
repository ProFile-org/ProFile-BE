using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentsByTitle;

public record GetDocumentsByTitleQuery : IRequest<PaginatedList<DocumentDto>>
{
    public Guid UserId { get; init; }
    public string? SearchTerm { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
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
            throw new KeyNotFoundException("User does not exist.");
        }

        if (currentUser.Department is null)
        {
            throw new KeyNotFoundException("Department does not exist.");
        }

        var currentUserDepartmentId = currentUser.Department.Id;
        var pageNumber = request.Page ?? 1;
        var sizeNumber = request.Size ?? 5;
        
        var documents = await _context.Documents
            .Include(x => x.Department)
            .Where(x => (currentUser.Role.Equals(IdentityData.Roles.Admin) 
                         || (x.Department != null && x.Department.Id.Equals(currentUserDepartmentId))) 
                        && !string.IsNullOrEmpty(request.SearchTerm)
                        && x.Title.ToLower().Contains(request.SearchTerm.ToLower()))
            .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.Title)
            .PaginatedListAsync(pageNumber, sizeNumber);
        return documents;
    }
}