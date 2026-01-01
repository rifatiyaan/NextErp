using MediatR;

namespace NextErp.Application.Commands
{
    // Create
    public record CreateCustomerCommand(
        string Title,
        string? Email,
        string? Phone,
        string? Address,
        bool IsActive = true
    ) : IRequest<Guid>; // Returns Id of created customer

    // Update
    public record UpdateCustomerCommand(
        Guid Id,
        string Title,
        string? Email,
        string? Phone,
        string? Address,
        bool IsActive = true
    ) : IRequest<Unit>; // No return

    // Soft Delete
    public record SoftDeleteCustomerCommand(Guid Id) : IRequest<Unit>; // No return
}
