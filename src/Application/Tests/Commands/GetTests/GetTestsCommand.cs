using Application.Common.Interfaces;
using MediatR;

namespace Application.Tests.Commands.GetTests;

public record GetTestsCommand : IRequest<int>
{
    
}

public class GetTestsCommandHandler : IRequestHandler<GetTestsCommand, int>
{
    private readonly IUnitOfWork _uow;

    public GetTestsCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> Handle(GetTestsCommand request, CancellationToken cancellationToken)
    {
        return await _uow.SampleRepository.GetTests();
    }
} 