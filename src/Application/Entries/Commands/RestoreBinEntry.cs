﻿using Application.Common.Exceptions;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Entries.Commands;

public class RestoreBinEntry
{
    public record Command : IRequest<EntryDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid EntryId { get; init; }
    }

    public class Handler : IRequestHandler<Command, EntryDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private const string BinString = "_bin";
        private readonly ILogger<RestoreBinEntry> _logger;
        public Handler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RestoreBinEntry> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<EntryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var binEntry = await _context.Entries
                .Include(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Id == request.EntryId, cancellationToken);

            if (binEntry is null)
            {
                throw new KeyNotFoundException("Entry does not exist.");
            }

            var entryPath = binEntry.Path;
            var firstSlashIndex = entryPath.IndexOf("/", StringComparison.Ordinal);
            var binCheck = firstSlashIndex < 0 ? entryPath : entryPath.Substring(0, firstSlashIndex);

            var path1 = request.CurrentUser.Username + BinString;
            if (!binCheck.Equals(path1))
            {
                throw new NotChangedException("Entry is not in bin.");
            }

            if (binEntry.Owner.Id != request.CurrentUser.Id)
            {
                throw new UnauthorizedAccessException("You do not have the permission to restore this entry.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var entryCheckPath = entryPath.Replace(binCheck, "");

            var entryCheck = await _context.Entries.FirstOrDefaultAsync(x =>
                (x.Path.Equals("/") ? x.Path + x.Name : x.Path + "/" + x.Name).Equals(entryCheckPath + "/" + binEntry.Name)
                    && x.OwnerId == request.CurrentUser.Id
                    && ((x.FileId != null &&  binEntry.FileId != null) || (x.FileId == null &&  binEntry.FileId == null)), cancellationToken);

            if (entryCheck is not null)
            {
                throw new ConflictException("Entry already exists outside of bin.");
            }

            var splitPath = binEntry.OldPath!.Split("/");
            var currentPath = "/";
            foreach (var node in splitPath)
            {
                if (Array.IndexOf(splitPath, node) == 0)
                {
                    continue;
                }

                if (currentPath.Equals("/") && node.Equals(""))
                {
                    continue;
                }
                
                var entrySearch = await _context.Entries.FirstOrDefaultAsync(x => x.Path.Equals(currentPath) 
                    && x.Name.Equals(node), cancellationToken);
                
                if (entrySearch is not null)
                {
                    currentPath = currentPath.Equals("/") ? currentPath + node : currentPath + "/" + node;
                    continue;
                }

                var parentEntry = new Entry()
                {
                    Name = node,
                    Path = currentPath,
                    Created = localDateTimeNow,
                    CreatedBy = request.CurrentUser.Id,
                    OwnerId = request.CurrentUser.Id,
                    Owner = request.CurrentUser,
                    Uploader = request.CurrentUser,
                };
                await _context.Entries.AddAsync(parentEntry, cancellationToken);
                currentPath = currentPath.Equals("/") ? currentPath + node : currentPath + "/" + node;
            }
            
            if (binEntry.IsDirectory)
            {
                var path = binEntry.Path[^1].Equals('/') ? binEntry.Path + binEntry.Name : $"{binEntry.Path}/{binEntry.Name}";
                var pattern = $"{path}/%";
                var childEntries = _context.Entries.Where(x => 
                    x.Path.Trim().Equals(path)
                    || EF.Functions.Like(x.Path, pattern));

                foreach (var childEntry in childEntries)
                {
                    childEntry.Path = childEntry.Path.Replace(binCheck, "");
                    childEntry.OldPath = null;
                    childEntry.LastModified = localDateTimeNow;
                    childEntry.LastModifiedBy = request.CurrentUser.Id;
                    _context.Entries.Update(childEntry);
                }
            }
            
            binEntry.Path = binEntry.OldPath;
            binEntry.OldPath = null;
            binEntry.LastModified = localDateTimeNow;
            binEntry.LastModifiedBy = request.CurrentUser.Id;

            var result = _context.Entries.Update(binEntry);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Entry), binEntry.Id, request.CurrentUser.Id))
            {
                _logger.LogRestoreBinEntry(request.CurrentUser.Username, binEntry.Id.ToString());
            }
            return _mapper.Map<EntryDto>(result.Entity);
        }
    }
}
