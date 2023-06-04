using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetBorrowRequestById
{
    public record Query : IRequest<BorrowDto>
    {
        public Guid BorrowId { get; init; }
        public User User { get; init; } = null!;
    }
    public class QueryHandler : IRequestHandler<Query, BorrowDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BorrowDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var borrow = await _context.Borrows
                .Include(x => x.Borrower)
                .Include(x => x.Document)
                .ThenInclude(y => y.Department)
                .FirstOrDefaultAsync(x => x.Id == request.BorrowId, cancellationToken);

            if (borrow is null)
            {
                throw new KeyNotFoundException("Borrow request does not exist.");
            }

            if (!request.User.Role.Equals(IdentityData.Roles.Employee))
            {
                return _mapper.Map<BorrowDto>(borrow);
            }
            
            return borrow.Borrower.Id != request.User.Id
                ? throw new UnauthorizedAccessException()
                : _mapper.Map<BorrowDto>(borrow);
        }
    }
}