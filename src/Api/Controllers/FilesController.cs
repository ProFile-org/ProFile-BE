using Api.Controllers.Payload.Requests.DigitalFile;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class FilesController  : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public FilesController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPost]
    public async Task<ActionResult<Result<EntryDto>>> UploadDigitalFile(
        [FromForm] UploadDigitalFileRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var fileData = new MemoryStream();
        await request.File.CopyToAsync(fileData);
        
        var command = new UploadDigitalFile.Command()
        {
            CurrentUser =  currentUser,
            Path = request.Directory,
            Name = request.File.FileName,
            FileType = request.File.ContentType,
            FileData = fileData,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
}