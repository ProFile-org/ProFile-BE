using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetAllEntriesPaginated
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.EntryPath)
                .NotEmpty().WithMessage("File's path is required.")
                .Matches("^(/(?!/)[a-z_.\\-0-9]*)+(?<!/)$|^/$").WithMessage("Invalid path format.");
        }
    }

    public record Query : IRequest<PaginatedList<EntryDto>>
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
        public string EntryPath { get; init; } = null!;
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<EntryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<EntryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entries = _context.Entries
                .Where(x => x.Path.Equals(request.EntryPath))
                .AsQueryable();
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<EntryDto>())
            {
                sortBy = nameof(EntryDto.Path);
            }
            
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var count = await entries.CountAsync(cancellationToken);
            var list  = await entries
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<EntryDto>>(list);
            return new PaginatedList<EntryDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}