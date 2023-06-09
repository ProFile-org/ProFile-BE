﻿using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Entries.Queries;

public class GetEntryById
{
    public record Query : IRequest<EntryDto>
    {
        public Guid EntryId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EntryDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var entry = await _context.Entries
                .Include(x => x.File)
                .Include(x => x.Uploader)
                .ThenInclude(x => x.Department)
                .Include(x => x.Owner)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (entry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            return _mapper.Map<EntryDto>(entry);
        }
    }
}