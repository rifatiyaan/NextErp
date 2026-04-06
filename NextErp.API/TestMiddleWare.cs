namespace NextErp.API
{
    public class TestMiddleWare(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            ctx.Response.Headers.Append("Testing custom middlewares", Guid.NewGuid().ToString());

            await next(ctx);
        }
    }
}
