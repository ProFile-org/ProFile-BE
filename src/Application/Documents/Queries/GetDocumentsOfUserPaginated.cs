using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetDocumentsOfUserPaginated
{
    public record Query : IRequest<PaginatedList<DocumentDto>>
    {
        public Guid UserId { get; set; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }

    public class Handler : IRequestHandler<Query, PaginatedList<DocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<DocumentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId
                                                                     && x.IsActive
                                                                     && x.IsActivated, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            var documents = _context.Documents
                .Include(x => x.Department)
                .Include(x => x.Folder)
                .AsQueryable()
                .Where(x => x.Importer!.Id.Equals(request.UserId) && !x.IsPrivate);
            
            return await documents
                .ListPaginateWithSortAsync<Document, DocumentDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}