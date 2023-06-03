using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllBorrowRequestsSpecificPaginated
{
    public record Query : IRequest<PaginatedList<BorrowDto>>
    {
        public Guid? DocumentId { get; set; }
        public Guid? EmployeeId { get; set; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<BorrowDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<BorrowDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var borrows = _context.Borrows.AsQueryable();

            borrows = borrows
                .Include(x => x.Borrower)
                .Include(x => x.Document)
                .ThenInclude(y => y.Department)
                .Include(x => x.Document)
                .ThenInclude(y => y.Folder)
                .ThenInclude(z => z!.Locker)
                .ThenInclude(t => t.Room)
                .ThenInclude(s => s.Department);

            if (request.EmployeeId is not null)
            {
                borrows = borrows.Where(x => x.Borrower.Id == request.EmployeeId);
            }
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<BorrowDto>())
            {
                sortBy = nameof(BorrowDto.Status);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
            var list  = await borrows
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .OrderByCustom(sortBy, sortOrder)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<BorrowDto>>(list);

            return new PaginatedList<BorrowDto>(result, result.Count, pageNumber.Value, sizeNumber.Value);
        }
    }
}