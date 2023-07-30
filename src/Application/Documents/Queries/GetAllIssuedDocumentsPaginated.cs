using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllIssuedDocumentsPaginated
{
    public record Query : IRequest<PaginatedList<IssuedDocumentDto>>
    {
        public Guid DepartmentId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<IssuedDocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<IssuedDocumentDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var documents = _context.Documents
                .Include(x => x.Importer)
                .Where(x => x.Status == DocumentStatus.Issued);

            documents = documents.Where(x => x.Department!.Id == request.DepartmentId);
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                documents = documents.Where(x =>
                    x.Title.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            return await documents
                .ListPaginateWithSortAsync<Document, IssuedDocumentDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}