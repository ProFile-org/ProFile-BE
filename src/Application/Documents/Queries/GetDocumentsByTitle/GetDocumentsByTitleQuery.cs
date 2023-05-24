using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentsByTitle;

public record GetDocumentsByTitleQuery : IRequest<PaginatedList<DocumentDto>>
{
    public string? SearchTerm { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
}

public class GetDocumentsByTitleQueryHandler : IRequestHandler<GetDocumentsByTitleQuery, PaginatedList<DocumentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _service;

    public GetDocumentsByTitleQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService service)
    {
        _context = context;
        _mapper = mapper;
        _service = service;
    }

    public async Task<PaginatedList<DocumentDto>> Handle(GetDocumentsByTitleQuery request, CancellationToken cancellationToken)
    {
        var currentUserRole = _service.GetRole();
        var currentUserDepartmentName = _service.GetDepartment();

        var pageNumber = request.Page ?? 1;
        var sizeNumber = request.Size ?? 5;

        if (currentUserDepartmentName is null)
        {
            return new PaginatedList<DocumentDto>(new List<DocumentDto>(), 0, pageNumber, sizeNumber);
        }
        
        var documents = _context.Documents.AsQueryable();
        documents = documents.Include(x => x.Department);
        if (CurrentUserIsNotAdmin(currentUserRole))
        {
            documents = documents.Where(x => BelongToDepartment(x, currentUserDepartmentName));
        }
        
        var result = await documents
            .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.Title)
            .PaginatedListAsync(pageNumber, sizeNumber);
        
        return result;
    }

    private bool CurrentUserIsNotAdmin(string currentUserRole) 
        => !currentUserRole.Equals(IdentityData.Roles.Admin);

    private bool BelongToDepartment(Document document, string currentUserDepartmentName)
        => document.Department!.Name.Equals(currentUserDepartmentName);


}