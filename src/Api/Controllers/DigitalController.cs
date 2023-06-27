using Api.Controllers.Payload.Requests.DigitalFile;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DigitalController  : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public DigitalController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    
    [HttpPost("upload")]
    public async Task<ActionResult<Result<EntryDto>>> UploadDigitalFile(
        [FromForm] UploadDigitalFileRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var stream = new MemoryStream();
        await request.File.CopyToAsync(stream);
        
        var command = new UploadDigitalFile.Command()
        {
            CurrentUser =  currentUser,
            Path = request.Path,
            Name = request.File.FileName,
            FileType = request.File.ContentType,
            FileData = stream,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
}