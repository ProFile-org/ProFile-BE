using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using Domain.Entities.Digital;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Commands;

public class CreateUserGroup
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(32).WithMessage("Name cannot exceed 32 characters.");
        }
    }

    public record Command : IRequest<UserGroupDto>
    {
        public string Name { get; init; } = null!;
    }

    public class CommandHandler : IRequestHandler<Command, UserGroupDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserGroupDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(x => x.Name.Trim() == request.Name.Trim(), cancellationToken);

            if (userGroup is not null)
            {
                throw new ConflictException("User group name already exists.");
            }

            var entity = new UserGroup()
            {
                Name = request.Name.Trim(),
            };

            var result = await _context.UserGroups.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserGroupDto>(result.Entity);
        }
    }
}