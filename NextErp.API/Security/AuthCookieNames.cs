namespace NextErp.API.Security;

/// <summary>
/// Names of the auth cookies, shared by the code that issues them
/// (AuthController), the JWT middleware that reads the access token, and the
/// CSRF middleware that validates the double-submit token.
/// </summary>
public static class AuthCookieNames
{
    // httpOnly — the short-lived JWT. JS can never read it.
    public const string Access = "nexterp_access";

    // httpOnly — the long-lived refresh token (raw value; only its hash is in DB).
    // Scoped to /api/auth so it rides only on auth calls, not every request.
    public const string Refresh = "nexterp_refresh";

    // NOT httpOnly — readable by JS so the SPA can echo it into X-CSRF-Token.
    public const string Csrf = "nexterp_csrf";

    // Path the refresh cookie is scoped to. Must be lowercase to match the
    // browser-sent request path (cookie Path matching is case-sensitive).
    public const string RefreshPath = "/api/auth";
}
