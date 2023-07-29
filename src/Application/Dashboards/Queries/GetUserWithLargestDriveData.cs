using Application.Common.Interfaces;
using Application.Common.Models.Dtos.DashBoard;
using Application.Users.Queries;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Dashboards.Queries;

public class GetUserWithLargestDriveData
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date can not be empty.");
        }
    }
    public record Query : IRequest<LargestDriveDto>
    {
        public DateTime Date { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, LargestDriveDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LargestDriveDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentDate = request.Date.Date;
            var nextDate = currentDate.AddDays(1);

            var usersWithFileData = await _context.Entries
                .Where(x => x.FileId != null && x.Created >= LocalDateTime.FromDateTime(currentDate)
                                             && x.Created < LocalDateTime.FromDateTime(nextDate))
                .GroupBy(entry => entry.Uploader)
                .Select(group => new LargestDriveDto()
                {
                    User = _mapper.Map<UserDto>(group.Key),
                    Label = $"{currentDate:dd-MM}",
                    Value = group.Sum(x => x.File!.FileData.Length)
                })
                .OrderByDescending(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);
            return usersWithFileData;
        }
    }
}