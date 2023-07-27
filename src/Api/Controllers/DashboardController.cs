using Application.Common.Interfaces;

namespace Api.Controllers;

public class DashboardController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    public DashboardController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
}