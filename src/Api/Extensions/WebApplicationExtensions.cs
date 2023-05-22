using Api.Middlewares;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        
        // Handle all exceptions: return appropriate status codes and messages
        app.UseMiddleware<ExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/", () => "Hello from ProFile!");
    }
}