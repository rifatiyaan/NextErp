using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Ecommerce;

public static class OnlineOrderNumberFactory
{
    private const string Prefix = "W";
    private const int Digits = 6;

    public static async Task<string> NextNumberAsync(
        Guid tenantId,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.OnlineOrders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.OrderNumber.StartsWith(Prefix))
            .Select(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var number in existing)
        {
            if (number.Length == Prefix.Length + Digits
                && int.TryParse(number.Substring(Prefix.Length), out var n)
                && n > max)
            {
                max = n;
            }
        }

        return Prefix + (max + 1).ToString("D" + Digits);
    }
}
