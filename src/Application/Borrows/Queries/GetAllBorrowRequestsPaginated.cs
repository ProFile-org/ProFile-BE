using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllBorrowRequestsPaginated
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        }
    }
    
    public record Query : IRequest<PaginatedList<BorrowDto>>
    {
        public Guid? DepartmentId { get; init; }
        public Guid? DocumentId { get; init; }
        public Guid? EmployeeId { get; init; }
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

            if (request.DepartmentId is not null)
            {
                borrows = borrows.Where(x => x.Document.Department!.Id == request.DepartmentId);
            }
            
            if (request.EmployeeId is not null)
            {
                borrows = borrows.Where(x => x.Borrower.Id == request.EmployeeId);
            }
            
            if (request.DocumentId is not null)
            {
                borrows = borrows.Where(x => x.Document.Id == request.DocumentId);
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
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<BorrowDto>>(list);

            return new PaginatedList<BorrowDto>(result, result.Count, pageNumber.Value, sizeNumber.Value);
        }
    }
}