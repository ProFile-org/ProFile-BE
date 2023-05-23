using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;
using Domain.Exceptions;

namespace Api.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private readonly IDictionary<Type, Action<HttpContext, Exception>> _exceptionHandlers;

    public ExceptionMiddleware()
    {
        _exceptionHandlers = new Dictionary<Type, Action<HttpContext, Exception>>
        {
            // Note: Handle every exception you throw here
            { typeof(KeyNotFoundException), HandleKeyNotFoundException },
            { typeof(ConflictException), HandleConflictException },
            { typeof(NotAllowedException), HandleNotAllowedException },
            { typeof(RequestValidationException), HandleRequestValidationException },
            { typeof(LimitExceededException) , HandleLimitExceededException },
            { typeof(InvalidOperationException), HandleInvalidOperationException },
        };
    }


    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var type = ex.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context, ex);
            return;
        }
        
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        Console.WriteLine(ex.ToString());
    }

    private async void HandleKeyNotFoundException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await WriteExceptionMessageAsync(context, ex);
    }
    
    private async void HandleConflictException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await WriteExceptionMessageAsync(context, ex);
    }
    
    private async void HandleNotAllowedException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
        await WriteExceptionMessageAsync(context, ex);
    }
    
    private async void HandleRequestValidationException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var rve = ex as RequestValidationException;
        var data = rve!.Errors!.ToDictionary(vf => vf.PropertyName.ToLower(), vf => vf.ErrorMessage);

        var result = Result<Dictionary<string, string>>.Fail(ex) with
        {
            Data = data
        };
        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(result));
    }

    private async void HandleLimitExceededException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await WriteExceptionMessageAsync(context, ex);
    }

    private async void HandleInvalidOperationException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await WriteExceptionMessageAsync(context, ex);
        
    }

    private static async Task WriteExceptionMessageAsync(HttpContext context, Exception ex)
    {
        await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(Result<string>.Fail(ex)));
    }

    private static byte[] SerializeToUtf8BytesWeb<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes<T>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}