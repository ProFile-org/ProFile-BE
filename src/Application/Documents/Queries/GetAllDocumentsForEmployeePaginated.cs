using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllDocumentsForEmployeePaginated
{
    public record Query : IRequest<PaginatedList<DocumentDto>>
    {
        public Guid CurrentUserId { get; init; }
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid? UserId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
        public string? DocumentStatus { get; init; }
        public bool IsPrivate { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<DocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<DocumentDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var documents = _context.Documents.AsQueryable();

            documents = documents
                .Include(x => x.Department)
                .Include(x => x.Folder)
                .ThenInclude(y => y.Locker)
                .ThenInclude(z => z.Room);

            if (request.IsPrivate)
            {
                var permissions = _context.Permissions.Where(x =>
                    IsSameUser(x.EmployeeId, request.CurrentUserId)
                    && InSameDepartmentAsUser(x.Document, request.CurrentUserDepartmentId)
                    && HasReadPermission(x.AllowedOperations))
                    .Select(x => x.DocumentId);

                documents = documents.Where(x =>
                    InSameDepartmentAsUser(x, request.CurrentUserDepartmentId)
                    && x.IsPrivate
                    && (CanRead(permissions, x.Id) || IsImporter(x, request.CurrentUserId)));
            }
            else
            {
                documents = documents.Where(x => 
                    InSameDepartmentAsUser(x, request.CurrentUserDepartmentId)
                     && !x.IsPrivate);
            }

            if (request.UserId is not null)
            {
                documents = documents.Where(x => x.Importer!.Id == request.UserId);
            }
            
            if (request.DocumentStatus is not null 
                 && Enum.TryParse(request.DocumentStatus, true, out DocumentStatus status))
            {
                documents = documents.Where(x => x.Status == status);
            }

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                documents = documents.Where(x =>
                    x.Title.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            return await documents
                .ListPaginateWithSortAsync<Document, DocumentDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }

        private static bool IsSameUser(Guid userId1, Guid userId2)
            => userId1 == userId2;

        private static bool InSameDepartmentAsUser(Document document, Guid userDepartmentId)
            => document.Department!.Id == userDepartmentId;

        private static bool HasReadPermission(string allowedPermissions)
            => allowedPermissions.Contains(DocumentOperation.Read.ToString());

        private static bool CanRead(IEnumerable<Guid> documentIds, Guid documentId)
            => documentIds.Contains(documentId);
        
        private static bool IsImporter(Document document, Guid userId)
            => document.ImporterId == userId;
    }
}