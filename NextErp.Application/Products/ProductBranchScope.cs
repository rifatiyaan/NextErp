using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Products;

internal static class ProductBranchScope
{
    public static async Task ApplyToProductAsync(
        Entities.Product product,
        IApplicationDbContext dbContext,
        IBranchProvider branchProvider,
        CancellationToken cancellationToken)
    {
        var branchId = branchProvider.GetRequiredBranchId();
        var branch = await dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken)
            ?? throw new InvalidOperationException($"Branch '{branchId}' was not found.");

        product.BranchId = branchId;
        product.TenantId = branch.TenantId;
    }
}
