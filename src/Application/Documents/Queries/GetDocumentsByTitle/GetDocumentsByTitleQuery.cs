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
    public string CurrentUserRole { get; init; } = null!;
    public string? CurrentUserDepartment { get; init; }
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
        var pageNumber = request.Page ?? 1;
        var sizeNumber = request.Size ?? 5;

        var documents = _context.Documents.AsQueryable();
        documents = documents
            .Include(x => x.Department);

        
        if (request.SearchTerm is not null)
        {
            documents = documents
                .Where(x => x.Title.Trim().ToLower()
                    .Contains(request.SearchTerm.Trim().ToLower()));
        }

        var result = await documents
            .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (CurrentUserIsAdmin(request.CurrentUserRole))
        {
            return new PaginatedList<DocumentDto>(result, result.Count(), pageNumber, sizeNumber);
        }

        if (request.CurrentUserDepartment is null)
        {
            return new PaginatedList<DocumentDto>(new List<DocumentDto>(), 0, pageNumber, sizeNumber);
        }
       
        result = result.Where(x => 
                x.Department != null
                && x.Department.Name.Equals(request.CurrentUserDepartment))
            .ToList();
        
        // if (string.IsNullOrEmpty(request.SearchTerm))
        // {
        //     result = result.Where(x => 
        //             x.Department != null
        //             && x.Department.Name.Equals(request.CurrentUserDepartment))
        //         .ToList();
        // }
        
        var paginatedList = new PaginatedList<DocumentDto>(result, result.Count(), pageNumber, sizeNumber);
        
        return paginatedList;
    }

    private static bool CurrentUserIsAdmin(string currentUserRole) 
        => currentUserRole.Equals(IdentityData.Roles.Admin);

    private static bool BelongToDepartment(DocumentDto document, string currentUserDepartmentName)
        => document.Department!.Name.Equals(currentUserDepartmentName);


}