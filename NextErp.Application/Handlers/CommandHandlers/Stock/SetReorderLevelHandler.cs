using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.Stock;
using NextErp.Application.Interfaces;

namespace NextErp.Application.Handlers.CommandHandlers.Stock;

public class SetReorderLevelHandler(IApplicationDbContext dbContext, IBranchProvider branchProvider)
    : IRequestHandler<SetReorderLevelCommand>
{
    public async Task Handle(SetReorderLevelCommand request, CancellationToken cancellationToken = default)
    {
        var branchId = branchProvider.GetRequiredBranchId();

        var stock = await dbContext.Stocks
            .FirstOrDefaultAsync(
                s => s.ProductVariantId == request.ProductVariantId && s.BranchId == branchId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"No stock record found for variant {request.ProductVariantId} in current branch.");

        stock.ReorderLevel = request.ReorderLevel;
        stock.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
