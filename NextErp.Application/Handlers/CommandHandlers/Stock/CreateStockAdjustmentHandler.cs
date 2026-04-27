using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NextErp.Application.Commands;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Application.Handlers.CommandHandlers.Stock;

public class CreateStockAdjustmentHandler(
    IStockService stockService,
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider)
    : IRequestHandler<CreateStockAdjustmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken = default)
    {
        // Input-shape validation (Quantity > 0, valid ReasonCode, Notes length, enum range)
        // is enforced by CreateStockAdjustmentCommandValidator via the FluentValidation pipeline.
        // Only business rules remain here.

        var variant = await dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Product variant {request.ProductVariantId} was not found.");

        var branchId = branchProvider.GetBranchId()
            ?? throw new InvalidOperationException("Branch context required");

        var current = await stockService.GetAvailableStockAsync(request.ProductVariantId, cancellationToken);

        decimal delta = request.Mode switch
        {
            StockAdjustmentMode.Increase => request.Quantity,
            StockAdjustmentMode.Decrease => -request.Quantity,
            StockAdjustmentMode.SetAbsolute => request.Quantity - current,
            _ => throw new InvalidOperationException("Unknown adjustment mode")
        };

        if (request.Mode == StockAdjustmentMode.Decrease && current + delta < 0)
            throw new InvalidOperationException(
                $"Adjustment would result in negative stock (current: {current}, requested decrease: {request.Quantity})");

        if (delta == 0)
            throw new InvalidOperationException("No change needed");

        await stockService.RecordMovementAsync(
            request.ProductVariantId,
            variant.TenantId,
            branchId,
            delta,
            StockMovementType.ManualAdjustment,
            Guid.Empty,
            reason: request.ReasonCode,
            notes: request.Notes,
            cancellationToken: cancellationToken);

        // RecordMovementAsync staged a StockMovement on the change tracker.
        // Capture its Id before SaveChanges so we can return it to the caller.
        var movementId = dbContext.StockMovements
            .Local
            .Where(m => m.ProductVariantId == request.ProductVariantId
                        && m.BranchId == branchId
                        && m.MovementType == StockMovementType.ManualAdjustment
                        && m.QuantityChanged == delta)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.Id)
            .FirstOrDefault();

        await dbContext.SaveChangesAsync(cancellationToken);
        return movementId;
    }
}
