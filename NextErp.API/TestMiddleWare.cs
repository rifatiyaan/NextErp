namespace NextErp.API
{
    public class TestMiddleWare
    {
        private readonly RequestDelegate _next;

        public TestMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            ctx.Response.Headers.Append("Testing custom middlewares", Guid.NewGuid().ToString());

            await _next(ctx);
        }

    }
}
