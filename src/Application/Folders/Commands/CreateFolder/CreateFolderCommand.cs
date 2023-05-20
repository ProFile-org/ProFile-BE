using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;

namespace Application.Folders.Commands.CreateFolder;

public record CreateFolderCommand : IRequest<FolderDto>
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public int Capacity { get; init; }
    public Guid LockerId { get; init; }
}

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, FolderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateFolderCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        // TODO: var locker = await _context.Folders.FirstOrDefault(l => ) 
        var entity = new Folder
        {
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            //Locker = request.LockerId
        };
        var result = await _context.Folders.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<FolderDto>(result.Entity);
    }
}
