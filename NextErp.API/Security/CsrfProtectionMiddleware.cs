using System.Security.Cryptography;
using System.Text;

namespace NextErp.API.Security;

/// <summary>
/// Double-submit CSRF guard for cookie-authenticated requests.
///
/// Because the auth cookie is httpOnly and sent automatically, a cross-site
/// page could trigger a state-changing request that rides the victim's session.
/// The defence: on every unsafe method we require an X-CSRF-Token header that
/// matches the (JS-readable) nexterp_csrf cookie. A cross-site attacker can make
/// the browser send the cookie but cannot READ it (same-origin policy) and so
/// cannot set the matching header.
///
/// Scope: only enforced when the request actually carries an auth cookie. The
/// anonymous storefront (guest COD checkout) has no cookie to ride, and Bearer
/// clients (Swagger) can't be CSRF'd either — both are skipped. Login/register
/// establish the session and carry no cookie yet, so they're exempt too.
/// </summary>
public class CsrfProtectionMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

    public const string HeaderName = "X-CSRF-Token";

    public async Task Invoke(HttpContext ctx)
    {
        var req = ctx.Request;

        if (!SafeMethods.Contains(req.Method) && RequiresCsrf(req))
        {
            var cookieToken = req.Cookies[AuthCookieNames.Csrf];
            var headerToken = req.Headers[HeaderName].ToString();

            if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(cookieToken), Encoding.UTF8.GetBytes(headerToken)))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.WriteAsync("CSRF token missing or invalid.");
                return;
            }
        }

        await next(ctx);
    }

    private static bool RequiresCsrf(HttpRequest req)
    {
        var hasAuthCookie = req.Cookies.ContainsKey(AuthCookieNames.Access)
                            || req.Cookies.ContainsKey(AuthCookieNames.Refresh);
        if (!hasAuthCookie) return false;

        var path = req.Path.Value ?? string.Empty;
        var isBootstrap =
            path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/api/auth/register", StringComparison.OrdinalIgnoreCase);
        return !isBootstrap;
    }
}
