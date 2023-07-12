using Application.Common.Exceptions;
using Application.Common.Extensions;
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
                .Include(x => x.Document)
                .ThenInclude(x => x.Folder)
                .ThenInclude(x => x.Locker)
                .ThenInclude(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == request.BorrowId, cancellationToken);

            if (borrow is null)
            {
                throw new KeyNotFoundException("Borrow request does not exist.");
            }

            if (request.User.Role.IsAdmin())
            {
                return _mapper.Map<BorrowDto>(borrow);
            }

            if (request.User.Role.IsStaff())
            {
                var staff = _context.Staffs
                    .Include(x => x.Room)
                    .FirstOrDefault(x => x.Id == request.User.Id);
                if (staff is null)
                {
                    throw new KeyNotFoundException("Staff does not exist.");
                }

                if (staff.Room is null)
                {
                    throw new ConflictException("Staff does not manage a room.");
                }
                
                if (staff.Room.Id != borrow.Document.Folder!.Locker.Room.Id )
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }
                
                return _mapper.Map<BorrowDto>(borrow);
            }
            
            return borrow.Borrower.Id != request.User.Id
                ? throw new UnauthorizedAccessException("User can not access this resource")
                : _mapper.Map<BorrowDto>(borrow);
        }
    }
}