using CloudAppServer.Application.Exceptions;

namespace CloudAppServer.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (HttpException ex)
        {
            ctx.Response.StatusCode = (int)ex.StatusCode;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}