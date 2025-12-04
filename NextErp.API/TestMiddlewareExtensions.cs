namespace NextErp.API
{
    public static class TestMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleWarePlease(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TestMiddleWare>();
        }
    }
}
