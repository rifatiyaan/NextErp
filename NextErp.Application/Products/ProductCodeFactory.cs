using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Products;

internal static class ProductCodeFactory
{
    private const string Prefix = "P";
    private const int Digits = 6;

    // A blank product code is auto-assigned as the prefix + a zero-padded
    // 6-digit running number (e.g. P000001), sequential per tenant. A
    // caller-supplied code is kept verbatim so manual codes still work.
    public static async Task<string> EnsureCodeAsync(
        string? code,
        Guid tenantId,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
        => !string.IsNullOrWhiteSpace(code)
            ? code.Trim()
            : await NextCodeAsync(tenantId, dbContext, cancellationToken);

    // The next auto code for a tenant (e.g. P000001). Used both to fill a blank
    // code on save and to preview/prefill the create form.
    public static async Task<string> NextCodeAsync(
        Guid tenantId,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var next = await NextSequenceAsync(tenantId, dbContext, cancellationToken);
        return Prefix + next.ToString("D" + Digits);
    }

    private static async Task<int> NextSequenceAsync(
        Guid tenantId,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Scan tenant-wide (every branch, including soft-deleted) so a number is
        // never reused — bypass the branch/IsActive global query filter.
        var existingCodes = await dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == tenantId && p.Code.StartsWith(Prefix))
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var existing in existingCodes)
        {
            if (existing.Length == Prefix.Length + Digits
                && int.TryParse(existing.Substring(Prefix.Length), out var number)
                && number > max)
            {
                max = number;
            }
        }

        return max + 1;
    }
}
