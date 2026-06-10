using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Products;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Product;

public class GetNextProductCodeHandler(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IRequestHandler<GetNextProductCodeQuery, string>
{
    public async Task<string> Handle(
        GetNextProductCodeQuery request,
        CancellationToken cancellationToken = default)
    {
        var branchId = branchProvider.GetRequiredBranchId();
        var branch = await dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken)
            ?? throw new InvalidOperationException($"Branch '{branchId}' was not found.");

        return await ProductCodeFactory.NextCodeAsync(branch.TenantId, dbContext, cancellationToken);
    }
}
