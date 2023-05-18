using System.Text.Json;
using Application.Common.Models;

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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        // Note: Handle every exception you throw here
        if (ex is KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteExceptionMessageAsync(context, ex);
        }

        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            Console.WriteLine(ex.ToString());
        }
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