using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Operations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetAllSharedEntriesPaginated
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
        public User CurrentUser { get; init; } = null!;
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
            var entries = _context.EntryPermissions
                .Where(x => x.EmployeeId == request.CurrentUser.Id
                            && x.AllowedOperations
                                .ToLower()
                                .Contains(EntryOperation.View.ToString()))
                .Select(x => x.Entry);
            
            return await entries
                .ListPaginateWithSortAsync<Entry, EntryDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}